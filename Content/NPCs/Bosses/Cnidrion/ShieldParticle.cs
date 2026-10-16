using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.NPCs.Bosses.Cnidrion
{
    public class ShieldParticle : ModDust
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale = Main.rand.Next(10, 12) / 10;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D glow = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/BloomCircleSmall").Value;

            Main.EntitySpriteDraw(glow, dust.position - Main.screenPosition, null, new Color(28, 224, 255, 0), 0f, glow.Size() / 2, dust.scale / 10, SpriteEffects.None);

            return true;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;

            dust.scale -= 0.01f;

            if (dust.scale < 0.01f)
            {
                dust.active = false;
            }

            return false;
        }
    }

    public class WaterBoltParticle : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale = Main.rand.Next(14, 20) / 10;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D water = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/WaterBoltParticle").Value;

            Main.EntitySpriteDraw(water, dust.position - Main.screenPosition, null, new Color(255, 255, 255) * 0.3f, 0f, water.Size() / 2, dust.scale, SpriteEffects.None);

            return false;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;

            if (dust.velocity.Y < 5)
            {
                dust.velocity.Y += 0.1f;
            }

            dust.scale -= 0.01f;

            if (dust.scale < 0.01f)
            {
                dust.active = false;
            }

            return false;
        }
    }

    public class WaterBoltSteam : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale = Main.rand.Next(18, 20) / 10;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D steam = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/WaterBoltSteam").Value;

            Main.EntitySpriteDraw(steam, dust.position - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.6f, dust.rotation, steam.Size() / 2, dust.scale / 10, SpriteEffects.None);

            return false;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;

            dust.scale -= 0.02f;

            dust.rotation += MathHelper.ToRadians(2);

            if (dust.scale < 0.02f)
            {
                dust.active = false;
            }

            return false;
        }
    }
}
