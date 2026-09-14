using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Environment.SecurityControllerProj
{
    public class SecurityControllerProj : ModProjectile
    {
        private const string BaseTexturePath = "CalamityRelics/Content/Items/Utilities/SecurityController/HoldingController";
        public override string Texture => BaseTexturePath;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.scale = 0.5f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.itemTime == 0 || player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            float forwardOffset = 22f;
            float verticalOffset = 0f;

            Projectile.Center = player.MountedCenter + new Vector2(forwardOffset * player.direction, verticalOffset);
            Projectile.spriteDirection = player.direction;

            player.heldProj = Projectile.whoAmI;
            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            string texturePath = Texture;

            if (player.itemAnimation > 120)
            {
                texturePath += "Press";
            }

            Texture2D drawTexture = ModContent.Request<Texture2D>(texturePath).Value;
            SpriteEffects effects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.spriteBatch.Draw(drawTexture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, drawTexture.Size() / 2, Projectile.scale, effects, 0f);

            return false;
        }
    }
}