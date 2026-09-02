using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Projectiles;
using CalamityMod.Particles;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class ElectromagneticSphereProjectileCharge0 : ModProjectile
    {
        // public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private int bounceCount;
        private bool initialized;

        public override void SetDefaults()
        {
            Projectile.width = 38;  
            Projectile.height = 38; 
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1; 
            Projectile.timeLeft = 360; 

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; 

            Projectile.aiStyle = 0; 
        }
        private Color color = new Color(158, 250, 179);
        private Color color2 = new Color(158, 250, 179, 0.6f);
        public override void AI()
        {
            Particle ring = new BloomRing(Projectile.Center, Projectile.velocity, color, 0.25f*Projectile.ai[0], 5);
            GeneralParticleHandler.SpawnParticle(ring);
            Particle spark2 = new BoltParticle(Projectile.Center, 
            Projectile.velocity + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)), false, 15, 0.12f * Projectile.ai[0], 
            color2, new Vector2(2.5f, 0.8f), false, false, false, 0.3f);
            GeneralParticleHandler.SpawnParticle(spark2);

            Particle spark = new BoltParticle(Projectile.Center, 
            new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.2f, 0.2f)), false, 2, 0.12f * Projectile.ai[0], 
            color, new Vector2(4f, 0.8f), false, true, false);
            GeneralParticleHandler.SpawnParticle(spark);

        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;
            
            if (bounceCount > 5)
            {
                return true; 
            }
            
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 1f; 
            }
            
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 1f; 
            }
            return false; 
        }
    }
}