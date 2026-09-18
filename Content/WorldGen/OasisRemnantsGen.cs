using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Terraria;
using Terraria.IO;
using CalamityRelics.Content.Systems.CustomStructureBehavior.OasisRemnant.RectangleDetection;
using CalamityMod.Schematics;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using Terraria.DataStructures;

namespace CalamityRelics.Content.WorldGen
{
    public class OasisRemnants : ModSystem
    {
        private const double CenterPercent = 0.6;
        private const double RequiredOpenFraction = 0.75;
        private static bool[] sandSetCache = null;

        private static bool IsSandLike(Tile t)
        {
            if (!t.HasTile) return false;
            int type = t.TileType;
            if (sandSetCache == null)
            {
                try
                {
                    var setsType = typeof(TileID).GetNestedType("Sets", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (setsType != null)
                    {
                        var field = setsType.GetField("Sand", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (field != null)
                        {
                            sandSetCache = field.GetValue(null) as bool[];
                        }
                    }
                }
                catch
                {
                    sandSetCache = null;
                }
            }
            if (sandSetCache != null)
            {
                if (type >= 0 && type < sandSetCache.Length)
                    return sandSetCache[type];
            }
            return type == TileID.Sand;
        }
        private const string CnidrionSchematicKey = "CalamityRelics:CnidrionComfyPond";
        private const string CnidrionSchematicPath = "Content/Structures/CnidrionComfyPond.csch";
        private const string CalamityModName = "CalamityMod";
        private const string CalamitySchematicIOType = "CalamityMod.Schematics.CalamitySchematicIO";
        private const string CalamitySchematicManagerType = "CalamityMod.Schematics.SchematicManager";

        private void AttemptFallbackPlacement(int startX, int endX, int halfWidth, int schematicWidth, int schematicHeight, Rectangle expandedLiquidRect, int bestClusterSize)
        {
            int bestFallbackScore = int.MinValue;
            int bestFallbackCx = -1;
            int bestFallbackSurfaceY = -1;

            for (int cx = startX + halfWidth; cx < endX - halfWidth; cx++)
            {
                int surfaceY = -1;
                for (int y = 100; y < Main.worldSurface; y++)
                {
                    Tile t = Framing.GetTileSafely(cx, y);
                    if (IsSandLike(t))
                    {
                        surfaceY = y;
                        break;
                    }
                }
                if (surfaceY == -1) continue;

                int left = cx - schematicWidth / 2;
                int right = cx + schematicWidth / 2;
                bool exposedToSky = true;
                int skyCheckTop = 10;
                int centerWidth = Math.Max(1, (int)(schematicWidth * CenterPercent));
                int sampleStart = cx - centerWidth / 2;
                int sampleEnd = cx + centerWidth / 2;
                int totalSamples = 0;
                int openCount = 0;
                int blockedCount = 0;
                int firstBlockerX = -1;
                int firstBlockerY = -1;
                int firstBlockerType = -1;
                for (int sampleX = sampleStart; sampleX <= sampleEnd; sampleX += 2)
                {
                    totalSamples++;
                    bool columnOpen = true;
                    for (int yCheck = surfaceY - 1; yCheck >= skyCheckTop; yCheck--)
                    {
                        Tile above = Framing.GetTileSafely(sampleX, yCheck);
                        if (above.HasTile && Main.tileSolid[above.TileType])
                        {
                            columnOpen = false;
                            blockedCount++;
                            if (firstBlockerX == -1)
                            {
                                firstBlockerX = sampleX;
                                firstBlockerY = yCheck;
                                firstBlockerType = above.TileType;
                            }
                            break;
                        }
                    }
                    if (columnOpen) openCount++;
                }
                int requiredOpen = (int)Math.Ceiling(totalSamples * RequiredOpenFraction);
                exposedToSky = (openCount >= requiredOpen);
                if (!exposedToSky)
                {
                    continue;
                }

                int contiguous = 0;
                for (int x = left; x <= right; x++)
                {
                    Tile tt = Framing.GetTileSafely(x, surfaceY);
                    if (IsSandLike(tt))
                        contiguous++;
                }

                int supportCount = 0;
                int holeCount = 0;
                int supportCheckTop = surfaceY - 1;
                int supportCheckBottom = surfaceY + schematicHeight;
                for (int x = left; x <= right; x += 2)
                {
                    bool hasSupport = false;
                    for (int y = supportCheckTop; y <= supportCheckBottom; y++)
                    {
                        Tile st = Framing.GetTileSafely(x, y);
                        if (st.HasTile)
                        {
                            hasSupport = true;
                            break;
                        }
                        if (st.LiquidAmount > 0) break;
                    }
                    if (hasSupport) supportCount++;
                    else holeCount++;
                }

                int score = (contiguous * 2) + (supportCount * 3) - (holeCount * 4);

                Rectangle placementRect = new Rectangle(cx - halfWidth, surfaceY - (schematicHeight / 2), schematicWidth, schematicHeight);
                if (bestClusterSize > 0 && placementRect.Intersects(expandedLiquidRect))
                {
                    score -= 1000;
                }

                if (score > bestFallbackScore)
                {
                    bestFallbackScore = score;
                    bestFallbackCx = cx;
                    bestFallbackSurfaceY = surfaceY;
                }
            }

            if (bestFallbackCx != -1)
            {
                Point p = new Point(bestFallbackCx - 20, bestFallbackSurfaceY - 8);
                bool specialCondition = false;
                try
                {
                    Mod.Logger.Info($"Calamity Relics: Fallback placing Cnidrion pond at {p} (score {bestFallbackScore}).");
                    SchematicManager.PlaceSchematic<System.Action<Terraria.Chest>>(CnidrionSchematicKey, p, SchematicAnchor.TopLeft, ref specialCondition, null);
                    OasisRemnantSystem.OasisRemnantRect = new Rectangle(p.X, p.Y, schematicWidth, schematicHeight);
                }
                catch (Exception ex)
                {
                    Mod.Logger.Warn($"Calamity Relics: Failed fallback placement for Cnidrion schematic at {p}. {ex}");
                }
            }
            else
            {
                Mod.Logger.Warn($"Calamity Relics: No surface candidate found for Cnidrion pond in region ({startX}..{endX}).");
            }
        }

        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod(CalamityModName, out Mod calamity))
            {
                try
                {
                    using (Stream stream = Mod.GetFileStream(CnidrionSchematicPath))
                    {
                        Type ioType = calamity.Code.GetType(CalamitySchematicIOType);
                        MethodInfo importMethod = ioType.GetMethod("ImportSchematic", BindingFlags.NonPublic | BindingFlags.Static);
                        object parsedSchematic = importMethod.Invoke(null, new object[] { stream });

                        Type managerType = calamity.Code.GetType(CalamitySchematicManagerType);
                        FieldInfo tileMapsField = managerType.GetField("TileMaps", BindingFlags.NonPublic | BindingFlags.Static);
                        var tileMaps = (System.Collections.IDictionary)tileMapsField.GetValue(null);

                        tileMaps[CnidrionSchematicKey] = parsedSchematic;
                    }
                }
                catch (System.Exception ex)
                {
                    Mod.Logger.Error($"Calamity Relics: Failed to inject Cnidrion schematic via reflection. {ex}");
                }
            }
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int oasisIndex = tasks.FindIndex(genpass => genpass.Name.Contains("Oasis"));

            PassLegacy pass = new PassLegacy("Oasis Remnants", (progress, configuration) =>
            {
                progress.Message = "Filling Cnidrion's Pond with cold water";
                GenOasisRemnants();
            });

            if (oasisIndex != -1)
            {
                tasks.Insert(oasisIndex + 1, pass);
            }
            else
            {
                tasks.Add(pass);
                Mod.Logger.Warn("Calamity Relics: Could not find 'Oasis' gen pass; added Cnidrion pass at end of worldgen tasks.");
            }
        }

