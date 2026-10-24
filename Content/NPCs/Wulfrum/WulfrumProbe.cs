using System;
using CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Sounds;
using CalamityRelics.Content.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.NPCs.Wulfrum
{
    internal enum ProbeAIState
    {
        Stalking,
        Charging,
        Beam
    }

    public class WulfrumProbe : ModNPC
    {
        internal ProbeAIState AIState
        {
            get => (ProbeAIState)(int)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        public float StateTimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        public int Time
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private static bool IsServer => Main.netMode != NetmodeID.MultiplayerClient;

        private void SetState(ProbeAIState newState)
        {
            AIState = newState;
            StateTimer = 0f;
            NPC.netUpdate = true;
        }

        public override void SetDefaults()
        {
            AIType = -1;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 210;
            NPC.defense = 40;
            NPC.knockBackResist = 0.35f;
            NPC.width = 102;
            NPC.height = 92;
            NPC.value = Item.buyPrice(copper: 20, silver: 2);
            NPC.noGravity = true;
            NPC.HitSound = WulfrumAmplifier.Hit;
            NPC.DeathSound = CommonCalamitySounds.WulfrumNPCDeathSound;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Time++;
            bool isCharged = NPC.ai[3] > 0;
            if (isCharged)
                NPC.ai[3]--;
            if (!NPC.HasValidTarget)
            {
                StateTimer = 0;
                return;
            }
        
            Player player = Main.player[NPC.target];

            if (player.Distance(NPC.Center) < 2000f)
                NPC.timeLeft = NPC.activeTime;
            // Did this kinda strange and I dont like it but this is how it finds the tower. Would be better to send that info somehow so it didnt have to look for it /:
            NPC tower = null;
            if (isCharged)
            {
                float closest = 1500f;
                foreach (NPC npc in Main.npc)
                {
                    if(!npc.active || npc.type != ModContent.NPCType<WulfrumTower>())
                        continue;
                    float dist = Vector2.Distance(npc.Center, NPC.Center);
                    if (dist < closest)
                    {
                        closest = dist;
                        tower = npc;
                    }
                }
            }

            if (isCharged && tower == null)
                isCharged = false;
            NPC.noTileCollide = isCharged;
            switch (AIState)
            {
                case ProbeAIState.Stalking:
                {
                    Vector2 directionToPlayer = Vector2.Normalize(player.Center - NPC.Center);
                    NPC.rotation = directionToPlayer.ToRotation();
                    Vector2 hoverPos = player.Center - directionToPlayer * 400f + new Vector2(MathF.Sin(Time * 0.05f) * 30f, -100f);
                    if (isCharged)
                        hoverPos = tower.Center + new Vector2(MathF.Sin(Time * 0.05f) * 50f, -200f);

                    Vector2 desired = hoverPos - NPC.Center;
                    float speed = 10f;
                    if (desired.Length() > speed)
                        desired = Vector2.Normalize(desired) * speed;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.08f);

                    StateTimer++;
                    if (StateTimer >= 500f || player.GetModPlayer<GrabbedPlayer>().IsGrabbed || isCharged)
                        SetState(ProbeAIState.Charging);
                    break;
                }
                case ProbeAIState.Charging:
                {
                    Vector2 directionToPlayer = Vector2.Normalize(player.Center - NPC.Center);
                    if (StateTimer <= 100)
                        NPC.rotation = NPC.rotation.AngleLerp(directionToPlayer.ToRotation(), 0.5f);
                    Vector2 hoverPos = player.Center - directionToPlayer * 200f + new Vector2(MathF.Sin(Time * 0.05f) * 100f, -50f);
                    if (isCharged)
                        hoverPos = tower.Center + new Vector2(MathF.Sin(Time * 0.05f) * 50f, -200f);
                    Vector2 desired = hoverPos - NPC.Center;
                    float speed = 10f;
                    if (desired.Length() > speed)
                        desired = Vector2.Normalize(desired) * speed;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.08f);

                    if (!Main.dedServ && Main.rand.NextFloat(3.5f - StateTimer) <= 1f)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Vortex, 0f, 0f, 100, default, 1f);
                        dust.color = Color.Green;
                        dust.scale = 0.675f;
                    }

                    StateTimer++;
                    if (StateTimer >= 80f)
                        SetState(ProbeAIState.Beam);
                    break;
                }
                case ProbeAIState.Beam:
                {
                    Vector2 directionToPlayer = NPC.DirectionTo(player.Center).SafeNormalize(Vector2.UnitX);
                    if (isCharged)
                        directionToPlayer = new Vector2(0, 1);
                    NPC.rotation = Utils.AngleLerp(NPC.rotation, directionToPlayer.ToRotation(), 0.08f);
                    if (isCharged)
                    {
                        Vector2 hoverPos = tower.Center + new Vector2(MathF.Sin(Time * 0.005f) * 1000f, -450f);
                        Vector2 desired = hoverPos - NPC.Center;
                        float speed = 10f;
                        if (desired.Length() > speed)
                            desired = Vector2.Normalize(desired) * speed;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.08f);
                    }
                    else
                        NPC.velocity *= 0.9f;

                    
                    if (NPC.localAI[0] == 0f)
                    {
                        NPC.localAI[0] = 1f;
                        SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                    }

                    if (StateTimer == 0f)
                    {
                        if (IsServer)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(),
                                NPC.Center, Vector2.Zero,
                                ModContent.ProjectileType<WulfrumBeam>(),
                                50, 0.2f, Main.myPlayer, ai0: NPC.whoAmI, isCharged ? 750 : 120);
                        }
                    }

                    StateTimer++;
                    if (StateTimer >= 200f)
                    {
                        if(isCharged && StateTimer < 800f)
                            break;
                        NPC.localAI[0] = 0f;
                        SetState(ProbeAIState.Stalking);
                    }
                    break;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);

            SpriteEffects effects = MathF.Cos(NPC.rotation) < 0f
                ? SpriteEffects.FlipVertically
                : SpriteEffects.None;

            spriteBatch.Draw(tex, drawPos, NPC.frame, NPC.GetAlpha(drawColor),
                NPC.rotation, origin, NPC.scale, effects, 0f);

            return false;
        }
    }
}