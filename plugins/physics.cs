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


public class PhysicsObj
{
    public ushort[] Position = new ushort[3]{0,0,0};
    public int[] Velocity=  new int[3]{0,0,0};
    public ushort BlockID = 1;
}
public class Physics : Plugin {
		public override string name { get { return "Physics"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }


		public static SchedulerTask physicsTask;
		
		public static ushort BLOCK_TNT = 46; // gray wool
		public static ushort BLOCK_FLINTSTEEL = 54;

        public static int AIR_RESISTANCE = 1;
        public static int GRAVITY = -1;
		
		public Dictionary<string, Dictionary<int, PhysicsObj>> physicsWorlds = new Dictionary<string, Dictionary<int, PhysicsObj>>();
		
		void AddLevel(Level level)
		{
			if (physicsWorlds.ContainsKey(level.name))
			{
				//redstoneGrid[level.name].Clear();//.Remove(level.name);
				return;
			}
			physicsWorlds.Add(level.name, new Dictionary<int, PhysicsObj>());
		}
		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			Server.MainScheduler.QueueRepeat(DoRedstoneTick, null, TimeSpan.FromMilliseconds(100));
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			Server.MainScheduler.Cancel(physicsTask);
			physicsWorlds.Clear();
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
        int toUnit(int input)
        {
            if (input < -1)
                input = -1;
            if (input > 1)
                input = 1;
            return input;
        }
        int[] toUnitVector(int[] input)
        {
            return new int[3] { toUnit(input[0]), toUnit(input[1]), toUnit(input[2]) };
        }
        ushort[] clampUshortVector(Level level, ushort[] vec)
        {
            return new ushort[3]{ Math.Max((ushort)Math.Min(vec[0], level.Width-1),(ushort)0), Math.Max((ushort)Math.Min(vec[1], level.Height-1),(ushort)0), Math.Max((ushort)Math.Min(vec[2], level.Length-1),(ushort)0) };
        }
        int[] resistVel(int[] velocity)
        {
            if (velocity[0] < 0)
                velocity[0]++;
            else 
                velocity[0]--;
            if (velocity[2] < 0)
                velocity[2]++;
            else 
                velocity[2]--;
            return velocity;
        }
		void DoRedstoneTick(SchedulerTask task)
		{
			physicsTask = task;
			foreach (Level level in LevelInfo.Loaded.Items)
			{
				if (!physicsWorlds.ContainsKey(level.name))
				{
					continue;
				}
                List<int> toRemove = new List<int>();
				foreach( var a in physicsWorlds[level.name])
				{
                    var x = a.Value;

                    bool isGrounded = level.FastGetBlock(x.Position[0], (ushort)Math.Max(x.Position[1]-1,0), x.Position[2]) != 0;
                    if (!isGrounded && x.Velocity[1] > GRAVITY)
                    {
                        x.Velocity[1] = x.Velocity[1] -1;
                    }
                    else if (isGrounded)
                    {
                        x.Velocity[1] = 0;
                    }
                    if (x.Velocity[0] + x.Velocity[1] + x.Velocity[2] == 0)
                    {
                        toRemove.Add(a.Key);
                        continue;
                    }
          
                    x.Velocity = resistVel(x.Velocity);//new int[3]{x.Velocity[0] - AIR_RESISTANCE, x.Velocity[1] - AIR_RESISTANCE, x.Velocity[2] - AIR_RESISTANCE };
                    
                    ushort[] oldPosition = x.Position;

                    int[] unitVelocity = toUnitVector(x.Velocity);
                    ushort[] projectedPosition = new ushort[3]{ (ushort)Math.Max(x.Position[0] + unitVelocity[0],0), (ushort)Math.Max(x.Position[1] + unitVelocity[1],0), (ushort)Math.Max(x.Position[2] + unitVelocity[2],0)};
                    projectedPosition = clampUshortVector(level, projectedPosition);
                    
                    int projectedPositionInt = level.PosToInt(projectedPosition[0],projectedPosition[1],projectedPosition[2]);
                    if (physicsWorlds[level.name].ContainsKey(projectedPositionInt))
                    {
                       physicsWorlds[level.name][projectedPositionInt].Velocity[0] += x.Velocity[0];
                        physicsWorlds[level.name][projectedPositionInt].Velocity[1] += x.Velocity[1];
                        physicsWorlds[level.name][projectedPositionInt].Velocity[2] += x.Velocity[2];
                        x.Velocity[0] = 0;
                        x.Velocity[2] = 0;
                        x.Velocity[1] = GRAVITY;
                        continue;
                    }

                    if (level.FastGetBlock(projectedPosition[0], projectedPosition[1], projectedPosition[2]) != 0)
                    {
                        if (isGrounded)
                            continue;
                        projectedPosition[0] = oldPosition[0];
                        projectedPosition[2] = oldPosition[2];
                    }
                    x.Position = projectedPosition;
                    level.UpdateBlock(Player.Console,oldPosition[0], oldPosition[1], oldPosition[2], 0);
                    level.UpdateBlock(Player.Console, projectedPosition[0], projectedPosition[1], projectedPosition[2], x.BlockID);
				}
                foreach (var a in toRemove)
                {
                    physicsWorlds[level.name].Remove(a);
                }
			}
		}
		public void AddPhysicsObject(Level level, ushort id, ushort x, ushort y, ushort z, int vx, int vy, int vz)
        {
            var newPhys = new PhysicsObj(){
                Position = new ushort[3]{x,y,z},
                Velocity = new int[3]{vx,vy,vz},
                BlockID = id
            };
            int posInt = level.PosToInt(x,y,z);
            if (!physicsWorlds[level.name].ContainsKey(posInt))
            {
                physicsWorlds[level.name].Add(posInt,newPhys);
                return;
            }
            physicsWorlds[level.name][posInt] = newPhys;
        }
        public void Explode(Level level, ushort x, ushort y, ushort z, int radius, int power)
        {
          //  x = (ushort)(x + (ushort)radius/2);
         //   y = (ushort)(y - (ushort)radius/1.5);
            //z = (ushort)(z + (ushort)radius/2);
            for (int sx = -radius/2; sx<radius/2; sx++)
            {
                int ax = sx + x;
                if (ax < 0 || ax >= level.Width)
                {
                    continue;
                }
                for (int sy = -radius/2; sy<radius/2; sy++)
                {
                    int ay = sy + y;
                    if (ay < 0|| ay >= level.Height)
                    {
                        continue;
                    }
                    for (int sz = -radius/2; sz<radius/2; sz++)
                    {
                        int az = sz + z;
                        if (az < 0 || az >= level.Length)
                        {
                            continue;
                        }
                        

                        int dist = (int)Math.Sqrt((sx*sx) + (sy*sy) + (sz*sz));
                        
                        if (dist > radius)
                        {
                            continue;
                        }
          
                        ushort blockId = (ushort)level.FastGetBlock((ushort)ax, (ushort)ay, (ushort)az);
                        if (blockId == 0)
                        {
                            continue;
                        }
                        int inverseSquareLaw = (1/(1 + (dist*dist)));
                        AddPhysicsObject(level, blockId, (ushort)ax, (ushort)ay, (ushort)az, toUnit(sx)*power * inverseSquareLaw, toUnit(sy) * power * inverseSquareLaw + 3, toUnit(sz)*power* inverseSquareLaw);
                        //if ( dist < radius/5 )
                       // {
                            level.UpdateBlock(Player.Console, (ushort)ax, (ushort)ay,(ushort)az,0);
                       //     continue;
                       // }
                    }
                }
            }
            AddPhysicsObject(level, 1, x, y, z, 1, 5, 1);
        }

		public void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			AddLevel(p.level);
            if (block == BLOCK_TNT)
            {
                Explode(p.level, x, y, z, 5, 4);
            }
        }
		
	}