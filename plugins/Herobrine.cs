using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Bots;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Events;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Commands;
using System;
using MCGalaxy.Maths;
using BlockRaw = System.Byte;
namespace MCGalaxy
{
    public class Herobrine : Plugin
    {
        public override string name { get { return "Herobrine"; } }
        public override string MCGalaxy_Version { get { return "1.9.4.1"; } }
        public override string creator { get { return "Morgana"; } }

		//public bool LoadAtStartup = true;
		public bool HerobrineSpawned = false;
		public static SchedulerTask Task;
		
		public bool AllowGrief = true;
		
		public Level HerobrineLevel;
		
		bool placedCross = false;
		
		public int CurrentHerobrineTask = 0; /*
			0 = Nothing
			1 = Stalk
			2 = anything (anything that uses this is responsible for turning it off)
		*/
		public override void Load(bool auto)
		{   
			//Player[] players = GetPlayersInLevel();
			HerobrineSpawned = false;
			placedCross = false;
			CurrentHerobrineTask = 0;
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.Low);
			Server.MainScheduler.QueueRepeat(DoHerobrineTick, null, TimeSpan.FromSeconds(1));
			int eventDelay = 40;
			Server.MainScheduler.QueueRepeat(DoHerobrineEvent, null, TimeSpan.FromSeconds(eventDelay));
			UpdateEnvAll();
		}
		public override void Unload(bool auto)
		{
			HerobrineSpawned = false;
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
			Server.MainScheduler.Cancel(Task);
			placedCross = false;
			CurrentHerobrineTask = 0;
			SetFog(0);
			DestroyHerobrine();
		}
		PlayerBot heroEntity;
		int stalkTimeLeft = 0;
		int stalkTime = 125;
		int stalkDisappearDistance = 600;
		ushort SpookyFog = 65;
		public void DestroyHerobrine()
		{
			if (heroEntity == null)
			{
				return;
			}
			/*Player[] players = PlayerInfo.Online.Items;
			foreach (Player p in players)
			{
				Entities.Despawn(p, heroEntity);
			}*/
			PlayerBot.Remove(heroEntity);
			heroEntity = null;
		}
		public Player[] GetPlayersInLevel()
		{
			Player[] players = PlayerInfo.Online.Items;
			int numOfPlayers = 0;
			for (int i=0; i < players.Length; i++)
			{
				var p = players[i];
				if (p.level == HerobrineLevel)
				{
					numOfPlayers++;
                    //foundPlayers[i] = p;
				}
			}
            Player[] foundPlayers = new Player[numOfPlayers];
            for (int i = 0; i < players.Length; i++)
            {
                var p = players[i];
                if (p.level == HerobrineLevel)
                {
                    //numOfPlayers++;
                    foundPlayers[numOfPlayers-1] = p;
					numOfPlayers--;
                }
            }
            return foundPlayers;
		}
		public void SpawnHerobrine(Level level, ushort x, ushort y, ushort z)
		{
			if (heroEntity != null)
			{
				return;
			}
			PlayerBot bot = new PlayerBot("Herobrine", level);
			bot.DisplayName = "";
			string skin = "herobrine";
			bot.SkinName = skin;
			bot.AIName = "stare";
			bot.id = 69;
			
			//+16 so that it's centered on the block instead of min corner
			Position pos = Position.FromFeet((int)(x*32) +16, (int)(y*32), (int)(z*32) +16);
			bot.SetInitialPos(pos);
			
			int yaw = 90;
			int pitch = 0;
			byte byteYaw = Orientation.DegreesToPacked(yaw);
			byte bytePitch = Orientation.DegreesToPacked(pitch);
			bot.SetYawPitch(byteYaw, bytePitch);
			
			heroEntity = bot;
			PlayerBot.Add(bot);
			/*
			Player[] players = PlayerInfo.Online.Items;
			foreach (Player p in players)
			{
				Entities.Spawn(p, bot);
			}*/
			
		}
		void DoStalk()
		{
			if (heroEntity == null)
			{
				CurrentHerobrineTask = 0;
				return;
			}
			if (stalkTimeLeft <= 0)
			{
				CurrentHerobrineTask = 0;
				DestroyHerobrine();
				return;
			}
			stalkTimeLeft--;
			Player[] players = GetPlayersInLevel();
			int shortestDist = 2000000;
			Player selPlayer = null;
			foreach (Player p in players)
			{
				//Player closest = p;
				PlayerBot bot = heroEntity;
				int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
				int playerDist = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
				if (playerDist < shortestDist)
				{
					shortestDist = playerDist;
					selPlayer = p;
				}
				if (playerDist < stalkDisappearDistance) // if closer than this, disappear!
				{
					CurrentHerobrineTask = 0;
					DestroyHerobrine();
					return;
				}
			}
			if (selPlayer != null)
			{
				LookAtPlayer(selPlayer);
			}
		}
		void DoHerobrineTick(SchedulerTask task)
		{
			if (!HerobrineSpawned)
			{
				return;
			}
			Player[] playerlist = PlayerInfo.Online.Items;
			if (playerlist.Length < 1)
			{
				CurrentHerobrineTask = 0;
				DestroyHerobrine();
				return;
			}
			if (CurrentHerobrineTask == 1)
			{
				DoStalk();
			}
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
		void LookAtPlayer(Player p)
		{
			if (heroEntity == null)
			{
				return;
			}
			PlayerBot bot = heroEntity;
			//p.Message("looking at you");
			int dstHeight = ModelInfo.CalcEyeHeight(bot);

			int dx = (p.Pos.X) - bot.Pos.X, dy = bot.Rot.RotY, dz = (p.Pos.Z) - bot.Pos.Z;
			Vec3F32 dir = new Vec3F32(dx, dy, dz);
			dir = Vec3F32.Normalise(dir);

			Orientation rot = bot.Rot;
			DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
			byte yaw = rot.RotY; byte pitch = rot.HeadX;
			bot.SetYawPitch(yaw, pitch);
			bot.Rot = rot;
		}
		void InitStalk()
		{
			Player[] players = GetPlayersInLevel();
			if (players.Length < 1)
			{
				CurrentHerobrineTask = 0;
				return;
			}
			Random rnd = new Random();
			Player selectedPlayer = players[rnd.Next(0, players.Length)];
			if (selectedPlayer == null)
			{
				return;
			}
			DestroyHerobrine();
			int rndX = rnd.Next(650, 1500);
			int rndZ = rnd.Next(650, 1500);
			if (rnd.Next(0,10) >= 5)
			{
				rndX = rndX * -1;
			}
			if (rnd.Next(0,10) >= 5)
			{
				rndZ = rndZ * -1;
			}
			int x = (selectedPlayer.Pos.X + rndX) / 32;
			int z = (selectedPlayer.Pos.Z + rndZ) / 32;
			if (x > selectedPlayer.level.Width)
			{
				x = selectedPlayer.level.Width;
			}
			if (z > selectedPlayer.level.Length)
			{
				z = selectedPlayer.level.Length;
			}
			if (x < 0)
			{
				x = 0;  
			}
			if (z < 0)
			{
				z = 0;
			}
			
			SpawnHerobrine(selectedPlayer.level, 
			(ushort)x,
			(ushort)FindGround(selectedPlayer.level,x/32 -16,selectedPlayer.Pos.Y/32,z/32 - 16),
			(ushort)z);
			LookAtPlayer(selectedPlayer);
			stalkTimeLeft = stalkTime;
		}
		void DoHerobrineEvent(SchedulerTask task)
		{
			if (!HerobrineSpawned)
			{
				return;
			}
			Player[] players = GetPlayersInLevel();
			if (players.Length < 1)
			{
				CurrentHerobrineTask = 0;
				return;
			}
			if (CurrentHerobrineTask != 0)
			{
				return;
			}
			

			Random rnd = new Random();
				
			int choice = rnd.Next(0, 5);
			
			if (choice == 1) // Stalk
			{
				stalkTimeLeft = stalkTime;
				InitStalk();
				CurrentHerobrineTask = 1;
				return;
			}
		
			if (choice == 2) // storm / clouds
			{
				//CurrentHerobrineTask = 2;
				//SetSky(150,150,170);
				//SetCloud(100,100,100);
				SetFog((ushort)(SpookyFog/1.5));
				Server.MainScheduler.QueueOnce( (SchedulerTask task2) => {
				//SetSky(112, 160, 237);
				//SetCloud(255,255,255);
				SetFog((ushort)(SpookyFog/2));
				//CurrentHerobrineTask = 0;
				}, null, TimeSpan.FromSeconds(1));
				Server.MainScheduler.QueueOnce( (SchedulerTask task2) => {
				//SetSky(150,150,170);
				//SetCloud(100,100,100);
				SetFog((ushort)(SpookyFog/1.5));
				//CurrentHerobrineTask = 0;
				}, null, TimeSpan.FromSeconds(2));
				Server.MainScheduler.QueueOnce( (SchedulerTask task2) => {
				//SetSky(112, 160, 237);
				//SetCloud(255,255,255);
				SetFog(SpookyFog);
				//CurrentHerobrineTask = 0;
				}, null, TimeSpan.FromSeconds(3));
			}
			if (choice == 3 && AllowGrief && !placedCross) // cross in ground
			{
				placedCross = true;
				CurrentHerobrineTask = 2;
				Level level = players[rnd.Next(0, players.Length)].level;
				int x = rnd.Next(0, level.Width);
				int z = rnd.Next(0, level.Length);
				int y = (int)FindGround(level, x, 20, z);
				//  cross in ground
				y = y-1;
				level.UpdateBlock(players[rnd.Next(0, players.Length)], (ushort)x,   (ushort)y, (ushort)z, 0);
				level.UpdateBlock(players[rnd.Next(0, players.Length)], (ushort)(x-1), (ushort)(y), (ushort)(z), 0);
				//level.UpdateBlock(players[rnd.Next(0, players.Length)], (ushort)(x-1), (ushort)(y+1), (ushort)(z), 4);
				level.UpdateBlock(players[rnd.Next(0, players.Length)], (ushort)(x-1), (ushort)(y), (ushort)(z+1), 0);
				level.UpdateBlock(players[rnd.Next(0, players.Length)], (ushort)(x-1), (ushort)(y), (ushort)(z-1), 0);
				level.UpdateBlock(players[rnd.Next(0, players.Length)], (ushort)(x-2), (ushort)(y), (ushort)(z), 0);
				level.UpdateBlock(players[rnd.Next(0, players.Length)], (ushort)(x-3), (ushort)(y), (ushort)(z), 0);
				Server.MainScheduler.QueueOnce( (SchedulerTask task2) => {
				CurrentHerobrineTask = 0;
				}, null, TimeSpan.FromSeconds(20));
				return;
			}
			if (choice == 4) // creepy sound
			{
				//RandomSound();
			}
			
		}
		void RandomSound()
		{
			Player[] players = GetPlayersInLevel();
			if (players.Length < 1)
			{
				return;
			}
			Random rnd = new Random();
			Player pl = players[rnd.Next(0, players.Length)];
			pl.Message("TEST");
			Level level = pl.level;
			ushort x = (ushort)((pl.Pos.X /32));
			ushort y = (ushort)((pl.Pos.Y /32));
			ushort z = (ushort)((pl.Pos.Z /32));
			var b = level.FastGetBlock(x, (ushort)(y-2), z);
			//level.UpdateBlock(pl, x, (ushort)(y-2), z, 20);
			level.SetTile(x, (ushort)(y-2), z, (BlockRaw)20);
			Server.MainScheduler.QueueOnce( (SchedulerTask task2) => {
				//level.UpdateBlock(pl, x, (ushort)(y-2), z, 0);
				level.SetTile(x, (ushort)(y-2), z, (BlockRaw)0);
			}, null, TimeSpan.FromMilliseconds(100));
			
			Server.MainScheduler.QueueOnce( (SchedulerTask task2) => {
				level.SetTile(x, (ushort)(y-2), z, (BlockRaw)b);
				//level.UpdateBlock(pl, x, (ushort)(y-2),z, b);
			}, null, TimeSpan.FromMilliseconds(150));
		}
		//ColorDesc sky = new ColorDesc((byte)112, (byte)160, (byte)237);
	
		//ColorDesc cloud = new ColorDesc((byte)255,(byte)255,(byte)255);

		ColorDesc fog = new ColorDesc((byte)255,(byte)255,(byte)255);
		void SetFog(ushort dist)
		{
			
			HerobrineLevel.Config.ExpFog = dist;
			fogDistance = dist;
			UpdateEnvAll();
		}
		/*void SetSky(byte r, byte g, byte b)
		{
			sky.R = r;
			sky.G = g;
			sky.B = b;
			UpdateEnvAll();
		}*/
		/*void SetCloud(byte r, byte g, byte b)
		{
			cloud.R = r;
			cloud.G = g;
			cloud.B = b;
			UpdateEnvAll();
		}*/
		ushort fogDistance = 0;
		void UpdateEnv(Player pl)
		{
			//pl.Send(Packet.EnvColor(0, sky.R, sky.G, sky.B));
			//pl.Send(Packet.EnvColor(1, cloud.R, cloud.G, cloud.B));
			//pl.Send(Packet.EnvColor(2, fog.R, fog.G, fog.B));
			// 96
			if (pl.Level == HerobrineLevel)
			{
				pl.Send(Packet.EnvMapProperty( MCGalaxy.EnvProp.MaxFog, fogDistance));
			}
			else 
			{
				
			}
		}
		void UpdateEnvAll()
		{
			Player[] players = GetPlayersInLevel();
			foreach (Player pl in players)
            {
				UpdateEnv(pl);
			}
	
		}
		void HandlePlayerConnect(Player p)
        {
            UpdateEnv(p);
        }
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			TrySummonHerobrine(p, x, y, z, block, placing);
        }
		
