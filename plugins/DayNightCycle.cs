using System;
using MCGalaxy.Commands;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Maths;
using System.Collections.Generic;
namespace MCGalaxy
{
    public class DayNightCycle : Plugin
    {
        public override string name { get { return "DayNightCycle"; } }
        public override string MCGalaxy_Version { get { return "1.9.3.0"; } }
        public override string creator { get { return "Venk"; } }

        public static int timeOfDay = 0;
        public static SchedulerTask Task;

        public override void Load(bool startup)
        {
            Command.Register(new CmdSetTime());
			Command.Register(new CmdGetTime());
            Server.MainScheduler.QueueRepeat(DoDayNightCycle, null, TimeSpan.FromMilliseconds(100));
        }
		
        public override void Unload(bool shutdown)
        {
            Command.Unregister(Command.Find("SetTime"));
			Command.Unregister(Command.Find("GetTime"));
            Server.MainScheduler.Cancel(Task);
        }
		public static Dictionary<int, string> SkyColors = new Dictionary<int, string>()
        {
			{0,   "#709DED"},
			{170, "#78A9FF"},
			{175, "#212E46"},
			{179, "#111111"},
			{180, "#000000"},
			{190, "#000000"},
			{200, "#000000"},
			{210, "#000000"},
			{220, "#000000"},
			{230, "#304365"},
			{235, "#3C547F"},
			{236, "#6F9CEC"},
		};
		public static Dictionary<int, string> CloudColors = new Dictionary<int, string>()
        {
			{0,   "#FFFFFF"},
			{170, "#FFFFFF"},
			{175, "#926864"},
			{180, "#000000"},
			{190, "#000000"},
			{200, "#000000"},
			{210, "#000000"},
			{220, "#000000"},
			{230, "#D15F36"},
		};
		public static Dictionary<int, string> LightingColors = new Dictionary<int, string>()
        {
			{0,   "#FFFFFF"},
			{177, "#5c5c5c"},
			{180, "#202020"},
			{190, "#202020"},
			{200, "#202020"},
			{210, "#202020"},
			{220, "#202020"},
			{225, "#5c5c5c"},
			{230, "#fac3af"},
		};
		public static Dictionary<int, string> ShadowColors = new Dictionary<int, string>()
        {
			{0,   "#9B9B9B"},
			{177, "#101010"},
			{180, "#090909"},
			{190, "#090909"},
			{200, "#090909"},
			{210, "#090909"},
			{220, "#090909"},
			{225, "#101010"},
			{230, "#757575"},
		};
		public static Dictionary<int, string> FogColors = new Dictionary<int, string>()
        {
			{0,   "#FFFFFF"},
			{170, "#ffc0ab"},
			{175, "#3b3b3b"},
			{180, "#0f0f0f"},
			{190, "#0f0f0f"},
			{200, "#0f0f0f"},
			{210, "#0f0f0f"},
			{220, "#0f0f0f"},
			{230, "#D15F36"},
		};
		ColorDesc GetColor(Dictionary<int, string> list, float hour)
		{
			int dist = 10;
			int ColorKey = 0;
			foreach (var pair in list)
			{
				if (hour < pair.Key)
					continue;
				int d = (int)Math.Abs(hour - pair.Key);
				if (d > dist)
					continue;
				dist = d;
				ColorKey = pair.Key;
			}
			ColorDesc oldColor = default(ColorDesc);
			CommandParser.GetHex(Player.Console,list[ColorKey] ,ref oldColor);
			return oldColor;
		}
		ColorDesc GetSkyColor(float hour)
		{
			return GetColor(SkyColors, hour);
		}
		ColorDesc GetCloudColor(float hour)
		{
			return GetColor(CloudColors, hour);
		}
		ColorDesc GetFogColor(float hour)
		{
			return GetColor(FogColors, hour);
		}
		ColorDesc GetSunColor(float hour)
		{
			return GetColor(LightingColors, hour);
		}
		ColorDesc GetShadowColor(float hour)
		{
			return GetColor(ShadowColors, hour);
		}
		float Lerp(float firstFloat, float secondFloat, float by)
		{
			 return firstFloat * (1 - by) + secondFloat * by;
		}
		byte LerpColourChannel(int col1, int col2, float t)
		{
			if (col1 == col2)
			{
				return (byte)col1;
			}
			int result = (int)Lerp((float)col1, (float)col2, t);//(int)((col2 -col1) * t + col1);
			if (result < 0)
			{
				result = 0;
			}
			if (result > 256)
			{
				result = 256;
			}
			return (byte)result;
		}
		ColorDesc LerpColor(ColorDesc col1, ColorDesc col2, float t)
		{
			return new ColorDesc(
			LerpColourChannel(col1.R,col2.R,t),
			LerpColourChannel(col1.G,col2.G,t),
			LerpColourChannel(col1.B,col2.B,t)
			);	
		}

        void DoDayNightCycle(SchedulerTask task)
        {
            if (timeOfDay > 23999) timeOfDay = 0;
            else timeOfDay += 5;

            Player[] players = PlayerInfo.Online.Items;

            foreach (Player pl in players)
            {
                if (!pl.level.Config.MOTD.Contains("daynightcycle=true")) continue;
				
				float hour = (timeOfDay / 100) ;
				
                ColorDesc sky    = GetSkyColor   (hour);
                ColorDesc cloud  = GetCloudColor (hour); 
                ColorDesc fog    = GetFogColor   (hour);
				ColorDesc sun    = GetSunColor   (hour);
				ColorDesc shadow = GetShadowColor(hour);
				
                pl.Send(Packet.EnvColor(0, sky.R, sky.G, sky.B));
				pl.Send(Packet.EnvColor(1, cloud.R, cloud.G, cloud.B));
				pl.Send(Packet.EnvColor(2, fog.R, fog.G, fog.B));
				pl.Send(Packet.EnvColor(3, shadow.R, shadow.G, shadow.B));
				pl.Send(Packet.EnvColor(4, sun.R, sun.G, sun.B));
            }

            Task = task;
        }
    }

    public sealed class CmdSetTime : Command2
    {
        public override string name { get { return "SetTime"; } }
        public override string shortcut { get { return "timeset"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0) { Help(p); return; }
            string[] args = message.SplitSpaces();

            DayNightCycle.timeOfDay = int.Parse(args[0]);
            p.Message("%STime set to: %b" + DayNightCycle.timeOfDay + "%S.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/SetTime [tick] - %HSets the day-night cycle time to [tick].");
        }
    }
	public sealed class CmdGetTime : Command2
    {
        public override string name { get { return "GetTime"; } }
        public override string shortcut { get { return "timeget"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data)
        {
            p.Message("%STime is: %b" + DayNightCycle.timeOfDay + "%S.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/GetTime - Gets the current time.");
        }
    }
}
