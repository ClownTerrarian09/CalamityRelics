using System.Collections.Generic;
using CalamityRelics.Content.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Mounts
{
    public class EnchantedBloom : ModItem
    {
        public override void SetDefaults() {
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ModContent.RarityType<CalamityMod.Rarities.HotPink>();
            Item.UseSound = SoundID.Item79;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<Mount_EnchantedBloom>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var blockline = new TooltipLine(Mod, "BlockLine", $"Reflects incoming projectiles")
            {
                OverrideColor = Color.Pink
            };
            var attackline = new TooltipLine(Mod, "AttackLine", $"Ballochorously launches homing seeds towards nearby enemies")
            {
                OverrideColor = Color.DarkSalmon
            };
            var bossline = new TooltipLine(Mod, "BossLine", $"Takes over the players mind and ability to dismount if used during a boss fight")
            {
                OverrideColor = Color.Salmon
            };
            var devline = new TooltipLine(Mod, "DevLine", $"- Dev item -")
            {
                OverrideColor = ModContent.GetInstance<CalamityMod.Rarities.HotPink>().RarityColor
            };
            int index = tooltips.FindLastIndex(t => t.Mod == "Terraria" && t.Name.StartsWith("Tooltip"));
            
            if (index != -1)
                tooltips.Insert(index + 1, blockline);
            else
                tooltips.Add(blockline);
            
            if (index != -1)
                tooltips.Insert(index + 2, attackline);
            else
                tooltips.Add(attackline);
            
            if (index != -1)
                tooltips.Insert(index + 3, bossline);
            else
                tooltips.Add(bossline);
            
            if (index != -1)
                tooltips.Insert(index + 4, devline);
            else
                tooltips.Add(devline);
        }
    }
}

