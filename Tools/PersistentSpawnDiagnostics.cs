using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;
using CalamityRelics.Content.Systems.CustomStructureBehavior.OasisRemnant.RectangleDetection;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;
using CalamityRelics.Content.NPCs.Bosses.Cnidrion;

namespace CalamityRelics.Tools
{
    public class PersistentSpawnDiagnostics : ModSystem
    {
        public static bool EnablePersistentSpawnDiagnostics = false;
        public static bool ForceRevealPersistentNPCs = false;
        public static bool ForcePositionPersistentNPCsToCenter = false;
        public static void TogglePersistentDiagnostics()
        {
            EnablePersistentSpawnDiagnostics = !EnablePersistentSpawnDiagnostics;
        }

        public static void ToggleRevealPersistentNPCs()
        {
            ForceRevealPersistentNPCs = !ForceRevealPersistentNPCs;
        }

        public static void TogglePositionPersistentNPCs()
        {
            ForcePositionPersistentNPCsToCenter = !ForcePositionPersistentNPCsToCenter;
        }

        private int tickCounter = 0;
        private const int LogIntervalTicks = 300;

        public override void PostUpdateWorld()
        {
            if (!EnablePersistentSpawnDiagnostics) return;
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            tickCounter++;
            if (tickCounter % LogIntervalTicks != 0) return;

            int barrierType = ModContent.NPCType<DraedonBarrierNPC>();
            int cnidrionType = ModContent.NPCType<Cnidrion>();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n == null || !n.active) continue;

                if (n.type == barrierType)
                {
                    var center = n.Center;
                    var tile = center.ToTileCoordinates();
                    bool inside = DraedonHouseSystem.DraedonHouseRect != Rectangle.Empty && DraedonHouseSystem.IsTileInHouse(tile.X, tile.Y);
                    if (ForceRevealPersistentNPCs)
                    {
                        n.hide = false;
                        n.noTileCollide = false;
                        n.netUpdate = true;
                    }
                    if (ForcePositionPersistentNPCsToCenter)
                    {
                        n.position = new(tile.X * 16 + 8 - n.width / 2, tile.Y * 16 + 8 - n.height / 2);
                        n.netUpdate = true;
                        center = n.Center;
                        tile = center.ToTileCoordinates();
                    }

                    Mod.Logger.Info($"Calamity Relics: Barrier NPC index:{i} active:true centerPx:({center.X:0.0},{center.Y:0.0}) tile:({tile.X},{tile.Y}) hitbox:({n.Hitbox.X},{n.Hitbox.Y},{n.Hitbox.Width},{n.Hitbox.Height}) hide:{n.hide} alpha:{n.alpha} noTileCollide:{n.noTileCollide} insideHouse:{inside} ai:({n.ai[0]},{n.ai[1]},{n.ai[2]},{n.ai[3]})");
                }

                if (n.type == cnidrionType)
                {
                    var center = n.Center;
                    var tile = center.ToTileCoordinates();
                    bool inside = OasisRemnantSystem.OasisRemnantRect != Rectangle.Empty && OasisRemnantSystem.OasisRemnantRect.Contains(tile.X, tile.Y);
                    if (ForceRevealPersistentNPCs)
                    {
                        n.hide = false;
                        n.noTileCollide = false;
                        n.netUpdate = true;
                    }
                    if (ForcePositionPersistentNPCsToCenter)
                    {
                        n.position = new(tile.X * 16 + 8 - n.width / 2, tile.Y * 16 + 8 - n.height / 2);
                        n.netUpdate = true;
                        center = n.Center;
                        tile = center.ToTileCoordinates();
                    }

                    Mod.Logger.Info($"Calamity Relics: Cnidrion NPC index:{i} active:true centerPx:({center.X:0.0},{center.Y:0.0}) tile:({tile.X},{tile.Y}) hitbox:({n.Hitbox.X},{n.Hitbox.Y},{n.Hitbox.Width},{n.Hitbox.Height}) hide:{n.hide} alpha:{n.alpha} noTileCollide:{n.noTileCollide} insideOasis:{inside} ai:({n.ai[0]},{n.ai[1]},{n.ai[2]},{n.ai[3]})");
                }
            }

            bool anyBarrier = NPC.AnyNPCs(barrierType);
            bool anyCnid = NPC.AnyNPCs(cnidrionType);
            if (!anyBarrier && !anyCnid)
            {
                Mod.Logger.Info("Calamity Relics: PersistentSpawnDiagnostics: No active barrier or Cnidrion NPCs found in Main.npc.");
            }
        }
    }
}
