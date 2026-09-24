using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;
using CalamityRelics.Content.Systems.CustomStructureBehavior.OasisRemnant.RectangleDetection;

namespace CalamityRelics.Content.Items.Utilities.CoordinateScanner
{
    public class OffsetScanner : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Ruler;
        protected override bool CloneNewInstances => true;
        private int targetIndex = 0;
        private readonly string[] targetNames =
        [
            "Draedon's House", "Cnidrion's Pond"
        ];

        /// <summary>
        /// Detect specific structure's coordinate.
        /// </summary>
        private Rectangle GetTargetRect()
        {
            switch (targetIndex)
            {
                case 0:
                    return DraedonHouseSystem.DraedonHouseRect;
                case 1:
                    return OasisRemnantSystem.OasisRemnantRect;
                default:
                    return Rectangle.Empty;
            }
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
        }

        // INVENTORY RIGHT CLICK (Switch Destination)
        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            targetIndex++;
            if (targetIndex >= targetNames.Length)
            {
                targetIndex = 0;
            }

            Main.NewText($"Scanner destination set to: {targetNames[targetIndex]}", Color.LimeGreen);
            Item.stack++;
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Rectangle rect = GetTargetRect();

                if (rect != Rectangle.Empty)
                {
                    if (player.altFunctionUse == 2)
                    {
                        player.Teleport(new(rect.X * 16, rect.Y * 16));
                        Main.NewText($"Teleported to {targetNames[targetIndex]} origin!", Color.Yellow);
                    }
                    else
                    {
                        int targetX = Player.tileTargetX;
                        int targetY = Player.tileTargetY;
                        int offsetX = targetX - rect.X;
                        int offsetY = targetY - rect.Y;

                        Main.NewText($"[{targetNames[targetIndex]}] Offset X: {offsetX}, Offset Y: {offsetY}", Color.Cyan);
                        Main.NewText($"[{targetNames[targetIndex]}] Rect X:{rect.X} Y:{rect.Y} W:{rect.Width} H:{rect.Height}", Color.CornflowerBlue);
                        var playerTile = player.Center.ToTileCoordinates();
                        Main.NewText($"Player tile: X:{playerTile.X} Y:{playerTile.Y} InsideRect:{rect.Contains(playerTile)}", Color.LightGoldenrodYellow);
                    }
                }
                else
                {
                    Main.NewText($"{targetNames[targetIndex]} Rect is empty! Generation failed or coordinates not exposed.", Color.Red);
                }
            }
            return true;
        }
    }
}