using System;
using System.Collections.Generic;
using CalamityRelics.Content.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace CalamityRelics.Content.WorldGen
{
    public class DraedonGarageGen : ModSystem
    {
        private readonly record struct Candidate(Point Origin, int SurfaceRange);

        private const int SchematicWidth = 66;
        private const int SchematicHeight = 38;
        private const int FlatnessScoreWidth = 48;
        private const int BuildingOffsetX = 1;
        private const int BuildingOffsetY = 1;
        private const int BuildingWidth = 64;
        private const int BuildingHeight = 36;
        private const int MinDistanceFromSpawn = 300;
        private const int MaxDistanceFromSpawn = 500;
        private const int GroundEmbedDepth = 10;
        private const int SkyClearance = 60;
        private const int WaterScanMargin = 8;
        private const int LargeLakeWaterTileThreshold = 100;
        private const double RequiredSkyOpenFraction = 0.6;

        private static bool IsSolidTile(int x, int y)
        {
            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return true;

            Tile tile = Main.tile[x, y];
            return tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
        }

        private static int FindSurfaceY(int x)
        {
            int searchStart = Math.Max(10, (int)Main.worldSurface - 320);
            int searchEnd = Math.Min(Main.maxTilesY - SchematicHeight - 1, (int)Main.worldSurface + 80);

            if (IsSolidTile(x, searchStart))
                return -1;

            for (int y = searchStart; y <= searchEnd; y++)
            {
                if (!IsSolidTile(x, y))
                    continue;

                if (y == 0 || !IsSolidTile(x, y - 1))
                    return y;
            }

            return -1;
        }

        private static bool IsSkyExposed(int left, int top)
        {
            int skyCheckTop = Math.Max(10, top - SkyClearance);
            int totalTiles = 0;
            int openTiles = 0;

            for (int x = left; x < left + BuildingWidth; x += 2)
            {
                for (int y = top - 1; y >= skyCheckTop; y--)
                {
                    totalTiles++;
                    if (!IsSolidTile(x, y))
                        openTiles++;
                }
            }

            return totalTiles > 0 && openTiles >= Math.Ceiling(totalTiles * RequiredSkyOpenFraction);
        }

        private static bool HasLargeWaterBody(int originX, int originY)
        {
            int left = Math.Max(0, originX - WaterScanMargin);
            int right = Math.Min(Main.maxTilesX - 1, originX + SchematicWidth - 1 + WaterScanMargin);
            int top = Math.Max(0, originY - WaterScanMargin);
            int bottom = Math.Min(Main.maxTilesY - 1, originY + SchematicHeight - 1 + WaterScanMargin);
            int waterTiles = 0;

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water && ++waterTiles >= LargeLakeWaterTileThreshold)
                        return true;
                }
            }

            return false;
        }

        private static bool TryGetCandidate(int originX, out Candidate candidate)
        {
            candidate = default;
            int flatnessStart = originX + (SchematicWidth - FlatnessScoreWidth) / 2;
            int flatnessEnd = flatnessStart + FlatnessScoreWidth;
            int minSurfaceY = int.MaxValue;
            int maxSurfaceY = int.MinValue;
            int surfaceYTotal = 0;
            for (int x = originX; x < originX + SchematicWidth; x++)
            {
                int surfaceY = FindSurfaceY(x);
                if (surfaceY < 0)
                    return false;

                surfaceYTotal += surfaceY;
                if (x >= flatnessStart && x < flatnessEnd)
                {
                    minSurfaceY = Math.Min(minSurfaceY, surfaceY);
                    maxSurfaceY = Math.Max(maxSurfaceY, surfaceY);
                }
            }

            int surfaceRange = maxSurfaceY - minSurfaceY;
            int averageSurfaceY = surfaceYTotal / SchematicWidth;
            int bodyLeft = originX + BuildingOffsetX;

            int buildingTop = averageSurfaceY + GroundEmbedDepth - (BuildingHeight - 1);
            Point origin = new(originX, buildingTop - BuildingOffsetY);

            if (origin.X < 10 || origin.Y < 10 || origin.X + SchematicWidth >= Main.maxTilesX - 10 || origin.Y + SchematicHeight >= Main.maxTilesY - 10)
                return false;

            if (!IsSkyExposed(bodyLeft, buildingTop))
                return false;

            if (HasLargeWaterBody(origin.X, origin.Y))
                return false;

            candidate = new Candidate(origin, surfaceRange);
            return true;
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            PassLegacy pass = new("Draedon Garage", (progress, configuration) =>
            {
                progress.Message = "Constructing Draedon's Garage";
                GenerateDraedonGarage();
            });

            int cleanupIndex = tasks.FindIndex(genpass => genpass.Name.Contains("Final Cleanup"));
            if (cleanupIndex >= 0)
            {
                tasks.Insert(cleanupIndex, pass);
            }
            else
            {
                tasks.Add(pass);
                Mod.Logger.Warn("Calamity Relics: Could not find the Final Cleanup pass; appended Draedon Garage generation to the worldgen tasks.");
            }
        }

        private void GenerateDraedonGarage()
        {
            int firstOriginX = Main.spawnTileX + MinDistanceFromSpawn;
            int lastOriginX = Math.Min(Main.spawnTileX + MaxDistanceFromSpawn, Main.maxTilesX - SchematicWidth - 10);
            List<Candidate> candidates = [];

            for (int x = firstOriginX; x <= lastOriginX; x += 4)
            {
                if (TryGetCandidate(x, out Candidate candidate))
                    candidates.Add(candidate);
            }

            if (candidates.Count == 0)
            {
                Mod.Logger.Warn("Calamity Relics: No suitable Draedon Garage candidate found.");
                return;
            }

            int flattestRange = int.MaxValue;
            List<Candidate> flattestCandidates = [];
            foreach (Candidate candidate in candidates)
            {
                if (candidate.SurfaceRange < flattestRange)
                {
                    flattestRange = candidate.SurfaceRange;
                    flattestCandidates.Clear();
                    flattestCandidates.Add(candidate);
                }
                else if (candidate.SurfaceRange == flattestRange)
                {
                    flattestCandidates.Add(candidate);
                }
            }

            Point placement = flattestCandidates[Main.rand.Next(flattestCandidates.Count)].Origin;
            if (!StructureSpawnerSystem.TryPlaceSchematic(StructureSpawnerSystem.DraedonGarageIndex, placement))
            {
                Mod.Logger.Warn($"Calamity Relics: Failed to place Draedon Garage schematic at {placement}.");
                return;
            }

        }
    }
}
