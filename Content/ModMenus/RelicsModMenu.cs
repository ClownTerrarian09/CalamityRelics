using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class RelicsModMenu : ModMenu
	{
        public override string DisplayName => "Relics Peak";

        //AroundtheCampfire

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            Texture2D Logo = ModContent.Request<Texture2D>("CalamityRelics/Assets/Menu/RelicsLogo").Value;

            spriteBatch.Draw(Logo, logoDrawCenter + new Vector2(0, 25 + (float)(Math.Sin(Main.GlobalTimeWrappedHourly / 2) * 10)), null, new Color(255, 255, 255), 0, Logo.Size() / 2, 1.25f, SpriteEffects.None, 1f);

            return false;
        }

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/TaleofaNewWorld");
        public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<RelicsBG>();
	}
}
