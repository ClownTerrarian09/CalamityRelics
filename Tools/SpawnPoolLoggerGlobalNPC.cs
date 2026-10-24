using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityRelics.Tools
{
    public class SpawnPoolLoggerGlobalNPC : GlobalNPC
    {
        private static int callCounter = 0;
        private const int LogEvery = 20;
        public static bool EnableSpawnPoolLogger = false;

        public static void ToggleSpawnPoolLogger()
        {
            EnableSpawnPoolLogger = !EnableSpawnPoolLogger;
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (!EnableSpawnPoolLogger) return;
            callCounter++;
            if (callCounter % LogEvery != 0) return;

            Player player = spawnInfo.Player;
            if (player == null) return;

            Point tile = player.Center.ToTileCoordinates();
            int poolCount = pool?.Count ?? 0;
            float totalWeight = 0f;
            if (pool != null)
            {
                foreach (var kv in pool) totalWeight += kv.Value;
            }

            Mod.Logger.Info($"Calamity Relics: EditSpawnPool called for player at tile({tile.X},{tile.Y}) poolCount:{poolCount} totalWeight:{totalWeight:0.00}");
        }
    }
}
