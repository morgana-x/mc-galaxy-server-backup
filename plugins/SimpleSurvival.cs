using System;
using System.IO;
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
using MCGalaxy.Commands;
using MCGalaxy.Bots;
namespace MCGalaxy {

	public class SimpleSurvival : Plugin {
		public override string name { get { return "SimpleSurvival"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override int build { get { return 100; } }
		public override string welcome { get { return "SimpleSurvival"; } }
		public override string creator { get { return "morgana, Venk"; } }
		public override bool LoadAtStartup { get { return true; } }

		public class BlockMineConfig
		{
			public float PickaxeTimeMultiplier  = 1;
			public float ShovelTimeMultiplier = 1;
			public float AxeTimeMultiplier = 1;
			public ushort MiningTime = 5;
			public int overrideBlock = -1;
			public BlockMineConfig(ushort time = 5)
			{
				this.MiningTime = time;
			}
		}
		public class StoneMineConfig : BlockMineConfig
		{
			public StoneMineConfig(ushort time = 15)
			{
				PickaxeTimeMultiplier = 1.5f;
				AxeTimeMultiplier = 0.2f;
				ShovelTimeMultiplier = 0.2f;
				MiningTime = time;
			}
		}
		public class WoodMineConfig : BlockMineConfig
		{
			public WoodMineConfig(ushort time = 10)
			{
				AxeTimeMultiplier = 1.5f;
				PickaxeTimeMultiplier = 1f;
			    ShovelTimeMultiplier = 0.2f;
			 	MiningTime = time;
			}
		}
		public class DirtMineConfig : BlockMineConfig
		{
			public DirtMineConfig(ushort time = 4)
			{
				AxeTimeMultiplier = 0.2f;
			 	PickaxeTimeMultiplier = 1f;
				ShovelTimeMultiplier = 1.5f;
				MiningTime = time;
			}
		}
		public class CraftRecipe
		{
			public CraftRecipe(Dictionary<ushort,ushort> ingredients, ushort amountMultiplier = 1)
			{
				Ingredients = ingredients;
				amountProduced = amountMultiplier;
				//needCraftingTable = needcraftingTable;
			}
			public Dictionary<ushort,ushort> Ingredients = new Dictionary<ushort,ushort>();
			public ushort amountProduced = 1;
			//public bool needCraftingTable = false;
			
		}
		public class Config {
				// Player
				public static int MaxHealth = 20;
				public static int MaxAir = 10;
				public static bool FallDamage = true;
				public static bool VoidKills = true;
				
				// Effects
				public static bool UseGoodlyEffects = true; // broken right now
				public static string HitParticle = "pvp"; // broken right now
				
				// General
				public static string Path = "./plugins/SimpleSurvival/"; // 
				
				
				// MOBS
				public static bool CanKillMobs = true;
				public static bool SpawnMobs = true; // Requires MobAI from https://github.com/ddinan/classicube-stuff/blob/master/MCGalaxy/Plugins/MobAI.cs
				public static int MaxMobs = 50;

				// Mining
				public static bool MiningEnabled = true;

				// Inventory
				public static bool InventoryEnabled = true;
		}
		
		ushort defaultMiningTime = 5;
	
		public Dictionary<ushort, BlockMineConfig> blockMiningTimes = new Dictionary<ushort, BlockMineConfig>()
		{
			// Stone
			{1, new StoneMineConfig(){overrideBlock = 4}},
			{2, new DirtMineConfig(){overrideBlock = 3}},
			{3, new DirtMineConfig()},
			{4, new StoneMineConfig()},
			{5, new WoodMineConfig()},
			{6, new BlockMineConfig(1)},
			{12, new DirtMineConfig(3)},
			{13, new DirtMineConfig(3)},
			{14, new StoneMineConfig(30)},
			{15, new StoneMineConfig(25)},
			{16, new StoneMineConfig(){overrideBlock = 114}},
			{17, new WoodMineConfig()},
			{18, new DirtMineConfig(1)},
			{19, new DirtMineConfig(1)},
			{20, new DirtMineConfig(1)},
			{21, new DirtMineConfig(1)},
			{22, new DirtMineConfig(1)},
			{23, new DirtMineConfig(1)},
			{24, new DirtMineConfig(1)},
			{25, new DirtMineConfig(1)},
			{26, new DirtMineConfig(1)},
			{27, new DirtMineConfig(1)},
			{28, new DirtMineConfig(1)},
			{29, new DirtMineConfig(1)},	
			{30, new DirtMineConfig(1)},
			{31, new DirtMineConfig(1)},
			{32, new DirtMineConfig(1)},
			{34, new DirtMineConfig(1)},
			{35, new DirtMineConfig(1)},
			{36, new DirtMineConfig(1)},
			{37, new DirtMineConfig(1)},
			{38, new DirtMineConfig(1)},
			{39, new DirtMineConfig(1)},
			{41, new StoneMineConfig()},
			{42, new StoneMineConfig()},
			{43, new StoneMineConfig()},
			{44, new StoneMineConfig()},
			{45, new StoneMineConfig()},
			{46, new DirtMineConfig(1)},
			{47, new WoodMineConfig()},
			{48, new StoneMineConfig()},
			{49, new StoneMineConfig(30)},
			{50, new StoneMineConfig()},
			{53, new DirtMineConfig(1)},
			{52, new StoneMineConfig()},
			{60, new StoneMineConfig(10)},
			{61, new StoneMineConfig()},
			{62, new StoneMineConfig()},
			{64, new WoodMineConfig()},
			{54, new BlockMineConfig(0)},
			{65, new StoneMineConfig()},
			{75, new BlockMineConfig(1)},
			{76, new WoodMineConfig()},
			{77, new StoneMineConfig()},
			{78, new StoneMineConfig(){overrideBlock = 77}},
			{79, new StoneMineConfig(4)},
			{80, new StoneMineConfig(4)},
			{81, new StoneMineConfig(4)},
			{82, new StoneMineConfig(4)},
			{85, new BlockMineConfig(1)},
			{86, new StoneMineConfig(25)},
			{87, new StoneMineConfig(25)},
			
		};
		public static Dictionary<ushort, CraftRecipe> craftingRecipies = new Dictionary<ushort, CraftRecipe>()
		{
			// Glass 											// Sand x1 (MOVE TO FURNACE LATER)
			 {20, new CraftRecipe(new Dictionary<ushort, ushort>(){{12, 1}})},
			// Furnace											// Cobblestone x8 = 1x Furnace
			{77, new CraftRecipe(new Dictionary<ushort, ushort>(){{4, 8}})},
			// Crafting Table									// Woodenblock x4 = 1x Crafting table
			{76, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 4}})},
			// Wood												// Log x 1 = 4x Wood planks
			{5, new CraftRecipe(new Dictionary<ushort, ushort>(){{17, 1}}, 4)},
			// Brick
			{45, new CraftRecipe(new Dictionary<ushort, ushort>(){{4, 2}}, 1)},
			// Door												// Wood x 8 = 1x Wooden Door
			{66, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 8}}, 1)},
			// Stick												// Wood x 2 = 1x Stick
			{115, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 2}}, 1)},
			// Torch											// Stick x 1 + Coal x 1 = 4x Torches
			{75, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {114, 1}}, 4)},
			// Glowstone										// 9x torches = 4x glowstone
			{79, new CraftRecipe(new Dictionary<ushort, ushort>(){{75, 9}}, 4)},
			// Bed											// Wood x 3 + Wool x 3 = 1x Bed
			{84, new CraftRecipe(new Dictionary<ushort, ushort>(){{5, 3}, {36, 3}})},
			// Cake											// Wool x 3 = 1x Cake
			{83, new CraftRecipe(new Dictionary<ushort, ushort>(){{36, 3}})},
			// Wooden Sword									// Stick x 1 + wood x 2 = 1x Wooden sword
			{99, new CraftRecipe(new Dictionary<ushort, ushort>(){{115, 1}, {5, 2}})},

		};
		
		
		
		SchedulerTask drownTask;
		SchedulerTask guiTask;
		SchedulerTask regenTask;
		SchedulerTask mobSpawningTask;
		
		Dictionary<ushort, ushort> toolDamage = new Dictionary<ushort, ushort>();
		static Dictionary<ushort, float> toolKnockback = new Dictionary<ushort, float>();

		public static Dictionary<string, Dictionary<string, Dictionary<ushort, ushort>>> playerInventories = new  Dictionary<string, Dictionary<string, Dictionary<ushort, ushort>>>();
		public Dictionary<Player, ushort[]> playerMiningProgress = new Dictionary<Player, ushort[]>();

		public override void Load(bool startup) {
			//LOAD YOUR PLUGIN WITH EVENTS OR OTHER THINGS!
			
			OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
			OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.Low);
			OnBlockChangingEvent.Register(HandleBlockChanged, Priority.High);
			OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
			OnSentMapEvent.Register(HandleSentMap, Priority.Low);
			OnPlayerDyingEvent.Register(HandlePlayerDying, Priority.High);
			Server.MainScheduler.QueueRepeat(HandleDrown, null, TimeSpan.FromMilliseconds(500));
			Server.MainScheduler.QueueRepeat(HandleGUI, null, TimeSpan.FromMilliseconds(100));
			Server.MainScheduler.QueueRepeat(HandleRegeneration, null, TimeSpan.FromSeconds(4));
			Server.MainScheduler.QueueRepeat(HandleMobSpawning, null, TimeSpan.FromSeconds(1));
			Command.Register(new CmdPvP());
			Command.Register(new CmdGiveBlock());
			Command.Register(new CmdCraft());
			
			loadMaps();
			foreach (Player p in PlayerInfo.Online.Items)
			{
				InitPlayer(p);
			}
			
			if(!Directory.Exists(Config.Path))
			{
				Directory.CreateDirectory(Config.Path);
			}
			if (true)
			{
				AddAi("hostile", new string[] {"", "hostile", "hostile"});
				AddAi("roam", new string[] {"", "roam", "roam"});
			}
			mobHealth.Clear();
			toolDamage.Clear();
			toolKnockback.Clear();
			for (ushort i=90;i<=110;i++)
			{
				toolDamage.Add(i, 3);
			}
			toolDamage[91] = 6; // iron
			toolDamage[95] = 5; // stone
			toolDamage[99] = 4; // wood
			toolDamage[103] = 7; // gold
			toolDamage[107] = 8; // diamond
			for (ushort i=90;i<=110;i++)
			{
				toolKnockback.Add(i, 1.1f);
			}
			toolKnockback[91] = 1.6f;
			toolKnockback[95] = 1.4f;
			toolKnockback[99] = 1.2f;
			toolKnockback[103] = 1.8f;
			toolKnockback[107] = 2.2f;


			foreach(Player p in PlayerInfo.Online.Items)
			{
				SendMiningUnbreakableMessage(p);
			}
		}
                        
		public override void Unload(bool shutdown) {
			//UNLOAD YOUR PLUGIN BY SAVING FILES OR DISPOSING OBJECTS!
			OnPlayerClickEvent.Unregister(HandleBlockClicked);
			OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
			OnPlayerMoveEvent.Unregister(HandlePlayerMove);
			OnBlockChangingEvent.Unregister(HandleBlockChanged);
			OnSentMapEvent.Unregister(HandleSentMap);
			OnPlayerDyingEvent.Unregister(HandlePlayerDying);
			
			Server.MainScheduler.Cancel(drownTask);
			Server.MainScheduler.Cancel(guiTask);
			Server.MainScheduler.Cancel(regenTask);
			Server.MainScheduler.Cancel(mobSpawningTask);
			
			Command.Unregister(Command.Find("PvP"));
			Command.Unregister(Command.Find("GiveBlock"));
			Command.Unregister(Command.Find("Craft"));
			mobHealth.Clear();
		}
		public override void Help(Player p) {
			//HELP INFO!
		}
		

		public static List<string> maplist = new List<string>();
		void loadMaps()
        {
            if (File.Exists(Config.Path + "maps.txt"))
            {
                using (var maplistreader = new StreamReader(Config.Path + "maps.txt"))
                {
                    string line;
                    while ((line = maplistreader.ReadLine()) != null)
                    {
                        maplist.Add(line);
                    }
                }
            }
            else File.Create(Config.Path+ "maps.txt").Dispose();
        }

		///////////////////////////////////////////////////////////
		// Inventory
		///////////////////////////////////////////////////////////
		public Dictionary<ushort,ushort> getPlayerInventory(Player pl )
		{
			Dictionary<ushort, ushort> nullInventory = new Dictionary<ushort, ushort>();
			if (!playerInventories.ContainsKey(pl.level.name))
				return nullInventory;
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				return nullInventory;
			return playerInventories[pl.level.name][pl.name];
		}
		public void InventorySetBlock(Player pl, ushort block, ushort amount)
		{
			
			if (!playerInventories.ContainsKey(pl.level.name))
				playerInventories.Add(pl.level.name, new Dictionary<string, Dictionary<ushort, ushort>>());
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				playerInventories[pl.level.name].Add(pl.name, new Dictionary<ushort, ushort>());
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				playerInventories[pl.level.name][pl.name].Add(block, 0);
			playerInventories[pl.level.name][pl.name][block] = amount;
			SendMiningUnbreakableMessage(pl, block);
		}

		BlockMineConfig getBlockMineTime(ushort blockId)
		{
			if (blockId > 256)
				blockId = (ushort)(blockId - 256);
			if (!blockMiningTimes.ContainsKey(blockId))
				return null;
			return blockMiningTimes[blockId];
		}
		public static void InventoryAddBlocks(Player pl, ushort block, int amount)
		{
			if (amount < 0)
			{
				InventoryRemoveBlocks(pl, block, (ushort)(Math.Abs(amount)));
				return;
			}
			InventoryAddBlocks(pl, block, (ushort)amount);
		}
		public static void InventoryAddBlocks(Player pl, ushort block, ushort amount)
		{
			ushort oldBlock = block;
			if (block > 256)
				block = (ushort)(block - 256);
			if (!playerInventories.ContainsKey(pl.level.name))
				playerInventories.Add(pl.level.name, new Dictionary<string, Dictionary<ushort, ushort>>());
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				playerInventories[pl.level.name].Add(pl.name, new Dictionary<ushort, ushort>());
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				playerInventories[pl.level.name][pl.name].Add(block, 0);
			playerInventories[pl.level.name][pl.name][block] = (ushort)(playerInventories[pl.level.name][pl.name][block]  + amount);
			SendMiningUnbreakableMessage(pl, block);
		}
		public static void InventoryRemoveBlocks(Player pl, ushort block, ushort amount)
		{
			ushort oldBlock = block;
			if (block > 256)
				block = (ushort)(block - 256);
			if (!playerInventories.ContainsKey(pl.level.name))
				return;
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				return;
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				return;
			if (amount >= playerInventories[pl.level.name][pl.name][block])
			{
				playerInventories[pl.level.name][pl.name].Remove(block);
				return;
			}
			playerInventories[pl.level.name][pl.name][block] = (ushort)(playerInventories[pl.level.name][pl.name][block] - amount);
			SendMiningUnbreakableMessage(pl, block);
		}
		public static bool InventoryHasEnoughBlock(Player pl, ushort block, ushort amount=1)
		{
			return InventoryGetBlockAmount(pl, block) >= amount;
		}
		public static ushort InventoryGetBlockAmount(Player pl, ushort block)
		{
			if (block > 256)
				block = (ushort)(block - 256);
			if (!playerInventories.ContainsKey(pl.level.name))
				return 0;
			if (!playerInventories[pl.level.name].ContainsKey(pl.name))
				return 0;
			if (!playerInventories[pl.level.name][pl.name].ContainsKey(block))
				return 0;
			return playerInventories[pl.level.name][pl.name][block];
		}
		public static void Craft(Player pl, ushort block, ushort amount=1)
		{
			if (!craftingRecipies.ContainsKey(block))
			{
				pl.Message("Recipe doesn't exist for this block!");
				return;
			}
			foreach(var pair in craftingRecipies[block].Ingredients)
			{
				if (!InventoryHasEnoughBlock(pl, pair.Key, (ushort)(pair.Value * amount)))
				{
					pl.Message("Not enough items!");
					return;
				}
			}
			foreach(var pair in craftingRecipies[block].Ingredients)
			{
				InventoryRemoveBlocks(pl, pair.Key, (ushort)(pair.Value * amount));
			}
			InventoryAddBlocks(pl, block, (ushort)(craftingRecipies[block].amountProduced * amount));
			SetHeldBlock(pl, 0);
			SetHeldBlock(pl, block);
		}
		public static Dictionary<ushort,CraftRecipe>  GenerateCraftOptions(Player pl)
		{
			Dictionary<ushort,CraftRecipe> validCraftables = new Dictionary<ushort,CraftRecipe> ();
			foreach(var recipePair in craftingRecipies)
			{
				bool valid = true;
				foreach(var pair in recipePair.Value.Ingredients)
				{
					if (!InventoryHasEnoughBlock(pl, pair.Key, pair.Value))
					{
						valid = false;
						break;
					}
				}
				if (!valid)
					continue;
				validCraftables.Add(recipePair.Key, recipePair.Value);
			}
			return validCraftables;
		}
		public static string GenerateCraftOptionsMessage(Player p)
		{
			Dictionary<ushort,CraftRecipe> validRecipes = GenerateCraftOptions(p);
			string message = "Valid craftables:\n";
			/*for (int i=0; i< validRecipes.Keys.Count; i++)
			{
				message += i.ToString() + ". " + validRecipes.Keys[i];
			}*/
			return message;
		}
		///////////////////////////////////////////////////////////
		// Mining
		///////////////////////////////////////////////////////////
		private static void SendMiningUnbreakableMessage(Player p)
		{
			if (!Config.MiningEnabled)
				return;
			bool extBlocks = p.Session.hasExtBlocks;
            int count = p.Session.MaxRawBlock + 1;
            int size  = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * size];
            Level level = p.level;
            for (int i = 0; i < count; i++) 
            {
                Packet.WriteBlockPermission((BlockID)i, i != 0 ? InventoryHasEnoughBlock(p, (ushort)i) : true, i == 0 ? true : false, p.Session.hasExtBlocks, bulk, i * size);
            }
            p.Send(bulk);
		}
		private static void SendMiningUnbreakableMessage(Player p, BlockID id)
		{
			if (!Config.MiningEnabled)
				return;
			bool extBlocks = p.Session.hasExtBlocks;
            int count = 1;//p.Session.MaxRawBlock + 1;
            int size  = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * size];
			if (id > 256)
				id = (ushort)(id - 256);
            Packet.WriteBlockPermission((BlockID)id, id != 0 ? InventoryHasEnoughBlock(p, (ushort)id) : true, id == 0 ? true : false, p.Session.hasExtBlocks, bulk, 0);
            p.Send(bulk);
		}
		private void MineBlock(Player pl, ushort[] pos)
		{
			ushort blockType = pl.level.GetBlock(pos[0], pos[1], pos[2]);
			ushort oldBlockType = blockType;
			if (blockType == 0 || blockType == 0xff)
				return;
			BlockMineConfig blockMineData = getBlockMineTime(blockType);
			if (blockMineData == null)
				return;
			if (!playerMiningProgress.ContainsKey(pl))
			{
				playerMiningProgress.Add(pl, new ushort[5]{blockType,0,pos[0], pos[1], pos[2]});
				return;
			}
			ushort[] currentProgress = playerMiningProgress[pl];
			if (currentProgress[0] != blockType || (pos[0] != currentProgress[2] || pos[1] != currentProgress[3] || pos[2] != currentProgress[4]))
			{
				playerMiningProgress[pl] = new ushort[5]{blockType,0,pos[0], pos[1], pos[2]};
				return;
			}
			playerMiningProgress[pl][1] += 2;

			if (playerMiningProgress[pl][1] < blockMineData.MiningTime)
				return;
			playerMiningProgress.Remove(pl);
			pl.level.UpdateBlock(pl, pos[0], pos[1], pos[2], 0);
			OnBlockChangedEvent.Call(Player.Console, pos[0], pos[1], pos[2], ChangeResult.Modified);
			if (blockMineData.overrideBlock != -1)
			{
				blockType = (ushort)blockMineData.overrideBlock;
				oldBlockType = blockType;
				if (blockType > 256)
					blockType = (ushort)(blockType -256);
			}
			InventoryAddBlocks(pl, blockType, 1);
			if (InventoryGetBlockAmount(pl, blockType) == 1)
			{
				SetHeldBlock(pl, 0);
				SetHeldBlock(pl, blockType);
			}
		}
		private void UnMineBlock(Player pl)
		{
			if (!playerMiningProgress.ContainsKey(pl))
				return;
			ushort lastProg = 0;
			try
			{
			 	lastProg = (ushort)pl.Extras["MINING_LASTPROG"];
			}
			catch
			{
				pl.Extras["MINING_LASTPROG"] = (ushort)0;
			}
			if (lastProg < playerMiningProgress[pl][1])
			{
				pl.Extras["MINING_LASTPROG"] = playerMiningProgress[pl][1];
				return;
			}
			playerMiningProgress[pl][1]--;
			pl.Extras["MINING_LASTPROG"] = playerMiningProgress[pl][1];
			if (playerMiningProgress[pl][1] > 1)
				return;
			playerMiningProgress.Remove(pl);
		}
		///////////////////////////////////////////////////////////
		// Handlers
		///////////////////////////////////////////////////////////
		void HandleGUI(SchedulerTask task)
        {
            guiTask = task;

            foreach (Player pl in PlayerInfo.Online.Items)
            {
				SendPlayerGui(pl);
			}
		}
		
		void HandlePlayerDying(Player p, BlockID deathblock, ref bool cancel)
        {
			
			if (!maplist.Contains(p.level.name))
			{
				SetGuiText(p,"","");
				p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
				p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
				return;
			}
			if (deathblock == Block.Orange)
			{
				string deathMessage = p.color +  p.name + " %e was killed by a monster.";// + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			else if (deathblock == 54)
			{
				string deathMessage = p.color +  p.name + " %e burned to death.";// + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			InitPlayer(p);
        }
		void HandleSentMap( Player p, Level prevLevel, Level level)
		{
			if (!maplist.Contains(level.name))
			{
				SetGuiText(p,"","");
				p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
				p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
				p.Extras["MINING_LASTPROG"] = 0;
				return;
			}
			InitPlayer(p);
			p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
			p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
			p.Extras["MINING_LASTPROG"] = 0;
			SendPlayerGui(p);
			SendMiningUnbreakableMessage(p);
		}
		void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
			if (p == Player.Console)
				return;
			if (placing == false && Config.MiningEnabled)
			{
				cancel = true;
				p.RevertBlock(x,y,z);
				return;
			}
			if (cancel || !placing)
				return;
			if (!Config.InventoryEnabled)
				return;
			if (!InventoryHasEnoughBlock(p, block))
			{
				cancel = true;
				p.RevertBlock(x,y,z);
				p.Message("Insufficent amount of block \"" + block.ToString() + "\".");
				return;
			}
			InventoryRemoveBlocks(p, block, 1);
			SendMiningUnbreakableMessage(p, block);
		}
		void HandleDrown(SchedulerTask task)
		{
			drownTask = task;
            foreach (Player p in PlayerInfo.Online.Items)
			{
				if (!maplist.Contains(p.level.name)) continue;
				UnMineBlock(p);
				if (p.invincible) continue;
				if (IsDrowning(p))
				{
					if (GetAir(p) > 0)
					{
						SetAir(p, GetAir(p)-1);
						SendPlayerGui(p);
					}
					else
					{
						Damage(p, 1, 8); 
					}
				}
				else if (GetAir(p) < Config.MaxAir)
				{
					SetAir(p, GetAir(p)+1);
				}
				
				if (IsBurning(p))
				{
					Damage(p, 3, 54); 
				}
				
			}
		}
		void HandlePlayerMove(Player p, Position next, byte rotX, byte rotY, ref bool cancel)
		{
			if (!maplist.Contains(p.level.name)) return;
			if (Config.VoidKills && next.Y < 0) Die(p, 4); // Player fell out of the world
			
			if (Config.FallDamage)
			{
				if (p.invincible) return;// || Hacks.CanUseFly(p)) return;

				ushort x = (ushort)(p.Pos.X / 32);
				ushort y = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) - 1);
				//ushort y2 = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) - 2);
				ushort z = (ushort)(p.Pos.Z / 32);

				BlockID block = p.level.GetBlock((ushort)x, ((ushort)y), (ushort)z);
				//BlockID block2 = p.level.GetBlock((ushort)x, ((ushort)y2), (ushort)z);

				string below = Block.GetName(p, block);
				//string below2 = Block.GetName(p, block2);

			
				if (below.ToLower().Contains("water"))// && below2.ToLower().Contains("water"))
				{
					int fall = p.Extras.GetInt("FALL_START") - y;
					//if (fallDamage(fall) > 0 && p.Session.ClientName().CaselessContains("cef")) p.Message("cef resume -n splash"); // Play splash sound effect
					p.Extras["FALLING"] = false;
					p.Extras["FALL_START"] = y;
					return;
				}

				if (!p.Extras.GetBoolean("FALLING") && below.ToLower() == "air")
				{
					p.Extras["FALLING"] = true;
					p.Extras["FALL_START"] = y;
				}

				else if (p.Extras.GetBoolean("FALLING") && below.ToLower() != "air")
				{
					if (p.Extras.GetBoolean("FALLING"))
					{
						int fall = p.Extras.GetInt("FALL_START") - y;

						if (fallDamage(fall) > 0){
						Damage(p, fallDamage(fall), 0);
						}

						// Reset extra variables
						p.Extras["FALLING"] = false;
						p.Extras["FALL_START"] = y;
					}
				}
			}
            
		}
        void HandlePlayerConnect(Player p)
		{
			if (!maplist.Contains(p.level.name)) return;
			InitPlayer(p);
			SendMiningUnbreakableMessage(p);
		}
		int GetLagCompensation(int ping)
		{
			int penalty = 0;

			if (ping == 0) penalty = 0; // "lagged-out"
			if (ping > 0 && ping <= 29) penalty = 50; // "great"
			if (ping > 29 && ping <= 59) penalty = 100; // "good"
			if (ping > 59 && ping <= 119) penalty = 150; // "okay"
			if (ping > 119 && ping <= 180) penalty = 200; // "bad"
			if (ping > 180) penalty = 250; // "horrible"
			return penalty;
		}
		Random rnd = new Random();
		PlayerBot[] GetMobsInLevel(Level lvl)
		{
			List<PlayerBot> players = new List<PlayerBot>();
			foreach (PlayerBot bot in lvl.Bots.Items)
			{
				if (bot.DisplayName != "" || !bot.name.Contains("ssmob"))
				{
					continue;
				}
				players.Add(bot);
			}
			return players.ToArray();
		}
		void CheckDespawn(Level level)
		{
			foreach (PlayerBot bot in level.Bots.Items)
			{
				if (bot.DisplayName != "" || !bot.name.Contains("ssmob"))
				{
					continue;
				}
				if (GetPlayersInLevel(level).Length < 1)
				{
					PlayerBot.Remove(bot);
					continue;
				}
				if (bot.Pos.BlockX > level.Width || bot.Pos.BlockY < 0 || bot.Pos.BlockZ > level.Length)
				{
					PlayerBot.Remove(bot);
				}
				BlockID gb = level.FastGetBlock((ushort)bot.Pos.BlockX, (ushort)(bot.Pos.BlockY+1), (ushort)bot.Pos.BlockZ);
				if (gb == 9 || gb == 8 || gb == 10 || gb == 11) // water, lava etc
				{
					PlayerBot.Remove(bot);
					continue;
				}
				int shortestDist = 650;
				foreach (Player p in GetPlayersInLevel(level)) // Don't want creepers spawning inside us now do we
				{
					int x = bot.Pos.BlockX;
					int y = bot.Pos.BlockY;
					int z = bot.Pos.BlockZ;
					int dx = (int)(p.Pos.BlockX) - x, dy = (int)(p.Pos.BlockY) - y, dz = (int)(p.Pos.BlockZ) -z;
					int playerDist = Math.Abs(dx) /*+ Math.Abs(dy)*/ + Math.Abs(dz);
					if (playerDist < shortestDist)
					{
						shortestDist = playerDist;
					}
				}
				if (shortestDist >= 210)
				{
					PlayerBot.Remove(bot);
				}
			}
		}
		Player[] GetPlayersInLevel(Level lvl)
		{
			List<Player> players = new List<Player>();
			foreach (Player p in PlayerInfo.Online.Items)
			{
				if (p.level == lvl)
				{
					players.Add(p);
				}
			}
			return players.ToArray();
		}
		void HandleMobSpawning(SchedulerTask task)
		{
			mobSpawningTask = task;
			if (!Config.SpawnMobs)
			{
				return;
			}
			Level[] levels = LevelInfo.Loaded.Items;
			if (PlayerInfo.Online.Items.Length < 1)
			{
				foreach (Level lvl in levels)
				{
					if (!maplist.Contains(lvl.name))
					{
						continue;
					}
					CheckDespawn(lvl);
				}
				return;
			}
			
			
			
			
			
			foreach (Level lvl in levels)
			{
				if (!maplist.Contains(lvl.name))
				{
					continue;
				}
				CheckDespawn(lvl);
				Player[] players = GetPlayersInLevel(lvl);
				if (players.Length < 1)
					continue;
				if (GetMobsInLevel(lvl).Length >= (Config.MaxMobs))
					continue;
			

				Player selectedPlayer = players[rnd.Next(players.Length)] ;
				if (selectedPlayer == null)
				{
					continue;
				}

				ushort x = (ushort)(	selectedPlayer.Pos.BlockX + (	rnd.Next(25, 128) * ((rnd.Next(2) == 1 ? 1 : -1)))	);
				ushort z = (ushort)(	selectedPlayer.Pos.BlockZ + (	rnd.Next(25, 128) * ((rnd.Next(2) == 1 ? 1 : -1)))	);
				if (x >= lvl.Width)
				{
					x = (ushort)(lvl.Width-1);
				}
				if (z >= lvl.Length)
				{
					z = (ushort)(lvl.Length-1);
				}
				if (x < 0)
				{
					x = 0;
				}
				if (z < 0)
				{
					z = 0;
				}
				ushort y = FindGround(lvl, x, lvl.Height, z);
				if (y < 0)
				{
					y = 0;
				}
				if (y>1)
				{
					BlockID gb = lvl.FastGetBlock(x, (ushort)(y-1), z);
					if (gb == 9 || gb == 8 || gb == 10 || gb == 11) // water, lava etc
					{
						continue;
					}
				}
				switch (rnd.Next(12))
				{
					case 1:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					case 2:
						SpawnEntity(lvl, "pig", "roam", x, y, z);
						break;
					case 3:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
					case 4:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					case 5:
						SpawnEntity(lvl, "zombie", "hostile", x, y, z);
						break;
					case 6:
						SpawnEntity(lvl, "skeleton", "hostile", x, y, z);
						break;
					case 7:
						SpawnEntity(lvl, "spider", "hostile", x, y, z);
						break;
					case 8: // Creeper? aww man
						SpawnEntity(lvl, "creeper", "hostile", x, y, z); // So we back in the mine 
						break; // Swinging that pickaxe side to side, side side to side // HEADS UP TOTAL SHOCK FILLS YOUR BODY // OH NO ITS YOU AGAIN
					case 9:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
					case 10:
						SpawnEntity(lvl, "pig", "roam", x, y, z);
						break;
					case 11:
						SpawnEntity(lvl, "sheep", "roam", x, y, z);
						break;
					default:
						SpawnEntity(lvl, "chicken", "roam", x, y, z);
						break;
				}
			}
		}
		static bool CanHitPlayer(Player p, Player victim)
        {
            Vec3F32 delta = p.Pos.ToVec3F32() - victim.Pos.ToVec3F32();
            float reachSq = 12f; // 3.46410161514 block reach distance

            int ping = p.Session.Ping.AveragePing();

            if (ping > 59 && ping <= 119) reachSq = 16f; // "okay"
            if (ping > 119 && ping <= 180) reachSq = 16f; // "bad"
            if (ping > 180) reachSq = 16f; // "horrible"

            // Don't allow clicking on players further away than their reach distance
            if (delta.LengthSquared > (reachSq + 1)) return false;

            // Check if they can kill players, determined by gamemode plugins
            //bool canKill = PvP.Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
            //if (!canKill) return false;

            if (p.Game.Referee || victim.Game.Referee || p.invincible || victim.invincible) return false; // Ref or invincible
            if (inSafeZone(p, p.level) || inSafeZone(victim, victim.level)) return false; // Either player is in a safezone

            if (!string.IsNullOrWhiteSpace(p.Extras.GetString("TEAM")) && (p.Extras.GetString("TEAM") == victim.Extras.GetString("TEAM")))
            {
                return false; // Players are on the same team
            }

            BlockID b = p.GetHeldBlock();

            if (Block.GetName(p, b).ToLower() == "bow") return false; // Bow damage comes from arrows, not player click

            // If all checks are complete, return true to allow knockback and damage
            return true;
        }
		static bool CanHitMob(Player p, PlayerBot victim)
        {
			if (victim.AIName == "")
				return false;
            Vec3F32 delta = p.Pos.ToVec3F32() - victim.Pos.ToVec3F32();
            float reachSq = 12f; // 3.46410161514 block reach distance

            int ping = p.Session.Ping.AveragePing();

            if (ping > 100) reachSq = 40f; // "horrible"

            // Don't allow clicking on players further away than their reach distance
            if (delta.LengthSquared > (reachSq + 1)) return false;

            // Check if they can kill players, determined by gamemode plugins
            //bool canKill = PvP.Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
            //if (!canKill) return false;

            if (p.Game.Referee || p.invincible ) return false; // Ref or invincible
            return true;
        }
		Dictionary<PlayerBot, int> mobHealth = new Dictionary<PlayerBot, int>();
		
		

		ushort getDamage(Player p)
		{
			ushort block = p.GetHeldBlock();
			if (block >= 66)
				block = (ushort)(block - 256);
			if (!toolDamage.ContainsKey(block))
				return 2;
			if (!InventoryHasEnoughBlock(p, block))
				return 2;
			return toolDamage[block];
		}
		static float getKnockback(Player p)
		{
			ushort block = p.GetHeldBlock();
			if (block >= 66)
				block = (ushort)(block - 256);
			if (!toolKnockback.ContainsKey(block))
				return 1f;
			if (!InventoryHasEnoughBlock(p, block))
				return 1f;
			return toolKnockback[block];
		}
		void HandleAttackMob (Player p, byte entity)
		{
			if (!Config.CanKillMobs)
			{
				return;
			}
			PlayerBot mob = null;
			foreach( PlayerBot b in p.level.Bots.Items)
			{
				if (b.EntityID == entity)
				{
					mob = b;
					break;
				}
			}
			if (mob == null)
			{
				return;
			}
			if (!CanHitMob(p, mob))
			{
				return;
			}
			HurtBot(getDamage(p), mob, p);
		}
	    public static void HurtBot(int damage, PlayerBot hit, Player bot)
        {
            int srcHeight = ModelInfo.CalcEyeHeight(hit);
            int dstHeight = ModelInfo.CalcEyeHeight(bot);
            int dx = bot.Pos.X - hit.Pos.X, dy = (bot.Pos.Y + srcHeight) - (hit.Pos.Y + dstHeight), dz = bot.Pos.Z - hit.Pos.Z;
            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);

            float mult = 1 / ModelInfo.GetRawScale(hit.Model);
            float plScale = ModelInfo.GetRawScale(hit.Model);

            Position newPos;
            newPos.X = hit.Pos.X + (int)(hit.Pos.X - bot.Pos.X);
            newPos.Y = hit.Pos.Y;// + 32;
            newPos.Z = hit.Pos.Z + (int)(hit.Pos.Z - bot.Pos.Z);

            Position newMidPos;
            newMidPos.X = hit.Pos.X + (int)((hit.Pos.X - bot.Pos.X) / 2);
            newMidPos.Y = hit.Pos.Y;// + 32;
            newMidPos.Z = hit.Pos.Z + (int)((hit.Pos.Z - bot.Pos.Z) / 2);

            if (hit.level.IsAirAt((ushort)newPos.BlockX, (ushort)newPos.BlockY, (ushort)newPos.BlockZ) && hit.level.IsAirAt((ushort)newPos.BlockX, (ushort)(newPos.BlockY - 1), (ushort)newPos.BlockZ) &&
            hit.level.IsAirAt((ushort)newMidPos.BlockX, (ushort)newMidPos.BlockY, (ushort)newMidPos.BlockZ) && hit.level.IsAirAt((ushort)newMidPos.BlockX, (ushort)(newMidPos.BlockY - 1), (ushort)newMidPos.BlockZ))
            {
                hit.Pos = new Position(newPos.X, newPos.Y, newPos.Z);
            }

            int hp;
            bool isNumber = int.TryParse(hit.Owner, out hp);
            if (hit.Owner == null || !isNumber) return;

            hit.Owner = (hp - damage).ToString();

            if (hp <= 0)
            {
                // Despawn bot
                //Command.Find("Effect").Use(p, "smoke " + bot.Pos.FeetBlockCoords.X + " " + bot.Pos.FeetBlockCoords.Y + " " + bot.Pos.FeetBlockCoords.Z + " 0 0 0 true");
                PlayerBot.Remove(hit);
            }
        }
		void AttemptPvp(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (!maplist.Contains(p.level.name)) return;
			if (button != MouseButton.Left) return;
			//if (action != MouseAction.Released) return;
			Player victim = null; // If not null, the player that is being hit

			Player[] players = PlayerInfo.Online.Items;

			foreach (Player pl in players)
			{
				// Clicked on a player

				if (pl.EntityID == entity)
				{
					victim = pl;
					break;
				}
			}
			
			if (victim == null)
			{
				p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow.AddMilliseconds(550 - GetLagCompensation(p.Session.Ping.AveragePing()));
				try
				{
				HandleAttackMob(p, entity);
				}
				catch
				{
				}
				return;
			}
			if (!p.Extras.Contains("PVP_HIT_COOLDOWN"))
			{
				p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow;
			}
			DateTime lastClickTime = (DateTime)p.Extras.Get("PVP_HIT_COOLDOWN");

			if (lastClickTime > DateTime.UtcNow) return;
			
			if (!CanHitPlayer(p, victim)) return;
			DoHit(p, victim);
			p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow.AddMilliseconds(400 - GetLagCompensation(p.Session.Ping.AveragePing()));
		}
		void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
		{
			if (Config.MiningEnabled && button == MouseButton.Left && entity == 255)
				MineBlock(p, new ushort[]{x, y, z});
			AttemptPvp(p, button, action, yaw, pitch, entity, x, y, z, face);
		}
		void HandleRegeneration(SchedulerTask task)
        {
            regenTask = task;
            foreach (Player pl in PlayerInfo.Online.Items)
            {
				if (!maplist.Contains(pl.level.name)) continue;
                int health = GetHealth(pl);

                if (health >= Config.MaxHealth) continue; // No need to regenerate health if player is already at max health

                pl.Extras["SURVIVAL_HEALTH"] = health + 1;
            }
        }
		///////////////////////////////////////////////////////////////////////////
		// GUI
		///////////////////////////////////////////////////////////////////////////
		static string GetHealthBar(int health)
		{
			if (health < 0)
			{
				health = 0;
			}

			int repeatHealth = (int)Math.Round(( (float) health / (float)Config.MaxHealth) * 10f);

			int repeatDepletedHealth = 10 - repeatHealth;


			return ("%c" + new string('♥', repeatHealth )) + "%8" + new string('♥', repeatDepletedHealth ) ;
		}
		static string GetAirBar(int air)
		{
			if (air < 0)
			{
				air = 0;
			}
			if (air >= Config.MaxAir)
			{
				return "";
			}
			int repeat = air; //(int)Math.Round((double)(air/Config.MaxAir) * 10);
			return ("%9" + new string('♥', repeat)+ "%8" + new string('♥', Config.MaxAir-air ));
		}
		string getHeldBlockAmount(Player p)
		{
			ushort block = p.GetHeldBlock();
			if (block <= 0)
				return "";
			ushort amount = InventoryGetBlockAmount(p, block);
			/*if (amount == 0)
			{
				//SetHeldBlock(p, 0);
				return "";
			}*/
			return amount.ToString();
		}
		string getMiningProgressBar(Player p)
		{
			if (!playerMiningProgress.ContainsKey(p))
				return "";
			if (playerMiningProgress[p][0] == 255)
				return "";
			BlockMineConfig blockMineData = getBlockMineTime(playerMiningProgress[p][0]);
			if (blockMineData == null)
				return "";
			if (playerMiningProgress[p][1] > blockMineData.MiningTime)
				return "";
			int amount =  Math.Min(10, (int) (( (float)playerMiningProgress[p][1] / (float)blockMineData.MiningTime) * 10));

			return "%7" + new string('#', amount) + "%8" + new string('#', 10 - amount);
		}
		void SendPlayerGui(Player p)
		{
			if (!maplist.Contains(p.level.name)) return;
			SetGuiText(p, GetHealthBar	(GetHealth	(p)),GetAirBar		(GetAir		(p)), getHeldBlockAmount(p));

			p.SendCpeMessage(CpeMessageType.SmallAnnouncement, getMiningProgressBar(p));
		}
		///////////////////////////////////////////////////////////////////////////
		// UTILITIES
		///////////////////////////////////////////////////////////////////////////
		int fallDamage(int height)
		{
			if (height < 4)
			{
				return 0;
			}
			return (height-4);
		}
		public void SetHealth(Player p, int health)
		{
			p.Extras["SURVIVAL_HEALTH"] = health;
		}
		public int GetHealth(Player p)
		{
			return p.Extras.GetInt("SURVIVAL_HEALTH");
		}
		public int GetAir(Player p)
		{
			return p.Extras.GetInt("SURVIVAL_AIR");
		}
		public void SetAir(Player p, int air)
		{
			p.Extras["SURVIVAL_AIR"] = air;
		}
		public static void SetHeldBlock(Player p, ushort blockId)
		{
			 if (!p.Supports(CpeExt.HeldBlock))
			 	return;
			p.Session.SendHoldThis(blockId, false);
		}
		public bool IsBurning(Player p)
		{
			bool burning = false;
			try
			{
				BlockID b = p.level.FastGetBlock((ushort)p.Pos.BlockX,(ushort)p.Pos.BlockY, (ushort)p.Pos.BlockZ);
				BlockID b2 = p.level.FastGetBlock((ushort)p.Pos.BlockX,(ushort)(p.Pos.BlockY - 1), (ushort)p.Pos.BlockZ);

				burning = (/*p.level.Props[b].KillerBlock &&*/ (b == 10 || b == 11 || b == 54)) || (/*p.level.Props[b2].KillerBlock && */ (b2 == 10 || b2 == 11 || b2 == 54));
			}
			catch
			{
			}
			return burning;
		}
		bool IsDrowning(Player p)
		{
			ushort x = (ushort)(p.Pos.X / 32);
			ushort y = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32));
			ushort z = (ushort)(p.Pos.Z / 32);
			bool drowning = false;
			try
			{
				BlockID bHead = p.level.FastGetBlock((ushort)x, (ushort)(y+1), (ushort)z);

				drowning = (p.level.FastGetBlock((ushort)x, (ushort)y, (ushort)z) != 0) && p.level.Props[bHead].Drownable;
			}
			catch
			{
				drowning = false;
			}
			return drowning;
		}
		public void Damage(Player p, int amount, BlockID reason = 0)
		{
			SetHealth(p, GetHealth(p) - amount);
			if (GetHealth(p) <= 0)
			{
				// die
				Die(p, reason);
			}
			SendPlayerGui(p);
		}
		public void Die(Player p, BlockID reason = 4)
		{
			p.HandleDeath(reason, immediate: true);	
			InitPlayer(p);
		}
		static void SetGuiText(Player p, string top, string bottom, string bottom2 = "")
		{
			p.SendCpeMessage(CpeMessageType.Status1, top);
			p.SendCpeMessage(CpeMessageType.Status2, bottom);
			p.SendCpeMessage(CpeMessageType.Status3, bottom2);
		}
		public void InitPlayer(Player p)
		{
			SetGuiText(p, "","");
			p.Extras["SURVIVAL_HEALTH"] = Config.MaxHealth;
			p.Extras["SURVIVAL_AIR"] = Config.MaxAir;
			p.Extras["PVP_HIT_COOLDOWN"] = DateTime.UtcNow;
			p.Extras["FALLING"] = false;
			p.Extras["FALL_START"] = p.Pos.Y;
			SendPlayerGui(p);
		}
		public static void ResetPlayerState(Player p)
        {
			SetGuiText(p,"","");
            p.Extras["SURVIVAL_HEALTH"] = 10;
            p.Extras["SURVIVAL_AIR"] = 10;
        }
		void DoHit(Player p, Player victim)
		{
			PushPlayer(p, victim); // Knock the victim back
			int dmg = getDamage(p);
			if (GetHealth(victim)-dmg <= 0)
			{
				Die(victim, 4);
				string deathMessage = p.color +  p.name + " %ekilled " + victim.color + victim.name + "%e.";
				foreach( Player pl in PlayerInfo.Online.Items)
				{
					if (p.level == pl.level || victim.level == pl.level)
					{
						pl.Message(deathMessage);
					}
				}
			}
			Damage(victim, dmg, 4);
		}
		static void PushPlayer(Player p, Player victim)
        {
            if (p.level.Config.MOTD.ToLower().Contains("-damage")) return;

            int srcHeight = ModelInfo.CalcEyeHeight(p);
            int dstHeight = ModelInfo.CalcEyeHeight(victim);
            int dx = p.Pos.X - victim.Pos.X, dy = (p.Pos.Y + srcHeight) - (victim.Pos.Y + dstHeight), dz = p.Pos.Z - victim.Pos.Z;

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);

            float mult = 1 / ModelInfo.GetRawScale(victim.Model);
			mult = mult * getKnockback(p);
            float victimScale = ModelInfo.GetRawScale(victim.Model);

            if (victim.Supports(CpeExt.VelocityControl) && p.Supports(CpeExt.VelocityControl))
            {
                // Intensity of force is in part determined by model scale
                victim.Send(Packet.VelocityControl((-dir.X * mult) * 0.5f, 0.87f * mult, (-dir.Z * mult) * 0.5f, 0, 1, 0));

                // If GoodlyEffects is enabled, show particles whenever a player is hit
                if (Config.UseGoodlyEffects)
                {
                    // Spawn effect when victim is hit
                    //Command.Find("Effect").Use(victim, Config.HitParticle + " " + (victim.Pos.X / 32) + " " + (victim.Pos.Y / 32) + " " + (victim.Pos.Z / 32) + " 0 0 0 true");
                }
            }
            else
            {
                p.Message("You can left and right click on players to hit them if you update your client!");
            }
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
			int uniqueMobId = level.Bots.Items.Length+1;
			string uniqueName = "ssmob" + uniqueMobId;
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
		ushort FindGround(Level level,int x, int y, int z)
		{
			if (x > level.Width)
			{
				x = level.Width-1;
			}
			if (z > level.Length)
			{
				z = level.Length-1;
			}
			if (x < 0)
			{
				x = 0;
			}
			if (z < 0)
			{
				z = 0;
			}
			for (int i = level.Height-1; i >= 0; i--)
			{
				if (level.FastGetBlock((ushort)x, (ushort)i, (ushort)z) != 0)
				{
					return (ushort)(i + 1);
				}
			}
			return (ushort)y;
			
		}
		///////////////////////////////////////////////////////////////////////////
		public static bool inSafeZone(Player p, Level level)
        {
			Zone[] zones = level.Zones.Items;
			foreach(Zone zone in zones)
			{
				if (zone.Contains(p.Pos.BlockX, p.Pos.BlockY, p.Pos.BlockZ))
				{
					return true;
				}
			}
			return false;
            /*if (File.Exists(Config.Path + "safezones" + map + ".txt"))
            {
                using (var r = new StreamReader(Config.Path + "safezones" + map + ".txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] temp = line.Split(';');
                        string[] coord1 = temp[0].Split(',');
                        string[] coord2 = temp[1].Split(',');

                        if ((p.Pos.BlockX <= int.Parse(coord1[0]) && p.Pos.BlockX >= int.Parse(coord2[0])) || (p.Pos.BlockX >= int.Parse(coord1[0]) && p.Pos.BlockX <= int.Parse(coord2[0])))
                        {
                            if ((p.Pos.BlockZ <= int.Parse(coord1[2]) && p.Pos.BlockZ >= int.Parse(coord2[2])) || (p.Pos.BlockZ >= int.Parse(coord1[2]) && p.Pos.BlockZ <= int.Parse(coord2[2])))
                            {
                                if ((p.Pos.BlockY <= int.Parse(coord1[1]) && p.Pos.BlockY >= int.Parse(coord2[1])) || (p.Pos.BlockY >= int.Parse(coord1[1]) && p.Pos.BlockY <= int.Parse(coord2[1])))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            return false;*/
        }
	}
	public sealed class CmdGiveBlock : Command2
    {
        public override string name { get { return "GiveBlock"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0 || message.SplitSpaces().Length < 2)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();
			int matches;
            Player who = PlayerInfo.FindMatches(p, args[0], out matches);
			if (who == null)
			{
				p.Message("Couldn't find player " + args[0]);
				return;
			}
			ushort blockId = 0;
			try
			{
				 blockId = ushort.Parse(args[1]);
			}
			catch(Exception e)
			{
				Help(p);
				return;
			}
			ushort amount = 1;
			if (args.Length > 2)
			{
				try
				{
					amount = ushort.Parse(args[2]);
				}
				catch(Exception e)
				{
					Help(p);
					return;
				}
			}
			MCGalaxy.SimpleSurvival.InventoryAddBlocks(p, blockId, amount);
			p.Message("Gave " + who.name + " x" + args[2] + " " + args[1]);
        }
		public override void Help(Player p)
        {
            p.Message("%T/GiveBlock <Player> <BlockId> <Amount=1>");
        }
	}
	public sealed class CmdCraft : Command2
    {
        public override string name { get { return "Craft"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0 || message.SplitSpaces().Length < 2)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();
			ushort blockId = 0;
			try
			{
				 blockId = ushort.Parse(args[0]);
			}
			catch(Exception e)
			{
				Help(p);
				return;
			}
			ushort amount = 1;
			if (args.Length > 1)
			{
				try
				{
					amount = ushort.Parse(args[1]);
				}
				catch(Exception e)
				{
					Help(p);
					return;
				}
			}
			MCGalaxy.SimpleSurvival.Craft(p, blockId, amount);
			p.Message("Crafted stuff");
        }
		public override void Help(Player p)
        {
            p.Message("%T/Craft <BlockId> <Amount=1>");
        }
	}
	public sealed class CmdPvP : Command2
    {
        public override string name { get { return "PvP"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage PvP") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args, data);
                    return;
                case "del":
                    HandleDelete(p, args, data);
                    return;
            }
        }
		void Save()
		{
			 using (StreamWriter maplistwriter =
                new StreamWriter(SimpleSurvival.Config.Path + "maps.txt"))
            {
                foreach (String s in SimpleSurvival.maplist)
                {
                    maplistwriter.WriteLine(s);
                }
            }
		}
        void HandleAdd(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to add.");
                return;
            }

            if (!HasExtraPerm(p, data.Rank, 1)) { p.Message("%cNo permission."); return; };

            string pvpMap = args[1];

            SimpleSurvival.maplist.Add(pvpMap);
			Save();
            p.Message("The map %b" + pvpMap + " %Shas been added to the PvP map list.");

            // Add the map to the map list
           

            Player[] players = PlayerInfo.Online.Items;

            foreach (Player pl in players)
            {
                if (pl.level.name.ToLower() == args[1].ToLower())
                {
                    SimpleSurvival.ResetPlayerState(pl);
                }
            }
        }

        void HandleDelete(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to remove.");
                return;
            }

            if (!HasExtraPerm(p, data.Rank, 1)) return;

            string pvpMap = args[1];

            SimpleSurvival.maplist.Remove(pvpMap);
			Save();
			Player[] players = PlayerInfo.Online.Items;
			foreach (Player pl in players)
            {
                if (pl.level.name.ToLower() == pvpMap)
                {
                    SimpleSurvival.ResetPlayerState(pl);
                }
            }
            p.Message("The map %b" + pvpMap + " %Shas been removed from the PvP map list.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/PvP add <map> %H- Adds a map to the PvP map list.");
            p.Message("%T/PvP del <map> %H- Deletes a map from the PvP map list.");
        }
    }
}