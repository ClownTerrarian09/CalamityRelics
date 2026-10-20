using System;
using CalamityRelics.Content.Buffs.SummonWeapons;
using CalamityMod.Items.Materials;
using CalamityRelics.Content.Projectiles.Summon;
using CalamityMod.Systems.Collections;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace  CalamityRelics.Content.Items.Weapons
{
    public class CnidlingEgg : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 26;
            Item.damage = 20;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item67;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<CnidlingBuff>();
            Item.shoot = ModContent.ProjectileType<Cnidling>();
            Item.DamageType = DamageClass.Summon;
            
            Item.rare = ItemRarityID.Pink;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld; 
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            for (int i = 0; i < 4; i++)
            {
                var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: i);
                float d = Item.damage;
                switch (i)
                {
                    // Chargin
                    case 0:
                    {
                        d *= 2f;
                        break;
                    }
                    // Shootin
                    case 1:
                    {
                        d *= 0.3f;
                        break;
                    }
                    // Spinnin
                    case 2:
                    {
                        d *= 0.2f;
                        break;
                    }
                    // Protectin
                    case 3:
                    {
                        d *= 1.25f;
                        break;
                    }
                }
                proj.originalDamage = (int)MathF.Floor(d);
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(); 
            recipe.AddIngredient(ItemID.SpikyBall, 50);
            recipe.AddIngredient(ModContent.ItemType<WulfrumMetalScrap>(), 1);
            recipe.AddTile(TileID.Anvils); 
            recipe.Register();
        }
    }
}
