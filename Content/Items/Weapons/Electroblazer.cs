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
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;

namespace CalamityRelics.Content.Items
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
			Item.shoot = ModContent.ProjectileType<WulfrumBlaze>();
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shootSpeed = 8f;
			Item.value = Item.buyPrice(silver: 17);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item34;
			Item.autoReuse = true;
			
		}
		public override void HoldItem(Player player)
		{
			player.Calamity().rightClickListener = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		
		public override void UseAnimation(Player player)
		{
			Item.useTime = 5;
			Item.useAnimation = 20;
			Item.UseSound = SoundID.Item34;
			if (player.altFunctionUse == 2)
			{
				Item.useTime = 15;
				Item.useAnimation = 40;
				Item.UseSound = null;
			}
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

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type,
			int damage, float knockback)
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
				Projectile.NewProjectile(source, position, velocity * 0.9f, ModContent.ProjectileType<Electrode>(), 5, knockback, player.whoAmI);
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
	}
}