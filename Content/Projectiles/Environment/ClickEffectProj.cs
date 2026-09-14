using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Environment.ClickEffectProj
{
    public class ClickEffectProj : ModProjectile
    {
        public override string Texture => "CalamityRelics/Content/Items/Utilities/SecurityController/Click";
        private const int FrameWidth = 48;
        private const int FrameHeight = 30;
        private const int TotalFrames = 16;
        private const int AnimationSpeed = 4;

        private readonly Vector2 CustomScale = new Vector2(1.2f, 1.2f);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = FrameWidth;
            Projectile.height = FrameHeight;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Projectile.Center = player.Top - new Vector2(0, 35);

            Projectile.frameCounter++;

            if (Projectile.frameCounter >= AnimationSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= TotalFrames)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Rectangle sourceRect = new Rectangle(FrameWidth * Projectile.frame, 0, FrameWidth, FrameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                Color.White,
                Projectile.rotation,
                origin,
                CustomScale,
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }
}