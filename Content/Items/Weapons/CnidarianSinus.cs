using System;
using CalamityMod;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Materials;
using CalamityRelics.Content.Projectiles;
using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;

namespace CalamityRelics.Content.Items.Weapons
{
	public class CnidarianSinus : ModItem
	{
		public override void SetDefaults()
		{
			Item.mana = 7;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 25;
			Item.useTime = 25;
			Item.knockBack = 7f;
			Item.width = 38;
			Item.height = 10;
			Item.damage = 4;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<CnidarianMist>();
			Item.shootSpeed = 12.5f;
			Item.UseSound = SoundID.Item85;
			Item.noMelee = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0,1);
			Item.DamageType = DamageClass.Magic;
			Item.holdStyle = 16;

		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type,
			int damage, float knockback)
		{
			Projectile.NewProjectile(source, position + (Vector2.Normalize(velocity) * 50), velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
		public override void UseAnimation(Player player)
		{
			var mp = player.GetModPlayer<CnidarianSinusPlayer>();
			int time = (int)MathF.Ceiling(MathF.Max(25 - (11 * (mp.useTimer / 20f)), 14));
			Item.useTime = time;
			Item.useAnimation = time;
			
		}
		public override void HoldItem(Player player)
		{
			var mp = player.GetModPlayer<CnidarianSinusPlayer>();
			bool holding = player.controlUseItem;

			if (holding)
			{
				if(mp.useAmnt < 0.5f)
					mp.useAmnt += 0.01f;
				mp.useTimer += mp.useAmnt;
			}
			else if (mp.useTimer > 0f)
			{
				mp.useTimer = 0;
				mp.useAmnt = 0.0166f;
			}
			mp.wasHolding = holding;
		}

		public override void HoldStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
		public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);

		public void SetItemInHand(Player player, Rectangle heldItemFrame)
		{
			var mp = player.GetModPlayer<CnidarianSinusPlayer>();
			
			if (Main.MouseWorld.X > player.Center.X)
			{
				player.ChangeDir(1);
			}
			else
			{
				player.ChangeDir(-1);
			}

			Vector2 itemPosition = player.MountedCenter + new Vector2(45f, 0).RotatedBy((Main.MouseWorld - player.MountedCenter).ToRotation());
			float itemRotation = (Main.MouseWorld - itemPosition).ToRotation();
			if(mp.useTimer > 0)
				itemPosition += new Vector2(MathF.Sin(mp.useTimer) * 1f, MathF.Cos(mp.useTimer) * 1f);
			
			Vector2 itemSize = new Vector2(38, 10);
			Vector2 itemOrigin = new Vector2(19, 5);
			CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
		}
	}
	
	public class CnidarianSinusPlayer : ModPlayer
	{
		public float useTimer;
		public float useAmnt;
		public bool wasHolding;
		
		public override void PostUpdate()
		{
			if (Player.HeldItem == null || Player.HeldItem.type != ModContent.ItemType<CnidarianSinus>())
				return;

			Vector2 direction = Main.MouseWorld - Player.Center;
			direction.Y *= Player.gravDir;

			float rotation = direction.ToRotation() - MathHelper.ToRadians(90);
			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			Player.SetCompositeArmFront(true, stretch, rotation);
		}

		public override void ResetEffects()
		{
			if (Player.statMana <= 7)
				useTimer = 0;
		}
	}

}
