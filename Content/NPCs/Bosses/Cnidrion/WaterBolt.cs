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
    public class WaterBolt : ModProjectile
    {
        int timere = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.damage = 15;
        }

        public void animation()
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter == 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame == 4)
                {
                    Projectile.frame = 0;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D water = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/WaterBolt").Value;
            Rectangle rect = new Rectangle(0, 22 * Projectile.frame, water.Width, 22);

            Main.spriteBatch.Draw(water, Projectile.Center - Main.screenPosition, rect, lightColor * 0.6f, Projectile.rotation, new Vector2(22, 11), 1, SpriteEffects.None, 1);

            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(Projectile.position, ModContent.DustType<WaterBoltParticle>(), new Vector2(Main.rand.Next(-3, 3), Main.rand.Next(-4, -1)));
            }
        }
        public override void AI()
        {
            timere++;

            animation();

            if (Main.rand.Next(1, 8) == 1)
            {
                Dust.NewDustPerfect(Projectile.position, ModContent.DustType<WaterBoltParticle>(), new Vector2(0, 0));
            }

            if (Main.rand.Next(1, 5) == 1)
            {
                Dust.NewDustPerfect(Projectile.position, ModContent.DustType<WaterBoltSteam>(), new Vector2(0, 0));
            }

            if (Projectile.velocity.Y < 10)
            {
                Projectile.velocity.Y += 0.2f;
            }

            if (timere > 300)
            {
                Projectile.tileCollide = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(180);
        }
    }

    public class CnidrionBubble : ModProjectile
    {
        float timere;
        float opac = 0;

        Player plr;
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 500;
        }

        public override void AI()
        {
            timere++;

            opac += (1 - opac) / 20f;

            Projectile.velocity *= 0.98f;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                plr = Main.player[i];

                if (plr.active)
                {
                    if (Projectile.Hitbox.Intersects(plr.Hitbox))
                    {
                        RelicsPlayer modPlayer = plr.GetModPlayer<RelicsPlayer>();

                        modPlayer.jumpBoostTime = 20;

                        Projectile.Kill();

                        break;
                    }
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/BubblePop"), Projectile.Center);
            for (int o = 0; o < 15; o++)
            {
                Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.Next(-35, 35), Main.rand.Next(-35, 35)), ModContent.DustType<WaterBoltSteam>(), new Vector2(0, 0));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bubble = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/CnidrionBubble").Value;

            Main.spriteBatch.Draw(bubble, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), 0, new Vector2(35, 35), new Vector2(opac + (float)(Math.Sin(timere / 15) * 0.1f), opac + (float)(Math.Cos(timere / 15) * 0.1f)), SpriteEffects.None, 1);

            return false;
        }
    }
}