using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Accessories {
    internal class WulfrumEnergizer : ModItem {
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.maxMinions++;
            WulfrumEnergizerPlayer energizedPlayer = player.GetModPlayer<WulfrumEnergizerPlayer>();
            energizedPlayer.wulfrumEnergizerItem = Item;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient<WulfrumEnergizer>()
                .AddTile(TileID.Anvils)
                .AddIngredient(ItemID.CrystalShard, 20)
                .AddIngredient<CalamityMod.Items.Materials.DubiousPlating>(14)
                .AddIngredient<CalamityMod.Items.Materials.MysteriousCircuitry>(7)
                .AddIngredient<CalamityMod.Items.Accessories.WulfrumBattery>()
                .Register();
        }
    }

    public class WulfrumEnergizerPlayer : ModPlayer {

        public Item wulfrumEnergizerItem;

        public int whipAttack = 0;
        public int whipMisses = 0;
        public bool isMax;

        public override void ResetEffects() {
            wulfrumEnergizerItem = null;
            //whipAttack = 0;
            //whipMisses = 0;
        }

        public override void PostUpdateBuffs() {
            Player.GetAttackSpeed<SummonMeleeSpeedDamageClass>() += ((float)whipAttack / 100f);
        }
    }

    public class WulfrumEnergizerWhip : GlobalProjectile {

        const int maxWhipAttackCount = 15;
        const int maxWhipMisses = 2;
        const int buffTimeTicks = 3 * 60;


        public override bool InstancePerEntity => true;
        public bool didHit;

        //public override void SetDefaults(Projectile entity) {
        //    didHit = false;
        //}

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
            if (!ProjectileID.Sets.IsAWhip[projectile.type]) return;

            WulfrumEnergizerPlayer energizedPlayer = Main.player[projectile.owner].GetModPlayer<WulfrumEnergizerPlayer>();
            if (energizedPlayer.wulfrumEnergizerItem == null) return;

            didHit = true;

            if (energizedPlayer.isMax) {
                target.AddBuff(ModContent.BuffType<CalamityMod.Buffs.DamageOverTime.StaticDischarge>(), buffTimeTicks);
                for (int i = 0; i < Main.rand.Next(2, 5); i++) {
                    Dust.NewDustPerfect(
                        projectile.Center,
                        DustID.Electric,
                        Main.rand.NextVector2Circular(15, 15)
                    );
                }
            }
        }

        // For multiplayer PVP
        public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) {
            if (!ProjectileID.Sets.IsAWhip[projectile.type]) return;

            WulfrumEnergizerPlayer energizedPlayer = Main.player[projectile.owner].GetModPlayer<WulfrumEnergizerPlayer>();
            if (energizedPlayer.wulfrumEnergizerItem == null) return;

            didHit = true;

            if (energizedPlayer.isMax) {
                target.AddBuff(ModContent.BuffType<CalamityMod.Buffs.DamageOverTime.StaticDischarge>(), buffTimeTicks);
                Dust.NewDustPerfect(
                    projectile.Center,
                    DustID.Electric,
                    Main.rand.NextVector2Circular(35, 35)
                );
            }
        }

        public override void OnKill(Projectile projectile, int timeLeft) {
            if (/*projectile.owner == Main.myPlayer &&*/ ProjectileID.Sets.IsAWhip[projectile.type]) {
                 WulfrumEnergizerPlayer energizedPlayer = Main.player[projectile.owner].GetModPlayer<WulfrumEnergizerPlayer>();

                if (energizedPlayer.wulfrumEnergizerItem == null) return;

                if (didHit) {
                    energizedPlayer.whipAttack++;
                    if (energizedPlayer.whipAttack > maxWhipAttackCount) {
                        energizedPlayer.whipAttack = maxWhipAttackCount;
                        energizedPlayer.isMax = true;
                    }
                } else {
                    if (energizedPlayer.whipMisses >= (maxWhipMisses - 1)) {
                        energizedPlayer.whipMisses = 0;
                        energizedPlayer.whipAttack--;
                        energizedPlayer.isMax = false;
                    } else {
                        energizedPlayer.whipMisses++;
                    }
                }
            }
        }
    }
}
