using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Dusts
{
    public class EnchantedPetal : ModDust
    {
        
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.frame = new Rectangle(0, 0, 16, 16);
        }


        public override bool Update(Dust dust)
        {
            dust.rotation += dust.velocity.X * 0.05f;
            dust.position += dust.velocity * 0.25f;
            dust.velocity.X += (float)Math.Sin(dust.rotation) * 0.5f;
            dust.velocity.Y += 0.1f;
            dust.scale *= 0.98f;
            dust.alpha = (int)(255f * (dust.scale / 1.0f));
            if (dust.scale < 0.1f)
            {
                dust.active = false;
            }


            return false;
        }
    }
}

