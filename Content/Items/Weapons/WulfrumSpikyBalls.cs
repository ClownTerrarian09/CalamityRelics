using Terraria;
using Terraria.ID;
using CalamityMod; 
using Terraria.DataStructures;   
using Microsoft.Xna.Framework;
using CalamityMod.Items.Materials;
using CalamityRelics.Content.Projectiles.Friendly;
using Terraria.ModLoader;
using CalamityRelics.Core;

namespace CalamityRelics.Content.Items.Weapons
{
    public class WulfrumSpikyBalls : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 20; 
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.width = 38;
            Item.height = 38;
            
            Item.useTime = 30; 
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing; 
            Item.knockBack = 1f;
            
            Item.value = Item.sellPrice(copper: 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            
            Item.autoReuse = true; 
            Item.noUseGraphic = true; 
            Item.noMelee = true; 
            
            Item.consumable = false; 
            Item.maxStack = 1; 
            
            
            Item.shoot = ModContent.ProjectileType<WulfrumSpikyBallsProjectile>();
            Item.shootSpeed = 10f; 
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int stealthProj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (stealthProj.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[stealthProj].Calamity().stealthStrike = true;
                    return false;
                }
            }
            return true;
        }
        
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(); 
            recipe.AddIngredient(ItemID.SpikyBall, 50);
            recipe.AddIngredient(ModContent.ItemType<WulfrumMetalScrap>(), 10);
            recipe.AddIngredient(ModContent.ItemType<AnodizedWulfrumMetal>(), 5);
            recipe.AddTile(TileID.Anvils); 
            recipe.AddCondition(RelicsConditions.UnlockedWulfrumRecipes);
            recipe.Register();
        }
        //modify crafting recipe
    }
}