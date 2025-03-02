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
	
	public class MikuPlushSpawnEggBlock
	{
		public ushort TEXTURE_ID;
		public BlockID BLOCK_ID;
		public string MODEL;
		public string NAME;
		public string SKIN;
	}
	public class MikuPlush : Plugin {
		public override string name { get { return "Miku_Plush"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		
		public ushort uniqueId = 0;
		
		public LevelPermission allowedRank { get { return LevelPermission.Guest; } }
		public List<MikuPlushSpawnEggBlock> EggConfigs = new List<MikuPlushSpawnEggBlock>(){
			new MikuPlushSpawnEggBlock()
			{
				TEXTURE_ID = 181,
				BLOCK_ID = 221,
				MODEL = "miku_plush",
				NAME = "Miku Plush",
				SKIN = "https://garbage.loan/f/morgana/miku-plush.png"
			},
		};

		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
			OnSentMapEvent.Register(HandleSentMap, Priority.Low);
			foreach (var egg in EggConfigs)
				AddBlockItem(egg.BLOCK_ID, egg.NAME, egg.TEXTURE_ID);
			/*
			foreach (PlayerBot bot in lvl.Bots.Items)
			{
				if (!bot.name.Contains("miku"))
					continue;
				bot.DisplayName = "";
			}*/

		}
		public static List<PlayerBot> GetSpawnedMikuPlushies(Level lvl)
		{
			List<PlayerBot> mikuplushies = new List<PlayerBot>();
			foreach (PlayerBot bot in lvl.Bots.Items)
			{
				if (!bot.name.Contains("miku"))
					continue;
				mikuplushies.Add(bot);
			}
			return mikuplushies;
		}	
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
			OnSentMapEvent.Unregister(HandleSentMap);
		}
		void HandleSentMap( Player p, Level prevLevel, Level level)
		{
			foreach( var miku in GetSpawnedMikuPlushies(level))
				miku.DisplayName = "";
		}
		public override void Help(Player p) {
			//HELP INFO!
		}
		public void AddBlock(BlockDefinition def)
		{
			BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null );
		}
		public void AddBlockItem(ushort Id, string Name, ushort Texture)
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
			AddBlock(def);
		}
		public void SpawnEntity(Level level, string model, string skin, ushort x, ushort y, ushort z, byte rot, int health=10)
		{
			uniqueId++;
			string uniqueName = model + uniqueId;
			PlayerBot bot = new PlayerBot(uniqueName, level);
			bot.DisplayName = "";
			bot.Model = model;
			bot.Owner = health.ToString();
			//+16 so that it's centered on the block instead of min corner
	
			bot.SkinName = skin;
			Position pos = Position.FromFeet((int)(x*32)+16, (int)(y*32), (int)(z*32)+16);
			bot.SetInitialPos(pos);
			//HandleAdd(Player.Console,  uniqueName,  new string[] {"add", uniqueName, ai, "75"});
			 bot.Rot = new Orientation(rot,0);
			PlayerBot.Add(bot);
	
			ScriptFile.Parse(Player.Console, bot, "");
			BotsFile.Save(level);
			
		}
		MikuPlushSpawnEggBlock getEgg(BlockID block)
		{
			foreach(var egg in EggConfigs)
			{
				if ((ushort)(egg.BLOCK_ID) == (block-256))
					return egg;
			}
			return null;
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (!placing){return;}
			
			var egg = getEgg(block);
			
			if (egg == null){return;}
			cancel = true;
			p.RevertBlock(x, y, z);
			if ( p.level.Bots.Items.Length >= 200)
			{
				p.Message("There are too many entities in this level!!!");
				return;
			}
			if (GetSpawnedMikuPlushies(p.level).Count > 20)
			{
				p.Message("There are too many miku plushies in this level!!!");
				return;
			}
			if (p.Rank < allowedRank)
			{
				p.Message("Need to be rank " + allowedRank.ToString() + "+ to use Spawn Eggs");
				return;
			}
			SpawnEntity(p.level, egg.MODEL, egg.SKIN, x, y, z, (byte)(p.Rot.RotY + (256/2)));
        }
		PlayerBot GetMikuFromEnt(Player p, byte entity)
		{
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
				return null;
			foreach(var egg in EggConfigs)
			{
				if (mob.Model == egg.MODEL)
					return mob;
			}
			return null;
		}
		void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (action != MouseAction.Pressed || button != MouseButton.Left)
				return;
            var bot = GetMikuFromEnt(p, entity);
			if (bot == null)
				return;
			PlayerBot.Remove(bot);
	
			
			/*if (block < cakeConfig.ID + 256)
				return;*/
		}
	}
}