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
	
	public class SpawnEggBlock
	{
		public ushort TEXTURE_ID;
		public BlockID BLOCK_ID;
		public string MODEL;
		public string NAME;
		public string AI;
	}
	public class SpawnEgg : Plugin {
		public override string name { get { return "SpawnEggs"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "Loaded Message!"; } }
		public override string creator { get { return "morgana"; } }
		public override bool LoadAtStartup { get { return true; } }
		
		public ushort uniqueId = 0;
		
		public LevelPermission allowedRank { get { return LevelPermission.Admin; } }
		public List<SpawnEggBlock> EggConfigs = new List<SpawnEggBlock>(){
			new SpawnEggBlock()
			{
				TEXTURE_ID = 128,
				BLOCK_ID = 69,
				MODEL = "sheep",
				NAME = "Sheep",
				AI = "roam"
			},
			new SpawnEggBlock()
			{
				TEXTURE_ID = 129,
				BLOCK_ID = 70,
				MODEL = "zombie",
				NAME = "Zombie",
				AI = "hostile"
			},
			new SpawnEggBlock()
			{
				TEXTURE_ID = 130,
				BLOCK_ID = 71,
				MODEL = "creeper",
				NAME = "Creeper",
				AI = "hostile"
			},
			new SpawnEggBlock()
			{
				TEXTURE_ID = 131,
				BLOCK_ID = 72,
				MODEL = "skeleton",
				NAME = "Skeleton",
				AI = "hostile"
			},
			new SpawnEggBlock()
			{
				TEXTURE_ID = 132,
				BLOCK_ID = 73,
				MODEL = "chicken",
				NAME = "Chicken",
				AI = "roam"
			},
			new SpawnEggBlock()
			{
				TEXTURE_ID = 133,
				BLOCK_ID = 74,
				MODEL = "pig",
				NAME = "Pig",
				AI = "roam"
			}
		};

		
		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);

			foreach (SpawnEggBlock egg in EggConfigs)
			{
				AddBlockItem(egg.BLOCK_ID, egg.NAME, egg.TEXTURE_ID);
			}

			AddAi("hostile", new string[] {"", "hostile", "hostile"});
			AddAi("roam", new string[] {"", "roam", "roam"});
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
        void AddAi( string ai, string[] args) {
			Player p = Player.Console;
			 if (File.Exists("bots/" + ai)) {
			File.Delete("bots/" + ai);
			 }
            if (!File.Exists("bots/" + ai)) {
                p.Message("Created new bot AI: &b" + ai);
                using (StreamWriter w = new StreamWriter("bots/" + ai)) {
                    // For backwards compatibility
                    w.WriteLine("#Version 2");
                }
            }

            string action = args.Length > 2 ? args[2] : "";
            string instruction = ScriptFile.Append(p, ai, action, args);
            if (instruction != null) {
                p.Message("Appended " + instruction + " instruction to bot AI &b" + ai);
            }
        }
		public void SpawnEntity(Level level, string model, string ai, ushort x, ushort y, ushort z, int health=10)
		{
			uniqueId++;
			string uniqueName = model + uniqueId;
			PlayerBot bot = new PlayerBot(uniqueName, level);
			bot.DisplayName = "";
			bot.Model = model;
			bot.Owner = health.ToString();
			//+16 so that it's centered on the block instead of min corner
			Position pos = Position.FromFeet((int)(x*32) +16, (int)(y*32), (int)(z*32) +16);
			bot.SetInitialPos(pos);
			bot.AIName = ai;
			//HandleAdd(Player.Console,  uniqueName,  new string[] {"add", uniqueName, ai, "75"});
			 
			PlayerBot.Add(bot);
			ScriptFile.Parse(Player.Console, bot, ai);
			BotsFile.Save(level);
			
		}
		SpawnEggBlock getEgg(BlockID block)
		{
			foreach(SpawnEggBlock egg in EggConfigs)
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
	
			SpawnEggBlock egg = getEgg(block);
			
			if (egg == null){return;}
			cancel = true;
			p.RevertBlock(x, y, z);
			if (p.Rank < allowedRank)
			{
				p.Message("Need to be rank " + allowedRank.ToString() + "+ to use Spawn Eggs");
				return;
			}
			SpawnEntity(p.level, egg.MODEL, egg.AI, x, y, z);
        }
	}
}