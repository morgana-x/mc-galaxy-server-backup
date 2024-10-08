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
	public class SurvivalTool
	{
		public ushort TEXTURE;
		public string NAME;
		public ushort ID;
		public bool IsSword=false;
		public bool IsSprite = false;
		public int Damage=2;
	}
	public class Tools : Plugin {
		public override string name { get { return "Tools"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }

		private List<SurvivalTool> customBlocks = new List<SurvivalTool>(){
			new SurvivalTool()
			{
				NAME = "Iron Sword",
				TEXTURE = 204,
				ID = 91,
				IsSword = true,
				Damage = 6
			},
			new SurvivalTool()
			{
				NAME = "Iron Pickaxe",
				TEXTURE = 236,
				ID = 92,
				IsSword = false,
				Damage = 4,
			},
			new SurvivalTool()
			{
				NAME = "Iron Axe",
				TEXTURE = 252,
				ID = 93,
				IsSword = false,
				Damage = 5,
			},
			new SurvivalTool()
			{
				NAME = "Iron Shovel",
				TEXTURE = 220,
				ID = 94,
				IsSword = false,
				Damage = 5,
			},
			new SurvivalTool()
			{
				NAME = "Stone Sword",
				TEXTURE = 203,
				ID = 95,
				IsSword = true,
				Damage = 5,
			},
			new SurvivalTool()
			{
				NAME = "Stone Axe",
				TEXTURE = 251,
				ID = 96,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Stone Pickaxe",
				TEXTURE = 235,
				ID = 97,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Stone Shovel",
				TEXTURE = 219,
				ID = 98,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Wooden Sword",
				TEXTURE = 202,
				ID = 99,
				IsSword = true,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Wooden Shovel",
				TEXTURE = 218,
				ID = 100,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Wooden Pickaxe",
				TEXTURE = 234,
				ID = 101,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
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
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Gold Shovel",
				TEXTURE = 222,
				ID = 104,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Gold Pickaxe",
				TEXTURE = 238,
				ID = 105,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Gold Axe",
				TEXTURE = 254,
				ID = 106,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Diamond Sword",
				TEXTURE = 205,
				ID = 107,
				IsSword = true,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Diamond Shovel",
				TEXTURE = 221,
				ID = 108,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Diamond Pickaxe",
				TEXTURE = 237,
				ID = 109,
				IsSword = false,
				Damage = 4
			},
			new SurvivalTool()
			{
				NAME = "Diamond Axe",
				TEXTURE = 253,
				ID = 110,
				IsSword = false,
				Damage = 4
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
				NAME = "Iron Ignot",
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
		};

		public override void Load(bool startup) {
			foreach (var a in customBlocks)
			{
				if (a.IsSprite)
				{
					AddBlockItem(a.ID, a.NAME, a.TEXTURE);
					continue;
				}
				AddBlockDef(a.NAME, a.ID, 16,0,0,16,16,16,a.TEXTURE, 85, 85, 85, true);
			}
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
		}
                        
		public override void Unload(bool shutdown) {
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
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
				 if (true) {
					BlockPerms perms = BlockPerms.GetPlace((ushort)(block + 256));
					perms.MinRank = LevelPermission.Nobody;
				 }
				BlockPerms.Save();
				BlockPerms.ApplyChanges();

				if (!Block.IsPhysicsType(block)) {
					BlockPerms.ResendAllBlockPermissions();
				}            
				//SetDoorBlockPerms(Id);
		}
		SurvivalTool getEgg(BlockID block)
		{
			foreach(SurvivalTool egg in customBlocks)
			{
				if ((ushort)(egg.ID) == (block-256))
				{
					return egg;
				}
			}
			return null;
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			SurvivalTool egg = getEgg(block);
			
			if (egg == null){return;}
			if (egg.IsSprite)
				return;
			if (placing)
			{
				cancel = true;
				p.RevertBlock(x, y, z);
				return;
			}
			if (!egg.IsSword)
				return;
			cancel = true;
			p.RevertBlock(x, y, z);
        }
	}
}