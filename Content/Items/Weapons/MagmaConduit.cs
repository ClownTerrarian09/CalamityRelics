using CalamityRelics.Content.Projectiles.Friendly;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Weapons
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class MagmaConduit : ModItem
	{
        public override void SetDefaults()
		{
			Item.damage = 40;
			Item.width = 34;
			Item.height = 46;
            Item.useTime = 25;
			Item.useAnimation = 25;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Orange;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.DamageType = DamageClass.Magic;
            Item.shoot = ModContent.ProjectileType<MagmaConduitProj>(); // Which projectile this item will shoot. We set this to our corresponding projectile.
            Item.shootSpeed = 0;
        }
	}
}
