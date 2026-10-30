using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CalamityRelics.Content.Items.Materials
{
    internal class RefinedWulfrum : ModItem
    {
        public override void SetDefaults(){
            Item.width = 26; Item.height = 28;

            Item.value = Item.buyPrice(20);
            Item.maxStack = 9999;
        }
    }
}