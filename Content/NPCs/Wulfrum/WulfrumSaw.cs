using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Sounds;
using CalamityMod.World;
using CalamityRelics.Content.Common;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityRelics.Content.NPCs.Wulfrum {
    internal class WulfrumSaw : ModNPC {

        public override void SetDefaults() {
            NPC.width = 80;
            NPC.height = 80;
            NPC.damage = 23;
            NPC.defense = 2;
            NPC.lifeMax = 130;
            NPC.value = Item.buyPrice(copper: 10, silver: 2);

            NPC.HitSound = WulfrumAmplifier.Hit;
            NPC.DeathSound = CommonCalamitySounds.WulfrumNPCDeathSound;

            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;

            NPC.aiStyle = -1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ModContent.ItemType<WulfrumMetalScrap>(), 1, 1, 2);
        }

        public override void AI() {
            NPC.TargetClosest(false);

            if (!NPC.HasValidTarget) {
                return;
            }

            Player player = Main.player[NPC.target];

            if ((NPC.Center.Y - player.Center.Y) >= 140f) {
                CalamityRegularEnemyAI.UnicornAI(NPC, Mod, true, 15f, 7f, 0.1f);
            } else {
                CalamityRegularEnemyAI.UnicornAI(NPC, Mod, true, 3f, 14f, 0.2f);
            }
        }
    }
}
