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
				TEXTURE = 178,
				ID = 90,
			},
			new SurvivalTool()
			{
				NAME = "Iron Pickaxe",
				TEXTURE = 146,
				ID = 91,
			},
			new SurvivalTool()
			{
				NAME = "Iron Axe",
				TEXTURE = 130,
				ID = 92,
			},
			new SurvivalTool()
			{
				NAME = "Iron Shovel",
				TEXTURE = 162,
				ID = 93,
			},
			new SurvivalTool()
			{
				NAME = "Stone Sword",
				TEXTURE = 129,
				ID = 94,
			},
			new SurvivalTool()
			{
				NAME = "Stone Axe",
				TEXTURE = 177,
				ID = 95,
			},
		};

		public override void Load(bool startup) {
			foreach (var a in customBlocks)
			{
				AddBlockDef(a.NAME, a.ID, 0,0,0,16,16,16,a.TEXTURE, a.TEXTURE, 85, 85, true);
			}
		}
                        
		public override void Unload(bool shutdown) {
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
		private void AddBlockDef(BlockDefinition def)
		{
			BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null );
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
	}
}