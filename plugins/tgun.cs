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
	
 
    public class BulletProjectile
    {
        public float[] pos = {0,0,0};
        public float[] dir = {0,0,0};
        public uint life;
        public Level lvl;
        public bool explode;
        public Player owner;
        private float gravity = -1.5f;
        public BulletProjectile(Level level,Player p,float[] pos, float[] d)
        {
            lvl = level;
            this.pos = pos;//new float[3]{(float)(pos[0]),(float)(pos[1]),(float)(pos[2])};
            dir = d;
            life = 3000;
            owner = p;
            explode = true;
        }
        private bool collide(Vec3F32 p)
        {
              Vec3F32 delta = new Vec3F32(pos[0], pos[1]-1, pos[2])- p;
              if (delta.LengthSquared < (2f)) return true;
              return false;
        }
        private bool collide(Player p)
        {
            Vec3F32 pPos = p.Pos.ToVec3F32();
            for (ushort i=0; i<3;i++)
            {
                if (collide(pPos))
                    return true;
                pPos.Y -= 0.5f;
            }
            return false;
        }
        public void Tick()
        {
            life--;
            //Player.Console.Message(life.ToString());
            pos[0] += (dir[0]*2);
            pos[1] += (dir[1]*2);
            pos[2] += (dir[2]*2);
            if (dir[1] > gravity)
            {
                dir[1] -= 0.1f;
                if (dir[1] < gravity)
                    dir[1] = gravity;
            }
            MCGalaxy.SimpleSurvival.spawnEffect(lvl, TGun.bulletTrailParticle,pos, false, 1000);
            //MCGalaxy.SimpleSurvival.spawnEffect(lvl, SimpleSurvival.explosionParticleEffect2,pos, false, 6000);
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
            foreach(Player p in PlayerInfo.Online.Items)
            {
                if (p == owner) continue;
                if (!collide(p)) continue;
               // Vec3F32 delta = new Vec3F32(pos[0], pos[1]-1, pos[2])- p.Pos.ToVec3F32();
                Player victim = p;
                //if (delta.LengthSquared > (2f)) continue;
                ushort bx = (ushort)(p.Pos.X / 32);
				ushort by = (ushort)((p.Pos.Y - Entities.CharacterHeight) / 32);
				ushort bz = (ushort)(p.Pos.Z / 32);
              
                life = 0;
                explode = false;
                ushort dmg = 2;
                MCGalaxy.SimpleSurvival.spawnHurtParticles(p);
                if (MCGalaxy.SimpleSurvival.GetHealth(victim)-dmg <= 0)
                {
                    MCGalaxy.SimpleSurvival.Die(victim, 4);
                    string deathMessage = owner.color +  owner.name + " %dshot " + p.color + p.name + "%e.";
                    foreach( Player pl in PlayerInfo.Online.Items)
                    {
                        if (owner.level == pl.level || victim.level == p.level)
                            pl.Message(deathMessage);
                    }
                    return;
                }
                MCGalaxy.SimpleSurvival.Damage(p, dmg, 10);
                MCGalaxy.SimpleSurvival.PushPlayer(new Vec3F32(-dir[0], -dir[1], -dir[2]), p);
                return;
            }
            ushort b = lvl.FastGetBlock((ushort)pos[0], (ushort)pos[1], (ushort)pos[2]);
            if (b != 0 && b != 9)
            {
                life = 0;
                return;
            }
            
         
        }
    }

	public class TGun : Plugin {
		public override string name { get { return "tgun"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }

        public class BulletParticle : GoodlyEffects.EffectConfig
		{
			public BulletParticle(byte r = 255, byte g = 250, byte b = 240)
			{
				pixelU1 = 0;pixelV1 = 0;pixelU2 = 10;pixelV2 = 10;
				//tint uses RGB color values to determine what color to tint the particle. Here we've set it to be tinted pink, since the original texture is white.
				tintRed = r;tintGreen = g;tintBlue = b;
				//#frameCount determines how many frames of animation will be played over the particle's lifespan (faster life, faster animation).
				//#Frames are always the same size as each other and are stored left-to-right in particles.png.
				frameCount = 3;particleCount = 15;pixelSize = 3;sizeVariation = 0f;spread = 0.04f;speed = 0.2f;
				//#gravity adds to the up/down speed of the particle over time. -1 here means the heart will float up
				gravity = 0;
				//#baseLifetime is the time (in seconds) this particle is allowed to live at most (colliding with blocks may kill it sooner).
				baseLifetime = 1f;
				//#lifetimeVariation is how much the particle's lifespan can randomly vary. 1 means 100% variation, 0 means 0% variation.
				lifetimeVariation = 0.01f;
				//#expireUponTouchingGround means particle dies if it hits solid floor
				expireUponTouchingGround = false;
				//#collides determine what blocks count as "solid".
				collidesSolid = false;
				collidesLiquid = false;
				collidesLeaves = false;
				//#fullBright means it will always have its original brightness, even in dark environments.
				fullBright = false;
				MCGalaxy.SimpleSurvival.definedEffects.Add(this);
			}
		}
        static SchedulerTask missileTickTask;
        public static BulletParticle bulletTrailParticle = new BulletParticle(){ID=5};
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!

            OnPlayerClickEvent.Register(HandleBlockClick, Priority.Low);
            Server.MainScheduler.QueueRepeat(MissileTick, null, TimeSpan.FromMilliseconds(50));
            foreach(var p in PlayerInfo.Online.Items)
            {
                MCGalaxy.SimpleSurvival.defineEffects(p);
            }
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!	
            OnPlayerClickEvent.Unregister(HandleBlockClick);
            Server.MainScheduler.Cancel(missileTickTask);
		}
	
		public override void Help(Player p) {
			//HELP INFO!
		}

        public static List<BulletProjectile> projectiles = new List<BulletProjectile>();
        public static void MissileTick(SchedulerTask task)
        {
			missileTickTask = task;
            List<BulletProjectile> projectilesToDelete = new List<BulletProjectile>();
            foreach(var p in projectiles)
            {
                try
                {
                    p.Tick();
                }
                catch(Exception e)
                {
                    p.life = 0;
                    p.explode = false;
                }
                if (p.life > 0 )
                    continue;
                projectilesToDelete.Add(p);
                /*if (p.explode)
                {
                    try
                    {
                        MCGalaxy.SimpleSurvival.spawnEffect(p.lvl,  MCGalaxy.SimpleSurvival.explosionParticleEffect3,  new float[3]{(float)p.pos[0], (float)p.pos[1], (float)p.pos[2]}, false, 2000);
                    }
                    catch(Exception e)
                    {

                    }
                }*/
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
				byte CollideType = 2;
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
        public static void SpawnBullet(Level l, Player p, float[] pos, float[] rot)
        {
            projectiles.Add(new BulletProjectile(l, p, pos, rot));
        }
        void HandleBlockClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (button != MouseButton.Right) return;
            if (p.Game.Referee || p.invincible ) return;

		    ushort heldBlock = p.GetHeldBlock();
			heldBlock = (heldBlock > 255) ? (ushort)(heldBlock - 256) : heldBlock;
			if (!(heldBlock == 123))
                return;


            if (!MCGalaxy.SimpleSurvival.InventoryHasEnoughBlock(p, (ushort)(124))) // Check if have arrow
                return;

            if (projectiles.Count > 80)
            {
                p.Message("too many arrows spawned! please wait!");
                return;
            }
            MCGalaxy.SimpleSurvival.InventoryAddBlocks(p, (ushort)(124+256), -1); // Remove arrow
            Vec3F32 dir = DirUtils.GetDirVector(p.Rot.RotY, p.Rot.HeadX);
            SpawnBullet(p.level,p, new float[3]{(float)p.Pos.X/32,(float)((p.Pos.Y/32) + 0.5f),(float)(p.Pos.Z/32)}, new float[3]{dir.X,dir.Y,dir.Z});
            return;
		}
	}
}