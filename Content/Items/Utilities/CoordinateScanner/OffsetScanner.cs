using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;

namespace CalamityRelics.Content.Items.Utilities.CoordinateScanner
{
    public class OffsetScanner : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Ruler;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Rectangle rect = DraedonHouseSystem.DraedonHouseRect;

                if (rect != Rectangle.Empty)
                {
                    if (player.altFunctionUse == 2)
                    {
                        player.Teleport(new Vector2(rect.X * 16, rect.Y * 16));
                        Main.NewText("Teleported to Draedon's House origin!", Color.Yellow);
                    }
                    else
                    {
                        int targetX = Player.tileTargetX;
                        int targetY = Player.tileTargetY;
                        int offsetX = targetX - rect.X;
                        int offsetY = targetY - rect.Y;

                        Main.NewText($"[Draedon Barrier Location] Offset X: {offsetX}, Offset Y: {offsetY}", Color.Cyan);
                    }
                }
                else
                {
                    Main.NewText("Draedon House Rect is empty! Generation failed.", Color.Red);
                }
            }
            return true;
        }
    }
}