using CalamityMod.Items.Materials;
using CalamityMod.Schematics;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;
using CalamityRelics.Content.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace CalamityRelics.Content.WorldGen
{
    public class RelicsIceStructureGen : ModSystem
    {
        private const string DraedonHouseSchematicKey = "CalamityRelics:DraedonHouse";

        private const string DraedonHouseSchematicPath = "Content/Structures/DraedonHouse.csch";
        private const string CalamityModName = "CalamityMod";
        private const string CalamitySchematicIOType = "CalamityMod.Schematics.CalamitySchematicIO";
        private const string CalamitySchematicManagerType = "CalamityMod.Schematics.SchematicManager";

        private static HashSet<int> RestrictedTiles = new HashSet<int>();
        private static int ElumplateID = -1;

        public override void PostSetupContent()
        {
            RestrictedTiles.Clear();

            RestrictedTiles.Add(TileID.LihzahrdBrick);
            RestrictedTiles.Add(TileID.BlueDungeonBrick);
            RestrictedTiles.Add(TileID.GreenDungeonBrick);
            RestrictedTiles.Add(TileID.PinkDungeonBrick);

            if (ModContent.TryFind("CalamityMod", "Elumplate", out ModTile elumplate))
            {
                ElumplateID = elumplate.Type;
                RestrictedTiles.Add(ElumplateID);
            }

            string[] calamityLabTiles = new string[]
            {
                "LaboratoryPlating",
                "LaboratoryPanels",
                "HazardChevronPanels",
                "LaboratoryPipePlating",
                "LaboratoryPlateBeam",
                "LaboratoryPlatePillar",
                "RustedPlating",
                "RustedPipes",
                "RustedPlateBeam",
                "RustedPlatePillar",
                "Navyplate",
                "Plagueplate",
                "Cinderplate",
                "Chaosplate"
            };

            foreach (string tileName in calamityLabTiles)
            {
                if (ModContent.TryFind("CalamityMod", tileName, out ModTile tile))
                {
                    RestrictedTiles.Add(tile.Type);
                }
            }

            if (ModLoader.TryGetMod(CalamityModName, out Mod calamity))
            {
                try
                {
                    using (Stream stream = Mod.GetFileStream(DraedonHouseSchematicPath))
                    {
                        Type ioType = calamity.Code.GetType(CalamitySchematicIOType);
                        MethodInfo importMethod = ioType.GetMethod("ImportSchematic", BindingFlags.NonPublic | BindingFlags.Static);
                        object parsedSchematic = importMethod.Invoke(null, new object[] { stream });

                        Type managerType = calamity.Code.GetType(CalamitySchematicManagerType);
                        FieldInfo tileMapsField = managerType.GetField("TileMaps", BindingFlags.NonPublic | BindingFlags.Static);
                        var tileMaps = (System.Collections.IDictionary)tileMapsField.GetValue(null);

                        tileMaps[DraedonHouseSchematicKey] = parsedSchematic;

                    }
                }
                catch (System.Exception ex)
                {
                    Mod.Logger.Error($"Calamity Relics: Failed to inject schematic via reflection. {ex}");
                }
            }
        }

        public override void Unload()
        {
            RestrictedTiles?.Clear();
            RestrictedTiles = null;
            ElumplateID = -1;
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int microBiomeIndex = tasks.FindIndex(genpass => genpass.Name.Contains("Draedon Structures"));

            if (microBiomeIndex != -1)
            {
                tasks.Insert(microBiomeIndex + 1, new PassLegacy("Draedon's House", (progress, configuration) =>
                {
                    progress.Message = "Forging Draedon's Past";
                    PlaceIceStructure();
                }));
            }
            else
            {
                Mod.Logger.Warn("Calamity Relics: Could not find 'Draedon Structures' generation pass. The Relics Ice Structure will not be generated.");
            }
        }

        private void PlaceIceStructure()
        {
            bool placed = false;
            int maxAttempts = 500;

            int schematicWidth = 149;
            int schematicHeight = 129;

            List<Point> validCandidates = new List<Point>();

            for (int x = 200; x < Main.maxTilesX - 200; x += 5)
            {
                for (int y = 100; y < (int)Main.worldSurface; y += 5)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && (tile.TileType == TileID.IceBlock || tile.TileType == TileID.SnowBlock))
                    {
                        validCandidates.Add(new Point(x, y));
                    }
                }
            }

            if (validCandidates.Count == 0)
            {
                Mod.Logger.Warn($"Calamity Relics: Failed to find valid surface tundra candidates for {DraedonHouseSchematicPath}.");
                return;
            }

            for (int attempts = 0; attempts < maxAttempts && !placed; attempts++)
            {
                Point p = validCandidates[Main.rand.Next(validCandidates.Count)];

                if (!IsAreaClear(p.X, p.Y, schematicWidth, schematicHeight)) continue;
                int scanCenterX = p.X + (schematicWidth / 2);
                int scanCenterY = p.Y + (schematicHeight / 2);
                if (!CheckIceBiomeDensity(scanCenterX, scanCenterY, 80, 3000)) continue;

                bool specialCondition = false;
                SchematicManager.PlaceSchematic<System.Action<Terraria.Chest>>(
                    DraedonHouseSchematicKey,
                    p,
                    SchematicAnchor.TopLeft,
                    ref specialCondition,
                    FillDraedonChests
                );

                int xOffset = 10;
                int yOffset = 13;
                int buildingWidth = 133;
                int buildingHeight = 69;

                DraedonHouseSystem.DraedonHouseRect = new Rectangle(p.X + xOffset, p.Y + yOffset, buildingWidth, buildingHeight);
                DraedonHouseSystem.DraedonHouseLegsRect = new Rectangle(
                    DraedonHouseSystem.DraedonHouseRect.X + 68,
                    DraedonHouseSystem.DraedonHouseRect.Y + 65,
                    17,
                    43
                );

                int npcSpawnX = (DraedonHouseSystem.DraedonHouseRect.X + DraedonHouseSystem.DoorOffsetX) * 16 + 8;
                int npcSpawnY = (DraedonHouseSystem.DraedonHouseRect.Y + DraedonHouseSystem.DoorOffsetY) * 16 + 8;

                NPC.NewNPC(
                    new Terraria.DataStructures.EntitySource_WorldGen(),
                    npcSpawnX,
                    npcSpawnY,
                    ModContent.NPCType<DraedonBarrierNPC>()
                );

                int codebreakerTileType = ModContent.TileType<RustedCodebreakerFurniture>();
                int codebreakerPlaceX = DraedonHouseSystem.DraedonHouseRect.X + 34;
                int codebreakerPlaceY = DraedonHouseSystem.DraedonHouseRect.Y + 20;

                Terraria.WorldGen.PlaceObject(codebreakerPlaceX, codebreakerPlaceY, codebreakerTileType);

                GenerateRoofTrees(p.X, p.Y, schematicWidth);

                placed = true;


            }

            if (!placed)
            {
                Mod.Logger.Warn($"Calamity Relics: Failed to find a suitable location for {DraedonHouseSchematicPath}.");
            }
        }

        private void FillDraedonChests(Chest chest)
        {
            Tile chestTile = Main.tile[chest.x, chest.y];

            int agedSecurityID = ModContent.TryFind("CalamityMod", "AgedSecurityChestTile", out ModTile aged) ? aged.Type : -1;
            int wulfrumChestID = ModContent.TryFind("CalamityMod", "AnodizedWulfrumChest", out ModTile wulf) ? wulf.Type : -1;

            if (chestTile.TileType == agedSecurityID)
            {
                PlaceItemInRandomSlot(chest, (Terraria.WorldGen.SavedOreTiers.Gold == TileID.Gold) ? ItemID.GoldBar : ItemID.PlatinumBar, Main.rand.Next(10, 20));
                PlaceItemInRandomSlot(chest, (Terraria.WorldGen.SavedOreTiers.Iron == TileID.Iron) ? ItemID.IronBar : ItemID.LeadBar, Main.rand.Next(10, 20));
                PlaceItemInRandomSlot(chest, (Terraria.WorldGen.SavedOreTiers.Silver == TileID.Silver) ? ItemID.SilverBar : ItemID.TungstenBar, Main.rand.Next(10, 20));
            }
            else if (chestTile.TileType == wulfrumChestID)
            {
                PlaceItemInRandomSlot(chest, ModContent.ItemType<DubiousPlating>(), Main.rand.Next(15, 30));
                PlaceItemInRandomSlot(chest, ModContent.ItemType<MysteriousCircuitry>(), Main.rand.Next(15, 30));
                PlaceItemInRandomSlot(chest, ModContent.ItemType<WulfrumMetalScrap>(), Main.rand.Next(20, 40));
            }
        }

        /// <summary>
        /// Generate randomized location item in chests.
        /// </summary>
        private void PlaceItemInRandomSlot(Chest chest, int itemType, int totalStack)
        {
            int remainingStack = totalStack;
            int maxAttempts = 150;
            int attempts = 0;

            while (remainingStack > 0 && attempts < maxAttempts)
            {
                attempts++;
                int randomSlot = Main.rand.Next(chest.item.Length);

                if (chest.item[randomSlot].IsAir)
                {
                    int stackToPlace = System.Math.Min(remainingStack, Main.rand.Next(1, 6));

                    chest.item[randomSlot].SetDefaults(itemType);
                    chest.item[randomSlot].stack = stackToPlace;

                    remainingStack -= stackToPlace;
                }
            }
        }

        /// <summary>
        /// Plant trees, and instantly clean up failures.
        /// </summary>
        private static void GenerateRoofTrees(int startX, int startY, int width)
        {
            for (int i = startX; i < startX + width; i += Main.rand.Next(4, 10))
            {
                for (int j = startY - 15; j < startY + 20; j++)
                {
                    Tile tile = Main.tile[i, j];
                    Tile tileAbove = Main.tile[i, j - 1];

                    if (tile.HasTile && tile.TileType == TileID.SnowBlock && !tileAbove.HasTile)
                    {
                        bool exposed = true;
                        for (int k = j - 1; k > j - 15; k--)
                        {
                            if (Main.tile[i, k].HasTile && Main.tileSolid[Main.tile[i, k].TileType])
                            {
                                exposed = false;
                                break;
                            }
                        }

                        if (exposed)
                        {
                            Terraria.WorldGen.PlaceTile(i, j - 1, TileID.Saplings, mute: true, forced: true);

                            bool successfullyGrew = Terraria.WorldGen.GrowTree(i, j - 1);

                            if (!successfullyGrew)
                            {
                                Terraria.WorldGen.KillTile(i, j - 1, noItem: true);
                            }
                        }
                        break;
                    }
                }
            }
        }

        private bool CheckIceBiomeDensity(int centerX, int centerY, int radius, int requiredTiles)
        {
            int iceCount = 0;

            for (int i = centerX - radius; i <= centerX + radius; i++)
            {
                for (int j = centerY - radius; j <= centerY + radius; j++)
                {
                    if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY) continue;

                    Tile tile = Main.tile[i, j];

                    if (tile.HasTile)
                    {
                        if (tile.TileType == TileID.IceBlock ||
                            tile.TileType == TileID.SnowBlock ||
                            tile.TileType == TileID.CorruptIce ||
                            tile.TileType == TileID.FleshIce ||
                            tile.TileType == TileID.HallowedIce)
                        {
                            iceCount++;

                            if (iceCount >= requiredTiles)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool IsAreaClear(int startX, int startY, int width, int height)
        {
            for (int i = startX; i < startX + width; i++)
            {
                for (int j = startY; j < startY + height; j++)
                {
                    Tile checkTile = Main.tile[i, j];

                    if (checkTile.HasTile && RestrictedTiles.Contains(checkTile.TileType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}