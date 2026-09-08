
using System;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Hostile
{

    public class WulfrumElectricOrb : ModProjectile
    {

        public static int maxTime = 750;
        public static float maxGravity => 5;
        public static float gravityFactor => 0.05f;


        public float xVelocity = 0;
        public int currentTime = 0;


        public override void SetStaticDefaults() {
			ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true; 
		}

		public override void SetDefaults() {
			Projectile.width = 50;
			Projectile.height = 50;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Ranged;
            Projectile.scale = 1.2f;
            Projectile.hostile = true;
		}


        public override void AI()
        {
            currentTime++;
            // Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if(currentTime >= maxTime)
            {
                Projectile.Kill();
            }
            else if(Projectile.velocity.Y < maxGravity)
            {
                Projectile.velocity.Y += gravityFactor;
            }

        }
    } 

}