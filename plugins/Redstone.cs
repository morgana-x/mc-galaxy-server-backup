//using System;
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
using System.Collections.Generic;
using BlockRaw = System.Byte;
//pluginref door.dll
namespace MCGalaxy {
	
	
	public class PowerGenerator {
		
	}
	public class RedstoneObject {
		public ushort BlockID;
		public ushort Power;
		public RedstoneObject(ushort blockID, ushort power)
		{
			BlockID = blockID;
			Power = power;
		}
		public void UpdateBlock(Level level, int location)
		{
			var obj = this;
			if ((!Redstone.IsRedstoneDust(obj.BlockID)))
			{
				return;
			}
			//Player.Console.Message("UPDATING BLOCK TURNING ON");
			ushort x;
			ushort y;
			ushort z;
			level.IntToPos(location, out x, out y, out z);
			//Player.Console.Message("UPDATING DUST");
			if (obj.Power < 1 && obj.BlockID != Redstone.BLOCK_REDSTONE_OFF)
			{
				obj.BlockID = Redstone.BLOCK_REDSTONE_OFF;
				level.UpdateBlock(Player.Console, x, y, z,Redstone.BLOCK_REDSTONE_OFF);
				return;
			}
			if (obj.Power > 0 && obj.BlockID != Redstone.BLOCK_REDSTONE_ON)
			{
				obj.BlockID = Redstone.BLOCK_REDSTONE_ON;
				level.UpdateBlock(Player.Console, x, y, z, Redstone.BLOCK_REDSTONE_ON);
				return;
			}
		}
	}
	
