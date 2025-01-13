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
	public class DiscordInvite : Plugin {
		public override string name { get { return "discordinvite"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		
		

		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
              Server.MainScheduler.QueueRepeat(DiscordMessageTick, null, TimeSpan.FromSeconds(2400));
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!

			   Server.MainScheduler.Cancel(messageTask);
		}
	
		public override void Help(Player p) {
			//HELP INFO!
		}
   
        SchedulerTask messageTask;
        void DiscordMessageTick(SchedulerTask task)
        {
            messageTask = task;
            foreach (var p in PlayerInfo.Online.Items)
            {
                p.Message("%eJoin the %9discord%e! %9https://discord.gg/rDpqCqPFkh");
            }
        }

	}
}