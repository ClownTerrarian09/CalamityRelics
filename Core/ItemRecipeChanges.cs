using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Placeables.DraedonStructures;
using CalamityRelics.Content.Items.TileItems;

namespace CalamityRelics.Core
{
    internal class ItemRecipeChanges : ModSystem{
        public override void PostAddRecipes() {
            for (int i = 0; i < Recipe.numRecipes; i++) {
                Recipe recipe = Main.recipe[i];

                if(recipe.HasResult(ModContent.ItemType<CodebreakerBase>())){
                    recipe.RemoveIngredient(ModContent.ItemType<ChargingStationItem>());
                    recipe.AddIngredient(ModContent.ItemType<RustedCodebreaker>());
                }
            }
        }
    } 
}