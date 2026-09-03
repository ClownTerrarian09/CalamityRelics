using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using CalamityRelics.Content.Items.TileItems;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Utilities;

namespace CalamityRelics.Content.Tiles{
    internal class RustedCodebreakerFurnitureOff : ModTile{
        public bool poweredOn;
        private Asset<Texture2D> glowMask;
        public override void SetStaticDefaults(){
            Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			DustType = DustID.Silt;
			AddMapEntry(new Color(70, 60, 50)); 

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.addTile(Type);
            poweredOn = true;
            glowMask = ModContent.Request<Texture2D>(Texture + "Glow");
        }
        public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
            player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
        }
        public override bool RightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (poweredOn)
            {
                SoundEngine.PlaySound(SoundID.Shatter);
                Player player = Main.LocalPlayer;
                Item.NewItem(player.GetSource_TileInteraction(i, j), player.Center, 1, 1, ModContent.ItemType<RustedCodebreaker>());
                poweredOn = false;
                return true;
            }
            return false;
        }
        public override bool CanKillTile(int i, int j, ref bool blockDamaged)
        {
            return !poweredOn;
        }
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            if (poweredOn){
                frameYOffset = 72;
            }
            else
            {
                frameYOffset = 0;
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            if (poweredOn)
            {
                // Code is from example mod campfire
                var tile = Main.tile[i, j];

			if (!TileDrawing.IsVisible(tile)) {
				return;
			}

				Color color = new Color(255, 255, 255, 0);

				Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

				int width = 16;
				int offsetY = 72;
				int height = 16;
				short frameX = tile.TileFrameX;
				short frameY = tile.TileFrameY;
				int addFrX = 0;
				int addFrY = 0;

				TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY); // calculates the draw offsets
				TileLoader.SetAnimationFrame(Type, i, j, ref addFrX, ref addFrY); // calculates the animation offsets

				Rectangle drawRectangle = new Rectangle(tile.TileFrameX, tile.TileFrameY + addFrY, 16, 16);

				// The flame is manually drawn separate from the tile texture so that it can be drawn at full brightness.
				spriteBatch.Draw(glowMask.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + offsetY) + zero, drawRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
			
		}

    }
}