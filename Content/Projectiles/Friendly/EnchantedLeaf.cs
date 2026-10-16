using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    
    public class EnchantedLeaf : ModProjectile
    {
        protected override bool CloneNewInstances => true;

        private NPC homing;
        private bool startedHoming;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
        }

        public override void AI()
        {
            

            if (Projectile.timeLeft <= 5 && !startedHoming)
            {
                float dist = 800;
                NPC target = null;
                foreach (NPC npc in Main.npc)
                {
                    if(!npc.active || !npc.CanBeChasedBy())
                        continue;
                    if (Vector2.Distance(Projectile.Center, npc.Center) <= dist)
                    {
                        target = npc;
                        dist = Vector2.Distance(Projectile.Center, npc.Center);
                    }
                }
                if (target != null)
                {
                    
                    startedHoming = true;
                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Item/LunicShot1"){Volume = 0.3f}, Projectile.position);
                    Projectile.timeLeft = 1000;
                    Projectile.extraUpdates = 2;
                    Projectile.penetrate = 1;
                    homing = target;
                }
            }

            if (homing != null && startedHoming)
            {
                if (!homing.active || !homing.CanBeChasedBy())
                {
                    homing = null;
                    startedHoming = false;
                }
                Vector2 direction = Vector2.Normalize(homing.Center - Projectile.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 20f, 0.3f);
                Particle bloom = new GenericBloom(Projectile.Center, Projectile.velocity * 0.5f, Color.Salmon, 0.05f, 10);
                GeneralParticleHandler.SpawnParticle(bloom);
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.EnchantedPetal>(), Main.rand.NextVector2Circular(2,2));
                
            }
            else
            {
                if (Projectile.timeLeft <= 30)
                {
                    Projectile.velocity *= 0.85f;
                    Projectile.alpha = (int)((1f - (MathF.Abs(Projectile.velocity.X + Projectile.velocity.Y) / 30f)) * 200f);
                }
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

       
        public override void OnKill(int timeLeft)
        {
            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.EnchantedPetal>(), Main.rand.NextVector2Circular(2,2));
        }

        
        public override bool PreDraw(ref Color lightColor)
        {
            if(!startedHoming)
                return true;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = frame.Size() / 2f;
            Vector2 centerOffset = Projectile.Size / 2f + new Vector2(0f, Projectile.gfxOffY);
            Color baseColor = Projectile.GetAlpha(Color.White);
            

            for (int k = Projectile.oldPos.Length - 1; k > 0; k--) {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue; 

                float progress = 1f - k / (float)Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[k] + centerOffset - Main.screenPosition;
                float scale = Projectile.scale * MathHelper.Lerp(0.6f, 1f, progress);

                Main.EntitySpriteDraw(texture, drawPos, frame, baseColor * progress * 0.5f,
                    Projectile.oldRot[k], origin, scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                frame, baseColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
