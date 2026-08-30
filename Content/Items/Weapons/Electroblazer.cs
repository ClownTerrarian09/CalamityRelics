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
	public class Electroblazer : ModItem
	{
		
		
		private SoundStyle electricSound = new("CalamityMod/Sounds/Item/WulfrumProsthesisShoot"){Volume = 0.3f};
		private SoundStyle outOfElectrodes = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit1");
		
		public override void SetDefaults()
		{
			Item.damage = 1;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 5;
			Item.useAnimation = 30;
			Item.shoot = ProjectileID.Flames;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shootSpeed = 5f;
			Item.value = Item.buyPrice(silver: 17);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item34;
			Item.autoReuse = true;
			Item.holdStyle = 16;
		}
		public override void HoldItem(Player player)
		{
			player.Calamity().rightClickListener = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		
		public override void UseAnimation(Player player)
		{
			Item.useTime = 8;
			Item.useAnimation = 16;
			Item.UseSound = SoundID.Item34;
			if (player.altFunctionUse == 2)
			{
				Item.useTime = 15;
				Item.useAnimation = 40;
				Item.UseSound = null;
			}
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
		public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
		public void SetItemInHand(Player player, Rectangle heldItemFrame)
		{
			if (Main.MouseWorld.X > player.Center.X)
			{
				player.ChangeDir(1);
			}
			else
			{
				player.ChangeDir(-1);
			}

			Vector2 itemPosition = player.MountedCenter + new Vector2(-8f * player.direction, -5f * player.gravDir);
			float itemRotation = (Main.MouseWorld - itemPosition).ToRotation();

			Vector2 itemSize = new Vector2(28, 14);
			Vector2 itemOrigin = new Vector2(-8, 0);
			CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
		}
		public override bool CanShoot(Player player)
		{
			return PlayerHasAmmo(player, false);
		}

		public override bool CanUseItem(Player player)
		{
			return PlayerHasAmmo(player, false);
		}

		private bool PlayerHasAmmo(Player player, bool useAmmo)
		{
			if (player.altFunctionUse == 2)
				return true;
			int ammoType = ItemID.Gel;
			bool hasItem = player.HasItem(ammoType);
			if(hasItem && useAmmo)
				player.ConsumeItem(ammoType);
			return hasItem;
		}

		public override bool? CanChooseAmmo(Item ammo, Player player)
		{
			return (player.altFunctionUse == 2) ? ammo.type == ModContent.ItemType<EnergyCore>() : ammo.type == ItemID.Gel;
			
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ElectroblazerPlayer ePlayer = player.GetModPlayer<ElectroblazerPlayer>();
			
			if (player.altFunctionUse == 2)
			{
				if (player.itemAnimation <= 15)
					return false;
				if (ePlayer.electrodeCount >= 5)
				{
					SoundEngine.PlaySound(outOfElectrodes);
					return false;
				}
				ePlayer.electrodeCount++;
				Projectile.NewProjectile(source, position, velocity * 1.5f, ModContent.ProjectileType<Electrode>(), 5, knockback, player.whoAmI);
				SoundEngine.PlaySound(electricSound);
				return false;
			}
			PlayerHasAmmo(player, true);
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
		
	}

	public class ElectroblazerPlayer : ModPlayer
	{
		public int electrodeCount;
		public override void PostUpdate()
		{
			if (Player.HeldItem == null || Player.HeldItem.type != ModContent.ItemType<Electroblazer>())
				return;
			Vector2 direction = Main.MouseWorld - Player.Center;
			float rotation = direction.ToRotation() - MathHelper.ToRadians(90);
			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			Player.SetCompositeArmFront(true, stretch, rotation);
		}
	}

	
}