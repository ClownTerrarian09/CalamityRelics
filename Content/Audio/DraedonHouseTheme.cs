using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using System.Collections.Generic;

namespace CalamityRelics.Content.Audio
{
    public class DraedonHouseTileCounter : ModSystem
    {
        public int ExclusiveBlockCount;
        public int StandardBlockCount;

        // --- EXCLUSIVE TILE ---

        private readonly int[] exclusiveVanillaTiles = {
            TileID.IronBrick
        };
        private readonly string[] exclusiveModdedTiles = {
            "CalamityMod/RustedPlatePillar",
            "CalamityMod/WulfrumPlating"
        };

        // --- STANDARD TILE ---
        private readonly int[] standardVanillaTiles = { 
            // Add any vanilla standard tiles here in the future (if there's any)
        };
        private readonly string[] standardModdedTiles = {
            "CalamityMod/RustedPlating",
            "CalamityMod/RustedPipes",
            "CalamityMod/WulfrumPanels"
        };

        private static int[] cachedExclusiveIDs;
        private static int[] cachedStandardIDs;

        public override void PostSetupContent()
        {
            cachedExclusiveIDs = GetValidTileIDs(exclusiveVanillaTiles, exclusiveModdedTiles);
            cachedStandardIDs = GetValidTileIDs(standardVanillaTiles, standardModdedTiles);
        }

        public override void Unload()
        {
            cachedExclusiveIDs = null;
            cachedStandardIDs = null;
        }

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            ExclusiveBlockCount = CountTiles(cachedExclusiveIDs, tileCounts);
            StandardBlockCount = CountTiles(cachedStandardIDs, tileCounts);
        }


        private int[] GetValidTileIDs(int[] vanillaIDs, string[] moddedNames)
        {
            List<int> validIDs = new List<int>(vanillaIDs);

            foreach (string fullName in moddedNames)
            {
                string[] split = fullName.Split('/');
                if (split.Length == 2)
                {
                    string targetMod = split[0];
                    string targetTile = split[1];

                    if (ModContent.TryFind(targetMod, targetTile, out ModTile modTile))
                    {
                        validIDs.Add(modTile.Type);
                    }
                }
                else
                {
                    Mod.Logger.Warn($"[DraedonHouseTheme] Invalid tile string format: {fullName}. Expected 'ModName/TileName'.");
                }
            }
            return validIDs.ToArray();
        }

        private int CountTiles(int[] cachedIDs, ReadOnlySpan<int> tileCounts)
        {
            int count = 0;
            if (cachedIDs != null)
            {
                for (int i = 0; i < cachedIDs.Length; i++)
                {
                    count += tileCounts[cachedIDs[i]];
                }
            }
            return count;
        }
    }

    public class DraedonHouseThemeEffect : ModSceneEffect
    {
        private const string ThemeMusicPath = "Music/DraedonHouse";

        private const int ExclusiveThreshold = 40;
        private const int StandardThreshold = 100;

        public override bool IsSceneEffectActive(Player player)
        {
            var counter = ModContent.GetInstance<DraedonHouseTileCounter>();

            return counter.ExclusiveBlockCount >= ExclusiveThreshold &&
                   counter.StandardBlockCount >= StandardThreshold;
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

        public override int Music => MusicLoader.GetMusicSlot(Mod, ThemeMusicPath);
    }
}