using CalamityRelics.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class WulfrumESHoldout : ModProjectile
    {
        public bool canShootNormal = true;
        public int cooldown, cooldownTimer, charge, chargeTimer, chargeCooldown;
        public override string Texture => $"CalamityRelics/Content/Items/Weapons/WulfrumElectromagneticSphere";

        public override void SetStaticDefaults() {
			// Prevents jitter when stepping up and down blocks and half blocks
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

        public override void SetDefaults()
        {
            Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.ignoreWater = true;
            Projectile.hide = true;

            DrawOffsetX = -10;
			DrawOriginOffsetY = -4;
            cooldownTimer = 20;
            cooldown = 0;
            chargeTimer = 30;
            chargeCooldown = 0;
            charge = 0;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

			Projectile.timeLeft = 60;
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
			if (Main.myPlayer == Projectile.owner) {
				// Code is copied from example mod. 
				if (player.channel) {
					float holdoutDistance = player.HeldItem.shootSpeed * Projectile.scale;
					// Calculate a normalized vector from player to mouse and multiply by holdoutDistance to determine resulting holdoutOffset
					Vector2 holdoutOffset = holdoutDistance * Vector2.Normalize(Main.MouseWorld - playerCenter);
					if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y) {
						// This will sync the projectile, most importantly, the velocity.
						Projectile.netUpdate = true;
					}

					// Projectile.velocity acts as a holdoutOffset for held projectiles.
					Projectile.velocity = holdoutOffset;
				}
				else {
					Projectile.Kill();
				}
			}

			if (Projectile.velocity.X > 0f) {
				player.ChangeDir(1);
			}
			else if (Projectile.velocity.X < 0f) {
				player.ChangeDir(-1);
			}

            // THIS PART IS NOT FROM THE EXAMPLE MOD. 

            // When you are clicking LM, shoot the regular bullet.
            if (canShootNormal && Main.myPlayer == Projectile.owner){
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), playerCenter + Projectile.velocity, Vector2.Normalize(Projectile.velocity) *4, 
                ModContent.ProjectileType<ElectromagneticSphereProjectileCharge0>(), 50, 2f, player.whoAmI, 1f);
                canShootNormal = false;
            }
            // Regular bullet recharge
            if (!canShootNormal)
            {
                if (cooldown < cooldownTimer)
                {
                    cooldown++;
                }
                else
                {
                    cooldown = 0;
                    canShootNormal = true;
                }
            }
            

			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction); // Change the player's direction based on the projectile's own
			// player.heldProj = Projectile.whoAmI; // We tell the player that the drill is the held projectile, so it will draw in their hand
			player.SetDummyItemTime(2); // Make sure the player's item time does not change while the projectile is out
			Projectile.Center = playerCenter; // Centers the projectile on the player. Projectile.velocity will be added to this in later Terraria code causing the projectile to be held away from the player at a set distance.
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

    }
}