using Terraria;
using Terraria.ID;
using CalamityMod; 
using Terraria.DataStructures;   
using Microsoft.Xna.Framework;
using CalamityMod.Items.Materials;
using CalamityRelics.Content.Projectiles.Friendly;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Weapons
{
    public class WulfrumSpikyBalls : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 20; 
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.width = 30;
            Item.height = 30;
            
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
        
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(); 
            recipe.AddIngredient(ItemID.SpikyBall, 50);
            recipe.AddIngredient(ModContent.ItemType<WulfrumMetalScrap>(), 1);
            recipe.AddTile(TileID.Anvils); 
            recipe.Register();
        }
        //modify crafting recipe
    }
}