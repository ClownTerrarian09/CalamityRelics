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
		private SoundStyle electroCharge = new("CalamityRelics/Assets/Sounds/Item/ElectroblazerCharge");
		private int charge;
		private int electrodeCooldown;
		private int chargeTimer;
		protected override bool CloneNewInstances => true;
		

		public override void SetDefaults()
		{
			Item.damage = 6;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 6;
			Item.useAnimation = 12;
			Item.shoot = ModContent.ProjectileType<WulfrumBlaze>();
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shootSpeed = 13f;
			Item.value = Item.buyPrice(silver: 17);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item34;
			Item.autoReuse = true;
			Item.holdStyle = 16;
		}
		public override void HoldItem(Player player)
		{
			player.Calamity().rightClickListener = true;
			ElectroblazerPlayer ePlayer = player.GetModPlayer<ElectroblazerPlayer>();

			if (ePlayer.electrodeCount >= 5)
			{
				charge = 0;
				chargeTimer = 0;
				return;
			}
			int totalCharge = charge + ePlayer.electrodeCount;
			if (player.Calamity().mouseRight)
			{
				if (chargeTimer >= 15 - charge * 3)
				{
					if (totalCharge < 5)
					{
						charge++;
						SoundStyle pitchShifted = electroCharge;
						pitchShifted.Pitch = (charge / 5f) * 0.5f;
						SoundEngine.PlaySound(pitchShifted);
						chargeTimer = 0;
					}
					else
					{
						if (Main.rand.Next(2) == 0)
						{
							float itemAngle = (Main.MouseWorld - player.MountedCenter).ToRotation();
							Vector2 along = itemAngle.ToRotationVector2();
							Vector2 perp  = new Vector2(-along.Y, along.X) * player.direction * player.gravDir;
							Vector2 itemPosition = player.MountedCenter + along * 20f + perp;
							Vector2 local = new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-10f, 10f));
							Dust.NewDustPerfect(itemPosition + local.RotatedBy(itemAngle), DustID.Electric, Scale: 0.4f);

						}
					}
				}
				else
				{
					chargeTimer++;
				}
				return;
			}


			if (charge > 0)
			{
				if (electrodeCooldown > 0)
				{
					electrodeCooldown--;
					return;
				}
				charge--;
				ShootElectrode(player);
				player.velocity -= Vector2.Normalize(Main.MouseWorld - player.Center) * 2f;
				CalamityMod.CalamityUtils.AddScreenshakeAt(player.Center, 1.5f);
				electrodeCooldown = 8;
			}
		}
		public override bool AltFunctionUse(Player player) => true;
		
		public override void UseAnimation(Player player)
		{
			Item.useTime = 6;
			Item.useAnimation = 12;
			Item.UseSound = SoundID.Item34;
			if (player.altFunctionUse == 2)
			{
				Item.useTime = 10;
				Item.useAnimation = 10;
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

			float chargeOffset = ((charge / 5f) * 2f);
			Vector2 itemPosition = player.MountedCenter + new Vector2(-8f * player.direction, -5f * player.gravDir);
			float itemRotation = (Main.MouseWorld - itemPosition).ToRotation();
			itemPosition += Main.rand.NextVector2Circular(chargeOffset, chargeOffset);

			Vector2 itemSize = new Vector2(28, 14);
			Vector2 itemOrigin = new Vector2(-8, 0);
			CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
		}
		public override bool CanShoot(Player player)
		{
			return PlayerHasAmmo(player, false) && charge == 0;
		}

		public override bool CanUseItem(Player player)
		{
			return PlayerHasAmmo(player, false) && charge == 0;
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
			if(player.altFunctionUse == 2)
				return false;
			if (PlayerHasAmmo(player, true))
			{
				Projectile.NewProjectile(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(5)), type, damage, knockback, player.whoAmI);
			}
			return false;
		}
//
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}


		private void ShootElectrode(Player player)
		{
			ElectroblazerPlayer ePlayer = player.GetModPlayer<ElectroblazerPlayer>();
			ePlayer.electrodeCount++;
			Vector2 velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * 8f;
			Projectile.NewProjectile(player.GetSource_ItemUse(Item),player.Center, velocity.RotatedByRandom(MathHelper.ToRadians(5)), ModContent.ProjectileType<Electrode>(), 5, 0.5f, player.whoAmI);
			SoundEngine.PlaySound(electricSound);
		}
		
	}
//
	public class ElectroblazerPlayer : ModPlayer
	{
		public int electrodeCount;
		private int damageEvery10;
		private int resetCounter;
		public override void PostUpdate()
		{
			if (Player.HeldItem == null || Player.HeldItem.type != ModContent.ItemType<Electroblazer>())
				return;
			Vector2 direction = Main.MouseWorld - Player.Center;
			float rotation = direction.ToRotation() - MathHelper.ToRadians(90);
			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			Player.SetCompositeArmFront(true, stretch, rotation);
			
		}
		/*
		public override void PreUpdate()
		{
			if (resetCounter > 600)
			{
				Main.NewText((float)damageEvery10 / 10f);
				resetCounter = 0;
				damageEvery10 = 0;
			}
			else
			{
				resetCounter++;
				
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			damageEvery10 += damageDone;
		}
		*/
	}

	
}
