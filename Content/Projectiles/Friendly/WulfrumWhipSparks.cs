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
            if (Main.rand.NextBool(5))
            {
            Particle spark2 = new BoltParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), 
            Projectile.velocity + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * 0.8f, false, 20, 0.1f, 
            new Color(143, 254, 255), new Vector2(1.8f, 0.8f), false, false, false, 0.6f);
            GeneralParticleHandler.SpawnParticle(spark2);
            }

            
            if (Main.rand.NextBool(50))
            {
                Dust.NewDust(Projectile.Center, 5, 5, DustID.Electric, 0f, 0f, 0, default(Color), 1f);
            }
        }
    }
}