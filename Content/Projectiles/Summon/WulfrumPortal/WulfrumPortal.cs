using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Summon.WulfrumPortal
{
    public class WulfrumPortal : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            if (Main.rand.NextBool())
            {
                Vector2 dustOffset = Main.rand.NextVector2CircularEdge(40f, 40f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Vortex, dustOffset * -0.1f, 50, Color.LightCyan, 1.2f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.5f).noGravity = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int botType = (int)Projectile.ai[0];
                NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, botType);
            }
        }
    }
}