        private void GenOasisRemnants()
        {
            int startX = GenVars.UndergroundDesertLocation.X;
            int endX = GenVars.UndergroundDesertLocation.X + GenVars.UndergroundDesertLocation.Width;

            int schematicWidth = 42;
            int schematicHeight = 27;
            int halfWidth = schematicWidth / 2;
            int step = 8;
            int minContiguousSand = 20;

            int scanTop = 100;
            int scanBottom = (int)Main.worldSurface;
            int scanWidth = endX - startX + 1;
            int scanHeight = Math.Max(1, scanBottom - scanTop + 1);
            bool[,] visited = new bool[scanWidth, scanHeight];

            int bestClusterSize = 0;
            int bestLeft = 0, bestRight = 0, bestTop = 0, bestBottom = 0;

            for (int i = startX; i <= endX; i++)
            {
                for (int j = scanTop; j <= scanBottom; j++)
                {
                    int vi = i - startX;
                    int vj = j - scanTop;
                    if (vi < 0 || vi >= scanWidth || vj < 0 || vj >= scanHeight) continue;
                    if (visited[vi, vj]) continue;
                    Tile t = Framing.GetTileSafely(i, j);
                    if (t.LiquidAmount == 0) continue;

                    int left = i, right = i, top = j, bottom = j;
                    int count = 0;
                    var q = new Queue<Point>();
                    q.Enqueue(new Point(i, j));
                    visited[vi, vj] = true;
                    while (q.Count > 0)
                    {
                        Point p = q.Dequeue();
                        count++;
                        left = Math.Min(left, p.X);
                        right = Math.Max(right, p.X);
                        top = Math.Min(top, p.Y);
                        bottom = Math.Max(bottom, p.Y);

                        Point[] nbrs = new Point[] { new Point(p.X - 1, p.Y), new Point(p.X + 1, p.Y), new Point(p.X, p.Y - 1), new Point(p.X, p.Y + 1) };
                        foreach (var n in nbrs)
                        {
                            if (n.X < startX || n.X > endX || n.Y < scanTop || n.Y > scanBottom) continue;
                            int nvi = n.X - startX;
                            int nvj = n.Y - scanTop;
                            if (visited[nvi, nvj]) continue;
                            Tile nt = Framing.GetTileSafely(n.X, n.Y);
                            if (nt.LiquidAmount == 0) continue;
                            visited[nvi, nvj] = true;
                            q.Enqueue(n);
                        }
                    }

                    if (count > bestClusterSize)
                    {
                        bestClusterSize = count;
                        bestLeft = left;
                        bestRight = right;
                        bestTop = top;
                        bestBottom = bottom;
                    }
                }
            }

            Rectangle expandedLiquidRect = Rectangle.Empty;
            int liquidPadding = 4;
            if (bestClusterSize > 0)
            {
                expandedLiquidRect = new Rectangle(bestLeft - liquidPadding, bestTop - liquidPadding, (bestRight - bestLeft + 1) + liquidPadding * 2, (bestBottom - bestTop + 1) + liquidPadding * 2);
            }

            int bestScore = int.MinValue;
            int bestCx = -1;
            int bestSurfaceY = -1;

            for (int cx = startX + halfWidth; cx < endX - halfWidth; cx += step)
            {
                int surfaceY = -1;
                for (int y = 100; y < Main.worldSurface; y++)
                {
                    Tile t = Framing.GetTileSafely(cx, y);
                    if (IsSandLike(t))
                    {
                        surfaceY = y;
                        break;
                    }
                }
                if (surfaceY == -1) continue;

                int left = cx - schematicWidth / 2;
                int right = cx + schematicWidth / 2;
                int contiguous = 0;
                for (int x = left; x <= right; x++)
                {
                    Tile tt = Framing.GetTileSafely(x, surfaceY);
                    if (tt.HasTile && tt.TileType == TileID.Sand)
                        contiguous++;
                }
                if (contiguous < minContiguousSand)
                {
                    continue;
                }

                int biomeEdgeBuffer = 12;
                if (left - biomeEdgeBuffer < startX || right + biomeEdgeBuffer > endX) continue;
                if (left - biomeEdgeBuffer < startX || right + biomeEdgeBuffer > endX)
                {
                    continue;
                }

                int supportCount = 0;
                int holeCount = 0;
                int supportCheckTop = surfaceY - 1;
                int supportCheckBottom = surfaceY + schematicHeight;
                for (int x = left; x <= right; x += 2)
                {
                    bool hasSupport = false;
                    for (int y = supportCheckTop; y <= supportCheckBottom; y++)
                    {
                        Tile st = Framing.GetTileSafely(x, y);
                        if (st.HasTile)
                        {
                            hasSupport = true;
                            break;
                        }
                        if (st.LiquidAmount > 0) break;
                    }
                    if (hasSupport) supportCount++;
                    else holeCount++;
                }

                Rectangle placementRect = new Rectangle(cx - halfWidth, surfaceY - (schematicHeight / 2), schematicWidth, schematicHeight);
                if (bestClusterSize > 0 && placementRect.Intersects(expandedLiquidRect))
                {
                    continue;
                }

                bool exposedToSky = true;
                int skyCheckTop = 10;
                int centerWidth = Math.Max(1, (int)(schematicWidth * CenterPercent));
                int sampleStart = cx - centerWidth / 2;
                int sampleEnd = cx + centerWidth / 2;
                int totalSamples = 0;
                int openCount = 0;
                int blockedCount = 0;
                int firstBlockerX = -1;
                int firstBlockerY = -1;
                int firstBlockerType = -1;
                for (int sampleX = sampleStart; sampleX <= sampleEnd; sampleX += 2)
                {
                    totalSamples++;
                    bool columnOpen = true;
                    for (int yCheck = surfaceY - 1; yCheck >= skyCheckTop; yCheck--)
                    {
                        Tile above = Framing.GetTileSafely(sampleX, yCheck);
                        if (above.HasTile && Main.tileSolid[above.TileType])
                        {
                            columnOpen = false;
                            blockedCount++;
                            if (firstBlockerX == -1)
                            {
                                firstBlockerX = sampleX;
                                firstBlockerY = yCheck;
                                firstBlockerType = above.TileType;
                            }
                            break;
                        }
                    }
                    if (columnOpen) openCount++;
                }
                int requiredOpen = (int)Math.Ceiling(totalSamples * RequiredOpenFraction);
                exposedToSky = (openCount >= requiredOpen);
                if (!exposedToSky)
                {
                    continue;
                }

                int score = (supportCount * 3) - (holeCount * 4);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCx = cx;
                    bestSurfaceY = surfaceY;
                }
            }

            if (bestSurfaceY != -1)
            {
                Point p = new Point(bestCx - 20, bestSurfaceY - 8);
                if (bestClusterSize == 0 || bestScore > 0)
                {
                    bool specialCondition = false;
                    try
                    {
                        if (bestClusterSize == 0)
                            Mod.Logger.Info($"Calamity Relics: No real oasis found; placing Cnidrion pond at {p} (score {bestScore}).");
                        else
                            Mod.Logger.Info($"Calamity Relics: Placed Cnidrion pond at {p} (score {bestScore}).");

                        SchematicManager.PlaceSchematic<System.Action<Terraria.Chest>>(CnidrionSchematicKey, p, SchematicAnchor.TopLeft, ref specialCondition, null);
                        OasisRemnantSystem.OasisRemnantRect = new Rectangle(p.X, p.Y, schematicWidth, schematicHeight);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Mod.Logger.Warn($"Calamity Relics: Failed to place Cnidrion schematic at {p}. {ex}");
                        return;
                    }
                }
                else
                {
                    AttemptFallbackPlacement(startX, endX, halfWidth, schematicWidth, schematicHeight, expandedLiquidRect, bestClusterSize);
                    return;
                }
            }
        }
    }
}