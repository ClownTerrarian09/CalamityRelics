using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class ElectromagneticSphereProjectileCharge3 : ModProjectile
    {
        private int bounceCount;
        private bool initialized;

        public override void SetDefaults()
        {
            Projectile.width = 96;  
            Projectile.height = 96; 
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1; 
            Projectile.timeLeft = 360; 

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; 

            Projectile.aiStyle = 0; 
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