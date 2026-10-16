using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Utilities.StructureSizeScanner
{
    public class StructureSizeScanner : ModItem
    {
        private List<Point> clickedPoints = new List<Point>();

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Cyan;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Point clickedTile = new Point(Player.tileTargetX, Player.tileTargetY);
                clickedPoints.Add(clickedTile);

                if (clickedPoints.Count < 4)
                {
                    Main.NewText($"[Scanner] Point {clickedPoints.Count} set at: {clickedTile.X}, {clickedTile.Y}. Need {4 - clickedPoints.Count} more.", Color.Cyan);
                }
                else
                {
                    int minX = clickedPoints.Min(p => p.X);
                    int maxX = clickedPoints.Max(p => p.X);
                    int minY = clickedPoints.Min(p => p.Y);
                    int maxY = clickedPoints.Max(p => p.Y);

                    int exactWidth = (maxX - minX) + 1;
                    int exactHeight = (maxY - minY) + 1;

                    Main.NewText($"[Scanner] Point 4 set at: {clickedTile.X}, {clickedTile.Y}", Color.Cyan);
                    Main.NewText($"[Scanner] EXACT STRUCTURE SIZE: {exactWidth} Width x {exactHeight} Height", Color.LimeGreen);
                    Main.NewText($"[Scanner] TOP-LEFT ORIGIN: {minX}, {minY}", Color.Yellow);

                    clickedPoints.Clear();
                }
            }

            return true;
        }
    }
}