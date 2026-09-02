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
            Item.damage = 100;
            Item.mana = 80;
            Item.knockBack = 5f;

           
            Item.useTime = 20;
            Item.useAnimation = 20;

            Item.UseSound = SoundID.Item71;

            Item.shoot = ModContent.ProjectileType<WulfrumESHoldout>();
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
                mult = 0.25f;
                base.ModifyManaCost(player, ref reduce, ref mult);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            player.Calamity().rightClickListener = true;
            if (player.Calamity().mouseRight)
            {
                type = ModContent.ProjectileType<WulfrumESHoldout2>();
            }
            else
            {
                type = ModContent.ProjectileType<WulfrumESHoldout>();
            }

			velocity = Vector2.Normalize(velocity) * holdDistance;

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer);
			return false;
		}
        
    }
}