using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using JetBrains.Annotations;


namespace CalamityRelics.Content.Items.Weapons
{
    internal class WulfrumElectromagneticSphere2 : ModItem
    {
        protected override bool CloneNewInstances => true;
        private float holdDistance = 10;
        public override string Texture => $"CalamityRelics/Content/Items/Weapons/WulfrumElectromagneticSphere";

        public override void SetStaticDefaults()
        {
            // ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 45;
            Item.useStyle = ItemUseStyleID.Shoot; 

            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.damage = 50;
            Item.mana = 15;
            Item.knockBack = 2f;

           
            Item.useTime = 30;
            Item.useAnimation = 30;

            Item.UseSound = SoundID.Item66;

            Item.shoot = ModContent.ProjectileType<WulfrumESHoldout2>();
            Item.shootSpeed = 2f;
            Item.autoReuse = true;
            Item.channel = true;
        }
        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {   
            player.Calamity().rightClickListener = true;
            if (player.Calamity().mouseRight)
            {
                mult = 1.5f;
                base.ModifyManaCost(player, ref reduce, ref mult);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            player.Calamity().rightClickListener = true;
            if (player.Calamity().mouseRight)
            {
                type = ModContent.ProjectileType<WulfrumESHoldout2>();
                velocity = Vector2.Normalize(velocity) * holdDistance;
            }
            else
            {
                type = ModContent.ProjectileType<ElectromagneticSphereProjectileCharge0>();
                velocity = Vector2.Normalize(velocity) * 5;
            }

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, 1f);
			return false;
		}
        
    }
}