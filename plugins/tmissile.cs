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
//pluginref goodlyeffects.dll
//pluginref simplesurvival.dll
namespace MCGalaxy {
	
    public class MissileProjectile
    {
        public float[] pos = {0,0,0};
        public float[] dir = {0,0,0};
        public uint life;
        public Level lvl;
        public bool explode;
        public MissileProjectile(Level level,ushort[] p, float[] d)
        {
            lvl = level;
            pos = new float[3]{(float)p[0],(float)p[1],(float)p[2]};
            dir = d;
            life = 100;
            explode = true;
            
        }

        public void Tick()
        {
            life--;
            pos[0] += (dir[0]*2);
            pos[1] += (dir[1]*2);
            pos[2] += (dir[2]*2);
            if (pos[0] >= lvl.Width || pos[0] >= lvl.Length)
            {
                life = 0;
                explode = false;
                return;
            }
            if (pos[2] >= lvl.Width || pos[2] >= lvl.Length)
            {
                life = 0;
                explode = false;
                return;
            }
            if (pos[1] >= lvl.Height)
            {
                life = 0;
                explode = false;
                return;
            }
            ushort b = lvl.FastGetBlock((ushort)pos[0], (ushort)pos[1], (ushort)pos[2]);
            if (b != 0 && b != 9)
            {
                life = 0;
                return;
            }
            MCGalaxy.SimpleSurvival.spawnEffect(lvl, SimpleSurvival.explosionParticleEffect,pos, false, 5000);
        }
    }

	public class TMissile : Plugin {
		public override string name { get { return "tmissile"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		
		
		public LevelPermission allowedRank { get { return LevelPermission.Admin; } }

        static ushort texture_id_side = 45;
        static ushort texture_id_top = 8;
        static ushort texture_id_bottom = 10;
		static ushort missile_id = 220;
    
        static SchedulerTask missileTickTask;
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
            AddMissileBlock("Missile", missile_id,  0, 0, 0, 16,16,16, texture_id_side,texture_id_side,texture_id_top,texture_id_bottom, 0, false);
            OnPlayerClickEvent.Register(HandleBlockClick, Priority.Low);
            Server.MainScheduler.QueueRepeat(MissileTick, null, TimeSpan.FromMilliseconds(50));
        
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!	
            OnPlayerClickEvent.Unregister(HandleBlockClick);
            Server.MainScheduler.Cancel(missileTickTask);
		}
	
		public override void Help(Player p) {
			//HELP INFO!
		}

        public static List<MissileProjectile> projectiles = new List<MissileProjectile>();
        public static void MissileTick(SchedulerTask task)
        {
			missileTickTask = task;
            List<MissileProjectile> projectilesToDelete = new List<MissileProjectile>();
            foreach(var p in projectiles)
            {
                try
                {
                p.Tick();
                if (p.life > 0 )
                    continue;
                }
                catch(Exception e)
                {
                    
                }
                projectilesToDelete.Add(p);
                if (p.explode)
                    MCGalaxy.SimpleSurvival.Explosion(p.lvl, (ushort)p.pos[0], (ushort)p.pos[1], (ushort)p.pos[2]);
            }
            foreach(var p in projectilesToDelete)
            {
                projectiles.Remove(p);
            }
        }
		public static void AddBlock(BlockDefinition def)
		{
			BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null );
		}
		public static void AddMissileBlock(string name, ushort Id, ushort MinX, ushort MinY, ushort MinZ, ushort MaxX, ushort MaxY, ushort MaxZ, ushort TEXTURE_SIDE, ushort TEXTURE_FRONT, ushort TEXTURE_TOP, ushort TEXTURE_BOTTOM, int Brightness, bool Transperant)
		{
				ushort RawID = Id;
				string Name = name;
				byte Speed = 1;
				byte CollideType = 0;
				bool BlocksLight = false;
				byte WalkSound = 1;
				bool FullBright = false;
				byte Shape = 16;
				byte BlockDraw =  (byte)(Transperant ? 1 : 0);
				byte FallBack = 5;
				byte FogDensity = 0;
				byte FogR = 0;
				byte FogG = 0;
				byte FogB = 0;
				ushort LeftTex = TEXTURE_SIDE;
				ushort RightTex = TEXTURE_SIDE;
				ushort FrontTex = TEXTURE_FRONT;
				ushort BackTex = TEXTURE_FRONT;
				ushort TopTex = TEXTURE_TOP;
				ushort BottomTex = TEXTURE_BOTTOM;
				int InventoryOrder = -1;
				BlockDefinition def = new BlockDefinition();
				def.RawID = RawID; def.Name = Name;
				def.Speed = Speed; def.CollideType = CollideType;
				def.TopTex = TopTex; def.BottomTex = BottomTex;
				
				def.BlocksLight = BlocksLight; def.WalkSound = WalkSound;
				def.FullBright = FullBright; def.Shape = Shape;
				def.BlockDraw = BlockDraw; def.FallBack = FallBack;
				
				def.FogDensity = FogDensity;
				def.FogR = FogR; def.FogG = FogG; def.FogB = FogB;
				def.MinX = (byte)MinX; def.MinY = (byte)MinY; def.MinZ = (byte)MinZ;
				def.MaxX = (byte)MaxX; def.MaxY = (byte)MaxY; def.MaxZ = (byte)MaxZ;
				
				def.LeftTex = LeftTex; def.RightTex = RightTex;
				def.FrontTex = FrontTex; def.BackTex = BackTex;
				def.InventoryOrder = InventoryOrder;
				def.UseLampBrightness = true;
				def.Brightness = Brightness;
				AddBlock(def);
		}
        public static void SpawnMissile(Level l, ushort[] pos, float[] rot)
        {
            projectiles.Add(new MissileProjectile(l,pos, rot));
        }
        void HandleBlockClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
			if (action != MouseAction.Pressed)
			 	return;
			ushort clickedBlock = p.level.GetBlock(x, y, z);
			if (clickedBlock == missile_id + 256 && button == MouseButton.Right)
			{
				p.level.UpdateBlock(p, x, y, z, (ushort)(0));
                
                Vec3F32 dir = DirUtils.GetDirVector(p.Rot.RotY, p.Rot.HeadX);
                SpawnMissile(p.level, new ushort[3]{x,y,z}, new float[3]{dir.X,dir.Y,dir.Z});
				return;
			}
		
		}
	}
}