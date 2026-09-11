using System.Collections;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityRelics.Core
{
    public class RelicsConditionsSystem : ModSystem
    {
        public static bool downedCnidrion = false;
        public static bool downedWulfrumSentinel = false;
        public static bool unlockedWulfrumRecipe = false;

        public override void ClearWorld()
        {
            downedCnidrion = false;
            downedWulfrumSentinel = false;
            unlockedWulfrumRecipe = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (unlockedWulfrumRecipe)
            {
                tag["unlockedWulfrumRecipe"] = true;
            }
        }
        public override void LoadWorldData(TagCompound tag)
        {
            unlockedWulfrumRecipe = tag.ContainsKey("unlockedWulfrumRecipe");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.WriteFlags(unlockedWulfrumRecipe);
        }

        public override void NetReceive(BinaryReader reader)
        {
            reader.ReadFlags(out unlockedWulfrumRecipe); 
        }
    }
}