using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;
using CalamityRelics.Content.NPCs.Bosses.Cnidrion;

namespace CalamityRelics.Tools
{
    public class SpawnImpactDiagnostics : ModSystem
    {
        public static bool EnableSpawnImpactDiagnostics = false;

        public static void ToggleSpawnImpactDiagnostics()
        {
            EnableSpawnImpactDiagnostics = !EnableSpawnImpactDiagnostics;
        }
        private int tickCounter = 0;
        private const int LogIntervalTicks = 300;
        private const int SampleHalfWidth = 200;

        public override void PostUpdateWorld()
        {
            if (!EnableSpawnImpactDiagnostics) return;
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            tickCounter++;
            if (tickCounter % LogIntervalTicks != 0) return;

            int totalActive = 0;
            int hostileActive = 0;
            int barrierCount = 0;
            int cnidCount = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n == null || !n.active) continue;
                totalActive++;
                if (!n.friendly) hostileActive++;
                if (n.type == ModContent.NPCType<DraedonBarrierNPC>()) barrierCount++;
                if (n.type == ModContent.NPCType<Cnidrion>()) cnidCount++;
            }

            int emptySlots = Main.maxNPCs - totalActive;

            Player p = Main.LocalPlayer;
            int playerX = p.Center.ToTileCoordinates().X;
            int worldSpawnX = Main.spawnTileX;
            int worldCenterX = Main.maxTilesX / 2;

            int samplePlayer = CountPotentialSpawnTilesInBand(playerX - SampleHalfWidth, playerX + SampleHalfWidth);
            int sampleSpawn = CountPotentialSpawnTilesInBand(Math.Max(0, worldSpawnX - SampleHalfWidth), Math.Min(Main.maxTilesX - 1, worldSpawnX + SampleHalfWidth));
            int sampleCenter = CountPotentialSpawnTilesInBand(Math.Max(0, worldCenterX - SampleHalfWidth), Math.Min(Main.maxTilesX - 1, worldCenterX + SampleHalfWidth));

            Mod.Logger.Info($"Calamity Relics: SpawnImpact: totalActive:{totalActive} hostileActive:{hostileActive} emptySlots:{emptySlots} barrier:{barrierCount} cnidrion:{cnidCount} samples(P: {samplePlayer}, Spawn: {sampleSpawn}, Center: {sampleCenter})");
        }

        private static int CountPotentialSpawnTilesInBand(int startX, int endX)
        {
            int potential = 0;
            int step = Math.Max(1, (endX - startX) / 200);
            for (int x = startX; x <= endX; x += step)
            {
                if (x < 0 || x >= Main.maxTilesX) continue;
                int surfaceY = (int)Main.worldSurface;
                for (int y = Math.Max(0, surfaceY - 50); y < Math.Min(Main.maxTilesY - 2, surfaceY + 50); y++)
                {
                    Tile t = Framing.GetTileSafely(x, y);
                    if (t == null) continue;
                    if (t.HasTile && Main.tileSolid[t.TileType] && !Framing.GetTileSafely(x, y - 1).HasTile)
                    {
                        potential++;
                        break;
                    }
                    if (t.LiquidAmount > 0) { break; }
                }
            }
            return potential;
        }
    }
}
