using System;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using MCGalaxy.Maths;
using MCGalaxy.Blocks;
using MCGalaxy.Network;
using MCGalaxy.Bots;
using System.Collections.Generic;
using BlockID = System.UInt16;
namespace MCGalaxy {
	public class Corpse : Plugin {
		public override string name { get { return "Corpse"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "Venk's Private Server"; } }
		public override bool LoadAtStartup { get { return true; } }
		public Dictionary<Player, PlayerBot> corpses = new Dictionary<Player, PlayerBot>();
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnPlayerDyingEvent.Register(HandlePlayerDying, Priority.High);
			OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.Low);
			corpses.Clear();
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnPlayerDyingEvent.Unregister(HandlePlayerDying);
			OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
			foreach( var pair in corpses)
			{
				if (pair.Value == null)
				{
					continue;
				}
				PlayerBot.Remove(pair.Value);
			}
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
		void HandlePlayerDying(Player p, BlockID deathblock, ref bool cancel)
        {
			if (corpses.ContainsKey(p))
			{
				if (corpses[p] != null)
				{
					PlayerBot.Remove(corpses[p]);
				}
				corpses.Remove(p);
				
			}
				
			SpawnCorpse(p);
        }
		void HandlePlayerDisconnect(Player p, string reason)
        {
			if (corpses.ContainsKey(p))
			{
				if (corpses[p] != null)
				{
					PlayerBot.Remove(corpses[p]);
				}
				corpses.Remove(p);
				
			}
        }
		int FindGround(Level level,int x, int y, int z)
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
			/*x = x;
			y = y;
			z = z;*/
			for (int i = y; i >= 0; i--)
			{
				if (level.FastGetBlock((ushort)x, (ushort)i, (ushort)z) != 0)
				{
					return (int)((i + 1));
				}
			}
			return y;
			
		}
		void SetCorpsePos(Player p, PlayerBot bot)
		{
			Orientation rot = p.Rot;
			rot.RotX = (byte)(190);
			//rot.RotY = (byte)(90);
			
			int y = p.Pos.Y;
			try {
				y = FindGround(p.level, p.Pos.X/32, (p.Pos.Y/32), p.Pos.Z/32) * 32;
				y += (32*2) - 5;
				//Player.Console.Message(y.ToString());
			}
			catch( Exception e)
			{
				y = p.Pos.Y;
				//Player.Console.Message(e.ToString());
			}
			//bot.Pos = new Position(p.Pos.X, y, p.Pos.Y);
			
			bot.SetInitialPos(new Position(p.Pos.X, y, p.Pos.Z));
			bot.Rot = rot;
		}
		public void SpawnCorpse(Player p)//, ushort x, ushort y, ushort z)
		{
			string uniqueName = p.name + "_corpse";
			PlayerBot bot = new PlayerBot(uniqueName, p.level);
			bot.DisplayName = "";
			bot.SkinName = p.SkinName;
			bot.Model = "corpse";
			//bot.skin = p.skin;

			SetCorpsePos(p, bot);
			
			PlayerBot.Add(bot);
			corpses.Add(p, bot);
		}
	}
}