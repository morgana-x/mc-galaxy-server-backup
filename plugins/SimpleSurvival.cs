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
using MCGalaxy.Commands;
using MCGalaxy.Bots;
using MCGalaxy.SQL;
using MCGalaxy.Generator.Foliage;
//pluginref door.dll
//pluginref bed.dll
//pluginref goodlyeffects.dll
//pluginref nasgen.dll
namespace MCGalaxy {

	public class SimpleSurvival : Plugin {
		public override string name { get { return "SimpleSurvival"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "SimpleSurvival"; } }
		public override string creator { get { return "morgana, Venk"; } }
		public override bool LoadAtStartup { get { return true; } }

		public class BlockMineConfig
		{
			public float PickaxeTimeMultiplier  = 1;
			public float ShovelTimeMultiplier = 1;
			public float AxeTimeMultiplier = 1;
			public ushort MiningTime = 5;
			public int overrideBlock = -1;
			public bool RequirePickaxe = false;
			public uint RequiredToolTier = 0;
			public float LootChance = 1f;
			public bool defaultToBlock = false;
			public BreakParticleEffect breakEffect = new BreakParticleEffect();
			public BlockMineConfig(ushort time = 5)
			{
				this.MiningTime = time;
			}
		}
		public class BreakParticleEffect : GoodlyEffects.EffectConfig
		{
			public BreakParticleEffect(byte r = 255, byte g = 255, byte b = 255, int grav = 2, byte fCount = 1)
			{
				pixelU1 = 0;
				pixelV1 = 0;
				pixelU2 = 10;
				pixelV2 = 10;
				//tint uses RGB color values to determine what color to tint the particle. Here we've set it to be tinted pink, since the original texture is white.
				tintRed = r;
				tintGreen = g;
				tintBlue = b;
				//#frameCount determines how many frames of animation will be played over the particle's lifespan (faster life, faster animation).
				//#Frames are always the same size as each other and are stored left-to-right in particles.png.
				frameCount = fCount;
				//#particleCount is how many hearts are spawned per-effect.
				particleCount = 10;
				//#pixelSize is how large in "pixel" units the particle is. 8 is the size of a player's head. You are allowed to be as precise as half a pixel, therefore the smallest possible size is 0.5.
				pixelSize = 4;
				//#sizeVariation is how much the particle can randomly vary in size. 1 means 100% variation, 0 means 0% variation.
				sizeVariation = 0.8f;
				//#spread allows the particles to spawn randomly around the point they were told to spawn at. A spread of "0.5" is equal to the width of a full block (because the spread goes both ways).
				spread = 1f;
				//#speed is how fast this particles moves away from the origin.
				speed = 0.25f;
				//#gravity adds to the up/down speed of the particle over time. -1 here means the heart will float up
				gravity = grav;
				//#baseLifetime is the time (in seconds) this particle is allowed to live at most (colliding with blocks may kill it sooner).
				baseLifetime = 2f;
				//#lifetimeVariation is how much the particle's lifespan can randomly vary. 1 means 100% variation, 0 means 0% variation.
				lifetimeVariation = 0;
				//#expireUponTouchingGround means particle dies if it hits solid floor
				expireUponTouchingGround = false;
				//#collides determine what blocks count as "solid".
				collidesSolid = true;
				collidesLiquid = false;
				collidesLeaves = false;
				//#fullBright means it will always have its original brightness, even in dark environments.
				fullBright = false;
			}
		}
		public class PVPParticleEffect : GoodlyEffects.EffectConfig
		{
			public PVPParticleEffect(byte r = 255, byte g = 0, byte b = 0, int grav = 4)
			{
				pixelU1 = 0;
				pixelV1 = 0;
				pixelU2 = 10;
				pixelV2 = 10;
				//tint uses RGB color values to determine what color to tint the particle. Here we've set it to be tinted pink, since the original texture is white.
				tintRed = r;
				tintGreen = g;
				tintBlue = b;
				//#frameCount determines how many frames of animation will be played over the particle's lifespan (faster life, faster animation).
				//#Frames are always the same size as each other and are stored left-to-right in particles.png.
				frameCount = 2;
				//#particleCount is how many hearts are spawned per-effect.
				particleCount = 10;
				//#pixelSize is how large in "pixel" units the particle is. 8 is the size of a player's head. You are allowed to be as precise as half a pixel, therefore the smallest possible size is 0.5.
				pixelSize = 4;
				//#sizeVariation is how much the particle can randomly vary in size. 1 means 100% variation, 0 means 0% variation.
				sizeVariation = 0.8f;
				//#spread allows the particles to spawn randomly around the point they were told to spawn at. A spread of "0.5" is equal to the width of a full block (because the spread goes both ways).
				spread = 0.5f;
				//#speed is how fast this particles moves away from the origin.
				speed = 0.4f;
				//#gravity adds to the up/down speed of the particle over time. -1 here means the heart will float up
				gravity = grav;
				//#baseLifetime is the time (in seconds) this particle is allowed to live at most (colliding with blocks may kill it sooner).
				baseLifetime = 1f;
				//#lifetimeVariation is how much the particle's lifespan can randomly vary. 1 means 100% variation, 0 means 0% variation.
				lifetimeVariation = 0;
				//#expireUponTouchingGround means particle dies if it hits solid floor
				expireUponTouchingGround = false;
				//#collides determine what blocks count as "solid".
				collidesSolid = true;
				collidesLiquid = false;
				collidesLeaves = false;
				//#fullBright means it will always have its original brightness, even in dark environments.
				fullBright = false;
				definedEffects.Add(this);
			}
		}
		public class ExplosionParticleEffect : GoodlyEffects.EffectConfig
		{
			public ExplosionParticleEffect(byte r = 100, byte g = 100, byte b = 100, int grav = -1)
			{
				pixelU1 = 0;
				pixelV1 = 0;
				pixelU2 = 10;
				pixelV2 = 10;
				//tint uses RGB color values to determine what color to tint the particle. Here we've set it to be tinted pink, since the original texture is white.
				tintRed = r;
				tintGreen = g;
				tintBlue = b;
				//#frameCount determines how many frames of animation will be played over the particle's lifespan (faster life, faster animation).
				//#Frames are always the same size as each other and are stored left-to-right in particles.png.
				frameCount = 8;
				//#particleCount is how many hearts are spawned per-effect.
				particleCount = 10;
				//#pixelSize is how large in "pixel" units the particle is. 8 is the size of a player's head. You are allowed to be as precise as half a pixel, therefore the smallest possible size is 0.5.
				pixelSize = 30;
				//#sizeVariation is how much the particle can randomly vary in size. 1 means 100% variation, 0 means 0% variation.
				sizeVariation = 0f;
				//#spread allows the particles to spawn randomly around the point they were told to spawn at. A spread of "0.5" is equal to the width of a full block (because the spread goes both ways).
				spread = 1f;
				//#speed is how fast this particles moves away from the origin.
				speed = 0.4f;
				//#gravity adds to the up/down speed of the particle over time. -1 here means the heart will float up
				gravity = grav;
				//#baseLifetime is the time (in seconds) this particle is allowed to live at most (colliding with blocks may kill it sooner).
				baseLifetime = 3f;
				//#lifetimeVariation is how much the particle's lifespan can randomly vary. 1 means 100% variation, 0 means 0% variation.
				lifetimeVariation = 0;
				//#expireUponTouchingGround means particle dies if it hits solid floor
				expireUponTouchingGround = false;
				//#collides determine what blocks count as "solid".
				collidesSolid = true;
				collidesLiquid = false;
				collidesLeaves = false;
				//#fullBright means it will always have its original brightness, even in dark environments.
				fullBright = false;
				definedEffects.Add(this);
			}
		}
		public class ExplosionParticleEffect2 : GoodlyEffects.EffectConfig
		{
			public ExplosionParticleEffect2(byte r = 255, byte g = 100, byte b = 10, int grav = 4)
			{
				pixelU1 = 0;
				pixelV1 = 0;
				pixelU2 = 10;
				pixelV2 = 10;
				//tint uses RGB color values to determine what color to tint the particle. Here we've set it to be tinted pink, since the original texture is white.
				tintRed = r;
				tintGreen = g;
				tintBlue = b;
				//#frameCount determines how many frames of animation will be played over the particle's lifespan (faster life, faster animation).
				//#Frames are always the same size as each other and are stored left-to-right in particles.png.
				frameCount = 2;
				//#particleCount is how many hearts are spawned per-effect.
				particleCount = 40;
				//#pixelSize is how large in "pixel" units the particle is. 8 is the size of a player's head. You are allowed to be as precise as half a pixel, therefore the smallest possible size is 0.5.
				pixelSize = 20;
				//#sizeVariation is how much the particle can randomly vary in size. 1 means 100% variation, 0 means 0% variation.
				sizeVariation = 0.8f;
				//#spread allows the particles to spawn randomly around the point they were told to spawn at. A spread of "0.5" is equal to the width of a full block (because the spread goes both ways).
				spread = 2f;
				//#speed is how fast this particles moves away from the origin.
				speed = 0.4f;
				//#gravity adds to the up/down speed of the particle over time. -1 here means the heart will float up
				gravity = grav;
				//#baseLifetime is the time (in seconds) this particle is allowed to live at most (colliding with blocks may kill it sooner).
				baseLifetime = 5f;
				//#lifetimeVariation is how much the particle's lifespan can randomly vary. 1 means 100% variation, 0 means 0% variation.
				lifetimeVariation = 0.1f;
				//#expireUponTouchingGround means particle dies if it hits solid floor
				expireUponTouchingGround = false;
				//#collides determine what blocks count as "solid".
				collidesSolid = true;
				collidesLiquid = false;
				collidesLeaves = false;
				//#fullBright means it will always have its original brightness, even in dark environments.
				fullBright = true;
				definedEffects.Add(this);
			}
		}
		public class ExplosionParticleEffect3 : GoodlyEffects.EffectConfig
		{
			public ExplosionParticleEffect3(byte r = 255, byte g = 200, byte b = 50, int grav = 2)
			{
				pixelU1 = 0;
				pixelV1 = 0;
				pixelU2 = 10;
				pixelV2 = 10;
				//tint uses RGB color values to determine what color to tint the particle. Here we've set it to be tinted pink, since the original texture is white.
				tintRed = r;
				tintGreen = g;
				tintBlue = b;
				//#frameCount determines how many frames of animation will be played over the particle's lifespan (faster life, faster animation).
				//#Frames are always the same size as each other and are stored left-to-right in particles.png.
				frameCount = 2;
				//#particleCount is how many hearts are spawned per-effect.
				particleCount = 20;
				//#pixelSize is how large in "pixel" units the particle is. 8 is the size of a player's head. You are allowed to be as precise as half a pixel, therefore the smallest possible size is 0.5.
				pixelSize = 15;
				//#sizeVariation is how much the particle can randomly vary in size. 1 means 100% variation, 0 means 0% variation.
				sizeVariation = 0.8f;
				//#spread allows the particles to spawn randomly around the point they were told to spawn at. A spread of "0.5" is equal to the width of a full block (because the spread goes both ways).
				spread = 2f;
				//#speed is how fast this particles moves away from the origin.
				speed = 0.4f;
				//#gravity adds to the up/down speed of the particle over time. -1 here means the heart will float up
				gravity = grav;
				//#baseLifetime is the time (in seconds) this particle is allowed to live at most (colliding with blocks may kill it sooner).
				baseLifetime = 5f;
				//#lifetimeVariation is how much the particle's lifespan can randomly vary. 1 means 100% variation, 0 means 0% variation.
				lifetimeVariation = 0.1f;
				//#expireUponTouchingGround means particle dies if it hits solid floor
				expireUponTouchingGround = false;
				//#collides determine what blocks count as "solid".
				collidesSolid = true;
				collidesLiquid = false;
				collidesLeaves = false;
				//#fullBright means it will always have its original brightness, even in dark environments.
				fullBright = true;
				definedEffects.Add(this);
			}
		}
		public static List<GoodlyEffects.EffectConfig> definedEffects = new List<GoodlyEffects.EffectConfig>();
		public static PVPParticleEffect pvpParticleEffect = new PVPParticleEffect(){ID = 9};
		public static ExplosionParticleEffect explosionParticleEffect = new ExplosionParticleEffect(){ID = 10};
		public static ExplosionParticleEffect2 explosionParticleEffect2 = new ExplosionParticleEffect2(){ID = 11};
		public static ExplosionParticleEffect3 explosionParticleEffect3  = new ExplosionParticleEffect3(){ID=12};
		public static ExplosionParticleEffect3 explosionParticleEffect4  = new ExplosionParticleEffect3(){ID=13};

		public static void defineEffects(Player p)
		{
			foreach(var a in definedEffects)
			{
				defineEffect(p, a);
			}
		}
		public class SurvivalTool
		{
			public ushort TEXTURE;
			public string NAME;
			public ushort ID;
			public bool IsSword=false;
			public bool IsPickaxe=false;
			public bool IsAxe = false;
			public bool IsShovel = false;
			public bool IsHoe = false;
			public bool IsSprite = false;
			public bool IsSheers = false;
			public bool IsPlaceable = false;
			public bool IsFlintAndSteel = false;
			public ushort Damage=2;
			public float Knockback = 1f;
			public float MiningBonus = 1f;
			public uint ToolTier = 0;

			public virtual void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
			{

			}
		}

		public class FlintSteelTool : SurvivalTool
		{
			public static void Lightblock(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
			{
				InventoryRemoveBlocks(p, 120, 1);
				BlockID block = p.level.FastGetBlock(x,y,z);
				if (block == 46)
				{
					p.level.UpdateBlock(p, x,y,z, (ushort)0);
					Explosion(p.level, x,y,z);
					return;
				}
				BlockID blockAbove = p.level.FastGetBlock(x,(ushort)(y+1),z);
				if (blockAbove == 0 && block != 0 && block != 54)
				{
					p.level.UpdateBlock(Player.Console, x,(ushort)(y+1),z, (ushort)54);
					return;
				}
	
			}
			public FlintSteelTool()
			{
				IsFlintAndSteel = true;
				IsSprite = true;
			}
		}
		static ushort Dist(ushort x1, ushort y1, ushort z1, int x2, int y2, int z2)
		{
			int distX = (x1 - x2);
			int distY = (y1 - y2);
			int distZ = (z1 - z2);
			return (ushort)Math.Sqrt( (distX * distX) + (distY * distY) + (distZ * distZ));
		}
		static ushort Dist(ushort x1, ushort y1, ushort z1, ushort x2, ushort y2, ushort z2)
		{
			int distX = (x1 - x2);
			int distY = (y1 - y2);
			int distZ = (z1 - z2);
			return (ushort)Math.Sqrt( (distX * distX) + (distY * distY) + (distZ * distZ));
		}
		//private System.Random rnd = new System.Random();

