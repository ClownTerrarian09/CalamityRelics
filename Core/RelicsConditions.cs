using System;
using Terraria;
using Terraria.Localization;

namespace CalamityRelics.Core
{
    public static class RelicsConditions
    {
        public static Condition UnlockedWulfrumRecipes = new ("Mods.CalamityRelics.Conditions.UnlockedWulfrumRecipes", () => RelicsConditionsSystem.unlockedWulfrumRecipe); 
    }
}