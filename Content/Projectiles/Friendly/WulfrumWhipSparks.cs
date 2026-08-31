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
	public class WulfrumWhipSparks : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = 3;

        }
        public override void AI()
        {
            Particle spark2 = new BoltParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), 
            -Projectile.velocity + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)), false, 18, 0.15f, 
            new Color(0, 216, 255, 0.1f), new Vector2(2.5f, 0.8f), true, true, false, 0.3f);
            GeneralParticleHandler.SpawnParticle(spark2);
            
            if (Main.rand.NextBool(50))
            {
                Dust.NewDust(Projectile.Center, 5, 5, DustID.Electric, 0f, 0f, 0, default(Color), 1f);
            }
        }
    }
}