		private static void explosionEffect(Level level, ushort x, ushort y, ushort z)
		{
			if (!Config.UseGoodlyEffects)
				return;

			spawnEffect(level, explosionParticleEffect, new float[3]{(float)x, (float)y, (float)z}, false);
			spawnEffect(level, explosionParticleEffect2, new float[3]{(float)x, (float)y, (float)z}, false);
			spawnEffect(level, explosionParticleEffect3, new float[3]{(float)x, (float)y, (float)z}, false);
			spawnEffect(level, explosionParticleEffect4, new float[3]{(float)x, (float)y, (float)z}, false);
		}
		private static List<QueuedExplosion> queuedExplosions = new List<QueuedExplosion>();
		class QueuedExplosion
		{
			public Level Level;
			public ushort[] Pos;
		}
		private static int getExplodeRate(int numOfExps)
		{
			if (numOfExps > 200)
				return 150;
			if (numOfExps > 100)
				return 80;
			if (numOfExps > 20)
				return 15;
			if (numOfExps > 10)
				return 8;
			return 5;
		}
		private void doExplosionQueue(SchedulerTask task)
		{
			explosionTask = task;
			if (queuedExplosions.Count == 0)
				return;
			for (int i=0; i < queuedExplosions.Count; i++)
			{
				if (i > getExplodeRate(queuedExplosions.Count))
					break;
				QueuedExplosion exp;
				try
				{
					exp = queuedExplosions[i];
					queuedExplosions.Remove(exp);
				}
				catch(Exception e)
				{
					continue;
				}
				Explosion(exp.Level, exp.Pos[0], exp.Pos[1], exp.Pos[2]);
			}
		}
		private static List<ushort[]> getNeighboringTNT(Level level, ushort x, ushort y, ushort z)
		{
			List<ushort[]> chainExplosions = new List<ushort[]>();
			int range = 2;
			for (int bx = -range; bx <= range; bx++)
			{
				for (int by = -range; by <= range; by++)
				{
					for(int bz = -range; bz <= range; bz++)
					{
						ushort ax = (ushort)(x + bx);
						ushort ay = (ushort)(y + by);
						ushort az = (ushort)(z + bz);
						if (ax == 0 && ay == 0 && az == 0)
							continue;
						ushort block = level.FastGetBlock(ax,ay,az);
						if (block != 46)
							continue;
						/*ushort dist = Dist(ax,ay,az,x,y,z);
						if (dist > 1)
							continue;*/
						chainExplosions.Add(new ushort[3]{ax,ay,az});
					}
				}
			}
			return chainExplosions;
		}
		public static void Explosion(Level level, ushort x, ushort y, ushort z, Player p = null)
		{
			explosionEffect(level, x, y, z);
			if (p == null)
				p = Player.Console;
			level.UpdateBlock(Player.Console, x,y,z, (ushort)0);
			List<ushort[]> chainExplosions = getNeighboringTNT(level, x, y, z);
			int range = 2;
			for (int bx = -range; bx <= range; bx++)
			{
				for (int by = -range; by <= range; by++)
				{
					for(int bz = -range; bz <= range; bz++)
					{
						ushort ax = (ushort)(x + bx);
						ushort ay = (ushort)(y + by);
						ushort az = (ushort)(z + bz);
						if (ax == 0 && ay == 0 && az == 0)
							continue;
						ushort block = level.FastGetBlock(ax,ay,az);
						if (block == 0)
							continue;
						/*if (block == 46)
							continue;*/
						int range2 = range;
						/*if (blockMiningTimes.ContainsKey(block) && blockMiningTimes[block].MiningTime >= 60)
							range2 = (int) (range2 /2);*/
						ushort dist = Dist(ax,ay,az,x,y,z);
						if (dist > range2)
							continue;
						level.UpdateBlock(Player.Console, ax, ay, az, 0);
					}
				}
			}
			foreach (Player v in PlayerInfo.Online.Items)
			{
				if (v.level != level)
					continue;
				if (!v.Extras.Contains("SURVIVAL_HEALTH"))
					continue;
				ushort dist = Dist(x,y,z, v.Pos.BlockX, v.Pos.BlockY, v.Pos.BlockZ); 
				if (dist >= 5)
					continue;
				SetHealth(v, GetHealth(v) - (((5 - dist) * 4) + 4));
				if (GetHealth(v) <= 0)
					Die(v, 46);
			}

			foreach (var a in chainExplosions)
			{
				queuedExplosions.Add(new QueuedExplosion(){Level = level, Pos = a});
				//Explosion(level, a[0], a[1], a[2], p);
			}
		
		}
		public class PickaxeTool : SurvivalTool
		{
			public PickaxeTool(float mining=1.5f)
			{
				IsPickaxe = true;
				MiningBonus = mining;
			}
		}
		public class SwordTool : SurvivalTool
		{
			public SwordTool(ushort dmg =2, float knkback =1f)
			{
				IsSword = true;
				Damage = dmg;
				Knockback = knkback;
				MiningBonus = 0.1f;
			}
		}
		public class AxeTool : SurvivalTool
		{
			public AxeTool(ushort dmg =2, float knkback =1f, float mining=1.5f)
			{
				IsAxe = true;
				Damage = dmg;
				Knockback = knkback;
				MiningBonus = mining;
			}
		}
		public class ShovelTool : SurvivalTool
		{
			public ShovelTool(float mining=1.5f)
			{
				IsShovel = true;
				MiningBonus = mining;
			}
		}
		public class SheerTool : SurvivalTool
		{
			public SheerTool()
			{
				IsSheers = true;
			}
		}
		public class StoneMineConfig : BlockMineConfig
		{
			public StoneMineConfig(ushort time = 60)
			{
				PickaxeTimeMultiplier = 1.5f;
				AxeTimeMultiplier = 0.2f;
				ShovelTimeMultiplier = 0.2f;
				MiningTime = time;
				RequirePickaxe = true;
				breakEffect = new BreakParticleEffect(143, 143, 143);
			}
		}
		public class ObsidianMineConfig : StoneMineConfig
		{
			public ObsidianMineConfig(ushort time = 1300)
			{
				PickaxeTimeMultiplier = 1f;
				AxeTimeMultiplier = 0.01f;
				ShovelTimeMultiplier = 0.01f;
				MiningTime = time;
				RequirePickaxe = true;
				RequiredToolTier = 3;
			}
		}
		public class OreMineConfig : StoneMineConfig
		{
			public OreMineConfig(ushort time = 80)
			{
				PickaxeTimeMultiplier = 1.5f;
				AxeTimeMultiplier = 0.2f;
				ShovelTimeMultiplier = 0.2f;
				MiningTime = time;
				RequirePickaxe = true;
			}
		}
		public class WoodMineConfig : BlockMineConfig
		{
			public WoodMineConfig(ushort time = 23)
			{
				AxeTimeMultiplier = 1.5f;
				PickaxeTimeMultiplier = 0.1f;
			    ShovelTimeMultiplier = 0.1f;
			 	MiningTime = time;
				breakEffect = new BreakParticleEffect(124, 98, 62);
			}
		}
		public class DirtMineConfig : BlockMineConfig
		{
			public DirtMineConfig(ushort time = 12)
			{
				AxeTimeMultiplier = 0.2f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1.5f;
				MiningTime = time;
				breakEffect = new BreakParticleEffect(121, 85, 58);
			}
		}
		public class WoolMineConfig : BlockMineConfig
		{
			public WoolMineConfig(ushort time = 4)
			{
				AxeTimeMultiplier = 1f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1f;
				MiningTime = time;
			}
		}
		public class LeafMineConfig : BlockMineConfig
		{
			public LeafMineConfig(ushort time = 2)
			{
				AxeTimeMultiplier = 1f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1f;
				MiningTime = time;
				breakEffect = new BreakParticleEffect(77, 214, 52);
				LootChance = 0.05f;
				overrideBlock = 6;
			}
		}
		public class FireMineConfig : BlockMineConfig
		{
			public FireMineConfig(ushort time = 0)
			{
				AxeTimeMultiplier = 1f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1f;
				MiningTime = time;
				breakEffect = new BreakParticleEffect(255, 100, 10);
				overrideBlock = 0;
			}
		}
		public class SandMineConfig : BlockMineConfig
		{
			public SandMineConfig(ushort time = 7)
			{
				AxeTimeMultiplier = 1f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1.5f;
				MiningTime = time;
				breakEffect = new BreakParticleEffect(214, 207, 157);
			}
		}
		public class TntMineConfig : BlockMineConfig
		{
			public TntMineConfig(ushort time = 7)
			{
				AxeTimeMultiplier = 1f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1.5f;
				MiningTime = time;
				breakEffect = new BreakParticleEffect(150, 50, 50);
			}
		}
		public class GravelMineConfig : BlockMineConfig
		{
			public GravelMineConfig(ushort time = 8)
			{
				AxeTimeMultiplier = 1f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1.5f;
				MiningTime = time;
				breakEffect = new BreakParticleEffect(130, 114, 114);
				overrideBlock = 122;
				LootChance = 0.1f;
				defaultToBlock = true;
			}
		}
		public class CraftRecipe
		{
			public CraftRecipe(Dictionary<ushort,ushort> ingredients, ushort amountMultiplier = 1, bool needCraftingTable = false)
			{
				Ingredients = ingredients;
				amountProduced = amountMultiplier;
				NeedCraftingTable = needCraftingTable;
			}
			public Dictionary<ushort,ushort> Ingredients = new Dictionary<ushort,ushort>();
			public ushort amountProduced = 1;
			public bool NeedCraftingTable = false;
			
		}
		public class Config {
				// Player
				public static int MaxHealth = 20;
				public static int MaxAir = 10;
				public static bool FallDamage = true;
				public static bool VoidKills = true;
				
				// Effects
				public static bool UseGoodlyEffects = true; // broken right now
				public static string HitParticle = "pvp"; // broken right now
				
				// General
				public static string Path = "./plugins/SimpleSurvival/"; // 
				
				
				// MOBS
				public static bool CanKillMobs = true;
				public static bool SpawnMobs = true; // Requires MobAI from https://github.com/ddinan/classicube-stuff/blob/master/MCGalaxy/Plugins/MobAI.cs
				public static int MaxMobs = 25;

				// Mining
				public static bool MiningEnabled = true;

				// Inventory
				public static bool InventoryEnabled = true;

				// Backend
				public static ushort MineBlockIndicatorStart = 490;
		}
		
		ushort defaultMiningTime = 5;
	
