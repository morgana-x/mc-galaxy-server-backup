//This is an example plugin source!
using System;
using System.Collections.Generic;
using System.IO;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
namespace MCGalaxy {
	public class NoSwear : Plugin {
		public override string name { get { return "noswear"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }

        private static string badwordlistfile = "./plugins/swears.txt";
		
		private static string[] badwords = new string[]{};
        
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnChatEvent.Register(HandleChatEvent, Priority.Low);

            /*string[] obfuscatedBadwords = badwords;
            for (int i=0; i<obfuscatedBadwords.Length; i++)
            {
                obfuscatedBadwords[i] = obfuscateString(obfuscatedBadwords[i]);
            }
            File.WriteAllText(badwordlistfile, string.Join("\n",obfuscatedBadwords));*/
            if (!File.Exists(badwordlistfile))
                return;
            badwords = File.ReadAllText(badwordlistfile).Split('\n');
            for (int i=0; i<badwords.Length; i++)
            {
                badwords[i] = deobfuscateString(badwords[i]);
            }
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnChatEvent.Unregister(HandleChatEvent);
			
		}
	
		public override void Help(Player p) {
			//HELP INFO!
		}

		void HandleChatEvent(ChatScope scope, Player source, ref string msg, object arg, ref ChatMessageFilter filter, bool relay=false)
        {
            for (int i=0; i < badwords.Length; i++)
            {
                if (msg.Replace(" ","").Replace("   ","").Replace("-","").Replace(".","").CaselessContains(badwords[i]))
                {
                    msg = source.DisplayName + " tried to say a bad word!";
                    return;
                }
            }
        }
        static string obfuscateString(string inp)
        {
            string newString = "";
            for (int i=0; i <inp.Length;i++)
            {
                newString = newString + (char)((byte)(inp[i] + 1));
            }
            return newString;
        }
        static string deobfuscateString(string inp)
        {
            string newString = "";
            for (int i=0; i <inp.Length;i++)
            {
                newString = newString + (char)((byte)(inp[i] - 1));
            }
            return newString;
        }
	}
}