using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;

namespace CalamityRelics.Content.NPCs
{
    public class DraedonHouseSpawnGlobalNPC : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            Player player = spawnInfo.Player;
            if (player == null || !player.active || player.dead) return;

            Point tileCoords = player.Center.ToTileCoordinates();
            if (!DraedonHouseSystem.IsTileInHouse(tileCoords.X, tileCoords.Y)) return;

            AddToPoolIfExists(pool, "CalamityMod", "Nanodroid", 0.6f);
            AddToPoolIfExists(pool, "CalamityMod", "NanodroidDysfunctional", 0.4f);
            AddToPoolIfExists(pool, "CalamityMod", "NanodroidPlagueRed", 0.3f);
            AddToPoolIfExists(pool, "CalamityMod", "NanodroidPlagueGreen", 0.3f);
            AddToPoolIfExists(pool, "CalamityMod", "Androomba", 0.5f);
            AddToPoolIfExists(pool, "CalamityMod", "RepairUnitCritter", 0.5f);
        }

        private static void AddToPoolIfExists(IDictionary<int, float> pool, string modName, string npcName, float weight)
        {
            if (ModContent.TryFind(modName, npcName, out ModNPC npc))
            {
                int type = npc.Type;
                if (pool.ContainsKey(type))
                    pool[type] = MathF.Max(pool[type], weight);
                else
                    pool[type] = weight;
            }
        }
    }
}
