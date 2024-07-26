using System;
using System.IO;
using System.Collections.Generic;
using BlockID = System.UInt16;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Events;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using MCGalaxy.Maths;
using MCGalaxy.Blocks;
using MCGalaxy.Network;
using MCGalaxy.Commands;
using MCGalaxy.Bots;
namespace MCGalaxy {
	
	
	public class SimpleSurvival : Plugin {
		public override string name { get { return "SimpleSurvival"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "SimpleSurvival"; } }
		public override string creator { get { return "morgana, Venk"; } }
		public override bool LoadAtStartup { get { return true; } }

	
		public class Config {
				// Player
				public static int MaxHealth = 20;
				public static int MaxAir = 10;
				public static bool FallDamage = true;
				public static bool VoidKills = true;
				
				// Effects
				public static bool UseGoodlyEffects = true; // broken right now
				public static string HitParticle = "pvp"; // broken right now
				
				// General
				public static string Path = "./plugins/SimpleSurvival/"; // 
				
				
				// MOBS
				public static bool CanKillMobs = true;
				public static bool SpawnMobs = true; // Requires MobAI from https://github.com/ddinan/classicube-stuff/blob/master/MCGalaxy/Plugins/MobAI.cs
				public static int MaxMobs = 12; // Per player
		}
		
		
		
		
		
		
		SchedulerTask drownTask;
		SchedulerTask guiTask;
		SchedulerTask regenTask;
		SchedulerTask mobSpawningTask;
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
			OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.Low);
			OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
			OnSentMapEvent.Register(HandleSentMap, Priority.Low);
			OnPlayerDyingEvent.Register(HandlePlayerDying, Priority.High);
			Server.MainScheduler.QueueRepeat(HandleDrown, null, TimeSpan.FromSeconds(1));
			Server.MainScheduler.QueueRepeat(HandleGUI, null, TimeSpan.FromMilliseconds(100));
			Server.MainScheduler.QueueRepeat(HandleRegeneration, null, TimeSpan.FromSeconds(4));
			Server.MainScheduler.QueueRepeat(HandleMobSpawning, null, TimeSpan.FromSeconds(1));
			Command.Register(new CmdPvP());
			
			loadMaps();
			foreach (Player p in PlayerInfo.Online.Items)
			{
				InitPlayer(p);
			}
			
			if(!Directory.Exists(Config.Path))
			{
				Directory.CreateDirectory(Config.Path);
			}
			if (true)
			{
				AddAi("hostile", new string[] {"", "hostile", "hostile"});
				AddAi("roam", new string[] {"", "roam", "roam"});
			}
			mobHealth.Clear();
	
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
			OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
			OnPlayerMoveEvent.Unregister(HandlePlayerMove);
			OnSentMapEvent.Unregister(HandleSentMap);
			OnPlayerDyingEvent.Unregister(HandlePlayerDying);
			
			Server.MainScheduler.Cancel(drownTask);
			Server.MainScheduler.Cancel(guiTask);
			Server.MainScheduler.Cancel(regenTask);
			Server.MainScheduler.Cancel(mobSpawningTask);
			
			Command.Unregister(Command.Find("PvP"));
			
