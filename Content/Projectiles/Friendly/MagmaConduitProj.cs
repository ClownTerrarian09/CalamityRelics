using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class MagmaConduitProj : ModProjectile
	{
		Player player;

		float dir;
        float timere = 0;
        float highlightSize = 0;

        float dirOffset;

		SpriteEffects effect;
        public override void SetStaticDefaults()
        {
			Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
			Projectile.damage = 0;
			Projectile.width = 30;
			Projectile.height = 34;
            Projectile.timeLeft = 10000;
			Projectile.tileCollide = false;

        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void AI()
        {
            timere++;

            if (timere == 1)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/FireCharge"), Projectile.Center);
            }

            player = Main.player[Projectile.owner];

            player.SetDummyItemTime(2);

			dir = (player.Center - Main.MouseWorld).ToRotation();

			Projectile.Center = player.Center + new Vector2(15, 0).RotatedBy(dir + MathHelper.ToRadians(180 + dirOffset));

			Projectile.rotation = dir + MathHelper.ToRadians(180 + dirOffset);

            if (Main.MouseWorld.X > player.Center.X)
            {
                effect = SpriteEffects.None;
                player.direction = 1;
            }
            else
            {
                effect = SpriteEffects.FlipVertically;
                player.direction = -1;
            }

            Projectile.frameCounter++;

            if (Projectile.frameCounter == 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;

                if (Projectile.frame == 6)
                {
                    Projectile.frame = 0;
                }
            }

            if (highlightSize < 1)
            {
                highlightSize += 0.01f;
            }

            if (timere == 80)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/FireShoot"), Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, new Vector2(0, 1), 6f, 15, 15, 1000f));
            }

            if (timere > 80)
            {
                dirOffset = (float)Math.Sin(timere * 0.1f) * 5;

                if (player.statMana > 0 + 2)
                {
                    player.statMana -= 2;
                }
                else
                {
                    Projectile.Kill();
                }

                if (timere % 2 == 1)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.Next(8, 12), Main.rand.Next(-2, 2)).RotatedBy(dir + MathHelper.ToRadians(180 + dirOffset)), ModContent.ProjectileType<MagmaProj>(), 40, 2, -1);
                }

                if (timere % 12 == 1)
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/LavaShoot"), Projectile.Center);
                }
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, dir + MathHelper.ToRadians(90));

            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, dir + MathHelper.ToRadians(90));

            if (player.channel)
            {
                Projectile.timeLeft = 10000;
            }
            else
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Content/Projectiles/Friendly/MagmaConduitProj").Value;

            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityRelics/Content/Projectiles/Friendly/MagmaConduitProjH").Value;

            Texture2D texture3 = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/BloomCircleSmall").Value;

            Rectangle rect = new Rectangle(0, Projectile.frame * 34, 30, 34);

            for (int i = 0; i < 12; i++)
            {
                Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition + new Vector2(5 * highlightSize, 0).RotatedBy(i * 30), rect, new Color(255, 140, 0, 0) * 0.25f, Projectile.rotation, new Vector2(15, 17), 1, effect, 1);
            }

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, rect, lightColor, Projectile.rotation, new Vector2(15, 17), 1, effect, 1);

            Main.spriteBatch.Draw(texture3, Projectile.Center - Main.screenPosition, null, new Color(255, 140, 0, 0) * highlightSize, 0, texture3.Size() / 2, 0.5f, effect, 1);

            for (int i = 0; i < 2; i++)
            {
                Main.spriteBatch.Draw(texture3, Projectile.Center - Main.screenPosition, null, new Color(255, 175, 126, 0) * highlightSize, 0, texture3.Size() / 2, 0.3f, effect, 1);
            }

            return false;
        }
	}

    public class MagmaProj : ModProjectile
    {
        int timere;
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.damage = 40;
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.timeLeft = 500;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.scale = 0.1f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.X *= 0.95f;

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void AI()
        {
            timere++;

            Projectile.velocity.X *= 0.99f;
            
            if (Projectile.velocity.Y < 15)
            {
                Projectile.velocity.Y += 0.1f;
            }

            if (timere > 10)
            {
                Projectile.scale -= 0.005f;

                if (Projectile.scale < 0.01f)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.scale += (1 - Projectile.scale) / 5;
            }

            Projectile.width = (int)(30 * Projectile.scale);
            Projectile.height = (int)(30 * Projectile.scale);

            Projectile.rotation = (Projectile.position - (Projectile.position + Projectile.velocity)).ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 150);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/BloomCircleSmall").Value;

            // new Color(255, 140, 0, 0)

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 96, 9, 0), Projectile.rotation, texture.Size() / 2, new Vector2(0.8f, 0.6f) * Projectile.scale, SpriteEffects.None, 1);

            for (int i = 0; i < 2; i++)
            {

                // new Color(255, 175, 126, 0)

                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 123, 36, 0), Projectile.rotation, texture.Size() / 2, new Vector2(0.6f, 0.4f) * Projectile.scale, SpriteEffects.None, 1);
            }

            return false;
        }
    }
}
