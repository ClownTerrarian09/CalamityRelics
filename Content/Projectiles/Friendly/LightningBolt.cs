using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Projectiles;
using CalamityMod.Particles;
using Terraria.Graphics.Shaders;
using CalamityMod.Graphics.Primitives;
using System.Linq;
using CalamityMod.Effects;
using System;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	public class LightningBolt : ModProjectile
    {
        private bool canSpawnParticle = true;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.timeLeft = 50;

        }
        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            return Color.Lerp(Color.White, Color.Aqua, 0.35f) * Projectile.Opacity;
        }
        internal float WidthFunction(float completionRatio, Vector2 vertexPos)
        {
            return Projectile.scale * 60 * (-3*MathF.Pow(Utils.GetLerpValue(0f, 1, completionRatio, true) - 0.5f, 2f)+ 0.75f);   
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // The trail render
            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["CalamityMod:OverpoweredTouhouSpearShader"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
            int rendered = 22;
            int points = 80;
            var positions = Projectile.oldPos.Take(points).ToArray();
            PrimitiveRenderer.RenderTrail(positions, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:OverpoweredTouhouSpearShader"], smoothen: false),rendered);
            Main.spriteBatch.ExitShaderRegion();
            
            // Add partile effects
            if (Main.rand.NextBool(3))
            {
            Particle spark2 = new BoltParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), 
            Projectile.velocity * 0.2f, false, 30, 0.4f, 
            new Color(143, 254, 255), new Vector2(1.8f, 0.8f), false, false, false, 0.6f);
            GeneralParticleHandler.SpawnParticle(spark2);
            }

            return true;
        }
    }
}