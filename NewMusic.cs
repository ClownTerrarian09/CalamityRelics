using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace CalamityRelics
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class NewMusic : ModSystem
	{
        
    }

    public class OverworldDay : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/Ingame/ForestDay");

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            return player.ZoneForest;
        }
    }
}
