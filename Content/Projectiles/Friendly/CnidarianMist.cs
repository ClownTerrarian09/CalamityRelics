using System;
using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Magic;
using CalamityRelics.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles;
using CalamityMod.Particles;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class CnidarianMist : ModProjectile
    {

        private int foamTimer = 0;
        private bool hitWater = false;
        public override void SetStaticDefaults()
        {
        
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 400;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            float amnt = ((MathF.Abs(Projectile.velocity.X) + MathF.Abs(Projectile.velocity.Y)) * 0.5f) / 12.5f;
            int alpha = 50 + (205 - (int)Math.Ceiling(amnt * 205f));
            
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                hitWater = true;
                Particle bubble = new GenericBubbleParticle(Projectile.Center, Projectile.velocity * 0.5f, 1f * Main.rand.NextFloat(0.8f, 1f), Projectile.rotation, 2);
                GeneralParticleHandler.SpawnParticle(bubble);
                Projectile.velocity.X *= 0.9f;
                Projectile.velocity.Y = MathF.Max(Projectile.velocity.Y - 0.3f, -4);
                Projectile.timeLeft++;
            }
            else
            {
                Particle water = new WaterFlavoredParticle(Projectile.Center, Projectile.velocity * 0.5f, false, 2, 1f * Main.rand.NextFloat(0.8f, 1f), new Color(100, 149, 273, alpha) * amnt);
                GeneralParticleHandler.SpawnParticle(water);
                if(Main.rand.NextFloat(amnt + 0.1f) > 0.08f)
                    SpawnDust(DustID.DungeonWater);
                Projectile.velocity *= 0.98f;
            }

            Projectile.alpha = alpha;
            
            Projectile.rotation += MathHelper.ToRadians(amnt * (Projectile.velocity.X > 0 ? 15 : -15));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity *= 0.3f;
        }

        private void SpawnDust(int type)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(0.05f, 0.05f);
                Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f),0, 0, type,velocity.X, velocity.Y);
                dust.noGravity = true;
            
                if (Main.rand.Next(4) == 0)
                {
                    dust.scale *= 1.2f;
                }
                else
                {
                    dust.scale *= 0.8f;
                }

                dust.customData = 1;
            }
            
        
        }


        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item54, Projectile.Center);
            foreach (NPC npc in Main.npc)
            {
                if (Vector2.Distance(Projectile.position, npc.position) < Projectile.width * 1.5f)
                {
                    if (npc.active && npc.active && npc.life > 0)
                    {
                        int damage = Main.rand.Next(20, 35);
                        npc.StrikeNPC(new NPC.HitInfo(){Damage = damage,Knockback = 0,DamageType = DamageClass.Magic});
                        Main.player[Projectile.owner].dpsDamage += damage;
                    }
                }
            }

            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(3, 3);
                Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),4, 4, DustID.DungeonWater,velocity.X, velocity.Y, 100,default, 1f);
            }
        }
        
        
    }

}