	public class Redstone : Plugin {
		public override string name { get { return "Redstone"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }


		public static SchedulerTask redstoneTask;
		
		public static ushort BLOCK_REDSTONE_OFF = 34; // gray wool
		public static ushort BLOCK_REDSTONE_ON = 21; // red wool
		public static ushort BLOCK_REDSTONE_BLOCK = 62; // magma
		
		public Dictionary<string, Dictionary<int, RedstoneObject>> redstoneGrid = new Dictionary<string, Dictionary<int, RedstoneObject>>();
		
		void AddLevel(Level level)
		{
			if (redstoneGrid.ContainsKey(level.name))
			{
				//redstoneGrid[level.name].Clear();//.Remove(level.name);
				return;
			}
			redstoneGrid.Add(level.name, new Dictionary<int, RedstoneObject>());
		}
		static Plugin GetDoorPlugin()
		{
			foreach (Plugin plugin in Plugin.custom)
			{
				if (plugin.name.ToLower() == "door")
				{
					return plugin;
				}
			}
			return null;
		}
		bool busy = false;
		public void Propagate(Level level, int location, ushort block, ushort power, int count)
		{
			count--;
			if (count == 0)
			{
				busy = false;
				return;
			}
			if (IsRedstonePowerSource(block))
			{
				power = 12;
			}
			if (!redstoneGrid[level.name].ContainsKey(location))
			{
				redstoneGrid[level.name].Add( location, new RedstoneObject(block, (ushort)(power)));
			}
			else
			{
				redstoneGrid[level.name][location].Power = power;
			}
			
			ushort x;
			ushort y;
			ushort z;
			level.IntToPos(location, out x, out y, out z);

	
			redstoneGrid[level.name][location].UpdateBlock(level, location);
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					for (int v = -1; v <= 1; v++)
					{

						int locToInt = level.PosToInt( (ushort)(x + i),(ushort)(y + j), (ushort)(z + v));
						//Player.Console.Message(locToInt.ToString());
						ushort blockId = level.FastGetBlock( (ushort)(x + i),(ushort)(y + j), (ushort)(z + v));
						if (!IsRedstone(blockId))
						{
							if (redstoneGrid[level.name].ContainsKey(locToInt))
							{
								redstoneGrid[level.name].Remove(locToInt);
							}
							Door doorPlugin = (Door)GetDoorPlugin();
							if (doorPlugin != null)
							{
								if (doorPlugin.IsDoor(level, (ushort)(x + i),(ushort)(y + j), (ushort)(z + v)))
								{
									//DoorBlock d = doorPlugin.GetDoorFromBlock(blockId);
									if (power > 0)
									{
										//Level level, DoorBlock d, BlockID b
										//if (!doorPlugin.IsOpen(level, d, (BlockID)blockId))
										//{
											doorPlugin.OpenDoor(level,  (ushort)(x + i),(ushort)(y + j), (ushort)(z + v));
										//}
										continue;
									}
									//if (doorPlugin.IsOpen(level, d, blockId))
									//{
										doorPlugin.CloseDoor(level,  (ushort)(x + i),(ushort)(y + j), (ushort)(z + v));
									//}
								}
							}
							continue;
						}
						try
						{
						if (redstoneGrid[level.name].ContainsKey(locToInt) && redstoneGrid[level.name][locToInt].Power > power)
						{
							continue;
						}
						if (redstoneGrid[level.name].ContainsKey(locToInt) && redstoneGrid[level.name][locToInt].BlockID != blockId)
						{
							redstoneGrid[level.name][locToInt].BlockID  = blockId;
						}
						}
						catch
						{
						}
						
						
						//Player.Console.Message(blockId.ToString());

					
						Propagate(level, locToInt, blockId, (ushort)( power > 0 ? (power-1) : 0), count);
					}
				}
			}
		}
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			Server.MainScheduler.QueueRepeat(DoRedstoneTick, null, TimeSpan.FromMilliseconds(100));
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			Server.MainScheduler.Cancel(redstoneTask);
			redstoneGrid.Clear();
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
		public static bool IsRedstoneDust(ushort block)
		{
			return (block == BLOCK_REDSTONE_OFF || block == BLOCK_REDSTONE_ON );
		}
		public bool IsRedstonePowerSource(ushort block)
		{
			return (block == BLOCK_REDSTONE_BLOCK );
		}
		public bool IsRedstone(ushort block)
		{
			return IsRedstoneDust(block) || IsRedstonePowerSource(block);
		}
		void DoRedstoneTick(SchedulerTask task)
		{
			redstoneTask = task;
			foreach (Level level in LevelInfo.Loaded.Items)
			{
				if (!redstoneGrid.ContainsKey(level.name))
				{
					continue;
				}
				//Player.Console.Message("TICKING");
				List<int> toEdit = new List<int>();
				foreach( var pair in redstoneGrid[level.name])
				{
					
					toEdit.Add(pair.Key);
				}
				
				//Player.Console.Message( toEdit.Count.ToString() + " ITEMS");
				foreach( int v in toEdit)
				{
					if (busy)
					{
						continue;
					}
					RedstoneObject obj = redstoneGrid[level.name][v];
					if (!IsRedstone(obj.BlockID))
					{
						redstoneGrid[level.name].Remove(v);
						continue;
					}
					if (!(IsRedstoneDust(obj.BlockID)))
					{
						continue;
					}
					if (obj.Power < 1)
					{
						//redstoneGrid[level.name][v].UpdateBlock(level, v);
						//redstoneGrid[level.name].Remove(v);
						continue;
					}
					obj.Power = 0;
		
					//Player.Console.Message("Set dust to 0");
					
				}
				foreach (int v in toEdit)
				{
					if (busy)
					{
						continue;
					}
					if (!redstoneGrid[level.name].ContainsKey(v))
					{
						continue;
					}
					if (!IsRedstonePowerSource(redstoneGrid[level.name][v].BlockID))
					{
						continue;
					}
				//	Player.Console.Message("UPDATING STUFF!");
					
					RedstoneObject obj = redstoneGrid[level.name][v];
					busy = true;
					Propagate(level, v, obj.BlockID, obj.Power, 12);
				}
				foreach(int v in toEdit)
				{
					if (!redstoneGrid[level.name].ContainsKey(v))
					{
						continue;
					}
					if (! IsRedstoneDust(redstoneGrid[level.name][v].BlockID))
					{
						continue;
					}
					redstoneGrid[level.name][v].UpdateBlock(level, v);
					if (redstoneGrid[level.name][v].Power < 1)
					{
						redstoneGrid[level.name].Remove(v);
					}
				}

			}
		}
		

		public void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			AddLevel(p.level);
			int posint = p.level.PosToInt(x,y,z);
			if (redstoneGrid[p.level.name].ContainsKey(posint))
			{
				redstoneGrid[p.level.name].Remove(posint);
			}
			//busy = true;
			Propagate(p.level, posint, (ushort)(placing ? block : 0), (ushort)(0), 4);
			
			//p.Message("UPdating");
        }
		
	}
}