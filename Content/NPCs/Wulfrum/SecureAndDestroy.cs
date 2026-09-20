using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using System.Collections.Generic;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using CalamityMod.Items.Materials;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.BiomeManagers;

namespace CalamityRelics.Content.NPCs.Wulfrum
{
    public class SecureAndDestroy : ModNPC
    {
        private static Texture2D texture;
        private SpriteEffects speffect;
        public override void SetDefaults()
        {
            texture = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Wulfrum/SecureAndDestroy_Glow").Value;
            NPC.width = 98;
            NPC.height = 52;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.damage = 25;
            NPC.defense = 3;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = WulfrumAmplifier.Hit;
            NPC.DeathSound = CommonCalamitySounds.WulfrumNPCDeathSound;

            // Uses bat AI
            NPC.aiStyle = 14;

            SpawnModBiomes = new int[1] { ModContent.GetInstance<ArsenalLabBiome>().Type };
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 9;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers();
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("Basic defence forces for an abandoned laboratory. Bears resemblance to the wulfrum enemies of the surface, yet remarkably different in quality and strength.")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<WulfrumMetalScrap>(), 1, 1, 2);
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            if (Main.masterMode){
                NPC.damage = 75;
                NPC.defense = 9;
                NPC.lifeMax = (int)(300*balance);
            }
            else if (Main.expertMode)
            {
                NPC.damage = 50;
                NPC.defense = 6;
                NPC.lifeMax = (int)(200*balance);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            speffect = (NPC.spriteDirection == -1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally; 
            Rectangle drawRectangle = texture.Frame(1, 9, 0, NPC.frame.Y/NPC.height);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            spriteBatch.Draw
            (
                texture,
                NPC.Center - screenPos + new Vector2(0, 3),
                drawRectangle,
                Color.White,
                NPC.rotation,
                drawOrigin,
                1f, 
                speffect, 
                0f
            );
        }

        public override bool PreAI()
        {
            if (NPC.target == 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active){
                NPC.TargetClosest(true);
            }
            Player player = Main.player[NPC.target];

            if (player.Center.X >= NPC.Center.X){
                NPC.spriteDirection = 1;
            }
            else{
                NPC.spriteDirection = -1;
            }

            NPC.rotation = NPC.velocity.X * 0.15f;
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int endFrame = 8;
            int frameSpeed = 4;
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= frameSpeed){
                NPC.frameCounter = 0f;
                NPC.frame.Y += frameHeight;

                if (frameHeight*endFrame < NPC.frame.Y){
                    NPC.frame.Y = startFrame*frameHeight;
                }
            }
        }
    }
}