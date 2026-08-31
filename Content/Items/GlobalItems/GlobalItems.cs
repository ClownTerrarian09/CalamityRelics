using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CalamityRelics.Content.Items.GlobalItems{
    internal class vanillaItemRecipes : GlobalItem{
        public override void AddRecipes(){
            RecipeBuilder(ItemID.DarkShard, 1, new Dictionary<int, int>{
                {ItemID.SoulofNight, 5}, 
                {ItemID.FossilOre, 5}},
                TileID.MythrilAnvil);
    
            RecipeBuilder(ItemID.LightShard, 1, new Dictionary<int, int>{
                {ItemID.SoulofLight, 5},
                {ItemID.FossilOre, 5}},
                TileID.MythrilAnvil);
        }

        public void RecipeBuilder(int result, int resultNo, IDictionary< int, int > ingredients, int tile){
                Recipe newRec = Recipe.Create(result, resultNo);
                foreach (KeyValuePair<int, int> ele in ingredients){
                    newRec.AddIngredient(ele.Key, ele.Value);
                }
                newRec.AddTile(tile);
                newRec.Register();
            }
        public void RecipeBuilder(int result, int resultNo, IDictionary< int, int > ingredients){
                Recipe newRec = Recipe.Create(result, resultNo);
                foreach (KeyValuePair<int, int> ele in ingredients){
                    newRec.AddIngredient(ele.Key, ele.Value);
                }
                newRec.Register();
            }
    }
}