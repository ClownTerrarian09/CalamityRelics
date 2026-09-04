using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Tiles;

namespace CalamityRelics.Content.Items.TileItems
{
    public class RustedCodebreaker : ModItem
    {
        public override void SetDefaults(){
            Item.width = 42; Item.height = 34;
            Item.DefaultToPlaceableTile(ModContent.TileType<RustedCodebreakerFurniture>(), 0);
            Item.rare = ItemRarityID.Orange;
        }
    }
}