using CalamityRelics.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.IL_Actions.NPCs;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class GrenadeProj : ModProjectile
	{
		Player player;

		float rawRot;
		float timere = 0;
        float targetRot;
        float bRawRot = 180;
        float dangerOpacity = 0;

        int plrDir;
        int state = 1;
        int bouttaBlowUp = 0;

        SpriteEffects sEffect;
        public override string Texture => "CalamityRelics/Content/Items/Reworks/Weapons/Grenade";

        public override void SetDefaults()
        {
			Projectile.damage = 0;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 1000;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }

        public void kaboom()
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, new Vector2(0, 1), 6f, 15, 15, 1000f));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GrenadeProjExp>(), 60, 5, -1, bouttaBlowUp);

            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(Projectile.position + new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(-50, 50)), 0, 0, ModContent.DustType<Smoke>(), 0, 0, 0);
            }

            if (bouttaBlowUp > 200)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, -8), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-4, -4), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(4, -4), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-6, -2), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(6, -2), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
            }

            Projectile.Kill();
        }
        public void stateHandler()
        {
            player = Main.player[Projectile.owner];

            // - - -

            if (state == 1)
            {
                if (timere == 1)
                {
                    if (Main.MouseWorld.X > player.Center.X)
                    {
                        plrDir = 1;
                    }
                    else
                    {
                        plrDir = -1;
                    }
                }

                player.direction = plrDir;

                rawRot = 90 * player.direction;

                if (plrDir > 0)
                {
                    sEffect = SpriteEffects.None;
                }
                else
                {
                    sEffect = SpriteEffects.FlipHorizontally;
                }

                Projectile.Center = player.Center + new Vector2(0, -22).RotatedBy(MathHelper.ToRadians(rawRot));

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(rawRot + 180));

                bRawRot += ((rawRot - (45 * plrDir)) - bRawRot) / 8;

                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(bRawRot + 180));

                if (timere == 12)
                {
                    int pin = Mod.Find<ModGore>($"GrenadePin").Type;

                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/GrenadePin"), Projectile.Center);

                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, 2), pin, 1);
                }
                
                if (timere == 20)
                {
                    SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                    timere = 0;
                    state = 2;
                }
            }

            if (state > 1)
            {
                bouttaBlowUp++;
            }

            if (state == 2)
            {
                player.direction = plrDir;

                rawRot += ((-45 * plrDir) - rawRot) / 10;

                Projectile.Center = player.Center + new Vector2(0, -22).RotatedBy(MathHelper.ToRadians(rawRot));

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(rawRot + 180));

                bRawRot += ((180) - bRawRot) / 5;

                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(bRawRot + 180));

                if (timere > 15)
                {
                    if (!player.channel)
                    {
                        timere = 0;
                        state = 3;
                    }
                }
            }

            if (state == 3)
            {
                if (timere == 1)
                {
                    Projectile.velocity = ((Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX)) * 8;
                }

                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (Projectile.Hitbox.Intersects(npc.Hitbox))
                    {
                        kaboom();
                        break;
                    }
                }

                Projectile.velocity.Y += 0.1f;

                if (Projectile.velocity.Y > 8)
                {
                    Projectile.velocity.Y = 8;
                }

                Projectile.velocity.X *= 0.995f;

                Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Projectile.velocity.Y) / 25;
            }

            if (bouttaBlowUp > 250)
            {
                kaboom();
            }
        }
        public override void AI()
        {
			timere++;           

            stateHandler();
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.X *= 0.99f;

            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Content/Items/Reworks/Weapons/Grenade").Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/GrenadeDanger").Value;

            if (bouttaBlowUp > 100)
            {
                dangerOpacity += 0.005f;
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, sEffect, 1);

            Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition, null, lightColor * dangerOpacity, Projectile.rotation, texture.Size() / 2, 1, sEffect, 1);

            return false;
        }
	}

    public class GrenadeProjExp : ModProjectile
    {
        int timere = 0;

        float EOpacity = 1;
        float ESize = 1;

        float EOpacity2 = 1;
        float ESize2 = 0;
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.damage = 60 + (int)(Projectile.ai[0]);
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.timeLeft = 100;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            timere++;

            if (timere == 1)
            {
                Projectile.damage += (int)(Projectile.ai[0] / 10);
            }

            if (timere == 3)
            {
                Projectile.damage = 0;
            }

            if (EOpacity > 0)
            {
                EOpacity -= 0.1f;
                ESize -= 0.02f;
            }

            ESize2 += (1 - ESize2) / 10;
            EOpacity2 -= 0.075f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/BloomFlare").Value;

            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/BloomCircleSmall").Value;

            Texture2D texture3 = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/ExplotionGraphic").Value;

            Texture2D texture4 = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/SmokeGraphic").Value;

            for (int i = 0; i < 2; i++)
            {
                Main.spriteBatch.Draw(texture3, Projectile.Center - Main.screenPosition, null, new Color(255, 144, 39, 0) * (0.5f * EOpacity2), i, texture3.Size() / 2, 1.25f * ESize2, SpriteEffects.None, 1);
            }

            Main.spriteBatch.Draw(texture4, Projectile.Center - Main.screenPosition, null, new Color(50, 50, 50) * (EOpacity2), 0, texture4.Size() / 2, 1.5f * ESize2, SpriteEffects.None, 1);

            Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition, null, new Color(255, 110, 69, 0) * (0.75f * EOpacity), 0, texture2.Size() / 2, 1.5f * EOpacity, SpriteEffects.None, 1);

            for (int i = 0; i < 2; i++)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 185, 129, 0) * EOpacity, i, texture.Size() / 2, 0.4f * ESize2, SpriteEffects.None, 1);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * EOpacity, 1, texture.Size() / 2, 0.2f * ESize2, SpriteEffects.None, 1);

            return false;
        }
    }

   public class GrenadeShrapnel : ModProjectile
    {
        public override string Texture => "CalamityRelics/Content/Projectiles/Friendly/GrenadeShrapnel";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            Projectile.frame = Main.rand.Next(0, 2);
        }
        public override void SetDefaults()
        {
            Projectile.damage = 23;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.1f;

            if (Projectile.velocity.Y > 8)
            {
                Projectile.velocity.Y = 8;
            }

            Projectile.velocity.X *= 0.995f;

            Projectile.rotation += MathHelper.ToRadians(15);
        }
    }

    public class StickyGrenadeProj : ModProjectile
    {
        Player player;

        float rawRot;
        float timere = 0;
        float targetRot;
        float bRawRot = 180;
        float dangerOpacity = 0;

        int plrDir;
        int state = 1;
        int bouttaBlowUp = 0;

        bool stuck = false;

        SpriteEffects sEffect;
        public override string Texture => "CalamityRelics/Content/Items/Reworks/Weapons/StickyGrenade";

        public override void SetDefaults()
        {
            Projectile.damage = 0;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 1000;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }

        public void kaboom()
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, new Vector2(0, 1), 6f, 15, 15, 1000f));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GrenadeProjExp>(), 60, 5, -1, bouttaBlowUp);

            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(Projectile.position + new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(-50, 50)), 0, 0, ModContent.DustType<Smoke>(), 0, 0, 0);
            }

            if (bouttaBlowUp > 200)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, -8), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-4, -4), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(4, -4), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-6, -2), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(6, -2), ModContent.ProjectileType<GrenadeShrapnel>(), 25, 5, -1);
            }

            Projectile.Kill();
        }
        public void stateHandler()
        {
            player = Main.player[Projectile.owner];

            // - - -

            if (state == 1)
            {
                if (timere == 1)
                {
                    if (Main.MouseWorld.X > player.Center.X)
                    {
                        plrDir = 1;
                    }
                    else
                    {
                        plrDir = -1;
                    }
                }

                player.direction = plrDir;

                rawRot = 90 * player.direction;

                if (plrDir > 0)
                {
                    sEffect = SpriteEffects.None;
                }
                else
                {
                    sEffect = SpriteEffects.FlipHorizontally;
                }

                Projectile.Center = player.Center + new Vector2(0, -22).RotatedBy(MathHelper.ToRadians(rawRot));

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(rawRot + 180));

                bRawRot += ((rawRot - (45 * plrDir)) - bRawRot) / 8;

                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(bRawRot + 180));

                if (timere == 12)
                {
                    int pin = Mod.Find<ModGore>($"GrenadePin").Type;

                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/GrenadePin"), Projectile.Center);

                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, 2), pin, 1);
                }

                if (timere == 20)
                {
                    SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                    timere = 0;
                    state = 2;
                }
            }

            if (state > 1)
            {
                bouttaBlowUp++;
            }

            if (state == 2)
            {
                player.direction = plrDir;

                rawRot += ((-45 * plrDir) - rawRot) / 10;

                Projectile.Center = player.Center + new Vector2(0, -22).RotatedBy(MathHelper.ToRadians(rawRot));

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(rawRot + 180));

                bRawRot += ((180) - bRawRot) / 5;

                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(bRawRot + 180));

                if (timere > 15)
                {
                    if (!player.channel)
                    {
                        timere = 0;
                        state = 3;
                    }
                }
            }

            if (state == 3)
            {
                if (timere == 1)
                {
                    Projectile.velocity = ((Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX)) * 8;
                }

                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (Projectile.Hitbox.Intersects(npc.Hitbox))
                    {
                        kaboom();
                        break;
                    }
                }

                if (!stuck)
                {
                    Projectile.velocity.Y += 0.1f;

                    if (Projectile.velocity.Y > 8)
                    {
                        Projectile.velocity.Y = 8;
                    }

                    Projectile.velocity.X *= 0.995f;

                    Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Projectile.velocity.Y) / 25;
                }
                else
                {
                    Projectile.velocity = Vector2.Zero;
                }
            }

            if (bouttaBlowUp > 250)
            {
                kaboom();
            }
        }
        public override void AI()
        {
            timere++;

            stateHandler();
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            stuck = true;

            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Content/Items/Reworks/Weapons/StickyGrenade").Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/StickyGrenadeDanger").Value;

            if (bouttaBlowUp > 100)
            {
                dangerOpacity += 0.005f;
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, sEffect, 1);

            Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition, null, lightColor * dangerOpacity, Projectile.rotation, texture.Size() / 2, 1, sEffect, 1);

            return false;
        }
    }
}
