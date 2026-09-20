using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	public class EnchantedBloomVine : ModProjectile
	{
		protected override bool CloneNewInstances => true;
		private static readonly List<Vector2> points = new();
		
		public const int FlyOutTime = 30;
		private const int Segments = 12;
		public const float MaxLength = 250f;

		private float TimeToFlyOut => FlyOutTime * Projectile.MaxUpdates;


		
		public override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = false;
			Projectile.extraUpdates = 1;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}


		private float ChargeTime {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead) {
				Projectile.Kill();
				return;
			}

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.Center = owner.MountedCenter;
			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			Timer++;
			if (Timer >= TimeToFlyOut) {
				Projectile.Kill();
				return;
			}

			FillPoints(points);

			if (Timer == (int)(TimeToFlyOut / 2)) {
				SoundEngine.PlaySound(SoundID.Grass, points[^1]);
			}

			float swingProgress = Timer / TimeToFlyOut;
			if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, true) > 0.5f && !Main.rand.NextBool(3)) {
				int pointIndex = Main.rand.Next(Math.Max(0, points.Count - 4), points.Count);
				Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
				Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height,
					ModContent.DustType<Dusts.EnchantedPetal>(), 0f, 0f, 100, Color.White);
			}

			HitProjectiles();
		}
		
		private void FillPoints(List<Vector2> points) {
			points.Clear();
			Player owner = Main.player[Projectile.owner];

			float progress = Timer / TimeToFlyOut;
			float extension = MathF.Sin(progress * MathHelper.Pi);
			Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);

			for (int i = 0; i <= Segments; i++) {
				float t = i / (float)Segments;
				float wave = MathF.Sin(t * MathHelper.TwoPi + progress * 10f) * 12f * t * extension;
				points.Add(owner.MountedCenter + dir * (MaxLength * extension * t) + perp * wave);
			}
		}
		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}

		public override bool CanHitPlayer(Player target)
		{
			return false;
		}

		private void DrawLine(List<Vector2> list) {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);

			Vector2 pos = list[0];
			for (int i = 0; i < list.Count - 2; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.CadetBlue);
				Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}
		private void HitProjectiles() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile other = Main.projectile[i];
				if (!other.active || !other.hostile || other.friendly || other.damage <= 0)
					continue;

				Vector2 pos = other.position, size = other.Size;
				float collisionPoint = 0f;
				for (int p = 0; p < points.Count - 1; p++) {
					if (Collision.CheckAABBvLineCollision(pos, size, points[p], points[p + 1], Projectile.width, ref collisionPoint)) {
						//other.Kill();
						other.friendly = true;
						other.hostile = false;
						other.velocity = -other.velocity;
						break;
					}
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			List<Vector2> list = new List<Vector2>();
			FillPoints(list);

			DrawLine(list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			int totalSegments = Segments;

			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count - 1; i++) {
				Rectangle frame = new Rectangle(0, 0, 10, 26);
				Vector2 origin = new Vector2(5, 8);
				float scale = 1;

				if (i == list.Count - 2) {
					frame.Y = 74;
					frame.Height = 18;

					float t = Timer / TimeToFlyOut;
					scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
				}
				else if (i > 2 * (totalSegments / 3)) {
					frame.Y = 58;
					frame.Height = 16;
				}
				else if (i > totalSegments / 3) {
					frame.Y = 42;
					frame.Height = 16;
				}
				else if (i != 0) {
					frame.Y = 26;
					frame.Height = 16;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
}