using Microsoft.Xna.Framework;
using Terraria;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Core;
using CalamityMod.UI;
using CalamityRelics.Content.UI.DraedonStuff;
using CalamityMod.UI.DraedonLogs;
using System.Collections.Generic;
using CalamityRelics.Core;
using CalamityRelics.Content.Items.Weapons;
using CalamityRelics.Content.Items.Weapons.Summon;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.Rarities;
using CalamityMod;

namespace CalamityRelics.Content.Items.DraedonItems
{
    public class DecayedSchematic : ModItem, ILocalizedModType
    {
        // Credits to Calamity Mod
        // This code was adapted from the various EncryptedSchematic classes. 
        // All credits goes to Azafure LLC
        // https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/LICENSE.md

        public override void SetDefaults(){
            Item.width = 42; Item.height = 34;
            Item.rare = ModContent.RarityType<DarkOrange>();
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override void UpdateInventory(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && !RelicsConditionsSystem.unlockedWulfrumRecipe){
                RelicsConditionsSystem.unlockedWulfrumRecipe = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            TooltipLine line = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
             if (RelicsConditionsSystem.unlockedWulfrumRecipe)
            {
                int insertIndex = list.FindIndex(x => x.Name == "Tooltip4" && x.Mod == "Terraria");
                if (insertIndex != -1)
                {
                    int meleeItem = ModContent.ItemType<WulfrumWavewire>();
                    TooltipLine meleeDisplay = new TooltipLine(this.Mod, "CalamityMod:MeleeDisplay", $"[i:{meleeItem}] {CalamityUtils.GetItemName(meleeItem)}");
                    meleeDisplay.OverrideColor = new Color(31, 242, 245);
                    list.Insert(insertIndex + 1, meleeDisplay);

                    int rangedItem = ModContent.ItemType<Electroblazer>();
                    TooltipLine rangedDisplay = new TooltipLine(this.Mod, "CalamityMod:RangedDisplay", $"[i:{rangedItem}] {CalamityUtils.GetItemName(rangedItem)}");
                    rangedDisplay.OverrideColor = new Color(149, 243, 43);
                    list.Insert(insertIndex + 2, rangedDisplay);

                    int mageItem = ModContent.ItemType<WulfrumElectromagneticSphere2>();
                    TooltipLine mageDisplay = new TooltipLine(this.Mod, "CalamityMod:MageDisplay", $"[i:{mageItem}] {CalamityUtils.GetItemName(mageItem)}");
                    mageDisplay.OverrideColor = new Color(201, 41, 255);
                    list.Insert(insertIndex + 3, mageDisplay);

                    int summonItem = ModContent.ItemType<WulfrumWhiplash>();
                    TooltipLine summonDisplay = new TooltipLine(this.Mod, "CalamityMod:SummonDisplay", $"[i:{summonItem}] {CalamityUtils.GetItemName(summonItem)}");
                    summonDisplay.OverrideColor = new Color(236, 255, 31);
                    list.Insert(insertIndex + 4, summonDisplay);

                    int rogueItem = ModContent.ItemType<WulfrumSpikyBalls>();
                    TooltipLine rogueDisplay = new TooltipLine(this.Mod, "CalamityMod:RogueDisplay", $"[i:{rogueItem}] {CalamityUtils.GetItemName(rogueItem)}");
                    rogueDisplay.OverrideColor = new Color(255, 64, 31);
                    list.Insert(insertIndex + 5, rogueDisplay);

                    int codeItem = ModContent.ItemType<CodebreakerBase>();
                    TooltipLine machineDisplay = new TooltipLine(this.Mod, "CalamityMod:CodeDisplay", $"[i:{codeItem}] {CalamityUtils.GetItemName(codeItem)}");
                    machineDisplay.OverrideColor = new Color(42, 242, 245);
                    list.Insert(insertIndex + 6, machineDisplay);

                }
            }
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                PopupGUIManager.FlipActivityOfGUIWithType(typeof(DecayedSchematicGUI));
            }
            return true;
        }
    }
}