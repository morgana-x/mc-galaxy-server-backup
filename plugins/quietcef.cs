//This is an example plugin source!
using System;
using System.Collections.Generic;
using System.IO;
using BlockID = System.UInt16;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Events;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using MCGalaxy.Maths;
using MCGalaxy.Blocks;
using MCGalaxy.Network;
using MCGalaxy.Bots;
namespace MCGalaxy {
	public class SpawnEgg : Plugin {
		public override string name { get { return "quietcef"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		
		

		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnChatEvent.Register(HandleChatEvent, Priority.Low);
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnChatEvent.Unregister(HandleChatEvent);
			
		}
	
		public override void Help(Player p) {
			//HELP INFO!
		}
		public bool filterCef(Player pl, object arg)
        {
            if(!pl.Session.ClientName().CaselessContains("cef"))
                return false;
            return true;
        }
		void HandleChatEvent(ChatScope scope, Player source, ref string msg, object arg, ref ChatMessageFilter filter, bool relay=false)
        {
			if (msg.CaselessContains("cef ") && !msg.CaselessContains(" cef") && source.Session.ClientName().CaselessContains("cef") )
            {
                filter = filterCef;
            }
        }
	}
}