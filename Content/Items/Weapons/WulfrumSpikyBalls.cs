using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Rarities;
using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Weapons
{
    public class WulfrumSpikyBalls : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 80;
            Item.crit = 20;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.UseSound = SoundID.Item60;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<WulfrumSpikyBallsProjectile>();
            Item.shootSpeed = 15f;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (Utils.IndexInRange(Main.projectile, proj))
                Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            return false;
        }
    }
}