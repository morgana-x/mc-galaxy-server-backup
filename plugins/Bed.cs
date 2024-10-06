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
	public class BedConfig
	{
		public ushort TEXTURE_FOOT_TOP;
		public ushort TEXTURE_FOOT_SIDE;
		public ushort TEXTURE_FOOT_BACK;
		public ushort TEXTURE_HEAD_TOP;
		public ushort TEXTURE_HEAD_SIDE;
		public ushort TEXTURE_HEAD_BACK;
		public ushort TEXTURE_ITEM;
		public string NAME;
		public ushort ID;
		public ushort ITEM_ID;
	}
	public class Bed : Plugin {
		public override string name { get { return "bed"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }

		public Dictionary<string,Dictionary<string, ushort[]>> SavePoints = new Dictionary<string,Dictionary<string, ushort[]>>();
		BedConfig bedConfig = new BedConfig()
		{
			ITEM_ID=84,
			ID=460,
			NAME="Bed",
			TEXTURE_ITEM=90,
			TEXTURE_FOOT_TOP=106,
			TEXTURE_FOOT_SIDE=122,
			TEXTURE_FOOT_BACK=121,
			TEXTURE_HEAD_TOP=107,
			TEXTURE_HEAD_SIDE=123,
			TEXTURE_HEAD_BACK=124
			
		};

		public override void Load(bool startup) {

			AddBlockItem(bedConfig.ITEM_ID, bedConfig.NAME, bedConfig.TEXTURE_ITEM);
			AddBedBlockDef(bedConfig.ID, bedConfig.TEXTURE_FOOT_SIDE, bedConfig.TEXTURE_FOOT_BACK, bedConfig.TEXTURE_FOOT_TOP);
			AddBedBlockDef((ushort)(bedConfig.ID+2), bedConfig.TEXTURE_HEAD_SIDE, bedConfig.TEXTURE_HEAD_BACK, bedConfig.TEXTURE_HEAD_TOP);
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
			OnPlayerSpawningEvent.Register(HandlePlayerSpawning, Priority.High);
		}
                        
		public override void Unload(bool shutdown) {
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
			OnPlayerSpawningEvent.Unregister(HandlePlayerSpawning);
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
		public void AddBedBlockDef(ushort Id, ushort text_side_left, ushort text_side_right, ushort text_front, ushort text_back, ushort text_top)
		{
			AddBlockDef("", Id, 0,0,0,16,16,8, text_side_left, text_side_right,text_front, text_front, text_top, 85, true, 0);
		}
		public void AddBedBlockDef(ushort Id, int dir,  ushort text_side, ushort text_front, ushort text_top)
		{
			//                             Left      Right      Front       Back        Top
			AddBedBlockDef((ushort)(Id  ), text_side,text_side, text_front, text_front, text_top );
			AddBedBlockDef((ushort)(Id+1), text_front,text_front, text_side, text_side, text_top );
		}
		public void AddBedBlockDef(ushort Id, ushort text_side, ushort text_front, ushort text_top)
		{
			AddBedBlockDef(Id, (int)0, text_side, text_front, text_top);
		}
		public void AddBlockDef(string name, ushort Id, ushort MinX, ushort MinY, ushort MinZ, ushort MaxX, ushort MaxY, ushort MaxZ, ushort TEXTURE_SIDE_LEFT, ushort TEXTURE_SIDE_RIGHT, ushort TEXTURE_FRONT, ushort TEXTURE_BACK, ushort TEXTURE_TOP, ushort TEXTURE_BOTTOM, bool Transperant, int Brightness=0)
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
				ushort LeftTex = TEXTURE_SIDE_LEFT;
				ushort RightTex = TEXTURE_SIDE_RIGHT;
				ushort FrontTex = TEXTURE_FRONT;
				ushort BackTex = TEXTURE_BACK;
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
		bool isBed(BlockID block)
		{
			return !(block<bedConfig.ID + 256 || block > bedConfig.ID + 256 + 4);
		}
		void PlaceBed(Level level, ushort[] pos, int dir)
		{
			ushort dirt = dir> 1 ? (ushort)1 : (ushort)0;
			ushort blockFoot= (ushort)(bedConfig.ID + 256 + dirt);
			ushort blockHead= (ushort)(bedConfig.ID + 256 + 2 + dirt);

			level.UpdateBlock(Player.Console, pos[0], pos[1], pos[2], blockFoot);
			switch (dir)
			{
			 	case 2:
					level.UpdateBlock(Player.Console, (ushort)(pos[0]+1), pos[1], pos[2], blockHead);
					break;
				case 3:
					level.UpdateBlock(Player.Console, (ushort)(pos[0]-1), pos[1], pos[2], blockHead);
					break;
				case 0:
					level.UpdateBlock(Player.Console, pos[0], pos[1],  (ushort)(pos[2]+1), blockHead);
					break;
				case 1:
					level.UpdateBlock(Player.Console, pos[0], pos[1],  (ushort)(pos[2]-1), blockHead);
					break;
			}
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (!placing)
				return;
			if (block != bedConfig.ITEM_ID+256)
				return;
			cancel = true;
			p.RevertBlock(x, y, z);
			PlaceBed(p.level, new ushort[]{x,y,z}, 2);
        }
		void setBedSavePoint(Level level, Player p, ushort[] pos)
		{
			if (!SavePoints.ContainsKey(level.name))
				SavePoints.Add(level.name, new Dictionary<string, ushort[]>());
			if (!SavePoints[level.name].ContainsKey(p.name))
				SavePoints[level.name].Add(p.name, null);
			SavePoints[level.name][p.name] = new ushort[]{pos[0], pos[1], pos[2]};
			p.Message("Your savepoint has been set to " + pos[0].ToString() + ", " + pos[1].ToString() + ", " + pos[2].ToString() + ".");
		}
		Position doBedSpawn(Player p)
		{
			Position nullPos = new Position(-1,-1,-1);
			if (!SavePoints.ContainsKey(p.level.name))
				return nullPos;
			if (!SavePoints[p.level.name].ContainsKey(p.name))
				return nullPos;
			if (SavePoints[p.level.name][p.name] == null)
				return nullPos;
			ushort[] pos = SavePoints[p.level.name][p.name];
			if (!isBed(p.level.FastGetBlock(pos[0], pos[1], pos[2])))
			{
				p.Message("Cannot respawn at your bed because your bed is either missing or destroyed!");
				SavePoints[p.level.name][p.name] = null;
				return nullPos;
			}

			return new Position(pos[0] << 5, ((pos[1] + 5) << 5), pos[2] << 5);
		}
		void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (action != MouseAction.Pressed)
				return;
			if (button != MouseButton.Right)
				return;
			BlockID block = p.level.FastGetBlock(x,y,z);
			if (!isBed(block))
				return;
			setBedSavePoint(p.level, p, new ushort[]{x,y,z});
		}

		void HandlePlayerSpawning(Player p, ref Position pos,  ref byte yaw, ref byte pitch, bool respawning)
		{
			if (!respawning)
				return;
			Position newPos = doBedSpawn(p);
			if (newPos.X != -1)
				pos = newPos;
		}
	}
}