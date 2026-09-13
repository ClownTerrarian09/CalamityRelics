using System;
using CalamityMod;
using CalamityMod.Items.Placeables.Banners;
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
        
        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            AIType = -1;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 80;
            NPC.defense = 10;
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

        override public void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            bool farFromPlayer = NPC.Distance(player.Center) > 960f;

            Time++;
            if (NPC.target < 0 || NPC.target >= Main.maxPlayers || farFromPlayer || player.dead || !player.active)
            {
                // Probe cant find a player so it idles
                StateTimer = 0;
                return;
            }
            if (Main.player[NPC.target].Distance(NPC.Center) < 2000f)
                NPC.timeLeft = NPC.activeTime;
            
            switch (AIState)
            {
                case ProbeAIState.Stalking:
                {
                    Vector2 directionToPlayer = Vector2.Normalize(player.Center - NPC.Center);
                    NPC.rotation = directionToPlayer.ToRotation();
                    Vector2 hoverPos = player.Center - directionToPlayer * 400f + new Vector2((MathF.Sin(Time * 0.05f) * 10), -100f);
                    Vector2 desired = hoverPos - NPC.Center;
                    float speed = 10f;
                    if (desired.Length() > speed)
                        desired = Vector2.Normalize(desired) * speed;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.08f);
                    StateTimer++;
                    if (StateTimer >= 1000f)
                    {
                        StateTimer = 0;
                        AIState = ProbeAIState.Charging;

                    }
                    
                    break;
                }
                case ProbeAIState.Charging:
                {
                    Vector2 directionToPlayer = Vector2.Normalize(player.Center - NPC.Center);
                    if(StateTimer <= 100)
                        NPC.rotation = NPC.rotation.AngleLerp(directionToPlayer.ToRotation(), 0.5f);
                    Vector2 hoverPos = player.Center - directionToPlayer * 200f + new Vector2((MathF.Sin(Time * 0.05f) * 100), -50f);;
                    Vector2 desired = hoverPos - NPC.Center;
                    float speed = 10f;
                    if (desired.Length() > speed)
                        desired = Vector2.Normalize(desired) * speed;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.08f);
                    if (Main.rand.NextFloat(3.5f - StateTimer) <= 1f)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Vortex, 0f, 0f, 100, default, 1f);
                        dust.color = Color.Green;
                        dust.scale = 0.675f;
                    }
                    StateTimer++;
                    if (StateTimer >= 80f)
                    {
                        StateTimer = 0;
                        AIState = ProbeAIState.Beam;
                    }
                    break;
                }
                case ProbeAIState.Beam:
                {
                    Vector2 directionToPlayer = NPC.DirectionTo(player.Center).SafeNormalize(Vector2.UnitX);
                    NPC.rotation = Utils.AngleLerp(NPC.rotation, directionToPlayer.ToRotation(), 0.08f);
                    NPC.velocity *= 0.9f;

                    if (StateTimer == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                        int p = Projectile.NewProjectile(NPC.GetSource_FromAI(),
                            NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<WulfrumBeam>(),
                            50, 0.2f, Main.maxPlayers);

                        if (p >= 0 && p < Main.maxProjectiles)
                        {
                            Main.projectile[p].ai[0] = NPC.whoAmI;
                            Main.projectile[p].netUpdate = true;
                        }
                    }

                    StateTimer++;
                    if (StateTimer >= 200f)
                    {
                        StateTimer = 0f;
                        AIState = ProbeAIState.Stalking;
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
