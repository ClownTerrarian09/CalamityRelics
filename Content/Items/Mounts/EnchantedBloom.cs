using CalamityRelics.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Mounts
{
    public class EnchantedBloom : ModItem
    {
        public override void SetDefaults() {
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item79;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<Mount_EnchantedBloom>();
        }

    }
}

