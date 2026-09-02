using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;

namespace CalamityRelics.Content.Items.Utilities.SecurityController
{
    public class SecurityController : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 120;
            Item.useAnimation = 120;
            Item.autoReuse = false;
            Item.consumable = false;
            Item.UseSound = SoundID.Item92;
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (IsHoveringOverBarrier(out _))
                {
                    player.cursorItemIconEnabled = true;
                    player.cursorItemIconID = Type;
                }
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (IsHoveringOverBarrier(out int npcIndex))
            {
                Item.useTime = 300;
                Item.useAnimation = 300;
                Item.UseSound = SoundID.Item93;

                if (player.whoAmI == Main.myPlayer)
                {
                    NPC barrier = Main.npc[npcIndex];
                    if (barrier.ModNPC is DraedonBarrierNPC barrierNPC)
                    {
                        barrierNPC.StartUnlockSequence();
                    }
                }
            }
            else
            {
                Item.useTime = 120;
                Item.useAnimation = 120;
                Item.UseSound = SoundID.Item92;
            }

            return true;
        }

        private bool IsHoveringOverBarrier(out int npcIndex)
        {
            npcIndex = -1;
            Vector2 mouseWorld = Main.MouseWorld;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.active && npc.type == ModContent.NPCType<DraedonBarrierNPC>() && npc.Hitbox.Contains(mouseWorld.ToPoint()))
                {
                    if (Vector2.Distance(Main.LocalPlayer.Center, npc.Center) < 200f)
                    {
                        npcIndex = i;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}