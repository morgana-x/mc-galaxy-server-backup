//This is an example plugin source!
using System;
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
namespace MCGalaxy {
	public class DoorBlock {
		public BlockID Item_Block        {get; set;}
		public BlockID Top_Block         {get; set;}
		public BlockID Top_Block_Open    {get; set;}
		public BlockID Bottom_Block      {get; set;}
		public BlockID Bottom_Block_Open {get; set;}
		public BlockID Top_Block_Inverse {get; set;}
		public BlockID Top_Block_Inverse_Open {get; set;}
		public BlockID Bottom_Block_Inverse {get; set;}
		public BlockID Bottom_Block_Inverse_Open {get; set;}
	}
	public class DoorConfig {
		public BlockID BLOCK_ITEM_ID 		{get; set;}
		public string  BLOCK_ITEM_NAME 		{get; set;}
		public ushort  TEXTURE_ITEM 		{get; set;}
		public ushort  TEXTURE_BLOCK_TOP    {get; set;}
		public ushort  TEXTURE_BLOCK_BOTTOM {get; set;}
	}
	public class Door : Plugin {
		public override string name { get { return "Door"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }

		public bool SoundEnabled = true; // Whether sound is enabled for CEF PLUGIN CLIENTS ONLY
		public int SoundRange = 6; // How many blocks away can players hear doors opening (CEF PLUGIN CLIENTS ONLY)
		
		public ushort DoorBlockIdStorageIndex = 300; // Beggining of reserved Door slots will take up 8*Number of doors
		
		public List<DoorConfig> DoorConfigs = new List<DoorConfig>()
		{
			new DoorConfig() // Just add more of these if you want more doors! (Make sure you have a unique id, that has 8 further free Ids after it)
			{
				BLOCK_ITEM_ID = 66,
				BLOCK_ITEM_NAME = "Wooden Door",
				TEXTURE_ITEM = 182, // Press F10 to see texture Ids
				TEXTURE_BLOCK_TOP = 183, // Press F10 to see texture Ids
				TEXTURE_BLOCK_BOTTOM = 184, // Press F10 to see texture Ids
			},
			new DoorConfig() // Iron Door
			{
				BLOCK_ITEM_ID = 67,
				BLOCK_ITEM_NAME = "Iron Door",
				TEXTURE_ITEM = 185, // Press F10 to see texture Ids
				TEXTURE_BLOCK_TOP = 186, // Press F10 to see texture Ids
				TEXTURE_BLOCK_BOTTOM = 187, // Press F10 to see texture Ids
			},
			new DoorConfig() // Dark Oak Door
			{
				BLOCK_ITEM_ID = 68,
				BLOCK_ITEM_NAME = "Dark Oak Door",
				TEXTURE_ITEM = 188, // Press F10 to see texture Ids
				TEXTURE_BLOCK_TOP = 189, // Press F10 to see texture Ids
				TEXTURE_BLOCK_BOTTOM = 190, // Press F10 to see texture Ids
			},
		};
		
		
		
		
		
		
		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
			OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.Low);
			OnSentMapEvent.Register(HandleSentMap, Priority.Low);
			foreach (var d in DoorConfigs)
			{
				LoadDoor(d);
			}
			SendDoorPermsAll();
			foreach (Player p in PlayerInfo.Online.Items)
			{
				InitCEF(p);
			}
		}
                    
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
			OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
			OnSentMapEvent.Unregister(HandleSentMap);
		}
                        
		public override void Help(Player p) {
			//HELP INFO!
		}
		
		public void AddBlock(BlockDefinition def)
		{
			BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null );
		}
		void HandleSentMap( Player p, Level prevLevel, Level level)
		{
			SendDoorPerms(p);
			InitCEF(p);
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
		List<BlockID> DisabledBlocks = new List<BlockID>();
		public void SetDoorBlockPerms(ushort block)
		{
			DisabledBlocks.Add((BlockID)block);
			SendDoorPermsAll();
		}
		internal unsafe static void SendLevelInventoryOrder(Player pl) {
		   BlockDefinition[] defs = pl.level.CustomBlockDefs;
		   int maxRaw = pl.Session.MaxRawBlock;
		   int count  = maxRaw + 1;

		   int* order_to_blocks = stackalloc int[Block.ExtendedCount];
		   int* block_to_orders = stackalloc int[Block.ExtendedCount];
		   for (int b = 0; b < Block.ExtendedCount; b++) 
		   {
			   order_to_blocks[b] = -1;
			   block_to_orders[b] = -1;
		   }
		   
		   // Fill slots with explicit order
		   for (int i = 0; i < defs.Length; i++) 
		   {
			   BlockDefinition def = defs[i];
			   if (def == null || def.RawID > maxRaw) continue;
			   if (def.InventoryOrder == -1) continue;
			   
			   if (def.InventoryOrder != 0) {
				   if (order_to_blocks[def.InventoryOrder] != -1) continue;
				   order_to_blocks[def.InventoryOrder] = def.RawID;
			   }
			   block_to_orders[def.RawID] = def.InventoryOrder;
		   }
		   
		   // Put blocks into their default slot if slot is unused
		   for (int i = 0; i < defs.Length; i++) 
		   {
			   BlockDefinition def = defs[i];
			   int raw = def != null ? def.RawID : i;
			   if (raw > maxRaw || (def == null && raw >= Block.CPE_COUNT)) continue;
			   
			   if (def != null && def.InventoryOrder >= 0) continue;
			   if (order_to_blocks[raw] == -1) {
				   order_to_blocks[raw] = raw;
				   block_to_orders[raw] = raw;
			   }
		   }
		   
		   // Push blocks whose slots conflict with other blocks into free slots at end
		   for (int i = defs.Length - 1; i >= 0; i--) 
		   {
			   BlockDefinition def = defs[i];
			   int raw = def != null ? def.RawID : i;
			   if (raw > maxRaw || (def == null && raw >= Block.CPE_COUNT)) continue;
			   
			   if (block_to_orders[raw] != -1) continue;
			   for (int slot = count - 1; slot >= 1; slot--) {
				   if (order_to_blocks[slot] != -1) continue;
				   
				   block_to_orders[raw]  = slot;
				   order_to_blocks[slot] = raw;
				   break;
			   }
		   }
		   
		   for (int raw = 0; raw < count; raw++) 
		   {
			   int order = block_to_orders[raw];
			   if (order == -1) order = 0;
			   
			   BlockDefinition def = defs[Block.FromRaw((BlockID)raw)];
			   if (def == null && raw >= Block.CPE_COUNT) continue;
			   // Special case, don't want 255 getting hidden by default
			   if (raw == 255 && def.InventoryOrder == -1) continue;
			   
			   pl.Send(Packet.SetInventoryOrder((BlockID)raw, (BlockID)order, pl.Session.hasExtBlocks));
		   }
	   }
		void SendDoorPerms(Player p)
		{
			bool extBlocks = p.Session.hasExtBlocks;
            int count = DisabledBlocks.Count;//p.Session.MaxRawBlock + 1;
            int size  = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * 5];
            Level level = p.level;
            for (int i = 0; i < count; i++) 
            {
				
                BlockID block = (BlockID)(DisabledBlocks[i]);
				if (block < DoorBlockIdStorageIndex)
				{
					return;
				}					
                Packet.WriteBlockPermission((BlockID)block, false, false, p.Session.hasExtBlocks, bulk, i * size);
            }
            p.Send(bulk);
			SendLevelInventoryOrder(p);
		}
		void SendDoorPermsAll()
		{
			foreach (Player p in PlayerInfo.Online.Items)
			{
				SendDoorPerms(p);
			}
		}
		void InitCEF(Player p)
		{
			if (p.Session.ClientName().CaselessContains("cef"))
			{
				// add g to asq if brokey
				p.Message("cef create -n dooropen -gasq https://youtu.be/G0XB3GaDSjM");
				/*p.Message("cef size 0 0 -n dooropen");
				p.Message("cef resolution 1 1 -n dooropen");
				p.Message("cef volume 2 1 -n dooropen");*/
				
				p.Message("cef create -n doorclose -gasq https://youtu.be/f--utoY1C9s");
				/*p.Message("cef size 0 0 -n doorclose");
				p.Message("cef resolution 1 1 -n doorclose");
				p.Message("cef volume 2 1 -n doorclose");*/
				
			}
		}
		void HandlePlayerConnect(Player p)
        {
            SendDoorPerms(p);
			InitCEF(p);
			
        }
		void PlaySound(Player p, string sound)
		{
			if (!SoundEnabled)
			{
				return;
			}
			if (!p.Session.ClientName().CaselessContains("cef"))
			{
				return;
			}
			p.Message("cef time -n " + sound + " " + 0.2);
			//p.Message("cef resume -n " + sound);
		}
		void PlaySound(Player p, string sound, ushort x, ushort y, ushort z)
		{
			if (!SoundEnabled)
			{
				return;
			}
			if (!p.Session.ClientName().CaselessContains("cef"))
			{
				return;
			}
			//p.Message("cef at -n " + sound + " " + x + " " + y + " " + z + " " + 0 + " " + 0); 
			p.Message("cef time -n " + sound + " " + 0);
			//p.Message("cef resume -n " + sound);
		}
		void PlaySound3D(Level level, ushort x, ushort y, ushort z, string sound)
		{
			if (!SoundEnabled)
			{
				return;
			}
			foreach (Player p in PlayerInfo.Online.Items)
			{
				if (p.level != level)
				{
					continue;
				}
				if (!p.Session.ClientName().CaselessContains("cef"))
				{
					continue;
				}
				int px = p.Pos.X / 32;
				int py = p.Pos.Y / 32;
				int pz = p.Pos.Z / 32;
				int dx = px-x;
				int dy = py-y;
				int dz = pz-z;
				double dist = Math.Sqrt( (dx*dx) + (dy*dy) + (dz*dz));
				if (dist > SoundRange)
				{
					continue;
				}
				PlaySound(p, sound, x, y, z);
			}
		}
		public void AddDoorBlock(ushort Id, ushort MinX, ushort MinY, ushort MinZ, ushort MaxX, ushort MaxY, ushort MaxZ, ushort TEXTURE_SIDE, ushort TEXTURE_FRONT, bool Transperant)
		{
				ushort RawID = Id;
				string Name = "";
				byte Speed = 1;
				byte CollideType = 2;
				ushort TopTex = TEXTURE_SIDE;
				ushort BottomTex = TEXTURE_SIDE;
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
				ushort LeftTex = TEXTURE_FRONT;
				ushort RightTex = TEXTURE_FRONT;
				ushort FrontTex = TEXTURE_FRONT;
				ushort BackTex = TEXTURE_FRONT;
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
				AddBlock(def);
				SetDoorBlockPerms(Id);
		}
		public void AddDoorBlocks(DoorBlock door, string BLOCK_ITEM_NAME, ushort TEXTURE_ITEM, ushort TEXTURE_BLOCK_BOTTOM, ushort TEXTURE_BLOCK_TOP)
		{
				AddBlockItem(door.Item_Block, BLOCK_ITEM_NAME, TEXTURE_ITEM);
				

				AddDoorBlock(door.Bottom_Block,		 		 0, 0, 0,   4, 16, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_BOTTOM, true);
				AddDoorBlock(door.Bottom_Block_Open, 		 0, 0, 0,   16, 4, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_BOTTOM, true);
				AddDoorBlock(door.Bottom_Block_Inverse,	 	 12, 0, 0,   16, 16, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_BOTTOM, true);
				AddDoorBlock(door.Bottom_Block_Inverse_Open, 0, 12, 0,   16, 16, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_BOTTOM, true);
				
				AddDoorBlock(door.Top_Block, 				0,0,0, 4, 16, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_TOP, true);
				AddDoorBlock(door.Top_Block_Open, 			0,0,0, 16, 4, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_TOP, true);
				AddDoorBlock(door.Top_Block_Inverse, 		12, 0, 0,   16, 16, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_TOP, true);
				AddDoorBlock(door.Top_Block_Inverse_Open, 	0, 12, 0,   16, 16, 16, TEXTURE_BLOCK_BOTTOM, TEXTURE_BLOCK_TOP, true);
				
				
				door.Item_Block 		       = (ushort)(door.Item_Block + 256					);
				door.Bottom_Block_Open         = (ushort)(door.Bottom_Block_Open + 256			);
				door.Bottom_Block 		  	   = (ushort)(door.Bottom_Block + 256				);
				door.Bottom_Block_Inverse	   = (ushort)(door.Bottom_Block_Inverse + 256		);
				door.Bottom_Block_Inverse_Open = (ushort)(door.Bottom_Block_Inverse_Open + 256	);
				
				door.Top_Block 					= (ushort)(door.Top_Block + 256						);
				door.Top_Block_Open 			= (ushort)(door.Top_Block_Open + 256				);
				door.Top_Block_Inverse 			= (ushort)(door.Top_Block_Inverse + 256				);
				door.Top_Block_Inverse_Open 	= (ushort)(door.Top_Block_Inverse_Open + 256		);
				
				// (0 0 0) (4 16 16)
				// (0 0 0) (16 16 4)
				// (4 0 0) (4 16 16)?
				// (4 0 0) (16 16 4)?

		}
		public void LoadDoor(DoorConfig config)
		{
			ushort DoorIndexBeginning = (ushort)(DoorBlockIdStorageIndex + (DoorConfigs.IndexOf(config) * 8));
			DoorBlock newDoor = new DoorBlock(){
				Item_Block = config.BLOCK_ITEM_ID,
				Top_Block = 				(ushort)(DoorIndexBeginning + 0),
				Top_Block_Open = 			(ushort)(DoorIndexBeginning + 1),
				Bottom_Block = 				(ushort)(DoorIndexBeginning + 2),
				Bottom_Block_Open = 		(ushort)(DoorIndexBeginning + 3),
				Top_Block_Inverse = 		(ushort)(DoorIndexBeginning + 4),
				Top_Block_Inverse_Open = 	(ushort)(DoorIndexBeginning + 5),
				Bottom_Block_Inverse 	= 	(ushort)(DoorIndexBeginning + 6),
				Bottom_Block_Inverse_Open = (ushort)(DoorIndexBeginning + 7),
			};
			AddDoorBlocks(newDoor, config.BLOCK_ITEM_NAME, config.TEXTURE_ITEM, config.TEXTURE_BLOCK_BOTTOM, config.TEXTURE_BLOCK_TOP);
			DoorTypes.Add(newDoor);
		}
	
		public List<DoorBlock> DoorTypes = new List<DoorBlock>(){};
		public bool IsDoor(BlockID block)
		{
			var b = block;
			DoorBlock d = GetDoorFromBlock(b);
			if ( d == null)
			{
				return false;
			}
			return true;
		}
		public bool IsDoorItem(BlockID block)
		{
			var b = block;
			
			DoorBlock d = GetDoorFromItem(b);
			if ( d == null)
			{
				return false;
			}
			return true;
		}
		public DoorBlock GetDoorFromItem(BlockID block)
		{
			foreach (DoorBlock door in DoorTypes)
			{
				if (door.Item_Block == block)
				{
					return door;
				}
			}
			return null;
		}
		public DoorBlock GetDoorFromBlock(BlockID block)
		{
			foreach (DoorBlock door in DoorTypes)
			{
				if (door.Bottom_Block == block)
				{
					return door;
				}
				if (door.Bottom_Block_Open == block)
				{
					return door;
				}
				if (door.Top_Block == block)
				{
					return door;
				}
				if (door.Top_Block_Open == block)
				{
					return door;
				}
				if (door.Top_Block_Inverse == block)
				{
					return door;
				}
				if (door.Top_Block_Inverse_Open == block)
				{
					return door;
				}
				if (door.Bottom_Block_Inverse == block)
				{
					return door;
				}
				if (door.Bottom_Block_Inverse_Open == block)
				{
					return door;
				}
			}
			return null;
		}
		public bool IsDoor(Level level, ushort x, ushort y, ushort z)
		{
			var b = level.FastGetBlock((ushort)x, (ushort)y, (ushort)z);
			
			return IsDoor(b);
		}
		public bool IsDoorBottom(Level level, ushort x, ushort y, ushort z)
		{
			var b = level.FastGetBlock((ushort)x, (ushort)y, (ushort)z);
			DoorBlock d = GetDoorFromBlock(b);
			if ( d == null)
			{
				return false;
			}
			return ( (b == d.Bottom_Block) || (b == d.Bottom_Block_Open) || (b == d.Bottom_Block_Inverse) || (b == d.Bottom_Block_Inverse_Open));
		}
	
		public void OpenDoor(Level level, ushort x, ushort y, ushort z, bool mute = false)
		{
			BlockID b = level.FastGetBlock((ushort)x, (ushort)(y), (ushort)z);
			DoorBlock d = GetDoorFromBlock(b);
			if ( d == null)
			{
				return;
			}
			if (IsOpen(level, d, b))
			{
				return;
			}
			int offset_y = 0;
			if (!IsDoorBottom(level, x, y, z))
			{
				offset_y = -1;
			}
			ushort result_bottom = (b == d.Top_Block_Inverse || b == d.Bottom_Block_Inverse) ? d.Bottom_Block_Inverse_Open : d.Bottom_Block_Open;
			ushort result_top    = (b == d.Top_Block_Inverse || b == d.Bottom_Block_Inverse) ? d.Top_Block_Inverse_Open    : d.Top_Block_Open;
			level.UpdateBlock(Player.Console, x, (ushort)(y + offset_y    ), z, result_bottom);
			level.UpdateBlock(Player.Console, x, (ushort)(y + offset_y + 1), z, result_top   );
			if (!mute)
			{
				PlaySound3D(level, x, y, z, "dooropen");
			}
			
		}
		public void CloseDoor(Level level, ushort x, ushort y, ushort z)
		{
			BlockID b = level.FastGetBlock((ushort)x, (ushort)(y), (ushort)z);
			DoorBlock d = GetDoorFromBlock(b);
			if ( d == null)
			{
				return;
			}
			if (!IsOpen(level, d, b))
			{
				return;
			}
			int offset_y = 0;
			if (!IsDoorBottom(level, x, y, z))
			{
				offset_y = -1;
			}
			ushort result_bottom = (b == d.Top_Block_Inverse_Open || b == d.Bottom_Block_Inverse_Open) ? d.Bottom_Block_Inverse : d.Bottom_Block;
			ushort result_top    = (b == d.Top_Block_Inverse_Open || b == d.Bottom_Block_Inverse_Open) ? d.Top_Block_Inverse    : d.Top_Block;
			level.UpdateBlock(Player.Console, x, (ushort)(y + offset_y    ), z, result_bottom);
			level.UpdateBlock(Player.Console, x, (ushort)(y + offset_y + 1), z, result_top   );	
			PlaySound3D(level, x, y, z, "doorclose");
		}
		public bool IsOpen(Level level, DoorBlock d, BlockID b)
		{
			return (!( b == d.Top_Block || b == d.Bottom_Block || b == d.Top_Block_Inverse || b == d.Bottom_Block_Inverse));
		}
		public void ToggleDoor(Level level, ushort x, ushort y, ushort z)
		{
			BlockID b = level.FastGetBlock((ushort)x, (ushort)(y), (ushort)z);
			DoorBlock d = GetDoorFromBlock(b);
			if ( d == null)
			{
				return;
			}
			if ( !IsOpen(level, d, b))
			{
				OpenDoor( level, x, y, z);
			}
			else
			{
				CloseDoor(level, x, y, z);
			}
		}
		
		void PlaceDoor(Player p, ushort x, ushort y, ushort z, BlockID block)
		{
			if (!IsDoorItem(block))
			{
				return;
			}
			if ( y > 0 && p.level.FastGetBlock((ushort)x, (ushort)(y-1), (ushort)z) == 0) // if placing above air
			{
				return;
			}
			DoorBlock d = GetDoorFromItem(block);
			
			bool Inverse = ( ((p.Pos.X /32 ) > x) || ((p.Pos.Z /32 ) > z) );
			bool Open = (p.Pos.Z /32 ) < z || (p.Pos.Z /32 ) > z;
			p.level.UpdateBlock(Player.Console, (ushort)x, (ushort)(y  ), (ushort)z, ( Inverse ? d.Bottom_Block_Inverse : d.Bottom_Block));
			p.level.UpdateBlock(Player.Console, (ushort)x, (ushort)(y+1), (ushort)z, ( Inverse ? d.Top_Block_Inverse    : d.Top_Block   ));
			
			if (Open)
			{
				OpenDoor(p.level, x, y, z, true);
			}
		
		}
		void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			/*x /= 16;
			y /= 16;
			z /= 16;*/
			if (x > p.level.Width)
			{
				return;
			}
			if (z > p.level.Length)
			{
				return;
			}
			if (y > p.level.Height)
			{
				return;
			}
			if (!IsDoor(p.level,x,y,z))
			{
				return;
			}
			if (action != MouseAction.Pressed)
			{
				return;
			}
			ToggleDoor(p.level, x, y, z);
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (IsDoor(p.level,x,y,z))
			{
				p.RevertBlock(x, y, z);
				cancel = true;
				//ToggleDoor(p.level, x, y, z);
				//return true;
			}
			if (placing)
			{
				if (IsDoorItem(block))
				{
					PlaceDoor(p, x, y, z, block);
					cancel = true;
					p.RevertBlock(x, y, z);
					//return true;
				}
			}
			CheckShouldDoorBreak(p, x, y, z, block, placing);
        }
		void CheckShouldDoorBreak(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing)
		{
			if (!(block == 0 || !placing))
			{
				return;
			}
			BlockID b = p.level.FastGetBlock((ushort)x, (ushort)(y+1), (ushort)z);
			DoorBlock d = GetDoorFromBlock(b);
			if (d == null)
			{
				return;
			}
			if (! ( b == d.Bottom_Block || b == d.Bottom_Block_Open || b == d.Bottom_Block_Inverse || b == d.Bottom_Block_Inverse_Open))
			{
				return;
			}
			p.level.UpdateBlock(Player.Console, x, (ushort)(y+1), z, 0);
			p.level.UpdateBlock(Player.Console, x, (ushort)(y+2), z, 0);
		}
	}
}