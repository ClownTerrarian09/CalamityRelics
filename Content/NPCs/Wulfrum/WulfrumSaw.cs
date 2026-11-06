using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Sounds;
using CalamityMod.World;
using CalamityRelics.Content.Common;
using CalamityRelics.Content.NPCs.Bosses.Cnidrion;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.NPCs.Wulfrum {
    internal class WulfrumSaw : ModNPC {

        static Asset<Texture2D> antennaTexture;

        public override void Load() {
            antennaTexture = Mod.Assets.Request<Texture2D>("Content/NPCs/Wulfrum/WulfrumSawAntenna");
        }

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
                // Bounce
                CalamityRegularEnemyAI.UnicornAI(NPC, Mod, true, 15f, 7f, 0.1f);
            } else {
                // Speed
                CalamityRegularEnemyAI.UnicornAI(NPC, Mod, true, 3f, 14f, 0.2f);

                if (!Main.dedServ && Main.rand.NextBool(3)) {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Vortex, 0f, 0f, 100, default, 1f);
                    dust.color = Color.Green;
                    dust.scale = 0.675f;
                }
            }

            if (NPC.velocity.Y <= -9) {
                if (!Main.dedServ && Main.rand.NextBool(3)) {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<ShieldParticle>(), 0f, 0f, 100, default, 1f);
                    //dust.color = Color.Green;
                    dust.scale = 0.65f;
                }
            }
        }

        public override void HitEffect(NPC.HitInfo hit) {
            if (!Main.dedServ && NPC.life <= 0) {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GetInstance<CalamityMod.CalamityMod>().Find<ModGore>("WulfrumGyratorGore").Type, 1f);

                int randomGoreCount = Main.rand.Next(1, 4);
                for (int i = 0; i < randomGoreCount; i++) {
                    // Temporary gore
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GetInstance<CalamityMod.CalamityMod>().Find<ModGore>("WulfrumEnemyGore" + Main.rand.Next(1, 11).ToString()).Type, 1.7f);
                }
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            Vector2 pos = NPC.Center;
            pos.Y -= (TextureAssets.Npc[Type].Value.Height * 0.7f);
            Main.EntitySpriteDraw(
                antennaTexture.Value,
                (pos - (Main.screenPosition)),
                null,
                drawColor,
                0,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0
            );
        }
    }
}
