using System;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using MCGalaxy.Maths;
using MCGalaxy.Blocks;
using MCGalaxy.Network;
using MCGalaxy.Bots;
using System.Collections.Generic;
using BlockID = System.UInt16;
namespace MCGalaxy {
	public class CakeConfig
	{
		public ushort TEXTURE_TOP;
		public ushort TEXTURE_SIDE;
		public ushort TEXTURE_SIDE_EAT;
		public ushort TEXTURE_BOTTOM;
		public ushort TEXTURE_ITEM;
		public string NAME;
		public ushort ID;
		public ushort ITEM_ID;
	}
	public class Cake : Plugin {
		public override string name { get { return "cake"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }

		CakeConfig cakeConfig = new CakeConfig()
		{
			ITEM_ID=83,
			ID=450,
			NAME="Cake",
			TEXTURE_ITEM=111,
			TEXTURE_TOP=92,
			TEXTURE_SIDE=93,
			TEXTURE_SIDE_EAT=94,
			TEXTURE_BOTTOM=95
			
		};

		public override void Load(bool startup) {

			AddBlockItem(cakeConfig.ITEM_ID, cakeConfig.NAME, cakeConfig.TEXTURE_ITEM);
			AddBlockDef(cakeConfig.NAME, cakeConfig.ID, 2,2,0,14,14,8,cakeConfig.TEXTURE_SIDE, cakeConfig.TEXTURE_SIDE, cakeConfig.TEXTURE_TOP, cakeConfig.TEXTURE_BOTTOM, false);
			AddBlockDef(cakeConfig.NAME, (ushort)(cakeConfig.ID+1), 2,2,0,10,14,8,cakeConfig.TEXTURE_SIDE, cakeConfig.TEXTURE_SIDE_EAT, cakeConfig.TEXTURE_TOP, cakeConfig.TEXTURE_BOTTOM, false);
			AddBlockDef(cakeConfig.NAME, (ushort)(cakeConfig.ID+2), 2,2,0,6,14,8,cakeConfig.TEXTURE_SIDE, cakeConfig.TEXTURE_SIDE_EAT, cakeConfig.TEXTURE_TOP, cakeConfig.TEXTURE_BOTTOM, false);
			AddBlockDef(cakeConfig.NAME, (ushort)(cakeConfig.ID+3), 2,2,0,4,14,8,cakeConfig.TEXTURE_SIDE, cakeConfig.TEXTURE_SIDE_EAT, cakeConfig.TEXTURE_TOP, cakeConfig.TEXTURE_BOTTOM, false);
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
		}
                        
		public override void Unload(bool shutdown) {
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
		private void AddBlockDef(BlockDefinition def)
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
			AddBlockDef(def);
		}
		public void AddBlockDef(string name, ushort Id, ushort MinX, ushort MinY, ushort MinZ, ushort MaxX, ushort MaxY, ushort MaxZ, ushort TEXTURE_SIDE, ushort TEXTURE_FRONT, ushort TEXTURE_TOP, ushort TEXTURE_BOTTOM, bool Transperant, int Brightness=0)
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
				ushort RightTex = TEXTURE_FRONT;
				ushort FrontTex = TEXTURE_SIDE;
				ushort BackTex = TEXTURE_SIDE;
				ushort TopTex = TEXTURE_TOP;
				ushort BottomTex = TEXTURE_BOTTOM;
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
				AddBlockDef(def);
				
				ushort block = Id;
				 if (true) {
					BlockPerms perms = BlockPerms.GetPlace((ushort)(block + 256));
					perms.MinRank = LevelPermission.Nobody;
				 }
				BlockPerms.Save();
				BlockPerms.ApplyChanges();

				if (!Block.IsPhysicsType(block)) {
					BlockPerms.ResendAllBlockPermissions();
				}            
		}
		int getCakeStage(BlockID block)
		{
			int difference = Math.Abs(block - (cakeConfig.ID+256));
			if (difference > 3)
				return -1;
			return difference;
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (placing && block == cakeConfig.ITEM_ID+256)
			{
				
				cancel = true;
				p.RevertBlock(x, y, z);
				p.level.UpdateBlock(Player.Console, x,y,z, (ushort)(cakeConfig.ID + 256));
				return;
			}	
        }
		void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (action != MouseAction.Pressed)
			{
				return;
			}
			BlockID block = p.level.FastGetBlock(x,y,z);
			if (block < cakeConfig.ID + 256)
				return;
			int stage = getCakeStage(block);
			if (stage == -1)
				return;
			if (p.Extras["SURVIVAL_HEALTH"] != null && (int)p.Extras["SURVIVAL_HEALTH"] < 20)
			{
				p.Extras["SURVIVAL_HEALTH"] = (int)p.Extras["SURVIVAL_HEALTH"] + 2;
			}
			if (stage > 2)
			{
				p.level.UpdateBlock(Player.Console, x,y,z, (ushort)0);
				return;
			}
			p.level.UpdateBlock(Player.Console, x,y,z, (ushort)(cakeConfig.ID + stage + 1 + 256));
		}
	}
}