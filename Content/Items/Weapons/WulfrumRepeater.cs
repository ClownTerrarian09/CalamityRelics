using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Weapons
{
    public class WulfrumRepeater : ModItem
    {
        private bool shootTracker;
        public override void SetDefaults()
        {
            Item.damage = 6;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 54;
            Item.height = 26;
            Item.useTime = 11;
            Item.useAnimation = 11;
            Item.shootSpeed = 15f;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAmmo = AmmoID.Arrow;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.buyPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
        }
        public override void UseAnimation(Player player)
        {
            Item.useTime = 11;
            Item.useAnimation = 11;
            Item.shootSpeed = 15f;
            
            WulfrumRepeaterPlayer p = player.GetModPlayer<WulfrumRepeaterPlayer>();
            shootTracker = p.UpdateUse();
            if (shootTracker)
            {
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.shootSpeed = 8f;
            }

        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (shootTracker)
            {
                Projectile p = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<WulfrumTrackerArrow>(), 50, knockback, player.whoAmI);
            }
            return !shootTracker;
        }
    }

    public class WulfrumRepeaterPlayer : ModPlayer
    {
        public int numberOfUses;

        public bool UpdateUse()
        {
            numberOfUses++;
            if (numberOfUses >= 3)
            {
                numberOfUses = 0;
                return true;
            }
            else
                return false;

        }
    }
}