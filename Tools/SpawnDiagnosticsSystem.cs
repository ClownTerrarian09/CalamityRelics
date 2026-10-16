using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Tools
{
    public class SpawnDiagnosticsSystem : ModSystem
    {
        public override void PostWorldGen()
        {
            int width = Main.maxTilesX;
            int height = Main.maxTilesY;

            int sampledColumns = 0;
            int potentialSpawnTiles = 0;
            int grassTiles = 0;
            int waterTiles = 0;

            int step = System.Math.Max(1, width / 200);

            for (int x = 0; x < width; x += step)
            {
                sampledColumns++;
                int surfaceY = (int)Main.worldSurface;
                for (int y = System.Math.Max(0, surfaceY - 50); y < System.Math.Min(height - 2, surfaceY + 50); y++)
                {
                    Tile t = Framing.GetTileSafely(x, y);
                    if (t == null) continue;
                    if (t.HasTile && Main.tileSolid[t.TileType] && !Framing.GetTileSafely(x, y - 1).HasTile)
                    {
                        potentialSpawnTiles++;
                        if (t.TileType == TileID.Grass || t.TileType == TileID.JungleGrass || t.TileType == TileID.CorruptGrass || t.TileType == TileID.HallowedGrass || t.TileType == TileID.SnowBlock)
                        {
                            grassTiles++;
                        }
                        break;
                    }
                    if (t.LiquidAmount > 0) { waterTiles++; break; }
                }
            }

            Mod.Logger.Info($"Calamity Relics: SpawnDiagnostics: SampledCols:{sampledColumns} PotentialSpawnTiles:{potentialSpawnTiles} GrassTiles:{grassTiles} WaterTiles:{waterTiles}");
        }
    }
}
