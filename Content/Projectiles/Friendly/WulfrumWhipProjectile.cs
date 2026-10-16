using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Buffs;
using Terraria.Audio;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	public class WulfrumWhipProjectile : ModProjectile
	{
		
		List<Vector2> positionList = new List<Vector2>();
		public int timer;
		public override void SetStaticDefaults() {
			// This makes the projectile use whip collision detection and allows flasks to be applied to it.
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			// This method quickly sets the whip's properties.
			Projectile.DefaultToWhip();

			// use these to change from the vanilla defaults
			Projectile.WhipSettings.Segments = 8;
			Projectile.light = 1.1f;
			Projectile.WhipSettings.RangeMultiplier = 1f;
			timer = 0;
			Projectile.soundDelay = 36;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<WulfrumWhipDebuff>(), 240);
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI; // Apply the targeting focus on the NPC who was hit.
			Projectile.damage = (int)(Projectile.damage * 0.3f); // Multihit penalty. Decrease the damage the more enemies the whip hits.
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(List<Vector2> list) {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);

			Vector2 pos = list[0];
			// If your whip has a long range and this line is poking out of the front, use list.Count - 2 instead of list.Count - 1.
			for (int i = 0; i < list.Count - 2; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.Cyan);
				Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			DrawLine(list);
            // This code is from ExampleMod's Example Whip
			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count - 1; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new Rectangle(0, 0, 18, 40); // The size of the Handle (measured in pixels)
				Vector2 origin = new Vector2(7, 10); // Offset for where the player's hand will start measured from the top left of the image.
				float scale = 0.8f;

				// These statements determine what part of the spritesheet to draw for the current segment.
				// They can also be changed to suit your sprite.
				if (i == list.Count - 2) {
					// This is the head of the whip. You need to measure the sprite to figure out these values.
					frame.Y = 240; // Distance from the top of the sprite to the start of the frame.
					frame.Height = 32; // Height of the frame.

					// For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
					Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
					float t = Timer / timeToFlyOut;
					scale = MathHelper.Lerp(0.6f, 1f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
				}
				else if (i == list.Count - 3) {
					// Third segment
					frame.Y = 186;
					frame.Height = 218-186;
				}
				else if (i > 2) {
					// Second Segment
					frame.Y = 128;
					frame.Height = 166-128;
				}
				else if (i > 0) {
					// First Segment
					frame.Y = 66;
					frame.Height = 104-66;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			positionList.Add(list[^1]);
			
            return false;
		}
		public override void PostAI()
        {
			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(SoundID.NPCHit34, Projectile.Center);
				Projectile.soundDelay = 36;
			}
			timer ++;
			if (timer > 15)
			{
			Vector2 currentPos = positionList[positionList.Count - 1];
			Vector2 lastPos = positionList[positionList.Count - 9];
			Vector2 midpoint1 =  Midpoint(currentPos, lastPos);
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), currentPos, new Vector2(0, 0), ModContent.ProjectileType<WulfrumWhipSparks>(), 5, 0f, Main.myPlayer);
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), midpoint1, new Vector2(0, 0), ModContent.ProjectileType<WulfrumWhipSparks>(), 5, 0f, Main.myPlayer);
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), Midpoint(lastPos, midpoint1), new Vector2(0, 0), ModContent.ProjectileType<WulfrumWhipSparks>(), 0, 0f, Main.myPlayer);
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), Midpoint(currentPos, midpoint1), new Vector2(0, 0), ModContent.ProjectileType<WulfrumWhipSparks>(), 0, 0f, Main.myPlayer);
			}
        }

		public Vector2 Midpoint(Vector2 v1, Vector2 v2)
		{
			return new Vector2((v1.X + v2.X)/2, (v1.Y + v2.Y)/2);
		}
	}
}