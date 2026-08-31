using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Buffs;

namespace CalamityRelics.Content.Projectiles.Friendly
{
	public class WulfrumWhipProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			// This makes the projectile use whip collision detection and allows flasks to be applied to it.
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			// This method quickly sets the whip's properties.
			Projectile.DefaultToWhip();

			// use these to change from the vanilla defaults
			Projectile.WhipSettings.Segments = 8;
			Projectile.light = 0.7f;
			Projectile.WhipSettings.RangeMultiplier = 1f;
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
			for (int i = 0; i < list.Count - 1; i++) {
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
            Main.DrawWhip_WhipBland(Projectile, list);

            Projectile.NewProjectile(Projectile.GetSource_FromAI(), list[list.Count - 1], new Vector2(0, 0), ModContent.ProjectileType<WulfrumWhipSparks>(), 1, 0f, Main.myPlayer);
			
            return false;
		}
	}
}