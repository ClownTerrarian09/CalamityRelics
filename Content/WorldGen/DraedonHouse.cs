using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.ID;
using Microsoft.Xna.Framework;
using CalamityMod.Schematics;
using CalamityMod.Items.Materials;
using Terraria.GameContent.Generation;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;
using CalamityRelics.Content.Items.DraedonItems;
using CalamityRelics.Content.Tiles;

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
                        Mod.Logger.Info($"Successfully injected {DraedonHouseSchematicPath} into Calamity's SchematicManager.");
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

            int schematicWidth = 200;
            int schematicHeight = 150;

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
                if (!CheckIceBiomeDensity(p.X, p.Y, 50, 400)) continue;

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
                int buildingHeight = 65;

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

                int codebreakerOffsetX = 34;
                int codebreakerOffsetY = 16;
                Terraria.WorldGen.PlaceTile(p.X + codebreakerOffsetX, p.Y + codebreakerOffsetY, ModContent.TileType<RustedCodebreakerFurniture>());

                placed = true;

                Mod.Logger.Info($"Calamity Relics: Draedon's House placed at {p.X}, {p.Y} in the Surface Tundra");
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