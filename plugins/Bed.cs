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
using System.IO;
using System.Text;
namespace MCGalaxy {
	public class BedConfig
	{
		public ushort TEXTURE_FOOT_TOP_NORTH;
		public ushort TEXTURE_FOOT_TOP_SOUTH;
		public ushort TEXTURE_FOOT_TOP_EAST;
		public ushort TEXTURE_FOOT_TOP_WEST;
		public ushort TEXTURE_FOOT_SIDE_LEFT;
		public ushort TEXTURE_FOOT_SIDE_RIGHT;
		public ushort TEXTURE_FOOT_BACK;
		public ushort TEXTURE_HEAD_TOP_NORTH;
		public ushort TEXTURE_HEAD_TOP_SOUTH;
		public ushort TEXTURE_HEAD_TOP_EAST;
		public ushort TEXTURE_HEAD_TOP_WEST;
		public ushort TEXTURE_HEAD_SIDE_LEFT;
		public ushort TEXTURE_HEAD_SIDE_RIGHT;
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
			TEXTURE_FOOT_TOP_NORTH=106,
			TEXTURE_FOOT_TOP_SOUTH = 139,
			TEXTURE_FOOT_TOP_EAST = 141,
			TEXTURE_FOOT_TOP_WEST = 156,
			TEXTURE_FOOT_SIDE_LEFT=155,
			TEXTURE_FOOT_SIDE_RIGHT=122,
			TEXTURE_FOOT_BACK=121,
			TEXTURE_HEAD_TOP_NORTH=107,
			TEXTURE_HEAD_TOP_SOUTH=138,
			TEXTURE_HEAD_TOP_EAST=157,
			TEXTURE_HEAD_TOP_WEST=140,
			TEXTURE_HEAD_SIDE_RIGHT=123,
			TEXTURE_HEAD_SIDE_LEFT=154,
			TEXTURE_HEAD_BACK=124
			
		};
		public static string SaveFolder = "./plugins/SimpleSurvival/";
		public static string SaveFile = "./plugins/SimpleSurvival/Bed.data";
    static string readString(Stream stream)
    {
        byte[] bufferInteger = new byte[2];
        stream.Read(bufferInteger, 0, 2);
        int stringLength = BitConverter.ToUInt16(bufferInteger,0);
        byte[] stringBytes = new byte[stringLength];
        stream.Read(stringBytes, 0 , stringLength);
        return Encoding.UTF8.GetString(stringBytes);
    }
    static void writeString(Stream stream, string text)
    {
        byte[] stringBytes = Encoding.UTF8.GetBytes(text);
        byte[] stringBytesLength = BitConverter.GetBytes((ushort)stringBytes.Length);
        stream.Write(stringBytesLength, 0, 2);
        stream.Write(stringBytes, 0, stringBytes.Length);
    }
    static int readInt(Stream stream)
    {
        byte[] intBuffer = new byte[4];
        stream.Read(intBuffer, 0 , 4);
        return BitConverter.ToInt32(intBuffer, 0);
    }
    void LoadSavePoints( )
    {
        if (!File.Exists(SaveFile))
            return;
        FileStream fileStream = new FileStream(SaveFile, FileMode.Open, FileAccess.Read);
        byte[] bufferLong = new byte[8];
        byte[] bufferShort = new byte[2];

        int numberOfLevelEntries = readInt(fileStream);

        for (int i =0; i < numberOfLevelEntries; i++)
        {
            string levelName = readString(fileStream);
            SavePoints.Add(levelName, new Dictionary<string, ushort[]>());

            int numOfEntires = readInt(fileStream);
            for (int x=0 ; x < numOfEntires; x++)
            {
                string playerName = readString(fileStream);
                ushort[] pos = new ushort[3];
                for (int b = 0; b < 3; b++)
                {
                    fileStream.Read(bufferShort, 0 , 2);
                    pos[b] = BitConverter.ToUInt16(bufferShort,0);
                }
                if (!SavePoints[levelName].ContainsKey(playerName))
                    SavePoints[levelName].Add(playerName, pos);
            }

        }
    }
    static void SaveSavePoints(Dictionary<string, Dictionary<string, ushort[]>> SavePoints)
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);

        FileStream headerStream = new FileStream(SaveFile, FileMode.Create, FileAccess.Write);
        MemoryStream dataStream = new MemoryStream();

        headerStream.Write(BitConverter.GetBytes(SavePoints.Keys.Count), 0, 4);

        foreach (var pair in SavePoints)
        {
            writeString(dataStream, pair.Key); // Write Level Name
            byte[] numOfEntries = BitConverter.GetBytes(pair.Value.Values.Count);
            dataStream.Write(numOfEntries, 0, 4);
            foreach (var pair2 in pair.Value)
            {
                writeString(dataStream, pair2.Key); // Write player Name
                for (int i=0; i <3; i++)
                {
                    dataStream.Write(BitConverter.GetBytes(pair2.Value[i]),0,2);
                }
            }
        }
        headerStream.Write(dataStream.ToArray(),0, (int)dataStream.Length);
        headerStream.Close();
    }

		public override void Load(bool startup) {

			AddBlockItem(bedConfig.ITEM_ID, bedConfig.NAME, bedConfig.TEXTURE_ITEM);
			AddBedBlockDef(bedConfig.ID, bedConfig.TEXTURE_FOOT_SIDE_LEFT, bedConfig.TEXTURE_FOOT_SIDE_RIGHT, bedConfig.TEXTURE_FOOT_BACK, bedConfig.TEXTURE_FOOT_TOP_NORTH, bedConfig.TEXTURE_FOOT_TOP_SOUTH, bedConfig.TEXTURE_FOOT_TOP_EAST, bedConfig.TEXTURE_FOOT_TOP_WEST);
			AddBedBlockDef((ushort)(bedConfig.ID+4), bedConfig.TEXTURE_HEAD_SIDE_LEFT, bedConfig.TEXTURE_HEAD_SIDE_RIGHT, bedConfig.TEXTURE_HEAD_BACK, bedConfig.TEXTURE_HEAD_TOP_NORTH, bedConfig.TEXTURE_HEAD_TOP_SOUTH, bedConfig.TEXTURE_HEAD_TOP_EAST, bedConfig.TEXTURE_HEAD_TOP_WEST);
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
			OnPlayerSpawningEvent.Register(HandlePlayerSpawning, Priority.High);
			OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.Low);
			LoadSavePoints();
		}
                        
		public override void Unload(bool shutdown) {
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
			OnPlayerSpawningEvent.Unregister(HandlePlayerSpawning);
			OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
			SaveSavePoints(SavePoints);
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
			AddBlockDef("", Id, 0,0,0,16,16,8, text_side_left, text_side_right,text_front, text_back, text_top, 85, true, 0);
		}
		void HandlePlayerDisconnect(Player p, string reason)
        {
			try
			{
				SaveSavePoints(SavePoints);
			}
			catch(Exception e)
			{

			}
		}

		public void AddBedBlockDef(ushort Id, ushort text_side_left, ushort text_side_right, ushort text_front, ushort text_top_north, ushort text_top_south, ushort text_top_east, ushort text_top_west )
		{
			//                             Left     			 Right      		Front       		Back        Top
			AddBedBlockDef((ushort)(Id  ), text_front		,text_front , text_side_left	  , text_side_right	   , text_top_north ); // North
			AddBedBlockDef((ushort)(Id+1), text_side_right			,text_side_left		 , text_front , text_front, text_top_east); // East
			AddBedBlockDef((ushort)(Id+2), text_front	    ,text_front  , text_side_right , text_side_left, text_top_south ); // South
			AddBedBlockDef((ushort)(Id+3), text_side_left			,text_side_right	 , text_front , text_front, text_top_west ); // West
		}
		public void AddBedBlockDef(ushort Id,  ushort text_side_left, ushort text_side_right,  ushort text_front, ushort text_top)
		{
			AddBedBlockDef(Id, (int)0, text_side_left, text_side_right, text_front, text_top);
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
			return !(block<bedConfig.ID + 256 || block > bedConfig.ID + 256 + 8);
		}
		void PlaceBed(Level level, ushort[] pos, int dir)
		{
			if (level.FastGetBlock(pos[0], pos[1], pos[2]) != 0)
				return;
			ushort dirt = (ushort)dir;
			ushort[] headPos = new ushort[]{pos[0], pos[1], pos[2]};
			ushort[] footPos = new ushort[]{pos[0], pos[1], pos[2]};
			switch (dir)
			{
			 	case 0:
					headPos[0] = (ushort)(headPos[0]+1); // North
					break;
				case 2:
					headPos[0] = (ushort)(headPos[0]-1); // South
					break;
				case 1:
					headPos[2] = (ushort)(headPos[2]+1); // East
					break;
				case 3:
					headPos[2] = (ushort)(headPos[2]-1); // West
					break;
			}
			if (level.FastGetBlock(headPos[0], headPos[1], headPos[2]) != 0)
				return;
			ushort blockFoot= (ushort)(bedConfig.ID + 256 + dirt);
			ushort blockHead= (ushort)(bedConfig.ID + 256 + 4 + dirt);
			level.UpdateBlock(Player.Console, footPos[0], footPos[1], footPos[2], blockFoot);
			level.UpdateBlock(Player.Console, headPos[0], headPos[1], headPos[2], blockHead);
		}
		int getDir(Player p )
		{
			byte angle = p.Rot.RotY;
			if (angle < 90) // North
				return 0;
			if (angle < 180) // East
				return 1;
			if (angle < 200) // South
				return 2;
			if (angle < 255) // West
				return 3;
			return 3;

		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (!placing)
				return;
			if (block != bedConfig.ITEM_ID+256)
				return;
			cancel = true;
			p.RevertBlock(x, y, z);
			PlaceBed(p.level, new ushort[]{x,y,z}, getDir(p));
        }
		void setBedSavePoint(Level level, Player p, ushort[] pos)
		{
			if (!SavePoints.ContainsKey(level.name))
				SavePoints.Add(level.name, new Dictionary<string, ushort[]>());
			if (!SavePoints[level.name].ContainsKey(p.name))
				SavePoints[level.name].Add(p.name, null);
			ushort[] newPos = new ushort[]{pos[0], pos[1], pos[2]};
			ushort[] oldPos = SavePoints[level.name][p.name];
			if (oldPos != null && oldPos[0] == newPos[0] && oldPos[1] == newPos[1] && oldPos[2] == newPos[2])
				return;
			SavePoints[level.name][p.name] = newPos;
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
				SavePoints[p.level.name].Remove(p.name);
				return nullPos;
			}

			return new Position(pos[0] << 5, ((pos[1] + 2) << 5), pos[2] << 5);
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