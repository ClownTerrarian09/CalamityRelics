using System;
using System.IO;
using CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.NPCs.Wulfrum
{
    public class WulfrumCrab : ModNPC
    {
        // Frame stuffs
        private const string idle = "CalamityRelics/Content/NPCs/Wulfrum/WulfrumCrab_Idle";
        private const string idleGlow = "CalamityRelics/Content/NPCs/Wulfrum/WulfrumCrab_Idle_Glow";
        private const string idleArms = "CalamityRelics/Content/NPCs/Wulfrum/WulfrumCrabArms_Idle";
        private const string idleArmsGlow = "CalamityRelics/Content/NPCs/Wulfrum/WulfrumCrabArms_Idle_Glow";
        private const string attack = "CalamityRelics/Content/NPCs/Wulfrum/WulfrumCrab_Attack";
        private const string attackGlow = "CalamityRelics/Content/NPCs/Wulfrum/WulfrumCrab_Attack_Glow";
        private const int idleArmsWidth = 84;
        private const int idleArmsHeight = 70;
        private const int armsOffsetX = -14;
        private const int armsOffsetY = 28;

        private static Asset<Texture2D> idleTex, idleGlowTex, attackTex, attackGlowTex, armsTex, armsGlowTex;

        private static Asset<Texture2D> TryRequest(string path) =>
            ModContent.HasAsset(path) ? ModContent.Request<Texture2D>(path) : null;

        private const int idleFrames = 12;
        private const int attackFrames = 7;
        private const int grabAnimTicks = attackFrames * 2;

        private enum State
        {
            Idle,
            Attacking,
            Grabbing,
            Holding
        }

        private State AIState
        {
            get => (State)(int)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        private float StateTimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        private float GrabCooldown
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        private bool WanderRight
        {
            get => NPC.ai[3] == 1f;
            set => NPC.ai[3] = value ? 1f : 0f;
        }

        private int sinTimer;
        private int heldPlayer = -1;

        private int frameTimer;
        private int frameCounter;
        private bool lastAnimWasAttack;
        private int bumpSoundCooldown;

        private static bool IsServer => Main.netMode != NetmodeID.MultiplayerClient;

        private void SetState(State newState)
        {
            AIState = newState;
            StateTimer = 0f;
            NPC.netUpdate = true;
        }

        private void Release()
        {
            heldPlayer = -1;
            SetState(State.Idle);
        }

        private bool HeldPlayerValid()
        {
            if (heldPlayer < 0 || heldPlayer >= Main.maxPlayers)
                return false;
            Player p = Main.player[heldPlayer];
            return p.active && !p.dead;
        }

        private void PlayBumpSound()
        {
            if (bumpSoundCooldown > 0)
                return;
            bumpSoundCooldown = 20;
            SoundEngine.PlaySound(WulfrumAmplifier.Hit, NPC.position);
        }

        private bool CheckPlayerInGrabRange(Player target)
        {
            Vector2 grabSize = new Vector2(NPC.width * 2f, NPC.height * 2f);
            Vector2 topLeft = NPC.Center - grabSize / 2f;
            return Collision.CheckAABBvAABBCollision(topLeft, grabSize, target.position, new Vector2(target.width, target.height));
        }

        private bool IsGroundWithin500()
        {
            Vector2 below = NPC.BottomLeft;
            return Collision.SolidCollision(below, NPC.width, 500, acceptTopSurfaces: true);
        }

        private bool IsCeilingWithin100()
        {
            Vector2 above = NPC.TopLeft - new Vector2(0f, 100f);
            return Collision.SolidCollision(above, NPC.width, 100);
        }

        public override string Texture => idle;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = idleFrames;

            if (!Main.dedServ)
            {
                idleTex = ModContent.Request<Texture2D>(idle);
                idleGlowTex = TryRequest(idleGlow);
                attackTex = TryRequest(attack);
                attackGlowTex = TryRequest(attackGlow);
                armsTex = TryRequest(idleArms);
                armsGlowTex = TryRequest(idleArmsGlow);
            }
        }

        public override void Unload()
        {
            idleTex = idleGlowTex = attackTex = attackGlowTex = armsTex = armsGlowTex = null;
        }

        public override void SetDefaults()
        {
            AIType = -1;
            NPC.aiStyle = -1;
            NPC.damage = 15;
            NPC.lifeMax = 100;
            NPC.defense = 100;
            NPC.knockBackResist = 0.35f;
            NPC.width = 66;
            NPC.height = 82;
            NPC.value = Item.buyPrice(copper: 20, silver: 2);
            NPC.noGravity = true;
            NPC.HitSound = WulfrumAmplifier.Hit;
            NPC.DeathSound = CommonCalamitySounds.WulfrumNPCDeathSound;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            GrabCooldown = 200f;
            heldPlayer = -1;
            NPC.netUpdate = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(sinTimer);
            writer.Write((short)heldPlayer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            sinTimer = reader.ReadInt32();
            heldPlayer = reader.ReadInt16();
        }

        public override void AI()
        {
            sinTimer++;
            if (bumpSoundCooldown > 0)
                bumpSoundCooldown--;

            if (AIState != State.Holding)
                NPC.TargetClosest();

            switch (AIState)
            {
                case State.Idle:
                {
                    float xVel = WanderRight ? -1f : 1f;
                    float yVel = MathF.Sin(sinTimer * 0.05f) * 4f;
                    NPC.velocity = new Vector2(xVel, yVel);

                    if (Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height).X != NPC.velocity.X)
                    {
                        PlayBumpSound();
                        if (IsServer)
                        {
                            WanderRight = !WanderRight;
                            NPC.netUpdate = true;
                        }
                    }

                    if (IsGroundWithin500())
                    {
                        NPC.velocity.Y -= 1f;
                        NPC.velocity.Y = MathF.Max(NPC.velocity.Y, -8f);
                    }

                    NPC.velocity *= 0.9f;

                    if (NPC.HasValidTarget)
                    {
                        if (GrabCooldown > 0f)
                            GrabCooldown--;
                        else if (IsServer)
                        {
                            SetState(State.Attacking);
                            NPC.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(3f, 5f));
                        }
                    }
                    break;
                }
                case State.Attacking:
                {
                    if (!NPC.HasValidTarget)
                    {
                        if (IsServer)
                            SetState(State.Idle);
                        break;
                    }

                    Player target = Main.player[NPC.target];

                    Vector2 aimPoint = target.Center - new Vector2(0f, NPC.height * 0.5f);
                    Vector2 desired = NPC.SafeDirectionTo(aimPoint) * 6f;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.2f);

                    if (Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height) != NPC.velocity)
                    {
                        PlayBumpSound();
                        if (IsServer)
                        {
                            GrabCooldown = 200f;
                            NPC.velocity = Vector2.Zero;
                            SetState(State.Idle);
                        }
                        break;
                    }

                    if (IsServer && CheckPlayerInGrabRange(target))
                    {
                        if (!target.GetModPlayer<GrabbedPlayer>().IsGrabbed)
                        {
                            heldPlayer = NPC.target;
                            SetState(State.Grabbing);
                        }
                        else
                        {
                            GrabCooldown = 500f;
                            SetState(State.Idle);
                        }
                    }
                    break;
                }
                case State.Grabbing:
                {
                    StateTimer++;
                    if (StateTimer >= grabAnimTicks && IsServer)
                    {
                        GrabCooldown = 500f;
                        if (HeldPlayerValid() && CheckPlayerInGrabRange(Main.player[heldPlayer]))
                            SetState(State.Holding);
                        else
                            Release();
                    }
                    break;
                }
                case State.Holding:
                {
                    if (!HeldPlayerValid())
                    {
                        if (IsServer)
                            Release();
                        break;
                    }

                    NPC.target = heldPlayer;

                    NPC.direction = NPC.spriteDirection =
                        Main.player[heldPlayer].Center.X < NPC.Center.X ? -1 : 1;
                    Main.player[heldPlayer].GetModPlayer<GrabbedPlayer>().Grab(NPC.whoAmI);

                    bool foundProbe = false;
                    float probeX = 0f;
                    float probeY = 0f;
                    int probeType = ModContent.NPCType<WulfrumProbe>();
                    foreach (NPC other in Main.ActiveNPCs)
                    {
                        if (other.type != probeType || other.whoAmI == NPC.whoAmI)
                            continue;

                        if (Vector2.Distance(other.Center, NPC.Center) < 1000f)
                        {
                            foundProbe = true;
                            probeX = other.velocity.X;
                            probeY = other.velocity.Y;
                            break;
                        }
                    }

                    NPC.velocity.X *= 0.6f;

                    if (foundProbe)
                    {
                        if (IsGroundWithin500())
                        {
                            NPC.velocity.Y = MathF.Max(NPC.velocity.Y - 0.3f, -6f);
                            NPC.velocity.X = probeX * 0.8f + MathF.Sin(sinTimer * 0.05f) * 1.5f;
                        }
                        else
                        {
                            NPC.velocity.X = probeX * 0.3f + MathF.Sin(sinTimer * 0.05f) * 1.5f;
                            NPC.velocity.Y = probeY * 0.3f + MathF.Cos(sinTimer * 0.03f) * 3f;
                        }
                        break;
                    }

                    NPC.velocity.Y = MathF.Max(NPC.velocity.Y - 0.3f, -15f);
                    NPC.velocity.X = MathF.Sin(sinTimer * 0.05f) * 1.5f;

                    if (IsServer && (NPC.Center.Y <= 200 || IsCeilingWithin100()))
                        Release();
                    break;
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            bool attackAnim = AIState == State.Grabbing || AIState == State.Holding;

            if (attackAnim != lastAnimWasAttack)
            {
                lastAnimWasAttack = attackAnim;
                frameTimer = 0;
                frameCounter = 0;
            }

            frameTimer++;
            if (attackAnim)
            {
                if (frameTimer >= 2)
                {
                    frameTimer = 0;
                    if (frameCounter < attackFrames - 1)
                        frameCounter++;
                }
            }
            else
            {
                if (frameTimer >= 6)
                {
                    frameTimer = 0;
                    frameCounter = (frameCounter + 1) % idleFrames;
                }
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool attackAnim = (AIState == State.Grabbing || AIState == State.Holding) && attackTex != null;

            Texture2D tex = attackAnim ? attackTex.Value : idleTex.Value;
            Texture2D glow = attackAnim ? attackGlowTex?.Value : idleGlowTex?.Value;
            int frames = attackAnim ? attackFrames : idleFrames;

            int frame = Utils.Clamp(frameCounter, 0, frames - 1);
            int frameHeight = tex.Height / frames;
            Rectangle src = new Rectangle(0, frame * frameHeight, tex.Width, frameHeight);

            Vector2 origin = src.Size() * 0.5f;
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
            bool flipped = NPC.direction == 1;
            SpriteEffects fx = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color lit = NPC.GetAlpha(drawColor);
            Color glowColor = NPC.GetAlpha(Color.White);

            if (!attackAnim && armsTex != null)
            {
                Texture2D arms = armsTex.Value;
                int armsFrameCount = Math.Max(1, arms.Height / idleArmsHeight);
                int armsFrame = frameCounter % armsFrameCount;
                Rectangle armsSrc = new Rectangle(0, armsFrame * idleArmsHeight, idleArmsWidth, idleArmsHeight);

                Vector2 armsOrigin = new Vector2(
                    src.Width * 0.5f - armsOffsetX,
                    idleArmsHeight - src.Height * 0.5f - armsOffsetY);

                if (flipped)
                    armsOrigin.X = idleArmsWidth - armsOrigin.X;

                spriteBatch.Draw(arms, drawPos, armsSrc, lit, NPC.rotation, armsOrigin, NPC.scale, fx, 0f);

                if (armsGlowTex != null)
                    spriteBatch.Draw(armsGlowTex.Value, drawPos, armsSrc, glowColor, NPC.rotation, armsOrigin, NPC.scale, fx, 0f);
            }

            spriteBatch.Draw(tex, drawPos, src, lit, NPC.rotation, origin, NPC.scale, fx, 0f);

            if (glow != null)
                spriteBatch.Draw(glow, drawPos, src, glowColor, NPC.rotation, origin, NPC.scale, fx, 0f);

            return false;
        }
    }

    public class GrabbedPlayer : ModPlayer
    {
        public int grabbedBy = -1;
        private int grabTimer;

        public bool IsGrabbed => grabbedBy >= 0;

        public void Grab(int npcIndex)
        {
            grabbedBy = npcIndex;
            grabTimer = 2;
        }

        private bool GrabberStillValid()
        {
            if (grabbedBy < 0 || grabbedBy >= Main.maxNPCs)
                return false;
            NPC npc = Main.npc[grabbedBy];
            return npc.active && npc.type == ModContent.NPCType<WulfrumCrab>();
        }

        public override void ResetEffects()
        {
            if (grabbedBy < 0)
                return;

            if (!GrabberStillValid())
            {
                grabbedBy = -1;
                return;
            }

            if (grabTimer > 0)
                grabTimer--;
            else
                grabbedBy = -1;
        }

        public override void SetControls()
        {
            if (IsGrabbed)
                DisableControls();
        }

        public override void PreUpdate()
        {
            if (IsGrabbed)
                DisableControls();
        }

        private void DisableControls()
        {
            Player.controlLeft = false;
            Player.releaseLeft = false;
            Player.controlRight = false;
            Player.releaseRight = false;
            Player.controlUp = false;
            Player.releaseUp = false;
            Player.controlDown = false;
            Player.releaseDown = false;
            Player.controlJump = false;
            Player.releaseJump = false;
            Player.controlHook = false;
            Player.releaseHook = false;
            Player.controlMount = false;
            Player.releaseMount = false;
            Player.controlQuickHeal = false;
            Player.releaseQuickHeal = false;
            Player.controlQuickMana = false;
            Player.releaseQuickMana = false;
            Player.controlSmart = false;
            Player.controlTorch = false;
        }

        public override void PreUpdateMovement()
        {
            if (!IsGrabbed)
                return;

            if (!GrabberStillValid())
            {
                grabbedBy = -1;
                return;
            }
            NPC npc = Main.npc[grabbedBy];
            Player.velocity = Vector2.Zero;
            Player.Center = npc.Center + new Vector2(-14, 28);
            Player.fallStart = (int)(Player.position.Y / 16f);
            Player.RemoveAllGrapplingHooks();
        }


        public override void PostUpdate()
        {
            const string key = "CalamityRelics:ElectricBorder";
            if (IsGrabbed)
            {
                if (!Filters.Scene[key].IsActive())
                    Filters.Scene.Activate(key);
            }
            else if (Filters.Scene[key].IsActive())
            {
                Filters.Scene.Deactivate(key);
            }
        }
    }
}