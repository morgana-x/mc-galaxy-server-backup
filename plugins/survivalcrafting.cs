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
	public class CraftingTableBlock
	{
		public ushort TEXTURE_TOP;
		public ushort TEXTURE_BOTTOM;
		public ushort TEXTURE_SIDE;
		public ushort TEXTURE_FRONT;
		public string NAME;
		public ushort ID;
	}
	public class FurnaceBlock
	{
		public ushort TEXTURE_TOP;
		public ushort TEXTURE_BOTTOM;
		public ushort TEXTURE_SIDE;
		public ushort TEXTURE_FRONT;
		public ushort TEXTURE_FRONT_ON;
		public string NAME;
		public ushort ID;
		public ushort ID_ON;
	}
	public class CustomBlock
	{
		public ushort TEXTURE_TOP;
		public ushort TEXTURE_BOTTOM;
		public ushort TEXTURE_SIDE;
		public ushort TEXTURE_FRONT;
		public string NAME;
		public ushort ID;
		public int BRIGHTNESS;
	}
	public class SurvivalCrafting : Plugin {
		public override string name { get { return "SurvivalCrafting"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }

		public Dictionary<Player, Dictionary<int,int>> playerInventories = new Dictionary<Player, Dictionary<int,int>>();

		static ushort blockMenuRowLength = 10;

		public CraftingTableBlock crafttableblockconfig = new CraftingTableBlock()
		{
			TEXTURE_BOTTOM = 4,
			TEXTURE_TOP = 43,
			TEXTURE_SIDE = 59,
			TEXTURE_FRONT = 60,
			NAME = "Crafting Table",
			ID = 76
		};
		
		public FurnaceBlock furnaceblockconfig = new FurnaceBlock()
		{
			TEXTURE_BOTTOM = 62,
			TEXTURE_TOP = 62,
			TEXTURE_SIDE = 45,
			TEXTURE_FRONT = 44,
			TEXTURE_FRONT_ON = 61,
			NAME = "Furnace",
			ID = 77,
			ID_ON = 78
		};
		private List<CustomBlock> customBlocks = new List<CustomBlock>(){
			new CustomBlock()
			{
				NAME = "Glowstone",
				TEXTURE_TOP = 99,
				TEXTURE_BOTTOM = 99,
				TEXTURE_SIDE = 99,
				TEXTURE_FRONT = 99,
				ID = 79,
				BRIGHTNESS = 18
			},
			new CustomBlock()
			{
				NAME = "Pumpkin",
				TEXTURE_TOP = 96,
				TEXTURE_BOTTOM = 96,
				TEXTURE_SIDE = 112,
				TEXTURE_FRONT = 112,
				ID = 80,
				BRIGHTNESS = 0
			},
			new CustomBlock()
			{
				NAME = "Carved Pumpkin",
				TEXTURE_TOP = 96,
				TEXTURE_BOTTOM = 96,
				TEXTURE_SIDE = 112,
				TEXTURE_FRONT = 113,
				ID = 81,
				BRIGHTNESS = 0
			},
		    new CustomBlock()
			{
				NAME = "Lit Carved Pumpkin",
				TEXTURE_TOP = 96,
				TEXTURE_BOTTOM = 96,
				TEXTURE_SIDE = 112,
				TEXTURE_FRONT = 114,
				ID = 82,
				BRIGHTNESS = 10
			},
			new CustomBlock()
			{
				NAME = "Diamond Ore",
				TEXTURE_TOP = 117,
				TEXTURE_BOTTOM = 117,
				TEXTURE_SIDE = 117,
				TEXTURE_FRONT = 117,
				ID = 87,
			},
			new CustomBlock()
			{
				NAME = "Redstone Ore",
				TEXTURE_TOP = 118,
				TEXTURE_BOTTOM = 118,
				TEXTURE_SIDE = 118,
				TEXTURE_FRONT = 118,
				ID = 88,
			},
			new CustomBlock()
			{
				NAME = "Diamond Block",
				TEXTURE_TOP = 27,
				TEXTURE_BOTTOM = 27,
				TEXTURE_SIDE = 27,
				TEXTURE_FRONT = 27,
				ID = 86,
			},
		};

		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnPlayerDyingEvent.Register(HandlePlayerDying, Priority.High);
			OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.Low);
			playerInventories.Clear();
			AddBlockDef(crafttableblockconfig.NAME, crafttableblockconfig.ID, 0,0,0,16,16,16,crafttableblockconfig.TEXTURE_SIDE, crafttableblockconfig.TEXTURE_FRONT, crafttableblockconfig.TEXTURE_TOP, crafttableblockconfig.TEXTURE_BOTTOM, false);
			AddBlockDef(furnaceblockconfig.NAME, furnaceblockconfig.ID, 0,0,0,16,16,16,furnaceblockconfig.TEXTURE_SIDE, furnaceblockconfig.TEXTURE_FRONT, furnaceblockconfig.TEXTURE_TOP, furnaceblockconfig.TEXTURE_BOTTOM, false);
			AddBlockDef(furnaceblockconfig.NAME + "_ON", furnaceblockconfig.ID_ON, 0,0,0,16,16,16,furnaceblockconfig.TEXTURE_SIDE, furnaceblockconfig.TEXTURE_FRONT_ON, furnaceblockconfig.TEXTURE_TOP, furnaceblockconfig.TEXTURE_BOTTOM, false, 15);
			foreach (var a in customBlocks)
			{
				AddBlockDef(a.NAME, a.ID, 0,0,0,16,16,16,a.TEXTURE_SIDE, a.TEXTURE_FRONT, a.TEXTURE_TOP, a.TEXTURE_BOTTOM, false, a.BRIGHTNESS);
			}
		    OnPlayerClickEvent.Register(HandleBlockClick, Priority.Low);
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnPlayerDyingEvent.Unregister(HandlePlayerDying);
			OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
			OnPlayerClickEvent.Unregister(HandleBlockClick);
			playerInventories.Clear();
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
		void HandlePlayerDying(Player p, BlockID deathblock, ref bool cancel)
        {
			if (playerInventories.ContainsKey(p))
			{
				if (playerInventories[p] != null)
				{
					playerInventories[p].Clear();
				}
			}
        }
		void HandlePlayerDisconnect(Player p, string reason)
        {
			if (playerInventories.ContainsKey(p))
			{
				playerInventories.Remove(p);
			}
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
				ushort BackTex = TEXTURE_SIDE;
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
				//SetDoorBlockPerms(Id);
		}
		void HandleBlockClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
			if (action != MouseAction.Pressed)
			 	return;
			ushort clickedBlock = p.level.GetBlock(x, y, z);
			if (clickedBlock == furnaceblockconfig.ID + 256 && button == MouseButton.Right)
			{
				p.level.UpdateBlock(p, x, y, z, (ushort)(furnaceblockconfig.ID_ON + 256));
				return;
			}
			if (clickedBlock == furnaceblockconfig.ID_ON + 256 && button == MouseButton.Right)
			{
				p.level.UpdateBlock(p, x, y, z,(ushort)(furnaceblockconfig.ID + 256));
				return;
			}
		}
	}
}