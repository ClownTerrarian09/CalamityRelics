using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace CalamityRelics.Content.Items
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class RelicsBG : ModSurfaceBackgroundStyle
	{
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            fades[Slot] = 1f;
        }

        public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
        {
            Texture2D BG = ModContent.Request<Texture2D>("CalamityRelics/Assets/Menu/RelicsMenuBackground").Value;

            Texture2D Stars = ModContent.Request<Texture2D>("CalamityRelics/Assets/Menu/RelicsMenuStars").Value;

            Texture2D Ribbon = ModContent.Request<Texture2D>("CalamityRelics/Assets/Menu/RelicsMenuRibbon").Value;

            Vector2 screenCenter = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

            Vector2 mouseOffset = Main.MouseScreen - screenCenter;

            spriteBatch.Draw(BG, (new Vector2(Main.screenWidth / 2, Main.screenHeight / 2)), null, Color.White, 0, BG.Size() / 2, 1.5f, SpriteEffects.None, 1);

            for (int i = 0; i < 12; i++)
            {
                spriteBatch.Draw(Stars, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) - new Vector2((float)(Math.Sin(Main.GlobalTimeWrappedHourly / 2)) * 6, 0).RotatedBy(i * 30), null, new Color(255, 255, 255) * 0.25f, 0, BG.Size() / 2, 1.5f, SpriteEffects.None, 1);
            }

            for (float i = 0; i < 180; i++)
            {
                Rectangle rect = new Rectangle(Ribbon.Width - 1 - ((int)i), 0, 1, Ribbon.Height);

                spriteBatch.Draw(Ribbon, screenCenter + new Vector2(130 + (Ribbon.Width * 1.5f), -200) - new Vector2(i * 1.5f, (float)(Math.Sin((Main.GlobalTimeWrappedHourly * -3) + (i / 18)) * (15 * (i / 180)))), rect, Color.White, 0, Ribbon.Size() / 2, 1.5f, SpriteEffects.None, 1);
            }

            return false;
        }
	}
}
