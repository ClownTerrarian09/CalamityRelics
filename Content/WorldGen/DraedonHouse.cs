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
using Terraria.GameContent.Generation;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;

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

            Point? iceLabPosition = FindIceLabPosition();
            List<Point> validCandidates = new List<Point>();

            for (int x = 200; x < Main.maxTilesX - 200; x += 5)
            {
                for (int y = (int)Main.rockLayer; y < Main.maxTilesY - 300; y += 5)
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
                Mod.Logger.Warn($"Calamity Relics: Failed to find valid ice candidates for {DraedonHouseSchematicPath}.");
                return;
            }

            for (int attempts = 0; attempts < maxAttempts && !placed; attempts++)
            {
                Point p = validCandidates[Main.rand.Next(validCandidates.Count)];

                if (iceLabPosition.HasValue)
                {
                    float distanceFromLab = Vector2.Distance(new Vector2(p.X, p.Y), new Vector2(iceLabPosition.Value.X, iceLabPosition.Value.Y));
                    if (distanceFromLab < 300f) continue;
                }

                if (!IsAreaClear(p.X, p.Y, schematicWidth, schematicHeight)) continue;

                if (!CheckIceBiomeDensity(p.X, p.Y, 50, 400)) continue;

                bool specialCondition = false;
                SchematicManager.PlaceSchematic<System.Action<Terraria.Chest>>(
                    DraedonHouseSchematicKey,
                    p,
                    SchematicAnchor.TopLeft,
                    ref specialCondition,
                    null
                );

                DraedonHouseSystem.DraedonHouseRect = new Rectangle(p.X, p.Y, schematicWidth, schematicHeight);

                int doorOffsetX = 0;
                int doorOffsetY = 0;

                int npcSpawnX = (p.X + doorOffsetX) * 16;
                int npcSpawnY = (p.Y + doorOffsetY) * 16;

                NPC.NewNPC(
                    new Terraria.DataStructures.EntitySource_WorldGen(),
                    npcSpawnX,
                    npcSpawnY,
                    ModContent.NPCType<DraedonBarrierNPC>()
                );

                placed = true;

                Mod.Logger.Info($"Calamity Relics: Draedon's House placed at {p.X}, {p.Y}");
            }

            if (!placed)
            {
                Mod.Logger.Warn($"Calamity Relics: Failed to find a suitable location for {DraedonHouseSchematicPath}.");
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

        private Point? FindIceLabPosition()
        {
            if (ElumplateID == -1) return null;

            for (int x = 100; x < Main.maxTilesX - 100; x += 15)
            {
                for (int y = (int)Main.rockLayer; y < Main.maxTilesY - 200; y += 15)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && tile.TileType == ElumplateID)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return null;
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