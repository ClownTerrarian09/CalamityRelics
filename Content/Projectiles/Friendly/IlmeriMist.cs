using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class IlmeriMist : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.timeLeft = 180;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(0f, MathF.PI * 2f);
            Projectile.velocity = Main.rand.NextVector2Circular(1, 1);
        }

        public override void AI()
        {
            
            Projectile.velocity *= 0.98f;
            Projectile.rotation += Projectile.velocity.X * 0.1f;
            Projectile.alpha = 100 + (int)((1f - MathF.Min(Projectile.timeLeft / 180f, 1)) * 155f);
            Particle mist = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f,Color.White, 30, 1.4f, 0.1f, glowing: true);
            GeneralParticleHandler.SpawnParticle(mist);
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                if(Main.rand.NextBool(5))
                    Dust.NewDust(Projectile.Center, Projectile.width,Projectile.height, DustID.BreatheBubble, Projectile.velocity.X, Projectile.velocity.Y);
                Projectile.velocity.Y -= 0.02f;
                
            }
            
        }


        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
                modifiers.SourceDamage += 2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.velocity *= 0.95f;
            target.AddBuff(BuffID.Wet, 10);
            
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Color drawColor = Projectile.GetAlpha(lightColor) * 0.5f;

            UnifiedRandom rng = new UnifiedRandom(Projectile.identity);

            for (int i = 0; i < 6; i++)
            {
                float rot = (i / 6f) * MathHelper.TwoPi;
                Vector2 offset = rot.ToRotationVector2() * rng.NextFloat(16, 25);
                Vector2 drawPos = Projectile.Center + offset - Main.screenPosition;
                float spin = Projectile.rotation + rng.NextFloat(-MathF.PI, MathF.PI);

                Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor, spin, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}

