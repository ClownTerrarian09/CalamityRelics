using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Items;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityRelics.Content.Projectiles.Friendly;

namespace CalamityRelics.Content.Items.Weapons.Summon
{
	public class WulfrumPowerCord : ModItem
	{
		public static readonly int TagDamage = 3;

		// public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ExampleWhipTagDamage);

		public override void SetStaticDefaults() {
			// Here is where we define how much TagDamage the whip does.
			// TagDuration and CritChance can be modified, too.
			// For more customizability, see Example Whip Advanced's tag effects.
			// ItemID.Sets.UniqueTagEffects[Type] = new WhipTagEffect() { TagDamage = TagDamage };
		}

		public override void SetDefaults() {
			Item.DefaultToWhip(ModContent.ProjectileType<WulfrumWhipProjectile>(), 15, 1, 3);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(gold: 1);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// This gives some visual variance on how fast the whip swinging animation plays out.
			// This has no effect on the actual collision.
			float swingDirection = 0.6f + (0.4f * Main.rand.NextFloat());

			if (Main.rand.NextBool(3)) {
				swingDirection *= -2.5f;
			}
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, swingDirection, player.whoAmI);
			return false;
		}

		public override bool MeleePrefix() {
			return true;
		}
	}
}