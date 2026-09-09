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
			Item.useAnimation = 16;
			Item.useTime = 16;
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
		}
	}

}
