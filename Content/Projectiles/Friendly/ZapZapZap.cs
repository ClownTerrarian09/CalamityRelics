using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles;
using CalamityMod.Particles;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	public class ZapZapZap : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;

        }
        public override void AI()
        {
            Particle bolt = new ThunderBoltVFX(Projectile.Center, Projectile.velocity.ToRotation() - MathHelper.PiOver2, Main.rand.NextFloat(0.1f, 0.4f), new Color(143, 254, 255), 15, 0f);
            for (int i = 0; i<3; i++)
            {  
            Particle spark2 = new BoltParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)), 
            Projectile.velocity + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * 0.8f, false, 20, Main.rand.NextFloat(0.2f, 0.5f), 
            new Color(143, 254, 255), new Vector2(1.8f, 0.8f), false, true, false, 0.6f);
            
            GeneralParticleHandler.SpawnParticle(spark2);
            }
            GeneralParticleHandler.SpawnParticle(bolt);

        }
    }
}