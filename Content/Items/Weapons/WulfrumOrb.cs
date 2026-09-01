using CalamityRelics.Content.Buffs.SummonWeapons;
using CalamityMod.Items.Materials;
using CalamityRelics.Content.Projectiles.Summon;
using CalamityMod.Systems.Collections;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace  CalamityRelics.Content.Items.Weapons
{
    public class WulfrumOrb : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true; //lock on enemy even if blocked by wall
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.damage = 70;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item67;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<WulfrumOrbBuff>();
            Item.shoot = ModContent.ProjectileType<WulfrumOrbProjectile>();
            Item.DamageType = DamageClass.Summon;
            
            Item.rare = ItemRarityID.Pink;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld; 
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(); 
            recipe.AddIngredient(ItemID.SpikyBall, 50);
            recipe.AddIngredient(ModContent.ItemType<WulfrumMetalScrap>(), 1);
            recipe.AddTile(TileID.Anvils); 
            recipe.Register();
        }
    }
}
