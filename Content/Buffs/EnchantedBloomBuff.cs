using Terraria;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Buffs
{
    public class EnchantedBloomBuff : ModBuff
    {
        public override void SetStaticDefaults() {
            Main.buffNoTimeDisplay[Type] = true; 
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) {
            player.mount.SetMount(ModContent.MountType<Mounts.Mount_EnchantedBloom>(), player);
            player.buffTime[buffIndex] = 10;
        }
    }
}