			mobHealth.Clear();
		}
		public override void Help(Player p) {
			//HELP INFO!
		}
		

		public static List<string> maplist = new List<string>();
		void loadMaps()
        {
            if (File.Exists(Config.Path + "maps.txt"))
            {
                using (var maplistreader = new StreamReader(Config.Path + "maps.txt"))
                {
                    string line;
                    while ((line = maplistreader.ReadLine()) != null)
                    {
                        maplist.Add(line);
                    }
                }
            }
            else File.Create(Config.Path + "maps.txt").Dispose();
        }
		///////////////////////////////////////////////////////////
		// Handlers
		///////////////////////////////////////////////////////////
		void HandleGUI(SchedulerTask task)
        {
            guiTask = task;

            foreach (Player pl in PlayerInfo.Online.Items)
            {
				SendPlayerGui(pl);
			}
		}
		
		void HandlePlayerDying(Player p, BlockID deathblock, ref bool cancel)
        {
			
			if (!maplist.Contains(p.level.name))
			{
				p.SendCpeMessage(CpeMessageType.BottomRight1, "");
				p.SendCpeMessage(CpeMessageType.BottomRight2, "");
				p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
				p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
				return;
			}
			if (deathblock == Block.Orange)
			{
				string deathMessage = p.color +  p.name + " %e was killed by a monster.";// + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			else if (deathblock == 54)
			{
				string deathMessage = p.color +  p.name + " %e burned to death.";// + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			InitPlayer(p);
        }
		void HandleSentMap( Player p, Level prevLevel, Level level)
		{
			if (!maplist.Contains(level.name))
			{
				p.SendCpeMessage(CpeMessageType.BottomRight1, "");
				p.SendCpeMessage(CpeMessageType.BottomRight2, "");
				p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
				p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
				return;
			}
			InitPlayer(p);
			p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
			p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
			SendPlayerGui(p);
		}
		void HandleDrown(SchedulerTask task)
		{
			drownTask = task;
            foreach (Player p in PlayerInfo.Online.Items)
			{
				if (!maplist.Contains(p.level.name)) continue;
				if (p.invincible) continue;
				if (IsDrowning(p))
				{
					if (GetAir(p) > 0)
					{
						SetAir(p, GetAir(p)-1);
						SendPlayerGui(p);
					}
					else
					{
						Damage(p, 1, 8); 
					}
				}
				else if (GetAir(p) < Config.MaxAir)
				{
					SetAir(p, GetAir(p)+1);
				}
				
				if (IsBurning(p))
				{
					Damage(p, 3, 54); 
				}
				
			}
		}
		void HandlePlayerMove(Player p, Position next, byte rotX, byte rotY, ref bool cancel)
		{
			if (!maplist.Contains(p.level.name)) return;
			if (Config.VoidKills && next.Y < 0) Die(p, 4); // Player fell out of the world
			
			if (Config.FallDamage)
			{
				if (p.invincible) return;// || Hacks.CanUseFly(p)) return;

				ushort x = (ushort)(p.Pos.X / 32);
				ushort y = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) - 1);
				//ushort y2 = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) - 2);
				ushort z = (ushort)(p.Pos.Z / 32);

				BlockID block = p.level.GetBlock((ushort)x, ((ushort)y), (ushort)z);
				//BlockID block2 = p.level.GetBlock((ushort)x, ((ushort)y2), (ushort)z);

				string below = Block.GetName(p, block);
				//string below2 = Block.GetName(p, block2);

			
				if (below.ToLower().Contains("water"))// && below2.ToLower().Contains("water"))
				{
					int fall = p.Extras.GetInt("FALL_START") - y;
					//if (fallDamage(fall) > 0 && p.Session.ClientName().CaselessContains("cef")) p.Message("cef resume -n splash"); // Play splash sound effect
					p.Extras["FALLING"] = false;
					p.Extras["FALL_START"] = y;
					return;
				}

				if (!p.Extras.GetBoolean("FALLING") && below.ToLower() == "air")
				{
					p.Extras["FALLING"] = true;
					p.Extras["FALL_START"] = y;
				}

				else if (p.Extras.GetBoolean("FALLING") && below.ToLower() != "air")
				{
					if (p.Extras.GetBoolean("FALLING"))
					{
						int fall = p.Extras.GetInt("FALL_START") - y;

						if (fallDamage(fall) > 0){
						Damage(p, fallDamage(fall), 0);
						}

						// Reset extra variables
						p.Extras["FALLING"] = false;
						p.Extras["FALL_START"] = y;
					}
				}
			}
            
		}
        void HandlePlayerConnect(Player p)
		{
			if (!maplist.Contains(p.level.name)) return;
			InitPlayer(p);
		}
		int GetLagCompensation(int ping)
		{
			int penalty = 0;

			if (ping == 0) penalty = 0; // "lagged-out"
			if (ping > 0 && ping <= 29) penalty = 50; // "great"
			if (ping > 29 && ping <= 59) penalty = 100; // "good"
			if (ping > 59 && ping <= 119) penalty = 150; // "okay"
			if (ping > 119 && ping <= 180) penalty = 200; // "bad"
			if (ping > 180) penalty = 250; // "horrible"
			return penalty;
		}
		Random rnd = new Random();
		PlayerBot[] GetMobsInLevel(Level lvl)
		{
			List<PlayerBot> players = new List<PlayerBot>();
			foreach (PlayerBot bot in lvl.Bots.Items)
			{
				if (bot.DisplayName != "" || !bot.name.Contains("ssmob"))
				{
					continue;
				}
				players.Add(bot);
			}
			return players.ToArray();
		}
		void CheckDespawn(Level level)
		{
			foreach (PlayerBot bot in level.Bots.Items)
			{
				if (bot.DisplayName != "" || !bot.name.Contains("ssmob"))
				{
					continue;
				}
				if (GetPlayersInLevel(level).Length < 1)
				{
					PlayerBot.Remove(bot);
					continue;
				}
				BlockID gb = level.FastGetBlock((ushort)bot.Pos.BlockX, (ushort)(bot.Pos.BlockY+1), (ushort)bot.Pos.BlockZ);
				if (gb == 9 || gb == 8 || gb == 10 || gb == 11) // water, lava etc
				{
					PlayerBot.Remove(bot);
					continue;
				}
				int shortestDist = 650;
				foreach (Player p in GetPlayersInLevel(level)) // Don't want creepers spawning inside us now do we
				{
					int x = bot.Pos.BlockX;
					int y = bot.Pos.BlockY;
					int z = bot.Pos.BlockZ;
					int dx = (int)(p.Pos.BlockX) - x, dy = (int)(p.Pos.BlockY) - y, dz = (int)(p.Pos.BlockZ) -z;
					int playerDist = Math.Abs(dx) /*+ Math.Abs(dy)*/ + Math.Abs(dz);
					if (playerDist < shortestDist)
					{
						shortestDist = playerDist;
					}
				}
				if (shortestDist >= 210)
				{
					PlayerBot.Remove(bot);
				}
			}
		}
		Player[] GetPlayersInLevel(Level lvl)
		{
			List<Player> players = new List<Player>();
			foreach (Player p in PlayerInfo.Online.Items)
			{
				if (p.level == lvl)
				{
					players.Add(p);
				}
			}
			return players.ToArray();
		}
		void HandleMobSpawning(SchedulerTask task)
		{
			mobSpawningTask = task;
			if (!Config.SpawnMobs)
			{
				return;
			}
			Level[] levels = LevelInfo.Loaded.Items;
			if (PlayerInfo.Online.Items.Length < 1)
			{
				foreach (Level lvl in levels)
				{
					if (!maplist.Contains(lvl.name))
					{
						continue;
					}
					CheckDespawn(lvl);
				}
				return;
			}
			
			
			
			
			
			foreach (Level lvl in levels)
			{
				if (!maplist.Contains(lvl.name))
				{
					continue;
				}
				CheckDespawn(lvl);
				Player[] players = GetPlayersInLevel(lvl);
				if (GetMobsInLevel(lvl).Length >= (Config.MaxMobs * players.Length))
				{
					continue;
				}
			

				Player selectedPlayer = players[rnd.Next(players.Length)] ;
				if (selectedPlayer == null)
				{
					continue;
				}

				ushort x = (ushort)(	selectedPlayer.Pos.BlockX + (	rnd.Next(25, 128) * ((rnd.Next(2) == 1 ? 1 : -1)))	);
				ushort z = (ushort)(	selectedPlayer.Pos.BlockZ + (	rnd.Next(25, 128) * ((rnd.Next(2) == 1 ? 1 : -1)))	);
				if (x >= lvl.Width)
				{
					x = (ushort)(lvl.Width-1);
				}
				if (z >= lvl.Length)
				{
					z = (ushort)(lvl.Length-1);
				}
				if (x < 0)
				{
					x = 0;
				}
				if (z < 0)
				{
					z = 0;
				}
				ushort y = FindGround(lvl, x, lvl.Height, z);
				if (y < 0)
				{
					y = 0;
				}
				if (y>1)
				{
					BlockID gb = lvl.FastGetBlock(x, (ushort)(y-1), z);
					if (gb == 9 || gb == 8 || gb == 10 || gb == 11) // water, lava etc
					{
						continue;
					}
				}
				switch (rnd.Next(12))
				{
					case 1:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					case 2:
						SpawnEntity(lvl, "pig", "roam", x, y, z);
						break;
					case 3:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
					case 4:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					case 5:
						SpawnEntity(lvl, "zombie", "hostile", x, y, z);
						break;
					case 6:
						SpawnEntity(lvl, "skeleton", "hostile", x, y, z);
						break;
					case 7:
						SpawnEntity(lvl, "spider", "hostile", x, y, z);
						break;
					case 8: // Creeper? aww man
						SpawnEntity(lvl, "creeper", "hostile", x, y, z); // So we back in the mine 
						break; // Swinging that pickaxe side to side, side side to side // HEADS UP TOTAL SHOCK FILLS YOUR BODY // OH NO ITS YOU AGAIN
					case 9:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
					case 10:
						SpawnEntity(lvl, "pig", "roam", x, y, z);
						break;
					case 11:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					default:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
				}
			}
		}
		static bool CanHitPlayer(Player p, Player victim)
        {
            Vec3F32 delta = p.Pos.ToVec3F32() - victim.Pos.ToVec3F32();
            float reachSq = 12f; // 3.46410161514 block reach distance

            int ping = p.Session.Ping.AveragePing();

            if (ping > 59 && ping <= 119) reachSq = 16f; // "okay"
            if (ping > 119 && ping <= 180) reachSq = 16f; // "bad"
            if (ping > 180) reachSq = 16f; // "horrible"

            // Don't allow clicking on players further away than their reach distance
            if (delta.LengthSquared > (reachSq + 1)) return false;

            // Check if they can kill players, determined by gamemode plugins
            //bool canKill = PvP.Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
            //if (!canKill) return false;

            if (p.Game.Referee || victim.Game.Referee || p.invincible || victim.invincible) return false; // Ref or invincible
            if (inSafeZone(p, p.level) || inSafeZone(victim, victim.level)) return false; // Either player is in a safezone

            if (!string.IsNullOrWhiteSpace(p.Extras.GetString("TEAM")) && (p.Extras.GetString("TEAM") == victim.Extras.GetString("TEAM")))
            {
                return false; // Players are on the same team
            }

            BlockID b = p.GetHeldBlock();

            if (Block.GetName(p, b).ToLower() == "bow") return false; // Bow damage comes from arrows, not player click

            // If all checks are complete, return true to allow knockback and damage
            return true;
        }
		static bool CanHitMob(Player p, PlayerBot victim)
        {
            Vec3F32 delta = p.Pos.ToVec3F32() - victim.Pos.ToVec3F32();
            float reachSq = 12f; // 3.46410161514 block reach distance

            int ping = p.Session.Ping.AveragePing();

            if (ping > 59 && ping <= 119) reachSq = 16f; // "okay"
            if (ping > 119 && ping <= 180) reachSq = 16f; // "bad"
            if (ping > 180) reachSq = 16f; // "horrible"

            // Don't allow clicking on players further away than their reach distance
            if (delta.LengthSquared > (reachSq + 1)) return false;

            // Check if they can kill players, determined by gamemode plugins
            //bool canKill = PvP.Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
            //if (!canKill) return false;

            //if (p.Game.Referee || p.invincible ) return false; // Ref or invincible
            //if (inSafeZone(p, p.level)) return false; // Either player is in a safezone

            /*if (!string.IsNullOrWhiteSpace(p.Extras.GetString("TEAM")) && (p.Extras.GetString("TEAM") == victim.Extras.GetString("TEAM")))
            {
                return false; // Players are on the same team
            }*/

            BlockID b = p.GetHeldBlock();

            if (Block.GetName(p, b).ToLower() == "bow") return false; // Bow damage comes from arrows, not player click

            // If all checks are complete, return true to allow knockback and damage
            return true;
        }
		Dictionary<PlayerBot, int> mobHealth = new Dictionary<PlayerBot, int>();

		void HandleAttackMob (Player p, byte entity)
		{
			if (!Config.CanKillMobs)
			{
				return;
			}
			PlayerBot mob = null;
			foreach( PlayerBot b in p.level.Bots.Items)
			{
				if (b.EntityID == entity)
				{
					mob = b;
					break;
				}
			}
			if (mob == null)
			{
				return;
			}
			if (!CanHitMob(p, mob))
			{
				return;
			}
			/*if (mob.Model == "Humanoid")
			{
				return;
			}*/
			if (!mobHealth.ContainsKey(mob))
			{
				mobHealth.Add(mob, 10);
			}
			mobHealth[mob] = mobHealth[mob] - 4;
			if (mobHealth[mob] <= 0)
			{
				PlayerBot.Remove(mob);
			}
		}
		void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (!maplist.Contains(p.level.name)) return;
			if (button != MouseButton.Left) return;
			//if (action != MouseAction.Released) return;
			Player victim = null; // If not null, the player that is being hit

			Player[] players = PlayerInfo.Online.Items;

			foreach (Player pl in players)
			{
				// Clicked on a player

				if (pl.EntityID == entity)
				{
					victim = pl;
					break;
				}
			}
			
			if (victim == null)
			{
				p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow.AddMilliseconds(550 - GetLagCompensation(p.Session.Ping.AveragePing()));
				try
				{
				HandleAttackMob(p, entity);
				}
				catch
				{
				}
				return;
			}
			if (!p.Extras.Contains("PVP_HIT_COOLDOWN"))
			{
				p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow;
			}
			DateTime lastClickTime = (DateTime)p.Extras.Get("PVP_HIT_COOLDOWN");

			if (lastClickTime > DateTime.UtcNow) return;
			
			if (!CanHitPlayer(p, victim)) return;
			DoHit(p, victim);
			p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow.AddMilliseconds(400 - GetLagCompensation(p.Session.Ping.AveragePing()));
		}
		void HandleRegeneration(SchedulerTask task)
        {
            regenTask = task;
            foreach (Player pl in PlayerInfo.Online.Items)
            {
				if (!maplist.Contains(pl.level.name)) continue;
                int health = GetHealth(pl);

                if (health >= Config.MaxHealth) continue; // No need to regenerate health if player is already at max health

                pl.Extras["SURVIVAL_HEALTH"] = health + 1;
            }
        }
		///////////////////////////////////////////////////////////////////////////
		// GUI
		///////////////////////////////////////////////////////////////////////////
		static string GetHealthBar(int health)
		{
			if (health < 0)
			{
				health = 0;
			}

			int repeatHealth = (int)Math.Round(( (float) health / (float)Config.MaxHealth) * 10f);

			int repeatDepletedHealth = 10 - repeatHealth;


			return ("%c" + new string('♥', repeatHealth )) + "%8" + new string('♥', repeatDepletedHealth ) ;
		}
		static string GetAirBar(int air)
		{
			if (air < 0)
			{
				air = 0;
			}
			if (air >= Config.MaxAir)
			{
				return "";
			}
			int repeat = air; //(int)Math.Round((double)(air/Config.MaxAir) * 10);
			return ("%9" + new string('♥', repeat)+ "%8" + new string('♥', Config.MaxAir-air ));
		}
		void SendPlayerGui(Player p)
		{
			if (!maplist.Contains(p.level.name)) return;
			p.SendCpeMessage(CpeMessageType.BottomRight1, GetHealthBar	(GetHealth	(p)));
			p.SendCpeMessage(CpeMessageType.BottomRight2, GetAirBar		(GetAir		(p)));
			
			//p.Message("%c" + GetHealth(p).ToString() + " " + ((GetHealth(p)/Config.MaxHealth) * 100).ToString() + " " +GetHealthBar(GetHealth(p)));
			///p.Message("%9" + GetAir(p).ToString()    + " " + ((GetAir(p)/Config.MaxAir) * 100).ToString() + " " + GetAirBar(GetAir(p)));
		}
		///////////////////////////////////////////////////////////////////////////
		// UTILITIES
		///////////////////////////////////////////////////////////////////////////
		int fallDamage(int height)
		{
			if (height < 4)
			{
				return 0;
			}
			return (height-4);
		}
		public void SetHealth(Player p, int health)
		{
			p.Extras["SURVIVAL_HEALTH"] = health;
		}
		public int GetHealth(Player p)
		{
			return p.Extras.GetInt("SURVIVAL_HEALTH");
		}
		public int GetAir(Player p)
		{
			return p.Extras.GetInt("SURVIVAL_AIR");
		}
		public void SetAir(Player p, int air)
		{
			p.Extras["SURVIVAL_AIR"] = air;
		}
		public bool IsBurning(Player p)
		{
			bool burning = false;
			try
			{
				BlockID b = p.level.FastGetBlock((ushort)p.Pos.BlockX,(ushort)p.Pos.BlockY, (ushort)p.Pos.BlockZ);
				BlockID b2 = p.level.FastGetBlock((ushort)p.Pos.BlockX,(ushort)(p.Pos.BlockY - 1), (ushort)p.Pos.BlockZ);

				burning = (/*p.level.Props[b].KillerBlock &&*/ (b == 10 || b == 11 || b == 54)) || (/*p.level.Props[b2].KillerBlock && */ (b2 == 10 || b2 == 11 || b2 == 54));
			}
			catch
			{
			}
			return burning;
		}
		bool IsDrowning(Player p)
		{
			ushort x = (ushort)(p.Pos.X / 32);
			ushort y = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32));
			ushort z = (ushort)(p.Pos.Z / 32);
			bool drowning = false;
			try
			{
				BlockID bHead = p.level.FastGetBlock((ushort)x, (ushort)(y+1), (ushort)z);

				drowning = (p.level.FastGetBlock((ushort)x, (ushort)y, (ushort)z) != 0) && p.level.Props[bHead].Drownable;
			}
			catch
			{
				drowning = false;
			}
			return drowning;
		}
		public void Damage(Player p, int amount, BlockID reason = 0)
		{
			SetHealth(p, GetHealth(p) - amount);
			if (GetHealth(p) <= 0)
			{
				// die
				Die(p, reason);
			}
			SendPlayerGui(p);
		}
		public void Die(Player p, BlockID reason = 4)
		{
			p.HandleDeath(reason, immediate: true);	
			InitPlayer(p);
		}
		public void InitPlayer(Player p)
		{
			p.SendCpeMessage(CpeMessageType.Status1, "");
			p.SendCpeMessage(CpeMessageType.Status2, "");
			p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
			p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
			p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow;
			p.Extras["FALLING"] = false;
			p.Extras["FALL_START"] = p.Pos.Y;
			SendPlayerGui(p);
		}
		public static void ResetPlayerState(Player p)
        {
			p.SendCpeMessage(CpeMessageType.Status1, "");
			p.SendCpeMessage(CpeMessageType.Status2, "");
            p.Extras["SURVIVAL_HEALTH"] = 10;
            p.Extras["SURVIVAL_AIR"] = 10;
        }
		void DoHit(Player p, Player victim)
		{
			PushPlayer(p, victim); // Knock the victim back
			int dmg = 1;
			if (GetHealth(victim)-dmg <= 0)
			{
				Die(victim, 4);
				string deathMessage = p.color +  p.name + " %ekilled " + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level || victim.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			Damage(victim, dmg, 4);
		}
		static void PushPlayer(Player p, Player victim)
        {
            if (p.level.Config.MOTD.ToLower().Contains("-damage")) return;

            int srcHeight = ModelInfo.CalcEyeHeight(p);
            int dstHeight = ModelInfo.CalcEyeHeight(victim);
            int dx = p.Pos.X - victim.Pos.X, dy = (p.Pos.Y + srcHeight) - (victim.Pos.Y + dstHeight), dz = p.Pos.Z - victim.Pos.Z;

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);

            float mult = 1 / ModelInfo.GetRawScale(victim.Model);
            float victimScale = ModelInfo.GetRawScale(victim.Model);

            if (victim.Supports(CpeExt.VelocityControl) && p.Supports(CpeExt.VelocityControl))
            {
                // Intensity of force is in part determined by model scale
                victim.Send(Packet.VelocityControl((-dir.X * mult) * 0.5f, 0.87f * mult, (-dir.Z * mult) * 0.5f, 0, 1, 0));

                // If GoodlyEffects is enabled, show particles whenever a player is hit
                if (Config.UseGoodlyEffects)
                {
                    // Spawn effect when victim is hit
                    //Command.Find("Effect").Use(victim, Config.HitParticle + " " + (victim.Pos.X / 32) + " " + (victim.Pos.Y / 32) + " " + (victim.Pos.Z / 32) + " 0 0 0 true");
                }
            }
            else
            {
                p.Message("You can left and right click on players to hit them if you update your client!");
            }
        }
		void AddAi( string ai, string[] args) {
			Player p = Player.Console;
			 if (File.Exists("bots/" + ai)) {
			File.Delete("bots/" + ai);
			 }
            if (!File.Exists("bots/" + ai)) {
                p.Message("Created new bot AI: &b" + ai);
                using (StreamWriter w = new StreamWriter("bots/" + ai)) {
                    // For backwards compatibility
                    w.WriteLine("#Version 2");
                }
            }

            string action = args.Length > 2 ? args[2] : "";
            string instruction = ScriptFile.Append(p, ai, action, args);
            if (instruction != null) {
                p.Message("Appended " + instruction + " instruction to bot AI &b" + ai);
            }
        }
		public void SpawnEntity(Level level, string model, string ai, ushort x, ushort y, ushort z)
		{
			int uniqueMobId = level.Bots.Items.Length+1;
			string uniqueName = "ssmob" + uniqueMobId;
			PlayerBot bot = new PlayerBot(uniqueName, level);
			bot.DisplayName = "";
			bot.Model = model;
			//+16 so that it's centered on the block instead of min corner
			Position pos = Position.FromFeet((int)(x*32) +16, (int)(y*32), (int)(z*32) +16);
			bot.SetInitialPos(pos);
			bot.AIName = ai;
			//HandleAdd(Player.Console,  uniqueName,  new string[] {"add", uniqueName, ai, "75"});
			 
			PlayerBot.Add(bot);
			ScriptFile.Parse(Player.Console, bot, ai);
			BotsFile.Save(level);
			
		}
		ushort FindGround(Level level,int x, int y, int z)
		{
			if (x > level.Width)
			{
				x = level.Width-1;
			}
			if (z > level.Length)
			{
				z = level.Length-1;
			}
			if (x < 0)
			{
				x = 0;
			}
			if (z < 0)
			{
				z = 0;
			}
			for (int i = level.Height-1; i >= 0; i--)
			{
				if (level.FastGetBlock((ushort)x, (ushort)i, (ushort)z) != 0)
				{
					return (ushort)(i + 1);
				}
			}
			return (ushort)y;
			
		}
		///////////////////////////////////////////////////////////////////////////
		public static bool inSafeZone(Player p, Level level)
        {
			Zone[] zones = level.Zones.Items;
			foreach(Zone zone in zones)
			{
				if (zone.Contains(p.Pos.BlockX, p.Pos.BlockY, p.Pos.BlockZ))
				{
					return true;
				}
			}
			return false;
            /*if (File.Exists(Config.Path + "safezones" + map + ".txt"))
            {
                using (var r = new StreamReader(Config.Path + "safezones" + map + ".txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] temp = line.Split(';');
                        string[] coord1 = temp[0].Split(',');
                        string[] coord2 = temp[1].Split(',');

                        if ((p.Pos.BlockX <= int.Parse(coord1[0]) && p.Pos.BlockX >= int.Parse(coord2[0])) || (p.Pos.BlockX >= int.Parse(coord1[0]) && p.Pos.BlockX <= int.Parse(coord2[0])))
                        {
                            if ((p.Pos.BlockZ <= int.Parse(coord1[2]) && p.Pos.BlockZ >= int.Parse(coord2[2])) || (p.Pos.BlockZ >= int.Parse(coord1[2]) && p.Pos.BlockZ <= int.Parse(coord2[2])))
                            {
                                if ((p.Pos.BlockY <= int.Parse(coord1[1]) && p.Pos.BlockY >= int.Parse(coord2[1])) || (p.Pos.BlockY >= int.Parse(coord1[1]) && p.Pos.BlockY <= int.Parse(coord2[1])))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            return false;*/
        }
	}
	public sealed class CmdPvP : Command2
    {
        public override string name { get { return "PvP"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage PvP") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args, data);
                    return;
                case "del":
                    HandleDelete(p, args, data);
                    return;
            }
        }
		void Save()
		{
			 using (StreamWriter maplistwriter =
                new StreamWriter(SimpleSurvival.Config.Path + "maps.txt"))
            {
                foreach (String s in SimpleSurvival.maplist)
                {
                    maplistwriter.WriteLine(s);
                }
            }
		}
        void HandleAdd(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to add.");
                return;
            }

            if (!HasExtraPerm(p, data.Rank, 1)) { p.Message("%cNo permission."); return; };

            string pvpMap = args[1];

            SimpleSurvival.maplist.Add(pvpMap);
			Save();
            p.Message("The map %b" + pvpMap + " %Shas been added to the PvP map list.");

            // Add the map to the map list
           

            Player[] players = PlayerInfo.Online.Items;

            foreach (Player pl in players)
            {
                if (pl.level.name.ToLower() == args[1].ToLower())
                {
                    SimpleSurvival.ResetPlayerState(pl);
                }
            }
        }

        void HandleDelete(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to remove.");
                return;
            }

            if (!HasExtraPerm(p, data.Rank, 1)) return;

            string pvpMap = args[1];

            SimpleSurvival.maplist.Remove(pvpMap);
			Save();
			Player[] players = PlayerInfo.Online.Items;
			foreach (Player pl in players)
            {
                if (pl.level.name.ToLower() == pvpMap)
                {
                    SimpleSurvival.ResetPlayerState(pl);
                }
            }
            p.Message("The map %b" + pvpMap + " %Shas been removed from the PvP map list.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/PvP add <map> %H- Adds a map to the PvP map list.");
            p.Message("%T/PvP del <map> %H- Deletes a map from the PvP map list.");
        }
    }
}