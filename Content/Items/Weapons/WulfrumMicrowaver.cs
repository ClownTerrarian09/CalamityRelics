using System;
using CalamityMod;
using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Weapons
{
    public class WulfrumMicrowaver : ModItem
    {
        protected override bool CloneNewInstances => true;
        private float rotation;
        public override void SetDefaults()
        {
            Item.mana = 40;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 200;
            Item.useTime = 200;
            Item.channel = true;
            Item.knockBack = 0.2f;
            Item.width = 34;
            Item.height = 32;
            Item.damage = 20;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<WulfrumMicrowave>();
            Item.shootSpeed = 0;
            Item.UseSound = null;
            Item.noMelee = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0,1);
            Item.DamageType = DamageClass.Magic;
            Item.holdStyle = 16;

        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
            rotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter + aim * 20f, aim, ModContent.ProjectileType<WulfrumMicrowave>(), Item.damage, Item.knockBack, player.whoAmI);

            return false;
        }

        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[ModContent.ProjectileType<WulfrumMicrowave>()] <= 0;
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
            rotation = Utils.AngleLerp(rotation, (Main.MouseWorld - player.Center).ToRotation(), 0.08f);

            Vector2 itemPosition = player.Center + new Vector2(40f,  10 * player.direction).RotatedBy(rotation);
            float itemRotation = rotation;
			
            Vector2 itemSize = new Vector2(Item.width, Item.height);
            Vector2 itemOrigin = new Vector2(Item.width * 0.5f, Item.height * 0.5f);
            CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
        }
        
    }
    
    public class WulfrumMicrowavePlayer : ModPlayer
    {
        protected override bool CloneNewInstances => true;
        private float rotation;

        public override void PostUpdate()
        {
            if (Player.HeldItem == null || Player.HeldItem.type != ModContent.ItemType<WulfrumMicrowaver>())
                return;
            

            Vector2 direction = Main.MouseWorld - Player.Center;
            direction.Y *= Player.gravDir;
            rotation = Utils.AngleLerp(rotation, (Main.MouseWorld - Player.Center).ToRotation() - MathHelper.ToRadians(90), 0.08f);
            Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
            Player.SetCompositeArmFront(true, stretch, rotation);
        }

    }
}

