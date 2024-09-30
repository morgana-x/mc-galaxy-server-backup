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
		public override string name { get { return "SurvivalCrafting"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		public Dictionary<Player, Dictionary<int,int>> playerInventories = new Dictionary<Player, Dictionary<int,int>>();
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnPlayerDyingEvent.Register(HandlePlayerDying, Priority.High);
			OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.Low);
			playerInventories.Clear();
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnPlayerDyingEvent.Unregister(HandlePlayerDying);
			OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
			playerInventories.Clear();
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
		void HandlePlayerDying(Player p, BlockID deathblock, ref bool cancel)
        {
			if (playerInventories.ContainsKey(p))
			{
				if (playerInventories[p] != null)
				{
					playerInventories[p].Clear()
				}
			}
        }
		void HandlePlayerDisconnect(Player p, string reason)
        {
			if (playerInventories.ContainsKey(p))
			{
				playerInventories.Remove(p);
			}
        }

}