		void TrySummonHerobrine(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing)
		{
			if (HerobrineSpawned)
			{
				return;
			}
			if (!placing)
			{
				return;
			}
			//RandomSound();
			if (block != 54)
			{
				return;
			}


			if (p.level.GetBlock(x, (ushort)(y-1), z) != 62) // If magma underneath fire
			{
				return;
			}
			if (p.level.GetBlock(x, (ushort)(y-2), z) != 48) // If mossy block underneath fire
			{
				return;
			}
			if (p.level.GetBlock((ushort)(x-1), (ushort)(y-2), z) != 41) // If gold underneath fire
			{
				return;
			}
			if (p.level.GetBlock((ushort)(x+1), (ushort)(y-2), z) != 41) // If gold underneath fire
			{
				return;
			}
			if (p.level.GetBlock((ushort)(x-1), (ushort)(y-2), (ushort)(z-1)) != 41) // If gold underneath fire
			{
				return;
			}
			if (p.level.GetBlock((ushort)(x+1), (ushort)(y-2), (ushort)(z-1)) != 41) // If gold underneath fire
			{
				return;
			}
			if (p.level.GetBlock((ushort)(x-1), (ushort)(y-2), (ushort)(z+1)) != 41) // If gold underneath fire
			{
				return;
			}
			if (p.level.GetBlock((ushort)(x+1), (ushort)(y-2), (ushort)(z+1)) != 41) // If gold underneath fire
			{
				return;
			}
			if (p.level.GetBlock(x, (ushort)(y-2), (ushort)(z+1)) != 41) // If gold underneath fire
			{
				return;
			}
			if (p.level.GetBlock(x, (ushort)(y-2), (ushort)(z-1)) != 41) // If gold underneath fire
			{
				return;
			}
    
			/*if (p.level.Bots.Count >= Server.Config.MaxBotsPerLevel)
            {
                p.Message("Reached maximum number of bots allowed on this map.");
                return;
            }*/
			HerobrineSpawned = true;
			SpawnHerobrine(p.level, x,y,z);
			HerobrineLevel = p.level;
			LookAtPlayer(p);
			//SetSky(150,150,170);
			//SetCloud(100,100,100);
			SetFog(16);
			Server.MainScheduler.QueueOnce( (SchedulerTask task) => {
				//SetSky(112, 160, 237);
				//SetCloud(255,255,255);
				SetFog(32);
				}, null, TimeSpan.FromSeconds(1));
			Server.MainScheduler.QueueOnce( (SchedulerTask task) => {
				//SetSky(150,150,170);
				//SetCloud(100,100,100);
				SetFog(64);
				}, null, TimeSpan.FromSeconds(2));
			Server.MainScheduler.QueueOnce( (SchedulerTask task) => {
				DestroyHerobrine();
				//SetSky(112, 160, 237);
				//SetCloud(255,255,255);
				SetFog(SpookyFog);
				}, null, TimeSpan.FromSeconds(3));
			
            //
            //bot.Owner = p.truename;
		}
		
	}
	
}