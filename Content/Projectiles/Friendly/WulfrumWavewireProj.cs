using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class WulfrumWavewireProj : ModProjectile
	{
        float dist;
        Player player;
        float rot;
        float PTimer;
        int PframeTimer;
        int Pframe;

        float widthSub;

        int notFirstSeg = 1;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Type] = 4f;
            ProjectileID.Sets.YoyosMaximumRange[Type] = 210;
            ProjectileID.Sets.YoyosTopSpeed[Type] = 8;
        }
        public override void SetDefaults()
        {
			Projectile.damage = 15;
            Projectile.aiStyle = ProjAIStyleID.Yoyo;

            Projectile.width = 18;
            Projectile.height = 18;
            
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
        }

        public void animation()
        {
            PframeTimer++;

            if (PframeTimer > 3)
            {
                Pframe++;
                PframeTimer = 0;

                if (Pframe > 5)
                {
                    Pframe = 0;
                }
            }
        }

        public override bool PreAI()
        {
            player = Main.player[Projectile.owner];

            dist = Vector2.Distance(player.Center, Projectile.Center);

            rot = (player.Center - Projectile.Center).ToRotation();

            return true;
        }
        public override void AI()
        {
            PTimer++;

            animation();
        }

        public override bool PreDrawExtras()
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.NPCHit53, Projectile.Center);

            for (int i = 0; i < 3; ++i)
            {
                Dust.NewDust(target.Center, 0, 0, DustID.Electric, 0, 0, 0, default, 1);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D line = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/WulfrumWavewireLine").Value;

            Texture2D highlight = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/WulfrumWavewireProjH").Value;

            Rectangle rect = new Rectangle(0, Pframe * 26, 32, 26);

            Rectangle rectFirst = new Rectangle(18, Pframe * 26, 18, 26);

            if (PTimer % 12 == 1)
            {
                Dust.NewDust(player.Center + new Vector2(-Main.rand.Next(0, (int)dist), 0).RotatedBy(rot), 0, 0, DustID.Electric, 0, 0, 0, default, 1);
            }

            for (float i = 0; i < dist + 36; i += 36)
            {
                if (PTimer + i % 5 == 1)
                {
                    Dust.NewDust(player.Center + new Vector2(-i, 0).RotatedBy(rot), 0, 0, DustID.Electric, 0, 0, 0, default, 1);
                }

                rect = new Rectangle(0, Pframe * 26, 36 - (int)MathF.Max(0, ((i + 18) - dist)), 26);

                notFirstSeg = 1;

                if (i < 36)
                {
                    rect = new Rectangle(18, Pframe * 26, 18, 26);
                    notFirstSeg = 0;
                }

                for (int x = 0; x < 12; x++)
                {
                    Main.spriteBatch.Draw(line, player.Center - Main.screenPosition + new Vector2(-i + ((36 - rect.Width) * notFirstSeg), 0).RotatedBy(rot) + new Vector2(4, 0).RotatedBy(x * 30), rect, new Color(0, 109, 255, 0) * 0.05f, rot, new Vector2(18, 13), 1f, SpriteEffects.None, 1f);
                }

                for (int x = 0; x < 12; x++)
                {
                    Main.spriteBatch.Draw(line, player.Center - Main.screenPosition + new Vector2(-i + ((36 - rect.Width) * notFirstSeg), 0).RotatedBy(rot) + new Vector2(2, 0).RotatedBy(x * 30), rect, new Color(60, 203, 255, 0) * 0.1f, rot, new Vector2(18, 13), 1f, SpriteEffects.None, 1f);
                }

                Main.spriteBatch.Draw(line, player.Center - Main.screenPosition + new Vector2(-i + ((36 - rect.Width) * notFirstSeg), 0).RotatedBy(rot), rect, new Color(255, 255, 255, 0), rot, new Vector2(18, 13), 1f, SpriteEffects.None, 1f);
            }

            for (int x = 0; x < 12; x++)
            {
                Main.spriteBatch.Draw(highlight, Projectile.Center - Main.screenPosition + new Vector2(4, 0).RotatedBy(x * 30), null, new Color(0, 109, 255, 0) * 0.05f, Projectile.rotation, new Vector2(9, 9), 1f, SpriteEffects.None, 1f);
            }

            for (int x = 0; x < 12; x++)
            {
                Main.spriteBatch.Draw(highlight, Projectile.Center - Main.screenPosition + new Vector2(2, 0).RotatedBy(x * 30), null, new Color(60, 203, 255, 0) * 0.1f, Projectile.rotation, new Vector2(9, 9), 1f, SpriteEffects.None, 1f);
            }

            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D glowMask = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/WulfrumWavewireProjGM").Value;

            Main.spriteBatch.Draw(glowMask, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(9, 9), 1f, SpriteEffects.None, 1f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), player.Center, Projectile.Center, 26, ref collisionPoint);
        }
	}
}
