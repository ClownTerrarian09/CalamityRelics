using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework;


namespace CalamityRelics.Content.Items.Weapons
{
    internal class WulfrumElectromagneticSphere : ModItem
    {
        protected override bool CloneNewInstances => true;
        private int charge;
        private int chargeTimer;
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 45;
            Item.useStyle = ItemUseStyleID.Shoot; 

            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.damage = 100;
            Item.knockBack = 5f;

           
            Item.useTime = 20;
            Item.useAnimation = 20;

            Item.UseSound = SoundID.Item71;

            Item.shoot = ModContent.ProjectileType<ElectromagneticSphereProjectileCharge0>();
            Item.shootSpeed = 6f;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override void HoldItem(Player player)
        {
            player.Calamity().rightClickListener = true;
            
            if (player.Calamity().mouseRight)
            {
                player.itemTime = 2;
                player.itemAnimation = 2;
                if (chargeTimer >= 30 )
                {
                    if (charge < 3)
                    {
                        charge++;
                        chargeTimer = 0;
                    }
                    else
                    {
                        float itemAngle = (Main.MouseWorld - player.MountedCenter).ToRotation();
                        Vector2 along = itemAngle.ToRotationVector2();
                        Vector2 perp  = new Vector2(-along.Y, along.X) * player.direction * player.gravDir;
                        Vector2 itemPosition = player.MountedCenter + along * 20f + perp;
                        Vector2 local = new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-10f, 10f)); 
                        Dust.NewDustPerfect(itemPosition + local.RotatedBy(itemAngle), DustID.Electric, Scale: 0.4f);

                        
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
                chargeTimer = 0;
                ShootCharged(player);
                charge = 0;
                player.velocity -= Vector2.Normalize(Main.MouseWorld - player.Center) * 2f;
                CalamityMod.CalamityUtils.AddScreenshakeAt(player.Center, 1.5f);
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

            float chargeOffset = charge;
            Vector2 itemPosition = player.MountedCenter + new Vector2(-8f * player.direction, -5f * player.gravDir);
            float itemRotation = (Main.MouseWorld - itemPosition).ToRotation();
            itemPosition += Main.rand.NextVector2Circular(chargeOffset, chargeOffset);

            Vector2 itemSize = new Vector2(28, 14);
            Vector2 itemOrigin = new Vector2(-8, 0);
            CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
        }
        private void ShootCharged(Player player)
        {
            Vector2 velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * 8f;
            switch (charge)
            {
                case 1 : Projectile.NewProjectile(player.GetSource_ItemUse(Item),player.Center, velocity, 
                    ModContent.ProjectileType<ElectromagneticSphereProjectileCharge1>(), 75, 2f, player.whoAmI);
                    break;
                case 2 : Projectile.NewProjectile(player.GetSource_ItemUse(Item),player.Center, velocity, 
                    ModContent.ProjectileType<ElectromagneticSphereProjectileCharge2>(), 100, 2f, player.whoAmI);
                    break;
                case 3 : Projectile.NewProjectile(player.GetSource_ItemUse(Item),player.Center, velocity, 
                    ModContent.ProjectileType<ElectromagneticSphereProjectileCharge3>(), 125, 2f, player.whoAmI);
                    break;
                default : Projectile.NewProjectile(player.GetSource_ItemUse(Item),player.Center, velocity, 
                    ModContent.ProjectileType<ElectromagneticSphereProjectileCharge0>(), 50, 2f, player.whoAmI);
                    break;
            }
        }
    }
}