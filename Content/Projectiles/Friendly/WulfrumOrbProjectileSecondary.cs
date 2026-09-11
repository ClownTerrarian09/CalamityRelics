using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class WulfrumOrbProjectileSecondary : ModProjectile
    {
        private int bounceCount;
        private bool initialized;
        // public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 95;  
            Projectile.height = 95; 
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 8; 
            Projectile.timeLeft = 360; 

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; 

            Projectile.aiStyle = 0; 
        }
        public override void AI()
        {
            
            Projectile.scale += 0.01f; 

            //Scale the physical damage hitbox
            int newSize = (int)(38 * Projectile.scale);

            //re-centre not sure if it applies tbh
            Vector2 currentCenter = Projectile.Center;
            Projectile.width = newSize;
            Projectile.height = newSize;
            Projectile.Center = currentCenter;
           
            
            int minionIndex = (int)Projectile.ai[0];
            if (minionIndex >= 0 && minionIndex < Main.maxProjectiles)
            {
                Projectile parentMinion = Main.projectile[minionIndex];
                if (parentMinion.active)
                {
                    Projectile.Center = parentMinion.Center;
                }
            }

            //Fade out as the blast dissipates
            // 0 (fully visible) to 255 (invisible)
            Projectile.alpha = (int)(255f * (1f - ((float)Projectile.timeLeft / 30f)));
        }
    }
}
