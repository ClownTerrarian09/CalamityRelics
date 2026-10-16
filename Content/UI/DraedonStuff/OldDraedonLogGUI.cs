using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using System.Linq;
using Terraria.ModLoader;
using Terraria.GameContent;
using CalamityMod.UI.DraedonLogs;
using System.Collections.Generic;

namespace CalamityRelics.Content.UI.DraedonStuff
{
    public abstract class OldDraedonLogGUI : DraedonsLogGUI
    {
        // Credits to Calamity Mod
        // This code was adapted from DraedonsLogGUI in order to change the texture of the UI. 
        // All credits goes to Azafure LLC
        // https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/LICENSE.md

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pageTexture = ModContent.Request<Texture2D>("CalamityRelics/Content/UI/DraedonStuff/RustedLog").Value;
            float xScale = MathHelper.Lerp(0.004f, 1f, FadeTime / (float)FadeTimeMax);
            Vector2 scale = new Vector2(xScale, 1f) * new Vector2(Main.screenWidth, Main.screenHeight) / pageTexture.Size();
            scale.Y *= 1.5f;
            scale *= 0.5f;

            float xResolutionScale = Main.screenWidth / 2560f;
            float yResolutionScale = Main.screenHeight / 1440f;
            float bookScale = 0.5f;
            scale *= bookScale;

            float yPageTop = MathHelper.Lerp(Main.screenHeight * 2, Main.screenHeight * 0.25f, FadeTime / (float)FadeTimeMax);

            Rectangle mouseRectangle = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 2, 2);
            float drawPositionX = Main.screenWidth * 0.5f;
            Vector2 drawPosition = new Vector2(drawPositionX, yPageTop);
            Rectangle pageRectangle = new Rectangle((int)drawPosition.X - (int)(pageTexture.Width * scale.X), (int)yPageTop, (int)(pageTexture.Width * scale.X) * 2, (int)(pageTexture.Height * scale.Y));
            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(pageTexture, drawPosition, null, Color.White, 0f, new Vector2(i == 0f ? pageTexture.Width : 0f, 0f), scale, i == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

                if (!HoveringOverBook)
                    HoveringOverBook = mouseRectangle.Intersects(pageRectangle);
            }

            // Create text and arrows.
            if (FadeTime >= FadeTimeMax - 4 && Active)
            {
                int textWidth = (int)(xScale * pageTexture.Width) - TextStartOffsetX;
                textWidth = (int)(textWidth * xResolutionScale);
                List<string> dialogLines = Utils.WordwrapString(GetTextByPage(), FontAssets.MouseText.Value, (int)(textWidth / xResolutionScale), 250, out _).ToList();
                dialogLines.RemoveAll(text => string.IsNullOrEmpty(text));

                int trimmedTextCharacterCount = string.Concat(dialogLines).Length;
                float yOffsetPerLine = 28f;
                if (dialogLines.Count > TotalLinesPerPage)
                    yOffsetPerLine *= string.Concat(dialogLines).Length / GetTextByPage().Length;

                // Ensure the page number doesn't become nonsensical as a result of a resolution change (such as by resizing the game).
                if (Page < 0)
                    Page = 0;
                if (Page > TotalPages)
                    Page = TotalPages;

                DrawArrows(spriteBatch, xResolutionScale, yResolutionScale, yPageTop + 506f * yResolutionScale, mouseRectangle);

                int textDrawPositionX = (int)(pageTexture.Width * xResolutionScale + 350 * xResolutionScale);
                int yScale = (int)(42 * yResolutionScale);
                int yScale2 = (int)(yOffsetPerLine * yResolutionScale);
                for (int i = 0; i < dialogLines.Count; i++)
                {
                    if (dialogLines[i] != null)
                    {
                        int textDrawPositionY = yScale + i * yScale2 + (int)yPageTop;
                        Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, dialogLines[i], textDrawPositionX, textDrawPositionY, Color.DarkCyan, Color.Black, Vector2.Zero, xResolutionScale);
                    }
                }

                DrawSpecialImage(spriteBatch, xResolutionScale, yResolutionScale, yPageTop - 70f * yResolutionScale);
            }
        }
    }
}