		public static Dictionary<ushort, BlockMineConfig> blockMiningTimes = new Dictionary<ushort, BlockMineConfig>()
		{
			// Stone
			{1, new StoneMineConfig(){overrideBlock = 4}},
			{2, new DirtMineConfig(){overrideBlock = 3}},
			{3, new DirtMineConfig()},
			{4, new StoneMineConfig()},
			{5, new WoodMineConfig()},
			{6, new BlockMineConfig(1)},
			{12, new SandMineConfig()},
			{13, new GravelMineConfig()},
			{14, new OreMineConfig(120){RequiredToolTier=2}},
			{15, new StoneMineConfig(100){RequiredToolTier=1}},
			{16, new OreMineConfig(){overrideBlock = 114}},
			{17, new WoodMineConfig()},
			{18, new LeafMineConfig()},
			{19, new DirtMineConfig(4)},
			{20, new DirtMineConfig(2)},
			{21, new WoolMineConfig()},
			{22, new WoolMineConfig()},
			{23, new WoolMineConfig()},
			{24, new WoolMineConfig()},
			{25, new WoolMineConfig()},
			{26, new WoolMineConfig()},
			{27, new WoolMineConfig()},
			{28, new WoolMineConfig()},
			{29, new WoolMineConfig()},	
			{30, new WoolMineConfig()},
			{31, new WoolMineConfig()},
			{32, new WoolMineConfig()},
			{33, new WoolMineConfig()},
			{34, new WoolMineConfig()},
			{35, new WoolMineConfig()},
			{36, new WoolMineConfig()},
			{37, new WoolMineConfig()},
			{38, new WoolMineConfig()},
			{39, new WoolMineConfig()},
			{41, new StoneMineConfig()},
			{42, new StoneMineConfig()},
			{43, new StoneMineConfig()},
			{44, new StoneMineConfig()},
			{45, new StoneMineConfig()},
			{46, new TntMineConfig()},
			{47, new WoodMineConfig()},
			{48, new StoneMineConfig()},
			{49, new ObsidianMineConfig()},
			{50, new StoneMineConfig()},
			{51, new WoodMineConfig()},
			{53, new DirtMineConfig(1)},
			{52, new StoneMineConfig()},
			{60, new StoneMineConfig(10)},
			{61, new StoneMineConfig()},
			{62, new StoneMineConfig()},
			{64, new WoodMineConfig()},
			{54, new FireMineConfig()},
			{65, new StoneMineConfig()},
			{75, new WoodMineConfig(5)}, // Torch
			{76, new WoodMineConfig()},
			{77, new StoneMineConfig()},
			{78, new StoneMineConfig(){overrideBlock = 77}},
			{79, new StoneMineConfig(4)},
			{80, new StoneMineConfig(4)},
			{81, new StoneMineConfig(4)},
			{82, new StoneMineConfig(4)},
			{85, new BlockMineConfig(1)},
			{86, new StoneMineConfig()},
			{87, new OreMineConfig(){overrideBlock = 111, RequiredToolTier=2}},
			{88, new OreMineConfig(){RequiredToolTier=2}},
			
		};
		public static List<SurvivalTool> customTools = new List<SurvivalTool>(){
			new SwordTool()
			{
				NAME = "Iron Sword",
				TEXTURE = 204,
				ID = 91,
				IsSword = true,
				Damage = 6,
				ToolTier = 2
			},
			new PickaxeTool(7f)
			{
				NAME = "Iron Pickaxe",
				TEXTURE = 236,
				ID = 92,
				IsSword = false,
				Damage = 4,
				ToolTier = 2
			},
			new AxeTool()
			{
				NAME = "Iron Axe",
				TEXTURE = 252,
				ID = 93,
				IsSword = false,
				Damage = 5,
				ToolTier = 2
			},
			new ShovelTool(4f)
			{
				NAME = "Iron Shovel",
				TEXTURE = 220,
				ID = 94,
				IsSword = false,
				Damage = 6,
				ToolTier = 2
			},
			new SwordTool()
			{
				NAME = "Stone Sword",
				TEXTURE = 203,
				ID = 95,
				IsSword = true,
				Damage = 5,
				ToolTier = 1
			},
			new SurvivalTool()
			{
				NAME = "Stone Axe",
				TEXTURE = 251,
				ID = 96,
				IsSword = false,
				Damage = 4,
				ToolTier = 1
			},
			new PickaxeTool(5f)
			{
				NAME = "Stone Pickaxe",
				TEXTURE = 235,
				ID = 97,
				IsSword = false,
				Damage = 4,
				ToolTier = 1
			},
			new ShovelTool(2f)
			{
				NAME = "Stone Shovel",
				TEXTURE = 219,
				ID = 98,
				IsSword = false,
				Damage = 4,
				ToolTier = 1
			},
			new SwordTool()
			{
				NAME = "Wooden Sword",
				TEXTURE = 202,
				ID = 99,
				IsSword = true,
				Damage = 4
			},
			new ShovelTool(2f)
			{
				NAME = "Wooden Shovel",
				TEXTURE = 218,
				ID = 100,
				IsSword = false,
				Damage = 3
			},
			new PickaxeTool(2f)
			{
				NAME = "Wooden Pickaxe",
				TEXTURE = 234,
				ID = 101,
				IsSword = false,
				Damage = 3
			},
			new AxeTool()
			{
				NAME = "Wooden Axe",
				TEXTURE = 250,
				ID = 102,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Gold Sword",
				TEXTURE = 206,
				ID = 103,
				IsSword = true,
				Damage = 7,
				ToolTier = 2
			},
			new ShovelTool(4f)
			{
				NAME = "Gold Shovel",
				TEXTURE = 222,
				ID = 104,
				IsSword = false,
				Damage = 5,
				ToolTier = 2
			},
			new PickaxeTool(7f)
			{
				NAME = "Gold Pickaxe",
				TEXTURE = 238,
				ID = 105,
				IsSword = false,
				Damage = 5,
				ToolTier = 2
			},
			new AxeTool()
			{
				NAME = "Gold Axe",
				TEXTURE = 254,
				ID = 106,
				IsSword = false,
				Damage = 6,
				ToolTier = 2
			},
			new SurvivalTool()
			{
				NAME = "Diamond Sword",
				TEXTURE = 205,
				ID = 107,
				IsSword = true,
				Damage = 8,
				ToolTier = 3
			},
			new ShovelTool(5)
			{
				NAME = "Diamond Shovel",
				TEXTURE = 221,
				ID = 108,
				IsSword = false,
				Damage = 6,
				ToolTier = 3
			},
			new PickaxeTool(12f)
			{
				NAME = "Diamond Pickaxe",
				TEXTURE = 237,
				ID = 109,
				IsSword = false,
				Damage = 6,
				ToolTier = 3
			},
			new AxeTool()
			{
				NAME = "Diamond Axe",
				TEXTURE = 253,
				ID = 110,
				IsSword = false,
				Damage = 7,
				ToolTier = 3
			},
			new SurvivalTool()
			{
				NAME = "Diamond",
				TEXTURE = 175,
				ID = 111,
				IsSword = false,
				IsSprite = true,
			},
			new SurvivalTool()
			{
				NAME = "Gold Bar",
				TEXTURE = 159,
				ID = 112,
				IsSword = false,
				IsSprite = true,
			},
			new SurvivalTool()
			{
				NAME = "Iron Ingot",
				TEXTURE = 143,
				ID = 113,
				IsSword = false,
				IsSprite = true,
			},
			new SurvivalTool()
			{
				NAME = "Coal",
				TEXTURE = 127,
				ID = 114,
				IsSword = false,
				IsSprite = true
			},
			new SurvivalTool()
			{
				NAME = "Stick",
				TEXTURE = 255,
				ID = 115,
				IsSword = false,
				IsSprite = true
			},
			new SurvivalTool()
			{
				NAME = "Cookie",
				TEXTURE = 201,
				ID = 116,
				IsSword = false,
				IsSprite = true
			},
			new SurvivalTool()
			{
				NAME = "Beef",
				TEXTURE = 214,
				ID = 117,
				IsSword = false,
				IsSprite = true
			},
			new SurvivalTool()
			{
				NAME = "Steak",
				TEXTURE = 215,
				ID = 118,
				IsSword = false,
				IsSprite = true
			},
			new SurvivalTool()
			{
				NAME = "Apple",
				TEXTURE = 229,
				ID = 119,
				IsSword = false,
				IsSprite = true
			},
			new FlintSteelTool()
			{
				NAME = "Flint and Steel",
				TEXTURE = 207,
				ID = 120,
				IsSword = false,
				IsSprite = true
			},
			new SurvivalTool()
			{
				NAME = "Gunpowder",
				TEXTURE = 210,
				ID = 121,
				IsSword = false,
				IsSprite = true
			},
			new SurvivalTool()
			{
				NAME = "Flint",
				TEXTURE = 211,
				ID = 122,
				IsSword = false,
				IsSprite = true
			},
		};

