using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace CalamityRelics.Content.Dusts
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class Smoke : ModDust
	{
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void OnSpawn(Dust dust)
        {
			dust.noGravity = true;
			dust.scale = Main.rand.Next(18, 21) / 10;
			dust.alpha = 100;
			dust.rotation = MathHelper.ToRadians(Main.rand.Next(0, 360));
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D glow = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/SmokeTexture").Value;

			Main.spriteBatch.Draw(glow, dust.position, null, new Color(255, 255, 255), 0, glow.Size() / 2, 0.4f, SpriteEffects.None, 1);
            
			return true;
        }
        public override bool Update(Dust dust)
		{
			dust.scale -= 0.05f;

			dust.rotation += MathHelper.ToRadians(1);

			if (dust.scale < 0.02f)
			{
				dust.active = false;
			}

			return false;
        }
	}
}
