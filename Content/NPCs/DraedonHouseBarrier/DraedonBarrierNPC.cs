using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;
using CalamityRelics.Content.Projectiles.Summon.WulfrumPortal;

namespace CalamityRelics.Content.NPCs.DraedonHouseBarrier
{
    public class DraedonBarrierNPC : ModNPC
    {
        private const float State_Idle = 0f;
        private const float State_WaveEvent = 1f;
        private const float State_Unlocking = 2f;

        public override void SetDefaults()
        {
            NPC.width = 48;
            NPC.height = 120;
            NPC.lifeMax = 250;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.hide = true;
        }

        public override void AI()
        {
            DrawBarrierDust();

            float currentState = NPC.ai[0];

            if (currentState != State_Unlocking)
            {
                HandleCollision();
            }

            if (currentState == State_WaveEvent)
            {
                HandleWaveEvent();
            }
            else if (currentState == State_Unlocking)
            {
                HandleUnlockSequence();
            }
        }

        private void DrawBarrierDust()
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = new Vector2(NPC.position.X + Main.rand.NextFloat(NPC.width), NPC.position.Y + Main.rand.NextFloat(NPC.height));
                Dust core = Dust.NewDustPerfect(pos, DustID.Electric, Vector2.Zero, 0, default, 1.2f);
                core.noGravity = true;
            }

            int arcFrequency = NPC.ai[0] == State_Unlocking ? 1 : 3;

            if (Main.GameUpdateCount % arcFrequency == 0)
            {
                float startX = NPC.position.X + Main.rand.NextFloat(4f, NPC.width - 4f);
                float endX = NPC.position.X + Main.rand.NextFloat(4f, NPC.width - 4f);

                Vector2 top = new Vector2(startX, NPC.position.Y);
                Vector2 bottom = new Vector2(endX, NPC.position.Y + NPC.height);

                int segments = 8;
                Vector2 lastPos = top;

                for (int j = 1; j <= segments; j++)
                {
                    float lerp = (float)j / segments;
                    Vector2 nextPos = Vector2.Lerp(top, bottom, lerp);

                    nextPos.X += Main.rand.NextFloat(-16f, 16f);

                    float minX = NPC.position.X + 2f;
                    float maxX = NPC.position.X + NPC.width - 2f;
                    nextPos.X = MathHelper.Clamp(nextPos.X, minX, maxX);

                    float distance = Vector2.Distance(lastPos, nextPos);
                    for (float k = 0; k < distance; k += 4f)
                    {
                        Vector2 dustPos = Vector2.Lerp(lastPos, nextPos, k / distance);

                        Dust spark = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 100, Color.White, 0.6f);
                        spark.noGravity = true;
                    }
                    lastPos = nextPos;
                }
            }
        }

        private void HandleCollision()
        {
            if (Main.netMode == NetmodeID.Server) return;

            Player player = Main.LocalPlayer;

            if (player.active && !player.dead && player.Hitbox.Intersects(NPC.Hitbox))
            {
                player.position = player.oldPosition;

                Vector2 knockbackDir = player.Center - NPC.Center;
                knockbackDir.Normalize();
                player.velocity = knockbackDir * 15f;

                if (!player.immune)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                    player.AddBuff(BuffID.Electrified, 180);

                    NetworkText deathMessage = NetworkText.FromLiteral(player.name + " got incinerated to dust by the high-voltage barrier.");
                    player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(deathMessage), 50, 0);

                    if (NPC.ai[0] == State_Idle)
                    {
                        NPC.target = player.whoAmI;
                        NPC.ai[0] = State_WaveEvent;
                        NPC.ai[1] = 2;
                        NPC.netUpdate = true;
                    }
                }
            }
        }

        private void HandleWaveEvent()
        {
            Player target = Main.player[NPC.target];

            if (!target.active || target.dead || !DraedonHouseSystem.DraedonHouseRect.Contains(target.Center.ToTileCoordinates()))
            {
                NPC.ai[0] = State_Idle;
                NPC.ai[1] = 0;
                NPC.netUpdate = true;
                return;
            }

            int activeBots = 0;

            int botType = NPCID.Probe;
            if (ModContent.TryFind("CalamityMod", "WulfrumDrone", out ModNPC wulfrumDrone))
            {
                botType = wulfrumDrone.Type;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == botType)
                {
                    activeBots++;
                }
            }

            int portalType = ModContent.ProjectileType<WulfrumPortal>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == portalType)
                {
                    activeBots++;
                }
            }

            if (activeBots == 0)
            {
                int botsToSpawn = (int)NPC.ai[1];

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < botsToSpawn; i++)
                    {
                        Vector2 spawnOffset = Main.rand.NextVector2Circular(250f, 250f);
                        spawnOffset.Y -= 80f;
                        Vector2 spawnPos = NPC.Center + spawnOffset;

                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            spawnPos,
                            Vector2.Zero,
                            portalType,
                            0,
                            0,
                            Main.myPlayer,
                            botType
                        );
                    }
                }

                if (NPC.ai[1] < 8)
                {
                    NPC.ai[1] += 2;
                }
                NPC.netUpdate = true;
            }
        }

        public void StartUnlockSequence()
        {
            if (NPC.ai[0] != State_Unlocking)
            {
                NPC.ai[0] = State_Unlocking;
                NPC.ai[3] = 300;
                NPC.netUpdate = true;
            }
        }

        private void HandleUnlockSequence()
        {
            NPC.ai[3]--;

            if (NPC.ai[3] <= 0)
            {
                DraedonHouseSystem.IsHouseUnlocked = true;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);

                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric);
                }

                NPC.active = false;
            }
        }
    }
}