		public static void SetMineTime(ushort blockId, BlockMineConfig config)
		{
			if (!blockMiningTimes.ContainsKey(blockId))
			{
				blockMiningTimes.Add(blockId, config);
				return;
			}
			blockMiningTimes[blockId] = config;
		}
		public static Dictionary<string, Dictionary<ushort, ushort>> mobLoot = new Dictionary<string, Dictionary<ushort, ushort>>()
		{
			{"sheep", new Dictionary<ushort, ushort>(){{36, 1}}},
			{"creeper", new Dictionary<ushort, ushort>(){{121, 1}}}
		};
		static ColumnDesc[] createInventories = new ColumnDesc[] {
            new ColumnDesc("Name", ColumnType.VarChar, 16),
            new ColumnDesc("Level", ColumnType.VarChar, 32),
			new ColumnDesc("Inventory", ColumnType.VarChar, 300)
        };
		public static Dictionary<ushort, CraftRecipe> craftingRecipies = new Dictionary<ushort, CraftRecipe>()
		{
			// Glass 											// Sand x1 (MOVE TO FURNACE LATER)
			 {20, new CraftRecipe(new Dictionary<ushort, ushort>(){{12, 1}})},
			 // Iron Ingot										// Iron Orex1 (MOVE TO FURNACE LATER)
			{113, new CraftRecipe(new Dictionary<ushort, ushort>(){{15, 1}})},
			// Gold bar											// Gold Orex1 (MOVE TO FURNACE LATER)
			{112, new CraftRecipe(new Dictionary<ushort, ushort>(){{14,1}})},
			// Furnace											// Cobblestone x8 = 1x Furnace
			{77, new CraftRecipe(new Dictionary<ushort, ushort>(){{4, 8}}, 1, true)},
			// Crafting Table									// Woodenblock x4 = 1x Crafting table
			{76, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 4}})},
			// Bookshelve									// Woodenblock x6 = 1x Book Shelve
			{47, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 6}}, 1, true)},
			// Crate									// Woodenblock x8 = 1x Book Shelve
			{64, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 8}})},
			// Wood												// Log x 1 = 4x Wood planks
			{5, new CraftRecipe(new Dictionary<ushort, ushort>(){{17, 1}}, 4)},
			// Brick
			{45, new CraftRecipe(new Dictionary<ushort, ushort>(){{4, 2}}, 1)},
			// Sandstone
			{52, new CraftRecipe(new Dictionary<ushort, ushort>(){{12, 2}}, 1)},
			// Ironblock
			{42, new CraftRecipe(new Dictionary<ushort, ushort>(){{113, 9}}, 1)},
			// Goldblock
			{41, new CraftRecipe(new Dictionary<ushort, ushort>(){{112, 9}}, 1)},
			// Diamond block
			{86, new CraftRecipe(new Dictionary<ushort, ushort>(){{111, 9}}, 1)},
			// Door												// Wood x 8 = 1x Wooden Door
			{66, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 8}}, 1, true)},
			// Iron Door												// Wood x 8 = 1x Wooden Door
			{67, new CraftRecipe(new Dictionary<ushort, ushort>(){{113, 8}}, 1, true)},
			// Dark Oak Door												// Wood x 8 = 1x Wooden Door
			{68, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 8}}, 1, true)},
			// Stick												// Wood x 2 = 4x Stick
			{115, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 2}}, 4)},
			// Torch											// Stick x 1 + Coal x 1 = 4x Torches
			{75, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {114, 1}}, 4)},
			// Glowstone										// 9x torches = 4x glowstone
			{79, new CraftRecipe(new Dictionary<ushort, ushort>(){{75, 9}}, 4, true)},
			// Bed											// Wood x 3 + Wool x 3 = 1x Bed
			{84, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 3}, {36, 3}}, 1, true)},
			// Cake											// Wool x 3 = 1x Cake
			{83, new CraftRecipe(new Dictionary<ushort, ushort>(){{36, 3}})},
			// Cobblestone Slab
			{50, new CraftRecipe(new Dictionary<ushort, ushort>(){{4, 1}}, 2, false)},
			// Double Slab
			{43, new CraftRecipe(new Dictionary<ushort, ushort>(){{4, 4}}, 1, false)},
			//  Slab
			{44, new CraftRecipe(new Dictionary<ushort, ushort>(){{43, 1}}, 2, false)},
			//  Rope
			{51, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 7}}, 4, false)},
			//  Redwool									// Rose + wool
			{21, new CraftRecipe(new Dictionary<ushort, ushort>(){{38, 1}, {36, 1}}, 1, false)},


			// Wooden Sword									// Stick x 1 + wood x 2 = 1x Wooden sword  [Need crafting table]
			{99, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {5, 2}}, 1, true)},
			// Wooden Shovel									// Stick x 2 + wood x 1 = 1x Wooden Shovel
			{100, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {5, 1}}, 1, true)},
			// Wooden PIck									// Stick x 2 + wood x 3 = 1x Wooden Pick
			{101, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {5, 3}}, 1, true)},
			// Wooden Axe									// Stick x 2 + wood x 4 = 1x Wooden Axe
			{102, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {5, 4}}, 1, true)},

			// Stone Sword									// Stick x 1 + Cobblestone x 2 = 1x stone sword
			{95, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {4, 2}}, 1, true)},
			// Stone Shovel									// Stick x 2 + Cobblestone x 1 = 1x stone Shovel
			{96, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {4, 1}}, 1, true)},
			// Stone PIck									// Stick x 2 + Cobblestone x 3 = 1x stone Pick
			{97, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {4, 3}}, 1, true)},
			// Stone Axe									// Stick x 2 + Cobblestone x 4 = 1x stone Axe
			{98, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {4, 4}}, 1, true)},

			// Iron Sword									// Stick x 1 + Iron x 2 = 1x Iron sword
			{91, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {113, 2}}, 1, true)},
			// Iron Shovel									// Stick x 2 + Iron x 1 = 1x Iron Shovel
			{94, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {113, 1}}, 1, true)},
			// Iron PIck									// Stick x 2 + Iron x 3 = 1x Iron Pick
			{92, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {113, 3}}, 1, true)},
			// Iron Axe									// Stick x 2 + Iron x 4 = 1x Iron Axe
			{93, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {113, 4}}, 1, true)},

			// Gold Sword									// Stick x 1 + Gold x 2 = 1x Gold sword
			{103, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {112, 2}}, 1, true)},
			// Gold Shovel									// Stick x 2 + Gold x 1 = 1x Gold Shovel
			{104, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {112, 1}}, 1, true)},
			// Gold PIck									// Stick x 2 + Gold x 3 = 1x Gold Pick
			{105, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {112, 3}}, 1, true)},
			// Gold Axe									// Stick x 2 + Gold x 4 = 1x Gold Axe
			{106, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {112, 4}}, 1, true)},

			// Diamond Sword									// Stick x 1 + Diamond x 2 = 1x Diamond sword
			{107, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {111, 2}}, 1, true)},
			// Diamond Shovel									// Stick x 2 + Diamond x 1 = 1x Diamond Shovel
			{108, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {111, 1}}, 1, true)},
			// Diamond PIck									// Stick x 2 + Diamond x 3 = 1x Diamond Pick
			{109, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {111, 3}}, 1, true)},
			// Diamond Axe									// Stick x 2 + Diamond x 4 = 1x Diamond Axe
			{110, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 2}, {111, 4}}, 1, true)},

			// Flint and Steel										// Flint x1, Iron igot x 1 = 12x flint and steel
			{120, new CraftRecipe(new Dictionary<ushort, ushort>(){{122, 1}, {113, 1}}, 12, false)},

			// TNT													// Sand x 10, Gunpowder x 10
			{46,  new CraftRecipe(new Dictionary<ushort, ushort>(){{12, 10}, {121, 10}}, 1, true)},
		};
		
		public class MiningProgress
		{
			public ushort BlockType;
			public ushort Progress = 0;
			public ushort[] Position;
			public ushort MiningIndicatorProgress = 0;
			public DateTime LastMine;

			public bool IsExpired()
			{
				return DateTime.Now.Subtract(LastMine).TotalMilliseconds > 1200;
			}


			public MiningProgress(ushort block, ushort[] pos)
			{
				BlockType = block;
				Position = pos;
				LastMine = DateTime.Now;
			}
		}
		
		SchedulerTask drownTask;
		SchedulerTask guiTask;
		SchedulerTask regenTask;
		SchedulerTask mobSpawningTask;
		SchedulerTask explosionTask;

		public static Dictionary<string, Dictionary<string, Dictionary<ushort, ushort>>> playerInventories = new  Dictionary<string, Dictionary<string, Dictionary<ushort, ushort>>>();
		public Dictionary<Player, MiningProgress> playerMiningProgress = new Dictionary<Player, MiningProgress>();
		public Dictionary<Player, PlayerBot> mineProgressIndicators = new Dictionary<Player, PlayerBot>();
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
			OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.Low);
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Normal);
			OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
			OnSentMapEvent.Register(HandleSentMap, Priority.Low);
			OnPlayerDyingEvent.Register(HandlePlayerDying, Priority.High);
			OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.Low);
			
			Server.MainScheduler.QueueRepeat(HandleDrown, null, TimeSpan.FromMilliseconds(500));
			Server.MainScheduler.QueueRepeat(HandleGUI, null, TimeSpan.FromMilliseconds(100));
			Server.MainScheduler.QueueRepeat(HandleRegeneration, null, TimeSpan.FromSeconds(4));
			Server.MainScheduler.QueueRepeat(HandleMobSpawning, null, TimeSpan.FromSeconds(1));
			Server.MainScheduler.QueueRepeat(doExplosionQueue, null, TimeSpan.FromMilliseconds(100));
			Command.Register(new CmdPvP());
			Command.Register(new CmdGiveBlock());
			Command.Register(new CmdCraft());
			Command.Register(new CmdTradeBlock());
			loadMaps();
			SetupInventoryDB();
			try
			{
				addBreakBlocks();
				AddToolBlocks();
				AddDoorBlocks();
				AddBedBlocks();
			}
			catch (Exception e)
			{
				Player.Console.Message(e.ToString());
				Player.Console.Message("WARNING YOU NEED THE INF ID VERSION OF MCGALAXY TO ACCESS FULL FEATURES OF SIMPLESURVIVAL!!");
			}
			foreach (Player p in PlayerInfo.Online.Items)
			{
				InitPlayer(p);
				defineEffects(p);
			}
			
			if(!Directory.Exists(Config.Path))
			{
				Directory.CreateDirectory(Config.Path);
			}
			if (true)
			{
				AddAi("hostile", new string[] {"", "hostile", "hostile"});
				AddAi("roam", new string[] {"", "roam", "roam"});
			}
			mobHealth.Clear();
			foreach(Player p in PlayerInfo.Online.Items)
			{
				LoadInventory(p);
				SendMiningUnbreakableMessage(p);
			}
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
			OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
			OnPlayerMoveEvent.Unregister(HandlePlayerMove);
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnSentMapEvent.Unregister(HandleSentMap);
			OnPlayerDyingEvent.Unregister(HandlePlayerDying);
			OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
			Server.MainScheduler.Cancel(drownTask);
			Server.MainScheduler.Cancel(guiTask);
			Server.MainScheduler.Cancel(regenTask);
			Server.MainScheduler.Cancel(mobSpawningTask);
			Server.MainScheduler.Cancel(explosionTask);
			Command.Unregister(Command.Find("PvP"));
			Command.Unregister(Command.Find("GiveBlock"));
			Command.Unregister(Command.Find("TradeBlock"));
			Command.Unregister(Command.Find("Craft"));

			SaveAllInventory();
			mobHealth.Clear();
			queuedExplosions.Clear();

			foreach(var pair in mineProgressIndicators)
			{
				PlayerBot.Remove(pair.Value);
			}
			mineProgressIndicators.Clear();
			playerMiningProgress.Clear();
			DespawnAllBots();
		}
		public override void Help(Player p) {
			//HELP INFO!
		}
		

		public static List<string> maplist = new List<string>();
		void loadMaps()
        {
            if (File.Exists(Config.Path + "maps.txt"))
            {
                using (var maplistreader = new StreamReader(Config.Path + "maps.txt"))
                {
                    string line;
                    while ((line = maplistreader.ReadLine()) != null)
                    {
                        maplist.Add(line);
                    }
                }
            }
            else File.Create(Config.Path+ "maps.txt").Dispose();
        }
		//////////
		// Tools
		///
		private void AddToolBlocks()
		{
			foreach (var a in customTools)
			{
				if (a.IsSprite)
				{
					AddBlockItem(a.ID, a.NAME, a.TEXTURE);
					if (!blockMiningTimes.ContainsKey(a.ID))
						blockMiningTimes.Add(a.ID, new BlockMineConfig(2));
					blockMiningTimes[a.ID] = new BlockMineConfig(2);
					continue;
				}
				AddBlockDef(a.NAME, a.ID, 16,0,0,16,16,16,a.TEXTURE, 85, 85, 85, true, 0, admin:true);
			}
		}
		private void AddDoorBlocks()
		{
			ushort i = 0;
			ushort storageId = 300;//(ushort)MCGalaxy.Door.DoorBlockIdStorageIndex;
			foreach(MCGalaxy.DoorConfig a in MCGalaxy.Door.DoorConfigs)
			{
				ushort start = (ushort)(storageId + (i * 8));
				for (ushort x=start; x < (ushort)(start+8); x++)
					blockMiningTimes.Add(x, new WoodMineConfig(){overrideBlock = a.BLOCK_ITEM_ID});
				i++;
			}
		}
		private void AddBedBlocks()
		{
			BedConfig bedConfig = new BedConfig(){ITEM_ID = 84, ID =460}; //MCGalaxy.Bed.bedConfig;
			for (ushort x=bedConfig.ID; x < (ushort)(bedConfig.ID+8); x++)
				blockMiningTimes.Add(x, new WoodMineConfig(){overrideBlock = bedConfig.ITEM_ID});
		}
		private void AddBlockDef(BlockDefinition def)
		{
			BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null );
		}
		public void AddBlockDef(string name, ushort Id, ushort MinX, ushort MinY, ushort MinZ, ushort MaxX, ushort MaxY, ushort MaxZ, ushort TEXTURE_SIDE, ushort TEXTURE_FRONT, ushort TEXTURE_TOP, ushort TEXTURE_BOTTOM, bool Transperant, int Brightness=0, bool admin=false)
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
				ushort BackTex = TEXTURE_TOP;
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
				def.UseLampBrightness = false;
				def.Brightness = Brightness;
				AddBlockDef(def);
				
				ushort block = Id;
				 if (admin) {
					BlockPerms perms = BlockPerms.GetPlace((ushort)(block + 256));
					perms.MinRank = LevelPermission.Nobody;
					BlockPerms.Save();
					BlockPerms.ApplyChanges();

					if (!Block.IsPhysicsType(block)) {
						BlockPerms.ResendAllBlockPermissions();
					}    
				 }        
				//SetDoorBlockPerms(Id);
		}
		public void AddBlockItem(ushort Id, string Name, ushort Texture, bool admin=false)
		{
			BlockDefinition def = new BlockDefinition();
				def.RawID = Id; def.Name = Name;
				def.Speed = 1; def.CollideType = 0;
				def.TopTex = Texture; def.BottomTex = Texture;
				
				def.BlocksLight = false; def.WalkSound = 1;
				def.FullBright = false; def.Shape = 0;
				def.BlockDraw = 2; def.FallBack = 5;
				
				def.FogDensity = 0;
				def.FogR = 0; def.FogG = 0; def.FogB = 0;
				def.MinX = 0; def.MinY = 0; def.MinZ = 0;
				def.MaxX = 0; def.MaxY = 0; def.MaxZ = 0;
				
				def.LeftTex = Texture; def.RightTex = Texture;
				def.FrontTex = Texture; def.BackTex = Texture;
				def.InventoryOrder = -1;
			ushort block = Id;
			 if (admin) {
					BlockPerms perms = BlockPerms.GetPlace((ushort)(block + 256));
					perms.MinRank = LevelPermission.Guest; // LevelPermission.Nobody
				 }
				BlockPerms.Save();
				BlockPerms.ApplyChanges();

				if (!Block.IsPhysicsType(block)) {
					BlockPerms.ResendAllBlockPermissions();
				}        
			AddBlockDef(def);
		}
		///////////////////////////////////////////////////////////
		// Inventory
		///////////////////////////////////////////////////////////
		public Dictionary<ushort,ushort> getPlayerInventory(Player pl )
		{
			Dictionary<ushort, ushort> nullInventory = new Dictionary<ushort, ushort>();
			if (!playerInventories.ContainsKey(pl.level.name))
				return nullInventory;
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				return nullInventory;
			return playerInventories[pl.level.name][pl.name];
		}
		public static void InventorySetBlock(Player pl, ushort block, ushort amount)
		{
			
			if (!playerInventories.ContainsKey(pl.level.name))
				playerInventories.Add(pl.level.name, new Dictionary<string, Dictionary<ushort, ushort>>());
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				playerInventories[pl.level.name].Add(pl.name, new Dictionary<ushort, ushort>());
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				playerInventories[pl.level.name][pl.name].Add(block, 0);
			playerInventories[pl.level.name][pl.name][block] = amount;
			SendMiningUnbreakableMessage(pl, block);
			SendInventory(pl);
		}
		public static Dictionary<ushort, ushort> InventoryGet(Player pl, string level)
		{
			if (!playerInventories.ContainsKey(level))
				return new Dictionary<ushort, ushort>();
			if (!playerInventories[level].ContainsKey(pl.name))
				return new Dictionary<ushort, ushort>();
			return playerInventories[level][pl.name];
		}
		BlockMineConfig getBlockMineTime(ushort blockId)
		{
			if (blockId > 256)
				blockId = (ushort)(blockId - 256);
			if (!blockMiningTimes.ContainsKey(blockId))
				return null;
			return blockMiningTimes[blockId];
		}
		public static void InventoryAddBlocks(Player pl, ushort block, int amount)
		{
			if (pl.level.Config.MOTD.ToLower().Contains("-inventory")) return;
			if (amount < 0)
			{
				InventoryRemoveBlocks(pl, block, (ushort)(Math.Abs(amount)));
				return;
			}
			InventoryAddBlocks(pl, block, (ushort)amount);
		}
		public static void InventoryAddBlocks(Player pl, ushort block, ushort amount)
		{
			ushort oldBlock = block;
			if (block > 256)
				block = (ushort)(block - 256);
			if (!playerInventories.ContainsKey(pl.level.name))
				playerInventories.Add(pl.level.name, new Dictionary<string, Dictionary<ushort, ushort>>());
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				playerInventories[pl.level.name].Add(pl.name, new Dictionary<ushort, ushort>());
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				playerInventories[pl.level.name][pl.name].Add(block, 0);
			playerInventories[pl.level.name][pl.name][block] = (ushort)(playerInventories[pl.level.name][pl.name][block]  + amount);
			SendMiningUnbreakableMessage(pl, block);
			SendInventory(pl);
		}
		public static void InventoryRemoveBlocks(Player pl, ushort block, ushort amount)
		{
			ushort oldBlock = block;
			if (block > 256)
				block = (ushort)(block - 256);
			if (!playerInventories.ContainsKey(pl.level.name))
				return;
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				return;
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				return;
			if (amount >= playerInventories[pl.level.name][pl.name][block])
			{
				playerInventories[pl.level.name][pl.name].Remove(block);
				ushort heldBlock = pl.GetHeldBlock();
				heldBlock = (heldBlock > 255) ? (ushort)(heldBlock - 256) : heldBlock;
				if ( heldBlock == block)
					SetHeldBlock(pl, 0);
				return;
			}
			playerInventories[pl.level.name][pl.name][block] = (ushort)(playerInventories[pl.level.name][pl.name][block] - amount);
			SendMiningUnbreakableMessage(pl, block);
			SendInventory(pl);
		}
		public static bool InventoryHasEnoughBlock(Player pl, ushort block, ushort amount=1)
		{
			return InventoryGetBlockAmount(pl, block) >= amount;
		}

		public static ushort InventoryGetBlockAmount(Player pl, ushort block)
		{
			if (block > 256)
				block = (ushort)(block - 256);
			if (!playerInventories.ContainsKey(pl.level.name))
				return 0;
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				return 0;
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				return 0;
			return playerInventories[pl.level.name][pl.name][block];
		}
		public static ushort InventoryGetBlockAmountRaw(Player pl, ushort block)
		{
			if (!playerInventories.ContainsKey(pl.level.name))
				return 0;
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				return 0;
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				return 0;
			return playerInventories[pl.level.name][pl.name][block];
		}
		static void SendMissingItems(Player pl, CraftRecipe recipe)
		{
			pl.Message("%cYou need:");
			foreach(var pair in recipe.Ingredients)
			{
				pl.Message("  " + Block.GetName(pl, pair.Key > 65 ? (ushort)(pair.Key + 256) : pair.Key) + "%5x" + pair.Value.ToString());
			}
		}
		public static void Craft(Player pl, ushort block, ushort amount=1)
		{
			if (!craftingRecipies.ContainsKey(block))
			{
				pl.Message("Recipe doesn't exist for this block!");
				return;
			}
			foreach(var pair in craftingRecipies[block].Ingredients)
			{
				if (!InventoryHasEnoughBlock(pl, pair.Key, (ushort)(pair.Value * amount)))
				{
					pl.Message("%cNot enough items!");
					SendMissingItems(pl, craftingRecipies[block]);
					return;
				}
			}
			if (craftingRecipies[block].NeedCraftingTable && !IsNearCraftingTable(pl))
			{
				pl.Message("You need to be near a crafting table to craft this!");
				return;
			}
			foreach(var pair in craftingRecipies[block].Ingredients)
			{
				InventoryRemoveBlocks(pl, pair.Key, (ushort)(pair.Value * amount));
			}
			InventoryAddBlocks(pl, block, (ushort)(craftingRecipies[block].amountProduced * amount));
			SetHeldBlock(pl, 0);
			SetHeldBlock(pl, block);
			pl.Message("Crafted " + (amount * craftingRecipies[block].amountProduced).ToString() + "x " + Block.GetName(pl, block > 65 ? (ushort)(block + 256) : block) + ".");
		}
		public static bool IsNearCraftingTable(Player p)
		{
			int blockx = p.Pos.BlockX;
			int blocky = p.Pos.BlockY;
			int blockz = p.Pos.BlockZ;
			for (int x = -1; x < 2; x++)
			{
				if (blockx + x < 0 || blockx + x >= p.level.Width)
					continue;
				for (int z=-1; z< 2; z++)
				{
					if (blockz + z < 0 || blockz + z >= p.level.Length)
						continue;
					for (int y=-1; y< 2; y++)
					{
						if (z ==0 && x ==0 && y == 0)
							continue;
						if (blocky + y < 0 || blocky + y >= p.level.Height)
							continue;
						ushort block = p.level.FastGetBlock((ushort)(blockx + x), (ushort)(blocky + y), (ushort)(blockz + z));
						if (block == 76 + 256 )
							return true;
					}
				}
			}
			return false;
		}
		public static Dictionary<ushort,CraftRecipe>  GenerateCraftOptions(Player pl)
		{
			Dictionary<ushort,CraftRecipe> validCraftables = new Dictionary<ushort,CraftRecipe> ();
			bool nearCraftingtable = IsNearCraftingTable(pl);
			foreach(var recipePair in craftingRecipies)
			{
				if (!nearCraftingtable && recipePair.Value.NeedCraftingTable)
					continue;
				bool valid = true;
				foreach(var pair in recipePair.Value.Ingredients)
				{
					if (!InventoryHasEnoughBlock(pl, pair.Key, pair.Value))
					{
						valid = false;
						break;
					}
				}
				if (!valid)
					continue;
				validCraftables.Add(recipePair.Key, recipePair.Value);
			}
			return validCraftables;
		}
		public static string GenerateCraftOptionsMessage(Player p)
		{
			Dictionary<ushort,CraftRecipe> validRecipes = GenerateCraftOptions(p);
			string message = "Craftable Items:\n";
			bool nearCraftingtable = IsNearCraftingTable(p);
			foreach( var pair in validRecipes)
			{
				message += /*"  %e[%5" + pair.Key + "%e] */"    %3" + Block.GetName(p, pair.Key > 65 ? (ushort)(pair.Key + 256) : pair.Key) + "%7 ==";
				foreach(var pair2 in pair.Value.Ingredients)
				{
					message += " %2" + Block.GetName(p, pair2.Key > 65 ? (ushort)(pair2.Key + 256) : pair2.Key) + "%dx" + pair2.Value.ToString();
				}
				message += "\n";
			}
			if (!nearCraftingtable)
				message += "%cStand near a crafting table to see items that require one!";
			return message;
		}
		private static void LoadInventory(Player pl)
		{
			if (!maplist.Contains(pl.level.name))
                return;
 			List<string[]> pRows = Database.GetRows("SurvivalInventory", "*", "WHERE Name=@0 AND Level=@1", pl.name, pl.level.name);
			if (pRows.Count == 0) return;
			string[] row = pRows[0];
		 	if (row.Length < 3) return;
			if (row[2].Length == 0)
				return;
			string[] inventoryEntries = row[2].Split(';');
			for (int i=0; i<inventoryEntries.Length-2; i=i+2)
			{
				ushort block = UInt16.Parse(inventoryEntries[i]);
				ushort amount = UInt16.Parse(inventoryEntries[i+1]);
				InventorySetBlock(pl,block, amount );
			}

		}
		private static void SaveInventory(Player pl, string level)
		{
			if (level == "")
			{
				return;
			}
			Dictionary<ushort, ushort> inv = InventoryGet(pl, level);
			string saveInventoryString = "0;0;";
			foreach(var pair in inv)
			{
				saveInventoryString += pair.Key.ToString() +";" + pair.Value.ToString() +";";
			}
 			List<string[]> pRows = Database.GetRows("SurvivalInventory", "*", "WHERE Name=@0 AND Level=@1", pl.name, level);
			
            if (pRows.Count == 0)
            {
                Database.AddRow("SurvivalInventory", "Name,  Level", pl.name, level);
            }
			Database.UpdateRows("SurvivalInventory", "Inventory=@0", "WHERE Name=@1 AND Level=@2", saveInventoryString, pl.name , level);

		}
		private static void SaveAllInventory()
		{
			foreach (var pl in PlayerInfo.Online.Items)
				SaveInventory(pl, pl.level.name);
		}
		private static void SetupInventoryDB()
		{
			   Database.CreateTable("SurvivalInventory", createInventories);
		}
		///////////////////////////////////////////////////////////
		// Mining
		///////////////////////////////////////////////////////////
		private static void SendMiningUnbreakableMessage(Player p)
		{
			if (!Config.MiningEnabled)
				return;
			if (p.level.Config.MOTD.ToLower().Contains("-inventory")) return;
			bool extBlocks = p.Session.hasExtBlocks;
            int count = p.Session.MaxRawBlock + 1;
            int size  = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * size];
            Level level = p.level;
            for (int i = 0; i < count; i++) 
            {
				bool canPlace = true;//(( i < p.group.CanPlace.Length && p.group.CanPlace[i]) || (i > 65 && i+256 < p.group.CanPlace.Length && p.group.CanPlace[i+256]));
				if (getTool((ushort)i) != null)
					canPlace = getTool((ushort)i).IsPlaceable;
                Packet.WriteBlockPermission((BlockID)i, i != 0 ? InventoryHasEnoughBlock(p, (ushort)i) && canPlace : true, i == 0 ? true : false, p.Session.hasExtBlocks, bulk, i * size);
            }
            p.Send(bulk);
		}
		private static void SendMiningUnbreakableMessage(Player p, BlockID id)
		{
			if (!Config.MiningEnabled)
				return;
			if (p.level.Config.MOTD.ToLower().Contains("-inventory")) return;
			bool extBlocks = p.Session.hasExtBlocks;
            int count = 1;//p.Session.MaxRawBlock + 1;
            int size  = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * size];
			bool canPlace = true;// ( id < p.group.CanPlace.Length && p.group.CanPlace[id]);
			if (id > 256)
				id = (ushort)(id - 256);
			if (getTool((ushort)id) != null)
				canPlace = getTool((ushort)id).IsPlaceable;
            Packet.WriteBlockPermission((BlockID)id, id != 0 ? InventoryHasEnoughBlock(p, (ushort)id) && canPlace : true, id == 0 ? true : false, p.Session.hasExtBlocks, bulk, 0);
            p.Send(bulk);
		}
		private void MineBlock(Player pl, ushort[] pos)
		{
			ushort blockType = pl.level.GetBlock(pos[0], pos[1], pos[2]);
			ushort oldBlockType = blockType;
			if (blockType == 0 || blockType == 0xff)
				return;
			BlockMineConfig blockMineData = getBlockMineTime(blockType);
			if (blockMineData == null)
				return;
			if (inSafeZone(pl, pos[0], pos[1], pos[2]))
				return;
			if (!playerMiningProgress.ContainsKey(pl))
			{
				playerMiningProgress.Add(pl, new MiningProgress(blockType, pos));
				setMineIndicator(pl, pos,0);
			}
			
			var currentProgress = playerMiningProgress[pl];
			if (DateTime.Now.Subtract(currentProgress.LastMine).TotalMilliseconds < 210) // avoid fast mining cheat
				return;
			if (currentProgress.BlockType != blockType || (pos[0] != currentProgress.Position[0]|| pos[1] !=  currentProgress.Position[1] || pos[2] != currentProgress.Position[2]))
			{
				//destroyMineIndicator(p);
				playerMiningProgress[pl] = new MiningProgress(blockType, pos);
				setMineIndicator(pl, pos,0);
			}
			float miningBonus = 1f;
			ushort heldBlock = pl.GetHeldBlock();
			SurvivalTool tool = getTool(pl, heldBlock);
			if (tool != null) miningBonus = tool.MiningBonus;
			if (tool != null && tool.IsPickaxe)
				miningBonus *= blockMineData.PickaxeTimeMultiplier;
			if (tool != null && tool.IsShovel)
				miningBonus *= blockMineData.ShovelTimeMultiplier;
			if (tool != null && tool.IsAxe)
				miningBonus *= blockMineData.AxeTimeMultiplier;
			if (tool != null && tool.IsSword)
				miningBonus *= 0.5f;
			playerMiningProgress[pl].Progress += (ushort)(1 * miningBonus);
			playerMiningProgress[pl].LastMine = DateTime.Now;
			ushort amount =  (ushort)Math.Min(9, (int) (( (float)playerMiningProgress[pl].Progress / (float)blockMineData.MiningTime) * 9));
			if (amount != playerMiningProgress[pl].MiningIndicatorProgress)
			{
				playerMiningProgress[pl].MiningIndicatorProgress = amount;
				setMineIndicator(pl, pos,amount);
			}
			spawnMineParticles(pl, pos, blockMineData.breakEffect);
			if (playerMiningProgress[pl].Progress < blockMineData.MiningTime)
				return;
			playerMiningProgress.Remove(pl);
			destroyMineIndicator(pl);
			pl.level.UpdateBlock(pl, pos[0], pos[1], pos[2], 0);
			OnBlockChangedEvent.Call(pl, pos[0], pos[1], pos[2], ChangeResult.Modified);
			if (blockMineData.overrideBlock == 0)
				return;
			if (blockMineData.RequirePickaxe && (tool == null || !tool.IsPickaxe))
				return;
			if (blockMineData.RequiredToolTier > 0 && (tool == null || tool.ToolTier < blockMineData.RequiredToolTier))
				return;
			if (blockMineData.overrideBlock != -1)
			{
				blockType = (ushort)blockMineData.overrideBlock;
				//oldBlockType = blockType;
				if (blockType > 256)
					blockType = (ushort)(blockType -256);
			}
			if (blockMineData.LootChance < 1f && new System.Random().NextDouble() > blockMineData.LootChance)
			{
				if (blockMineData.defaultToBlock)
					blockType = oldBlockType;
				else
					return;
			}
			InventoryAddBlocks(pl, blockType, 1);
			if ( true || InventoryGetBlockAmount(pl, blockType) == 1)
			{
				SetHeldBlock(pl, 0);
				SetHeldBlock(pl, blockType);
				if (heldBlock != 0)
					SetHeldBlock(pl, heldBlock);
			}
		}
		private void UnMineBlock(Player pl)
		{
			if (!playerMiningProgress.ContainsKey(pl))
				return;
			if (!playerMiningProgress[pl].IsExpired())
				return;
			playerMiningProgress.Remove(pl);
			destroyMineIndicator(pl);
		}
		private void destroyMineIndicator(Player p)
		{
			if (!mineProgressIndicators.ContainsKey(p))
				return;
			if (mineProgressIndicators[p] != null)
				PlayerBot.Remove(mineProgressIndicators[p]);
			mineProgressIndicators.Remove(p);
		}
		private void createMineIndicator(Player p, Position pos)
		{
			if (mineProgressIndicators.ContainsKey(p))
				destroyMineIndicator(p);
			
			string uniqueName = p.name + "_miningIndicator";
			PlayerBot bot = new PlayerBot(uniqueName, p.level);
			bot.DisplayName = "";
			bot.Model = "break0|1.005";
			bot.SetInitialPos(pos);
			
			PlayerBot.Add(bot);
			mineProgressIndicators.Add(p, bot);
		}
		private static void defineEffect(Player pl, byte id, GoodlyEffects.EffectConfig effect)
		{
			// Spawn break particle
			//pl.Send(Packet.DefineEffect(ID, 0, 105, 15, 120, 255, 255, 255, 10, 1, 28, 0, 0, 0, 0, 10f, 0, true, true, true, true, true));
			effect.ID = id;
			GoodlyEffects.DefineEffect(pl, effect); //new GoodlyEffects.EffectConfig(){ID = ID});
		}
		private static void defineEffect(Player pl, GoodlyEffects.EffectConfig effect)
		{
			GoodlyEffects.DefineEffect(pl, effect);
		}
		static byte currentEffectId = 100;
		private static void spawnEffect(Level lvl, GoodlyEffects.EffectConfig effect, float[] pos, float[] origin, bool define=true)
		{

			byte ID = effect.ID;
			if (define)
			{
				currentEffectId += 1;
				if (currentEffectId > 254)
					currentEffectId = 100;
				ID = currentEffectId;
			}
			Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players) {
                if (p.level != lvl || !p.Supports(CpeExt.CustomParticles)) { continue; }
				if ((p.Pos.ToVec3F32() -  new Vec3F32(pos[0], pos[1], pos[2])).LengthSquared > (2250)){continue;}
				if (define)
					defineEffect(p, ID, effect);
                p.Send(Packet.SpawnEffect(ID, pos[0], pos[1], pos[2], origin[0], origin[1], origin[2]));
            }
		}
		private static void spawnEffect(Level lvl, GoodlyEffects.EffectConfig effect, float[] pos, bool define=true)
		{
			spawnEffect(lvl, effect, pos, pos, define);
		}
		private static void spawnMineParticles(Player pl, ushort[] pos, GoodlyEffects.EffectConfig effect)
		{
			if (!Config.UseGoodlyEffects)
				return;
			float px = (float) pos[0], py =(float)pos[1] + 0.5f, pz =(float) pos[2]; //Convert.ToSingle(pos[0]), py = Convert.ToSingle(pos[1]), pz = Convert.ToSingle(pos[2]);
			px += 0.5f;
            pz += 0.5f;
			spawnEffect(pl.level, effect, new float[3]{px, py, pz});
		}
		private static void spawnHurtParticles(Player pl)
		{
			if (!Config.UseGoodlyEffects)
				return;
			spawnEffect(pl.level, pvpParticleEffect, new float[3]{(float)pl.Pos.BlockX, (float)pl.Pos.BlockY + 0.5f, (float)pl.Pos.BlockZ});
		}
		private static  void spawnHurtParticles(PlayerBot pl)
		{
			if (!Config.UseGoodlyEffects)
				return;
			spawnEffect(pl.level, pvpParticleEffect, new float[3]{(float)pl.Pos.BlockX, (float)pl.Pos.BlockY, (float)pl.Pos.BlockZ});
		}
		private void setMineIndicator(Player pl, ushort[] pos, ushort amount)
		{
			Position indicatorPosition =  Position.FromFeet((int)(pos[0]*32) +16, (int)(pos[1]*32), (int)(pos[2]*32) +16); //new Position(pos[0] << 5, pos[1] << 5, pos[2] << 5);
			if (!mineProgressIndicators.ContainsKey(pl))
				createMineIndicator(pl, indicatorPosition);
			if (mineProgressIndicators[pl].Pos != indicatorPosition)
				mineProgressIndicators[pl].Pos=indicatorPosition;
			string newModel = /*"break"*/ (Config.MineBlockIndicatorStart + amount).ToString() + "|1.01";
			if (newModel == mineProgressIndicators[pl].Model)
				return;
			mineProgressIndicators[pl].UpdateModel(newModel);
		}
		private void addBreakBlocks()
		{
			for (ushort i =0; i < 10; i++)
			{
				AddBlockDef("break" + i.ToString(), (ushort)(Config.MineBlockIndicatorStart + i), 0,0,0,16,16,16, (ushort)(240 + i), true);
			}
			// AddBlockItem(ushort Id, string Name, ushort Texture, bool admin=false)
			AddBlockItem((ushort)256, "", 85, true );
		}
		public void AddBlockDef(string name, ushort Id, ushort MinX, ushort MinY, ushort MinZ, ushort MaxX, ushort MaxY, ushort MaxZ, ushort TEXTURE, bool Transperant=true, int Brightness=0)
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
				ushort LeftTex = TEXTURE;
				ushort RightTex = TEXTURE;
				ushort FrontTex = TEXTURE;
				ushort BackTex = TEXTURE;
				ushort TopTex = TEXTURE;
				ushort BottomTex = TEXTURE;
				int InventoryOrder = 0;
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
				def.UseLampBrightness = false;
				def.Brightness = Brightness;
				BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null );
				
				ushort block = Id;
				BlockPerms perms = BlockPerms.GetPlace((ushort)(block + 256));
				perms.MinRank = LevelPermission.Nobody;
				BlockPerms.Save();
				BlockPerms.ApplyChanges();

				if (!Block.IsPhysicsType(block)) {
					BlockPerms.ResendAllBlockPermissions();
				}            
		}
		///////////////////////////////////////////////////////////
		// Handlers
		///////////////////////////////////////////////////////////
		void HandlePlayerDisconnect(Player p, string reason)
        {
			SaveInventory(p, p.level.name);
			destroyMineIndicator(p);
			if (playerMiningProgress.ContainsKey(p))
				playerMiningProgress.Remove(p);
        }
		void HandleGUI(SchedulerTask task)
        {
            guiTask = task;

            foreach (Player pl in PlayerInfo.Online.Items)
            {
				SendPlayerGui(pl);
			}
		}
		static void SendInventory(Player p)
		{
			if (!Config.InventoryEnabled)
				return;
			if (p.level.Config.MOTD.ToLower().Contains("-inventory")) return;
			ushort x=1;
			for (ushort i=0; i < 767; i++)
			{
				if (InventoryGetBlockAmountRaw(p, i) < 1)
				{
					p.Send(Packet.SetInventoryOrder(Block.Air, (BlockID)i, p.Session.hasExtBlocks));
					continue;
				}
				p.Send(Packet.SetInventoryOrder((BlockID)Convert.ToUInt16(i), (BlockID)x, p.Session.hasExtBlocks));
				x++;
			}
		}
		void HandlePlayerDying(Player p, BlockID deathblock, ref bool cancel)
        {
			
			if (!maplist.Contains(p.level.name))
			{
				SetGuiText(p,"","");
				p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
				p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
				return;
			}
			if (deathblock == Block.Orange)
			{
				string deathMessage = p.color +  p.name + " %e was killed by a monster.";// + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			else if (deathblock == 54)
			{
				string deathMessage = p.color +  p.name + " %e burned to death.";// + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			else if (deathblock == 46)
			{
				string deathMessage = p.color +  p.name + " %e blew up!";// + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			InitPlayer(p);
        }
		void HandleSentMap( Player p, Level prevLevel, Level level)
		{
			if (prevLevel != null && maplist.Contains(prevLevel.name))
				SaveInventory(p, prevLevel.name);
			if (!maplist.Contains(level.name))
			{
				SetGuiText(p,"","");
				p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
				p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
				p.Extras["MINING_LASTPROG"] = 0;
				return;
			}
			defineEffects(p);
			LoadInventory(p);
			ClearHotbar(p);
			InitPlayer(p);
			p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
			p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
			p.Extras["MINING_LASTPROG"] = 0;
			SendPlayerGui(p);
			SendMiningUnbreakableMessage(p);
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (p == Player.Console)
				return;
			if (cancel)
				return;
			if (!maplist.Contains(p.level.name))
				return;
			var tool = getTool(block);
			if (tool != null && !tool.IsSprite)
			{
				cancel = true;
				p.RevertBlock(x,y,z);
				return;
			}
			if (p.level.Config.MOTD.ToLower().Contains("-inventory")) return;
			if ((placing == false || block == 0) && Config.MiningEnabled)
			{
				cancel = true;
				p.RevertBlock(x,y,z);
				return;
			}
			if (cancel || !placing)
				return;
			if (!Config.InventoryEnabled)
				return;
			if (!InventoryHasEnoughBlock(p, block))
			{
				cancel = true;
				p.RevertBlock(x,y,z);
				return;
			}
			InventoryRemoveBlocks(p, block, 1);
			SendMiningUnbreakableMessage(p, block);
			SendInventory(p);
			handleTreePlace(p.level, x,y,z,block, ref cancel);
		}
		void handleTreePlace(Level lvl, ushort x, ushort y, ushort z, BlockID block, ref bool cancel)
		{
			if (block != 6)
				return;
			ushort ny = FindGround(lvl, x, lvl.Height, z);
			if (ny > y+1)
				return;
			cancel = true;
			Player.Console.Message("Spawning tree at " + x.ToString());
			var tree = new ClassicTree();
			tree.SetData(new System.Random(), 6);
			tree.Generate((ushort)x, (ushort)(y), (ushort)z, (X, Y, Z, raw) =>
            {
                BlockID here = lvl.GetBlock(X, Y, Z);
                if (here == Block.Air || here == Block.Leaves || (ushort)here == (ushort)6)
                {
                    lvl.SetTile(X, Y, Z, (byte)raw);
                    lvl.BroadcastChange(X, Y, Z, raw);
                }
            });
		}
		void HandleDrown(SchedulerTask task)
		{
			drownTask = task;
            foreach (Player p in PlayerInfo.Online.Items)
			{
				if (!maplist.Contains(p.level.name)) continue;
				UnMineBlock(p);
				if (p.invincible) continue;
				if (IsDrowning(p))
				{
					if (GetAir(p) > 0)
					{
						SetAir(p, GetAir(p)-1);
						SendPlayerGui(p);
					}
					else
					{
						Damage(p, 1, 8); 
					}
				}
				else if (GetAir(p) < Config.MaxAir)
				{
					SetAir(p, GetAir(p)+1);
				}
				
				if (IsBurning(p))
				{
					Damage(p, 3, 54); 
				}
				
			}
		}
		void HandlePlayerMove(Player p, Position next, byte rotX, byte rotY, ref bool cancel)
		{
			if (!maplist.Contains(p.level.name)) return;
			if (Config.VoidKills && next.Y < 0) Die(p, 4); // Player fell out of the world
			
			if (Config.FallDamage)
			{
				if (p.invincible) return;// || Hacks.CanUseFly(p)) return;

				ushort x = (ushort)(p.Pos.X / 32);
				ushort y = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) - 1);
				//ushort y2 = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) - 2);
				ushort z = (ushort)(p.Pos.Z / 32);

				BlockID block = p.level.GetBlock((ushort)x, ((ushort)y), (ushort)z);
				//BlockID block2 = p.level.GetBlock((ushort)x, ((ushort)y2), (ushort)z);

				string below = Block.GetName(p, block);
				//string below2 = Block.GetName(p, block2);

			
				if (below.ToLower().Contains("water"))// && below2.ToLower().Contains("water"))
				{
					int fall = p.Extras.GetInt("FALL_START") - y;
					//if (fallDamage(fall) > 0 && p.Session.ClientName().CaselessContains("cef")) p.Message("cef resume -n splash"); // Play splash sound effect
					p.Extras["FALLING"] = false;
					p.Extras["FALL_START"] = y;
					return;
				}

				if (!p.Extras.GetBoolean("FALLING") && below.ToLower() == "air")
				{
					p.Extras["FALLING"] = true;
					p.Extras["FALL_START"] = y;
				}

				else if (p.Extras.GetBoolean("FALLING") && below.ToLower() != "air")
				{
					if (p.Extras.GetBoolean("FALLING"))
					{
						int fall = p.Extras.GetInt("FALL_START") - y;

						if (fallDamage(fall) > 0){
						Damage(p, fallDamage(fall), 0);
						}

						// Reset extra variables
						p.Extras["FALLING"] = false;
						p.Extras["FALL_START"] = y;
					}
				}
			}
            
		}
        void HandlePlayerConnect(Player p)
		{
			if (!maplist.Contains(p.level.name)) return;
			LoadInventory(p);
			InitPlayer(p);
			SendMiningUnbreakableMessage(p);
			ClearHotbar(p);
			defineEffects(p);
		}

		int GetLagCompensation(int ping)
		{
			int penalty = 0;

			if (ping == 0) penalty = 0; // "lagged-out"
			if (ping > 0 && ping <= 29) penalty = 50; // "great"
			if (ping > 29 && ping <= 59) penalty = 100; // "good"
			if (ping > 59 && ping <= 119) penalty = 150; // "okay"
			if (ping > 119 && ping <= 180) penalty = 200; // "bad"
			if (ping > 180) penalty = 250; // "horrible"
			return penalty;
		}
		static Random rnd = new Random();
		PlayerBot[] GetMobsInLevel(Level lvl)
		{
			List<PlayerBot> players = new List<PlayerBot>();
			foreach (PlayerBot bot in lvl.Bots.Items)
			{
				if (!bot.name.Contains("ssmob"))
				{
					continue;
				}
				players.Add(bot);
			}
			return players.ToArray();
		}
		void DespawnAllBots()
		{
			Level[] levels = LevelInfo.Loaded.Items;
			foreach (Level level in levels)
			{
				if (!maplist.Contains(level.name))
				{
					continue;
				}
				foreach (PlayerBot bot in level.Bots.Items)
				{
					if (!bot.name.Contains("ssmob"))
					{
						continue;
					}
					PlayerBot.Remove(bot);
				}
			}
		}
		void CheckDespawn(Level level)
		{
			foreach (PlayerBot bot in level.Bots.Items)
			{
				if (!bot.name.Contains("ssmob"))
				{
					continue;
				}
				if (GetPlayersInLevel(level).Length < 1)
				{
					PlayerBot.Remove(bot);
					continue;
				}
				if (bot.Pos.BlockX >= level.Length || bot.Pos.BlockY < 0 || bot.Pos.BlockY+1 > level.Height || bot.Pos.BlockZ >= level.Width ||  bot.Pos.BlockX < 0 || bot.Pos.BlockZ < 0)
				{
					PlayerBot.Remove(bot);
					return;
				}
			
				BlockID gb = level.FastGetBlock((ushort)bot.Pos.BlockX, (ushort)(bot.Pos.BlockY+1), (ushort)bot.Pos.BlockZ);
				if (gb == 9 || gb == 8 || gb == 10 || gb == 11) // water, lava etc
				{
					PlayerBot.Remove(bot);
					continue;
				}
				int shortestDist = 650;
				foreach (Player p in GetPlayersInLevel(level)) // Don't want creepers spawning inside us now do we
				{
					int x = bot.Pos.BlockX;
					int y = bot.Pos.BlockY;
					int z = bot.Pos.BlockZ;
					int dx = (int)(p.Pos.BlockX) - x, dy = (int)(p.Pos.BlockY) - y, dz = (int)(p.Pos.BlockZ) -z;
					int playerDist = Math.Abs(dx) /*+ Math.Abs(dy)*/ + Math.Abs(dz);
					if (playerDist < shortestDist)
					{
						shortestDist = playerDist;
					}
				}
				if (shortestDist >= 210)
				{
					PlayerBot.Remove(bot);
				}
			}
		}
		Player[] GetPlayersInLevel(Level lvl)
		{
			List<Player> players = new List<Player>();
			foreach (Player p in PlayerInfo.Online.Items)
			{
				if (p.level == lvl)
				{
					players.Add(p);
				}
			}
			return players.ToArray();
		}
		void HandleMobSpawning(SchedulerTask task)
		{
			mobSpawningTask = task;
			if (!Config.SpawnMobs)
			{
				return;
			}
			Level[] levels = LevelInfo.Loaded.Items;
			if (PlayerInfo.Online.Items.Length < 1)
			{
				foreach (Level lvl in levels)
				{
					if (!maplist.Contains(lvl.name))
					{
						continue;
					}
					CheckDespawn(lvl);
				}
				return;
			}
			
			
			
			
			
			foreach (Level lvl in levels)
			{
				if (!maplist.Contains(lvl.name))
				{
					continue;
				}
				CheckDespawn(lvl);
				Player[] players = GetPlayersInLevel(lvl);
				if (players.Length < 1)
					continue;
				if (GetMobsInLevel(lvl).Length >= (Config.MaxMobs))
					continue;
			

				Player selectedPlayer = players[rnd.Next(players.Length)] ;
				if (selectedPlayer == null)
				{
					continue;
				}

				ushort x = (ushort)(	selectedPlayer.Pos.BlockX + (	rnd.Next(25, 128) * ((rnd.Next(2) == 1 ? 1 : -1)))	);
				ushort z = (ushort)(	selectedPlayer.Pos.BlockZ + (	rnd.Next(25, 128) * ((rnd.Next(2) == 1 ? 1 : -1)))	);
				if (x >= lvl.Width)
				{
					x = (ushort)(lvl.Width-1);
				}
				if (z >= lvl.Length)
				{
					z = (ushort)(lvl.Length-1);
				}
				if (x < 0)
				{
					x = 0;
				}
				if (z < 0)
				{
					z = 0;
				}
				ushort y = FindGround(lvl, x, lvl.Height, z);
				if (y < 0)
				{
					y = 0;
				}
				if (y>1)
				{
					BlockID gb = lvl.FastGetBlock(x, (ushort)(y-1), z);
					if (gb == 9 || gb == 8 || gb == 10 || gb == 11) // water, lava etc
					{
						continue;
					}
				}
				switch (rnd.Next(12))
				{
					case 1:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					case 2:
						SpawnEntity(lvl, "pig", "roam", x, y, z);
						break;
					case 3:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
					case 4:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					case 5:
						SpawnEntity(lvl, "zombie", "hostile", x, y, z);
						break;
					case 6:
						SpawnEntity(lvl, "skeleton", "hostile", x, y, z);
						break;
					case 7:
						SpawnEntity(lvl, "spider", "hostile", x, y, z);
						break;
					case 8: // Creeper? aww man
						SpawnEntity(lvl, "creeper", "hostile", x, y, z); // So we back in the mine 
						break; // Swinging that pickaxe side to side, side side to side // HEADS UP TOTAL SHOCK FILLS YOUR BODY // OH NO ITS YOU AGAIN
					case 9:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
					case 10:
						SpawnEntity(lvl, "pig", "roam", x, y, z);
						break;
					case 11:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					default:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
				}
			}
		}
		static bool CanHitPlayer(Player p, Player victim)
        {
            Vec3F32 delta = p.Pos.ToVec3F32() - victim.Pos.ToVec3F32();
            float reachSq = 12f; // 3.46410161514 block reach distance

            int ping = p.Session.Ping.AveragePing();

            if (ping > 59 && ping <= 119) reachSq = 16f; // "okay"
            if (ping > 119 && ping <= 180) reachSq = 16f; // "bad"
            if (ping > 180) reachSq = 16f; // "horrible"

            // Don't allow clicking on players further away than their reach distance
            if (delta.LengthSquared > (reachSq + 1)) return false;

            // Check if they can kill players, determined by gamemode plugins
            //bool canKill = PvP.Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
            //if (!canKill) return false;

            if (p.Game.Referee || victim.Game.Referee || p.invincible || victim.invincible) return false; // Ref or invincible
            if (inSafeZone(p, p.level) || inSafeZone(victim, victim.level)) return false; // Either player is in a safezone

            if (!string.IsNullOrWhiteSpace(p.Extras.GetString("TEAM")) && (p.Extras.GetString("TEAM") == victim.Extras.GetString("TEAM")))
            {
                return false; // Players are on the same team
            }

            BlockID b = p.GetHeldBlock();

            if (Block.GetName(p, b).ToLower() == "bow") return false; // Bow damage comes from arrows, not player click

            // If all checks are complete, return true to allow knockback and damage
            return true;
        }
		static bool CanHitMob(Player p, PlayerBot victim)
        {
			if (!victim.name.Contains("mob"))
				return false;
            Vec3F32 delta = p.Pos.ToVec3F32() - victim.Pos.ToVec3F32();
            float reachSq = 12f; // 3.46410161514 block reach distance

            int ping = p.Session.Ping.AveragePing();

            if (ping > 100) reachSq = 40f; // "horrible"

            // Don't allow clicking on players further away than their reach distance
            if (delta.LengthSquared > (reachSq + 1)) return false;

            // Check if they can kill players, determined by gamemode plugins
            //bool canKill = PvP.Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
            //if (!canKill) return false;

            if (p.Game.Referee || p.invincible ) return false; // Ref or invincible
            return true;
        }
		Dictionary<PlayerBot, int> mobHealth = new Dictionary<PlayerBot, int>();
		
		
		static SurvivalTool getTool(ushort blockId)
		{
			if (blockId > 256)
				blockId = (ushort)(blockId-256);
			foreach (var tool in customTools)
			{
				if (tool.ID == blockId)
					return tool;
			}
			return null;
		}
		static SurvivalTool getTool(Player p, ushort blockId)
		{
			if (!InventoryHasEnoughBlock(p, blockId))
				return null;
			return getTool(blockId);
		}
		static ushort getDamage(Player p)
		{
			ushort block = p.GetHeldBlock();
			if (block >= 66)
				block = (ushort)(block - 256);
			if (!InventoryHasEnoughBlock(p, block))
				return 2;
			SurvivalTool tool = getTool(block);
			if (tool == null)
				return 2;
			return (ushort)tool.Damage;
		}

		static float getKnockback(Player p)
		{
			ushort block = p.GetHeldBlock();
			if (block >= 66)
				block = (ushort)(block - 256);
			if (!InventoryHasEnoughBlock(p, block))
				return 0.5f;
			SurvivalTool tool = getTool(block);
			if (tool == null)
				return 0.5f;
			return tool.Knockback;
		}
		void HandleAttackMob (Player p, byte entity)
		{
			if (!Config.CanKillMobs)
			{
				return;
			}
			PlayerBot mob = null;
			foreach( PlayerBot b in p.level.Bots.Items)
			{
				if (b.EntityID == entity)
				{
					mob = b;
					break;
				}
			}
			if (mob == null)
			{
				return;
			}
			if (!CanHitMob(p, mob))
			{
				return;
			}
			HurtBot(getDamage(p), mob, p);
		}
	    public static void HurtBot(int damage, PlayerBot hit, Player bot)
        {
            int srcHeight = ModelInfo.CalcEyeHeight(hit);
            int dstHeight = ModelInfo.CalcEyeHeight(bot);
            int dx = bot.Pos.X - hit.Pos.X, dy = (bot.Pos.Y + srcHeight) - (hit.Pos.Y + dstHeight), dz = bot.Pos.Z - hit.Pos.Z;
            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);

            float mult = 1 / ModelInfo.GetRawScale(hit.Model);
            float plScale = ModelInfo.GetRawScale(hit.Model);

            Position newPos;
            newPos.X = hit.Pos.X + (int)(hit.Pos.X - bot.Pos.X);
            newPos.Y = hit.Pos.Y;// + 32;
            newPos.Z = hit.Pos.Z + (int)(hit.Pos.Z - bot.Pos.Z);

            Position newMidPos;
            newMidPos.X = hit.Pos.X + (int)((hit.Pos.X - bot.Pos.X) / 2);
            newMidPos.Y = hit.Pos.Y;// + 32;
            newMidPos.Z = hit.Pos.Z + (int)((hit.Pos.Z - bot.Pos.Z) / 2);

            if (hit.level.IsAirAt((ushort)newPos.BlockX, (ushort)newPos.BlockY, (ushort)newPos.BlockZ) && hit.level.IsAirAt((ushort)newPos.BlockX, (ushort)(newPos.BlockY - 1), (ushort)newPos.BlockZ) &&
            hit.level.IsAirAt((ushort)newMidPos.BlockX, (ushort)newMidPos.BlockY, (ushort)newMidPos.BlockZ) && hit.level.IsAirAt((ushort)newMidPos.BlockX, (ushort)(newMidPos.BlockY - 1), (ushort)newMidPos.BlockZ))
            {
                hit.Pos = new Position(newPos.X, newPos.Y, newPos.Z);
            }

            int hp;
            bool isNumber = int.TryParse(hit.Owner, out hp);
            if (hit.Owner == null || !isNumber) return;

            hit.Owner = (hp - damage).ToString();
			spawnHurtParticles(hit);
            if (hp <= 0)
            {
                // Despawn bot
                //Command.Find("Effect").Use(p, "smoke " + bot.Pos.FeetBlockCoords.X + " " + bot.Pos.FeetBlockCoords.Y + " " + bot.Pos.FeetBlockCoords.Z + " 0 0 0 true");
				if (mobLoot.ContainsKey(hit.Model))
				{
					foreach(var lootbox in mobLoot[hit.Model])
					{
						InventoryAddBlocks(bot, lootbox.Key, lootbox.Value);
					}
				}
                PlayerBot.Remove(hit);
				
            }
        }
		bool AttemptPvp(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (!maplist.Contains(p.level.name)) return false;;
			if (button != MouseButton.Left) return false;
			//if (action != MouseAction.Released) return;
			Player victim = null; // If not null, the player that is being hit

			Player[] players = PlayerInfo.Online.Items;

			foreach (Player pl in players)
			{
				// Clicked on a player

				if (pl.EntityID == entity)
				{
					victim = pl;
					break;
				}
			}
			
			if (victim == null)
			{
				p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow.AddMilliseconds(550 - GetLagCompensation(p.Session.Ping.AveragePing()));
				try
				{
				HandleAttackMob(p, entity);
				}
				catch
				{
				}
				return false;
			}
			if (!p.Extras.Contains("PVP_HIT_COOLDOWN"))
			{
				p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow;
			}
			DateTime lastClickTime = (DateTime)p.Extras.Get("PVP_HIT_COOLDOWN");

			if (lastClickTime > DateTime.UtcNow) return false;
			
			if (!CanHitPlayer(p, victim)) return false;
			DoHit(p, victim);
			p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow.AddMilliseconds(400 - GetLagCompensation(p.Session.Ping.AveragePing()));
			return true;
		}
		void toolUse(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (button != MouseButton.Right)
				return;
			if (action != MouseAction.Pressed)
				return;
			ushort block = p.GetHeldBlock();
			if (block >= 66)
				block = (ushort)(block - 256);
			if (!InventoryHasEnoughBlock(p, block))
				return;
			SurvivalTool tool = getTool(block);
			if (tool == null)
				return;
			if (tool.IsFlintAndSteel)
			{
				FlintSteelTool.Lightblock(p, button, action, yaw, pitch, entity, x , y, z, face);
				return;
			}
			tool.HandleBlockClicked(p, button, action, yaw, pitch, entity, x , y, z, face);
		}
		void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (!maplist.Contains(p.level.name)) return;
			if (entity != 255 && AttemptPvp(p, button, action, yaw, pitch, entity, x, y, z, face))
				return;
			if (p.level.Config.MOTD.ToLower().Contains("-inventory")) return;
			if (Config.MiningEnabled && button == MouseButton.Left)
				MineBlock(p, new ushort[]{x, y, z});
			toolUse(p, button, action, yaw, pitch, entity, x, y, z, face);
			
		}
		void HandleRegeneration(SchedulerTask task)
        {
            regenTask = task;
            foreach (Player pl in PlayerInfo.Online.Items)
            {
				if (!maplist.Contains(pl.level.name)) continue;
                int health = GetHealth(pl);

                if (health >= Config.MaxHealth) continue; // No need to regenerate health if player is already at max health

                pl.Extras["SURVIVAL_HEALTH"] = health + 1;
            }
        }
		///////////////////////////////////////////////////////////////////////////
		// GUI
		///////////////////////////////////////////////////////////////////////////
		static string GetHealthBar(int health)
		{
			if (health < 0)
			{
				health = 0;
			}

			int repeatHealth = (int)Math.Round(( (float) health / (float)Config.MaxHealth) * 10f);

			int repeatDepletedHealth = 10 - repeatHealth;


			return ("%f" + new string('♥', repeatHealth )) + "%0" + new string('♥', repeatDepletedHealth ) ;
		}
		static string GetAirBar(int air)
		{
			if (air < 0)
			{
				air = 0;
			}
			if (air >= Config.MaxAir)
			{
				return "";
			}
			int repeat = air; //(int)Math.Round((double)(air/Config.MaxAir) * 10);
			///help emotes 9○ &e- (circle), (o)
			return ("%3" + new string('○', air)+ "%8" + new string('○', Config.MaxAir-air ));
		}
		static string getHeldBlockAmount(Player p)
		{
			ushort block = p.GetHeldBlock();
			if (block <= 0)
				return "";
			ushort amount = InventoryGetBlockAmount(p, block);
			if (amount == 0 && p.level.Config.MOTD.ToLower().Contains("-inventory"))
				return "";
			/*if (block != 256 && amount == 0)
			{
				SetHeldBlock(p, (ushort)(256+256));
				SetHeldBlock(p, (ushort)(5));
				return "";
			}*/
			return amount.ToString();
		}
		static void SendPlayerGui(Player p)
		{
			if (!maplist.Contains(p.level.name)) return;
			SetGuiText(p, GetHealthBar	(GetHealth	(p)),GetAirBar		(GetAir		(p)), getHeldBlockAmount(p));
		}
		///////////////////////////////////////////////////////////////////////////
		// UTILITIES
		///////////////////////////////////////////////////////////////////////////
		int fallDamage(int height)
		{
			if (height < 4)
			{
				return 0;
			}
			return (height-4);
		}
		public static void SetHealth(Player p, int health)
		{
			p.Extras["SURVIVAL_HEALTH"] = health;
		}
		public static int GetHealth(Player p)
		{
			return p.Extras.GetInt("SURVIVAL_HEALTH");
		}
		public static int GetAir(Player p)
		{
			return p.Extras.GetInt("SURVIVAL_AIR");
		}
		public static void SetAir(Player p, int air)
		{
			p.Extras["SURVIVAL_AIR"] = air;
		}
		public static void SetHeldBlock(Player p, ushort blockId, bool locked=false)
		{
			 if (!p.Supports(CpeExt.HeldBlock))
			 	return;
			if (blockId > 65 && blockId < 256)
				blockId = (ushort)(blockId + 256);
			p.Session.SendHoldThis(blockId, locked);
		}
		public bool IsBurning(Player p)
		{
			bool burning = false;
			try
			{
				BlockID b = p.level.FastGetBlock((ushort)p.Pos.BlockX,(ushort)p.Pos.BlockY, (ushort)p.Pos.BlockZ);
				BlockID b2 = p.level.FastGetBlock((ushort)p.Pos.BlockX,(ushort)(p.Pos.BlockY - 1), (ushort)p.Pos.BlockZ);

				burning = (/*p.level.Props[b].KillerBlock &&*/ (b == 10 || b == 11 || b == 54)) || (/*p.level.Props[b2].KillerBlock && */ (b2 == 10 || b2 == 11 || b2 == 54));
			}
			catch
			{
			}
			return burning;
		}
		bool IsDrowning(Player p)
		{
			ushort x = (ushort)(p.Pos.X / 32);
			ushort y = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32));
			ushort z = (ushort)(p.Pos.Z / 32);
			bool drowning = false;
			try
			{
				BlockID bHead = p.level.FastGetBlock((ushort)x, (ushort)(y+1), (ushort)z);

				drowning = (p.level.FastGetBlock((ushort)x, (ushort)y, (ushort)z) != 0) && p.level.Props[bHead].Drownable;
			}
			catch
			{
				drowning = false;
			}
			return drowning;
		}
		public static void Damage(Player p, int amount, BlockID reason = 0)
		{
			SetHealth(p, GetHealth(p) - amount);
			if (GetHealth(p) <= 0)
			{
				// die
				Die(p, reason);
			}
			SendPlayerGui(p);
		}
		public static void Die(Player p, BlockID reason = 4)
		{
			p.HandleDeath(reason, immediate: true);	
			InitPlayer(p);
		}
		static void SetGuiText(Player p, string top, string bottom, string bottom2 = "")
		{
			if (!p.Extras.Contains("LAST_STATUS1"))
				return;
			if ((string)p.Extras["LAST_STATUS1"] != top)
			{
				p.Extras["LAST_STATUS1"] = top;
				p.SendCpeMessage(CpeMessageType.Status1, top);
			}
			if ((string)p.Extras["LAST_STATUS2"] != bottom)
			{
				p.Extras["LAST_STATUS2"] = bottom;
				p.SendCpeMessage(CpeMessageType.Status2, bottom);
			}
			if ((string)p.Extras["LAST_STATUS3"] != bottom2)
			{
				p.Extras["LAST_STATUS3"] = bottom2;
				p.SendCpeMessage(CpeMessageType.Status3, bottom2);
			}
		}
		public static void InitPlayer(Player p)
		{
	
			p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
			p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
			p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow;
			p.Extras["FALLING"] = false;
			p.Extras["FALL_START"] = p.Pos.Y;
			p.Extras["LAST_STATUS1"] = "NULL";
			p.Extras["LAST_STATUS2"] = "NULL";
			p.Extras["LAST_STATUS3"] = "NULL";
			SendPlayerGui(p);
			SendMiningUnbreakableMessage(p);
			SendInventory(p);
		}
		public static void ResetPlayerState(Player p)
        {
			p.Extras["LAST_STATUS1"] = "NULL";
			p.Extras["LAST_STATUS2"] = "NULL";
			p.Extras["LAST_STATUS3"] = "NULL";
			SetGuiText(p,"","");
            p.Extras["SURVIVAL_HEALTH"] = 10;
            p.Extras["SURVIVAL_AIR"] = 10;
        }
		void DoHit(Player p, Player victim)
		{
			PushPlayer(p, victim); // Knock the victim back
			int dmg = getDamage(p);
			if (GetHealth(victim)-dmg <= 0)
			{
				Die(victim, 4);
				string deathMessage = p.color +  p.name + " %ekilled " + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level || victim.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			Damage(victim, dmg, 4);
		}
		public static void SetHotbar(Player p, byte slot, ushort block)
		{
			byte[] buffer = Packet.SetHotbar(block, slot, p.Session.hasExtBlocks);
			p.Send(buffer);

		}

		private static void ClearHotbar(Player p)
		{
			for (byte i=0; i <9; i++)
			{
				SetHotbar(p, i, 0);
			}
		}
		static void PushPlayer(Player p, Player victim)
        {
            if (p.level.Config.MOTD.ToLower().Contains("-damage")) return;

            int srcHeight = ModelInfo.CalcEyeHeight(p);
            int dstHeight = ModelInfo.CalcEyeHeight(victim);
            int dx = p.Pos.X - victim.Pos.X, dy = (p.Pos.Y + srcHeight) - (victim.Pos.Y + dstHeight), dz = p.Pos.Z - victim.Pos.Z;

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);

            float mult = 1 / ModelInfo.GetRawScale(victim.Model);
			mult = mult * getKnockback(p);
            float victimScale = ModelInfo.GetRawScale(victim.Model);

            if (victim.Supports(CpeExt.VelocityControl) && p.Supports(CpeExt.VelocityControl))
            {
                // Intensity of force is in part determined by model scale
                victim.Send(Packet.VelocityControl((-dir.X * mult) * 0.5f, 0.87f * mult, (-dir.Z * mult) * 0.5f, 0, 1, 0));

                // If GoodlyEffects is enabled, show particles whenever a player is hit
                if (Config.UseGoodlyEffects)
                {
                    // Spawn effect when victim is hit
                    //Command.Find("Effect").Use(victim, Config.HitParticle + " " + (victim.Pos.X / 32) + " " + (victim.Pos.Y / 32) + " " + (victim.Pos.Z / 32) + " 0 0 0 true");
					spawnHurtParticles(victim);
                }
            }
            else
            {
                p.Message("You can left and right click on players to hit them if you update your client!");
            }
        }
		void AddAi( string ai, string[] args) {
			Player p = Player.Console;
			 if (File.Exists("bots/" + ai)) {
			File.Delete("bots/" + ai);
			 }
            if (!File.Exists("bots/" + ai)) {
                p.Message("Created new bot AI: &b" + ai);
                using (StreamWriter w = new StreamWriter("bots/" + ai)) {
                    // For backwards compatibility
                    w.WriteLine("#Version 2");
                }
            }

            string action = args.Length > 2 ? args[2] : "";
            string instruction = ScriptFile.Append(p, ai, action, args);
            if (instruction != null) {
                p.Message("Appended " + instruction + " instruction to bot AI &b" + ai);
            }
        }
		public void SpawnEntity(Level level, string model, string ai, ushort x, ushort y, ushort z, int health=10)
		{
			int uniqueMobId = level.Bots.Items.Length+1;
			string uniqueName = "ssmob" + uniqueMobId;
			PlayerBot bot = new PlayerBot(uniqueName, level);
			bot.DisplayName = "";
			bot.Model = model;
			bot.Owner = health.ToString();
			//+16 so that it's centered on the block instead of min corner
			Position pos = Position.FromFeet((int)(x*32) +16, (int)(y*32), (int)(z*32) +16);
			bot.SetInitialPos(pos);
			bot.AIName = ai;
			//HandleAdd(Player.Console,  uniqueName,  new string[] {"add", uniqueName, ai, "75"});
			 
			PlayerBot.Add(bot);
			ScriptFile.Parse(Player.Console, bot, ai);
			BotsFile.Save(level);
			
		}
		ushort FindGround(Level level,int x, int y, int z)
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
			for (int i = level.Height-1; i >= 0; i--)
			{
				if (level.FastGetBlock((ushort)x, (ushort)i, (ushort)z) != 0)
				{
					return (ushort)(i + 1);
				}
			}
			return (ushort)y;
			
		}
		///////////////////////////////////////////////////////////////////////////
		public static bool inSafeZone(Player p, ushort x, ushort y, ushort z)
		{
			Zone[] zones = p.level.Zones.Items;
			foreach(Zone zone in zones)
			{
				if (zone.Contains(x, y, z))
				{
					return true;
				}
			}
			return false;
		}
		public static bool inSafeZone(Player p, Level level)
        {
			Zone[] zones = level.Zones.Items;
			foreach(Zone zone in zones)
			{
				if (zone.Contains(p.Pos.BlockX, p.Pos.BlockY, p.Pos.BlockZ))
				{
					return true;
				}
			}
			return false;
        }
	}
	public sealed class CmdGiveBlock : Command2
    {
        public override string name { get { return "GiveBlock"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0 || message.SplitSpaces().Length < 2)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();
			int matches;
            Player who = PlayerInfo.FindMatches(p, args[0], out matches);
			if (who == null)
			{
				p.Message("Couldn't find player " + args[0]);
				return;
			}
			ushort blockId = 0;
			try
			{
				 blockId = ushort.Parse(args[1]);
			}
			catch(Exception e)
			{
				if (!CommandParser.GetBlock(p, args[1], out blockId))
				{
					p.Message("%cBlock %5\"" + args[1] + "\"%c doesn't exist!");
					return;
				}
				if (blockId > 256)
					blockId = (ushort)(blockId - 256);
			}
			ushort amount = 1;
			if (args.Length > 2)
			{
				try
				{
					amount = ushort.Parse(args[2]);
				}
				catch(Exception e)
				{
					Help(p);
					return;
				}
			}
			MCGalaxy.SimpleSurvival.InventoryAddBlocks(who, blockId, amount);
			MCGalaxy.SimpleSurvival.SetHeldBlock(who, blockId);
			p.Message("Gave " + who.name + " %5" + Block.GetName(p,blockId > 65 ? (ushort)(blockId + 256) : blockId) + "%a x" + (args.Length > 2 ? args[2].ToString() : "1"));
        }
		public override void Help(Player p)
        {
            p.Message("%T/GiveBlock <Player> <BlockId> <Amount=1>");
        }
	}


	public sealed class CmdTradeBlock : Command2
    {
        public override string name { get { return "TradeBlock"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }

		private static string safeGetBlockName(Player p ,ushort blockId)
		{
			try
			{
				return Block.GetName(p,blockId > 65 ? (ushort)(blockId + 256) : blockId);
			}
			catch(Exception e)
			{
				return "???";
			}
		}
        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0 || message.SplitSpaces().Length < 2)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();
			int matches;
            Player who = PlayerInfo.FindMatches(p, args[0], out matches);
			if (who == null)
			{
				p.Message("Couldn't find player " + args[0]);
				return;
			}
			if (who == p)
			{
				p.Message("Can't trade with yourself!");
				return;
			}
			if (p.level != who.level)
			{
				p.Message("You can only trade with people in the same level!");
				return;
			}
			ushort blockId = 0;
			try
			{
				 blockId = ushort.Parse(args[1]);
			}
			catch(Exception e)
			{
				if (!CommandParser.GetBlock(p, args[1], out blockId))
				{
					p.Message("%cBlock %5\"" + args[1] + "\"%c doesn't exist!");
					return;
				}
			}
			if (blockId > 256)
				blockId = (ushort)(blockId - 256);
			ushort amount = 1;
			if (args.Length > 2)
			{
				try
				{
					amount = ushort.Parse(args[2]);
				}
				catch(Exception e)
				{
					Help(p);
					return;
				}
			}
			if (amount == 0 || amount < 0)
			{
				p.Message("Can't give 0 blocks!");
				return;
			}
			if (!MCGalaxy.SimpleSurvival.InventoryHasEnoughBlock(p, blockId, amount))
			{
				p.Message("You don't have %5" + safeGetBlockName(p,blockId) + " %ax" + amount.ToString() + "%e!");
				return;
			}
			MCGalaxy.SimpleSurvival.InventoryRemoveBlocks(p, blockId, amount);
			MCGalaxy.SimpleSurvival.InventoryAddBlocks(who, blockId, amount);
			p.Message("Gave %2" + who.name + " %5" + safeGetBlockName(p,blockId) + "%a x" + (args.Length > 2 ? args[2].ToString() : "1"));
			who.Message("&2" + p.name + " &egave you %5"  + safeGetBlockName(p,blockId) + "%a x" + (args.Length > 2 ? args[2].ToString() : "1"));
        }
		public override void Help(Player p)
        {
            p.Message("%T/TradeBlock <Player> <BlockId> <Amount=1>");
        }
	}
	public sealed class CmdCraft : Command2
    {
        public override string name { get { return "Craft"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }

        public override void Use(Player p, string message, CommandData data)
        {
			if (p.level.Config.MOTD.ToLower().Contains("-inventory"))
			{
				p.Message("%cYou can't craft on an inventory disabled world!");
				return;
			}
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
			string[] args = message.SplitSpaces();
			if (args[0] == "list")
			{
				string blockList = MCGalaxy.SimpleSurvival.GenerateCraftOptionsMessage(p);
				string[] msgSplit = blockList.Split('\n');
				foreach(var a in msgSplit)
					p.Message(a);
				return;
			}
			ushort blockId = 0;
			try
			{
				 blockId = ushort.Parse(args[0]);
			}
			catch(Exception e)
			{
				
				if (!CommandParser.GetBlock(p, args[0], out blockId))
				{
					p.Message("%cBlock %5\"" + args[0] + "\"%c doesn't exist!");
					return;
				}
				if (blockId > 256)
					blockId = (ushort)(blockId - 256);
			}
			ushort amount = 1;
			if (args.Length > 1)
			{
				try
				{
					amount = ushort.Parse(args[1]);
				}
				catch(Exception e)
				{
					Help(p);
					return;
				}
			}
			MCGalaxy.SimpleSurvival.Craft(p, blockId, amount);
        }
		public override void Help(Player p)
        {
            p.Message("%T/Craft <BlockId> <Amount=1>");
			p.Message("%TType %5/Craft list%T to see a list of blocks you can craft!");
        }
	}
	public sealed class CmdPvP : Command2
    {
        public override string name { get { return "PvP"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage PvP") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args, data);
                    return;
                case "del":
                    HandleDelete(p, args, data);
                    return;
            }
        }
		void Save()
		{
			 using (StreamWriter maplistwriter =
                new StreamWriter(SimpleSurvival.Config.Path + "maps.txt"))
            {
                foreach (String s in SimpleSurvival.maplist)
                {
                    maplistwriter.WriteLine(s);
                }
            }
		}
        void HandleAdd(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to add.");
                return;
            }

            if (!HasExtraPerm(p, data.Rank, 1)) { p.Message("%cNo permission."); return; };

            string pvpMap = args[1];

            SimpleSurvival.maplist.Add(pvpMap);
			Save();
            p.Message("The map %b" + pvpMap + " %Shas been added to the PvP map list.");

            // Add the map to the map list
           

            Player[] players = PlayerInfo.Online.Items;

            foreach (Player pl in players)
            {
                if (pl.level.name.ToLower() == args[1].ToLower())
                {
                    SimpleSurvival.ResetPlayerState(pl);
					SimpleSurvival.InitPlayer(pl);
					SimpleSurvival.defineEffects(pl);
                }
            }
        }

        void HandleDelete(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to remove.");
                return;
            }

            if (!HasExtraPerm(p, data.Rank, 1)) return;

            string pvpMap = args[1];

            SimpleSurvival.maplist.Remove(pvpMap);
			Save();
			Player[] players = PlayerInfo.Online.Items;
			foreach (Player pl in players)
            {
                if (pl.level.name.ToLower() == pvpMap)
                {
                    SimpleSurvival.ResetPlayerState(pl);
                }
            }
            p.Message("The map %b" + pvpMap + " %Shas been removed from the PvP map list.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/PvP add <map> %H- Adds a map to the PvP map list.");
            p.Message("%T/PvP del <map> %H- Deletes a map from the PvP map list.");
        }
    }
}