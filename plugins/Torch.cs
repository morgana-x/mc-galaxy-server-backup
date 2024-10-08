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
	
	public class TorchItem
	{
		public ushort TEXTURE_ID;
		public ushort TEXTURE_BOTTOM;
		public ushort TEXTURE_SIDE;
		public ushort TEXTURE_TOP;
		public ushort HIDDEN_BLOCK_ID;
		public BlockID BLOCK_ID;
		public string NAME;
		public int BRIGHTNESS;
	}
	public class SpawnEgg : Plugin {
		public override string name { get { return "torch"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		
		
		public LevelPermission allowedRank { get { return LevelPermission.Admin; } }
		public List<TorchItem> SignConfigs = new List<TorchItem>(){
			new TorchItem()
			{
				TEXTURE_ID = 180,
				BLOCK_ID = 75,
				NAME = "Torch",
				BRIGHTNESS = 15,
				TEXTURE_SIDE = 180,
				TEXTURE_BOTTOM = 4,
				TEXTURE_TOP = 180,
			},
			new TorchItem()
			{
				TEXTURE_ID = 179,
				BLOCK_ID = 85,
				NAME = "Redstone Torch",
				BRIGHTNESS = 5,
				TEXTURE_SIDE = 179,
				TEXTURE_BOTTOM = 4,
				TEXTURE_TOP = 179,
			},
		};

		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			foreach (TorchItem egg in SignConfigs)
			{
				AddTorchBlock(egg.NAME, egg.BLOCK_ID,  7, 7, 0, 9,9,9, egg.TEXTURE_SIDE,egg.TEXTURE_SIDE,egg.TEXTURE_TOP,egg.TEXTURE_BOTTOM, egg.BRIGHTNESS, false);
			}

		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!		
		}
	
		public override void Help(Player p) {
			//HELP INFO!
		}
		public void AddBlock(BlockDefinition def)
		{
			BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null );
		}
		public void AddTorchBlock(string name, ushort Id, ushort MinX, ushort MinY, ushort MinZ, ushort MaxX, ushort MaxY, ushort MaxZ, ushort TEXTURE_SIDE, ushort TEXTURE_FRONT, ushort TEXTURE_TOP, ushort TEXTURE_BOTTOM, int Brightness, bool Transperant)
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
				//SetDoorBlockPerms(Id);
		}

	}
}