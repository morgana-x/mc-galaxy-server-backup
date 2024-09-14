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
	
	public class SignItem
	{
		public ushort TEXTURE_ID;
		public BlockID BLOCK_ID;
		public string NAME;
	}
	public class SpawnEgg : Plugin {
		public override string name { get { return "sign"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		
		public ushort uniqueId = 0;
		
		public LevelPermission allowedRank { get { return LevelPermission.Admin; } }
		public List<SignItem> SignConfigs = new List<SignItem>(){
			new SignItem()
			{
				TEXTURE_ID = 4,
				BLOCK_ID = 75,
				NAME = "Wooden",
			}
		};

		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);

			foreach (SignItem egg in SignConfigs)
			{
				AddBlockItem(egg.BLOCK_ID, egg.NAME, egg.TEXTURE_ID);
			}

		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			
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
		
		public void SpawnEntity(Level level, Player player,  ushort x, ushort y, ushort z)
		{
			uniqueId++;
			string uniqueName = "sign_" + uniqueId;
			string model = "pig";
			PlayerBot bot = new PlayerBot(uniqueName, level);
			bot.DisplayName = "";
			bot.Model = model;
			//+16 so that it's centered on the block instead of min corner
			Position pos = Position.FromFeet((int)(x*32) +16, (int)(y*32), (int)(z*32) +16);
			bot.SetInitialPos(pos);
			//HandleAdd(Player.Console,  uniqueName,  new string[] {"add", uniqueName, ai, "75"});
			 
			PlayerBot.Add(bot);
			ScriptFile.Parse(Player.Console, bot, "");
			BotsFile.Save(level);
			player.Message("Type the message you want to show on this sign");
		}
		SignItem getSignItem(BlockID block)
		{
			foreach(SignItem egg in SignConfigs)
			{
				if ((ushort)(egg.BLOCK_ID) == (block-256))
				{
					return egg;
				}
			}
			return null;
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (!placing){return;}
	
			SignItem egg = getSignItem(block);
			
			if (egg == null){return;}
			cancel = true;
			p.RevertBlock(x, y, z);
			if (p.Rank < allowedRank)
			{
				p.Message("Need to be rank " + allowedRank.ToString() + "+ to use Spawn Eggs");
				return;
			}
			SpawnEntity(p.level, p, x, y, z);
        }
	}
}