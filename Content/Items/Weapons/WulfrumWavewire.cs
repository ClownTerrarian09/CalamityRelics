using CalamityRelics.Content.Projectiles.Friendly;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Weapons
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class WulfrumWavewire : ModItem
	{
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Yoyo[Type] = true;
            ItemID.Sets.GamepadExtraRange[Type] = 10;
            ItemID.Sets.GamepadSmartQuickReach[Type] = true;
        }
        public override void SetDefaults()
		{
			Item.damage = 15;
			Item.width = 34;
			Item.height = 46;
            Item.useTime = 25;
			Item.useAnimation = 25;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.channel = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.shoot = ModContent.ProjectileType<WulfrumWavewireProj>(); // Which projectile this item will shoot. We set this to our corresponding projectile.
            Item.shootSpeed = 16f;
        }
	}
}
