using Terraria;
using Terraria.ModLoader;
using CalamityRelics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader.Utilities;
using CalamityRelics.Content.Projectiles;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityRelics.Content.NPCs.WulfrumSentinel.RollingBombs;
using CalamityRelics.Content.Projectiles.Hostile;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using CalamityRelics.Assets.Effects;
using CalamityMod;

namespace CalamityRelics.Content.NPCs.WulfrumSentinel{

    [AutoloadBossHead]
    public class WulfrumSentinel : ModNPC
    {

        public static float deaccelerationFactor => 0.25f; 
        public static float accelerationFactor => 0.1f; 
        public static float YVelMissile => 11f; 
        public static float YVelOrb=> 6f; 
        public static float maxXVelMissile => 6f; 
        public static float maxSpeed => 3.5f; 
        public static float barrageHoverFlyMaxSpeed => 9f; 
        public static float barrageXVel => 3; 
        public static float jumpForce => 7.5f; 

        public static float phaseBHealthThreshold => 0.5f; 
        public static float dashVel => 7f; 
        public static float dashPreVel => -4f; 
        public static float dashWindupTime => 60; 
        public static float stompHoverTime => 240; 
        public static float dashTime => 60; 
        public static float laserVelocity => 10f;


        public static int minFireRate => 120;
        public static int maxFireRate => 50;
        public static int rollingRateMin => 150;
        public static int rollingRateMax => 480;
        public static int stompSpeed => 10; 
        public static float maxStompXFactor => 1.65f; 
        
        public static SoundStyle laserShootSound => SoundID.DD2_BetsyFireballShot;
        public static SoundStyle barrelShootSound => SoundID.DD2_DrakinShot;
        public static SoundStyle hitSound => SoundID.NPCHit1;
        public static SoundStyle hurtSound => SoundID.NPCDeath1;


        // Like WOF, fire rates should scale similarily
        public int currentFireRate => (int)float.Lerp(maxFireRate, minFireRate, NPC.GetLifePercent());
        public int currentRollFireRate => (int)float.Lerp(rollingRateMin, rollingRateMax, NPC.GetLifePercent());


        // I am probably leaving out a built in variable or method that does this already that would be able to replace this later
        public bool flipped => targetPlayer.Center.X - NPC.Center.X < 0;
        public int flipFactor => flipped ? -1 : 1;
        public bool grounded => NPC.collideY && NPC.velocity.Y == 0;
        public int initialBarrageFlipFactor;


        // public WulfrumSentinelDrone drone;

        public Player targetPlayer => Main.player[NPC.target]; 
        public Vector2 dirToTarget => Vector2.Normalize(NPC.DirectionTo(targetPlayer.Center)); 
        public Vector2 rocketFirePos => NPC.Center + new Vector2(0, -100); 
        public Vector2 laserBarrelPos => NPC.Center + new Vector2(0, -30); 
        public Vector2 stompHoverOffset => new Vector2(0, -500);
        public Vector2 postStompCorrectionalBounceVel => new Vector2(0, -5);

        private float activeDashVel; 
        private Vector2 activeStompVel; 
        public float timeElapsed => (float)timer / Main.frameRate; 
        public WulfrumBossStates[] currentStatesList, phaseAStatesList, phaseBStatesList; 
        private int stateIndex = -1;
        private bool newStateQueued; 
        public bool facePlayer = true;
        
        public float stepSpeed = 1; 
        public int timer = 0; 
        public int stateTimer = 0; 
        public float gfxOffY = 0;

        // Visual Animation Variables
        private static Asset<Texture2D> textureMainBody, textureWheelTreadBack, textureWheelTreadFront, textureTurret;
        private static Asset<Texture2D> textureBaseMap;       
        private static Asset<Texture2D> textureBaseAlphaMap;  
        private static Asset<Texture2D> textureNoiseCell;     
        private static Asset<Texture2D> textureNoiseWeb;      
        private static Asset<Texture2D> textureNoisePerlin;
        private static Asset<Texture2D> textureNoiseWebGlow;  
        private static Asset<Texture2D> textureRect;  
        private static Asset<Effect> effectRocketRam;

        private bool inDash, inFloat;

        private Vector2 spriteTarget => NPC.position + new Vector2(48, 36) + new Vector2(8, 0) * NPC.spriteDirection;
        private Vector2 currentWheelOffset;
        private Vector2 wheelKp => new Vector2(0.2f, 0.2f);
        private Vector2 wheelClampRanges => new Vector2(20, 8);
        private Vector2 mainBodyWP => NPC.Center + new Vector2(0, 0);
        private Vector2 turretWP => NPC.Center + new Vector2(0, 36);
        private Vector2 dashTrailWP => NPC.Center+ new Vector2(-201 + -201 * -NPC.spriteDirection, 110);
        private Vector2 backWheelOffset => new Vector2(-33, 0) * NPC.spriteDirection;
        private Vector2 frontWheelOffset => new Vector2(33, 0) * NPC.spriteDirection;
        private Vector2 wheelSize => new Vector2(108, 38);
        private WulfrumSentinelDrone drone;

        // public bool canJump;

        public enum WulfrumBossStates
        {
            Chase,
            Missile,
            Spawn,
            Dash,
            Stomp,
            Barrage
        }
        
        public WulfrumBossStates currentState;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1; 
            NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.ShimmerSlime;    
        }

        public override void SetDefaults()
        {
            NPC.width = 210;
            NPC.height= 72;
            NPC.aiStyle = -1;
            // NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.damage = 50;
            NPC.defense = 10;
            NPC.lifeMax = 5000;
            NPC.HitSound = hitSound;
            NPC.HitSound = hurtSound;
            NPC.value = 25f;
            NPC.knockBackResist = 0.1f;
            // NPC.;

            phaseAStatesList = new[]
            {
                WulfrumBossStates.Chase,  
                WulfrumBossStates.Dash,
                WulfrumBossStates.Chase,  
                WulfrumBossStates.Missile
                // WulfrumBossStates.Stomp
            };
            phaseBStatesList = new[]
            {
                // WulfrumBossStates.Barrage,
                WulfrumBossStates.Missile,
                WulfrumBossStates.Chase,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Missile,
                // WulfrumBossStates.Stomp
            };

            currentStatesList = phaseAStatesList;

            // Banner = Type;
            // BannerItem = 
            
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0;
            // return SpawnCondition.OverworldDaySlime.Chance * 1;
        }
        // public override dra

        public override void OnSpawn(IEntitySource source)
        {
            gfxOffY = 0;
            stateIndex = -1;
            SetNewState();
        }

        public override void Load()
        {
            if(Main.netMode != NetmodeID.Server)
            {
                textureMainBody = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/WulfrumBody");
                textureTurret = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/WulfrumTurret");
                textureWheelTreadFront = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/FrontWheel");
                textureWheelTreadBack = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/BackWheel");
                
                textureBaseMap =        ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/RocketRamTextures/BaseMap");       
                textureBaseAlphaMap =   ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/RocketRamTextures/AlphaMap");  
                textureNoiseCell =      ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/RocketRamTextures/VoronoiMap");     
                textureNoiseWeb =       ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/RocketRamTextures/WebMap");      
                textureNoisePerlin =    ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/RocketRamTextures/PerlinMap");       
                textureNoiseWebGlow =   ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/RocketRamTextures/WebGlowMap");  
                textureRect =           ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/RocketRamTextures/OpaqueRect");  


                effectRocketRam = ModContent.Request<Effect>("CalamityRelics/Assets/Effects/RocketRamEffect");

                // GameShaders.Misc["CalamityRelics:RocketRam"] = new MiscShaderData(effectRocketRam, "TestEffect").UseImage0(textureBaseMap).UseSamplerState(SamplerState.LinearWrap);

                GameShaders.Misc["CalamityRelics:RocketRam"] = new RocketRamShaderData(
                    effectRocketRam, "Main", 
                    textureBaseMap,
                    textureBaseAlphaMap,
                    textureNoiseCell,
                    textureNoiseWeb,
                    textureNoisePerlin,
                    textureNoiseWebGlow,
                    Vector2.UnitX * 1.4f,
                    Vector2.UnitX * 2.3f,
                    Vector2.UnitX * -0.07f,
                    new Vector2(2.8f, 0.4f),
                    new Color(0f, 1f, 0.8f, 1f),
                    0.4f);
            }
        }
        public void StepUpGroundedMovement()
        {
            // ported --
            if (gfxOffY > 0f) {
                gfxOffY -= 1 * 1;
                if (gfxOffY < 0f)
                    gfxOffY = 0f;
            }
            else if (gfxOffY < 0f) {
                gfxOffY += 1 * 1;
                if (gfxOffY > 0f)
                    gfxOffY = 0f;
            }

            if (gfxOffY > 32f)
                gfxOffY = 32f;

            if (gfxOffY < -32f)
                gfxOffY = -32f;

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref stepSpeed, ref gfxOffY, 1);
        }


        // ------------------------- STATE MACHINE --------------------------
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if(Main.netMode == NetmodeID.Server) return true;

            // Vector2 dir = targetPlayer.position - laserBarrelPos;
            Vector2 imagePos = NPC.position - screenPos;

            
            Vector2 newVal = Vector2.Zero;
            
            newVal.X = float.Lerp(currentWheelOffset.X, spriteTarget.X, wheelKp.X);
            newVal.Y = float.Lerp(currentWheelOffset.Y, spriteTarget.Y, wheelKp.Y);
            
            Vector2 relativeOffset = spriteTarget - newVal;
            relativeOffset.X = Math.Clamp(relativeOffset.X,  -wheelClampRanges.X, wheelClampRanges.X);
            relativeOffset.Y = Math.Clamp(relativeOffset.Y, -wheelClampRanges.Y, wheelClampRanges.Y);


            // Main.NewText(currentWheelOffset + " ::: " + spriteTarget);
            
            currentWheelOffset = spriteTarget - relativeOffset;
            Vector2 wheelOffsetPos = currentWheelOffset - screenPos;

            Vector2 backWheelPos = wheelOffsetPos + backWheelOffset;
            Vector2 frontWheelPos = wheelOffsetPos + frontWheelOffset;
            Vector2 turretPos = GetPositionFromCentered(turretWP - screenPos, textureTurret.Value);
            Vector2 turretSize = textureTurret.Value.Size();

            Vector2 mainBodyPos = GetPositionFromCentered(mainBodyWP - screenPos, textureMainBody.Value);
            Vector2 mainBodySize = textureMainBody.Value.Size();

            
            spriteBatch.Draw(textureTurret.Value,    new Rectangle((int)turretPos.X, (int)turretPos.Y, (int)turretSize.X, (int)turretSize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            spriteBatch.Draw(textureWheelTreadBack.Value,    new Rectangle((int)backWheelPos.X, (int)backWheelPos.Y, (int)wheelSize.X, (int)wheelSize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            spriteBatch.Draw(textureMainBody.Value, new Rectangle((int)mainBodyPos.X, (int)mainBodyPos.Y, (int)mainBodySize.X, (int)mainBodySize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            spriteBatch.Draw(textureWheelTreadFront.Value,   new Rectangle((int)frontWheelPos.X, (int)frontWheelPos.Y, (int)wheelSize.X, (int)wheelSize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);

            // spriteBatch.Begin(SpriteSortMode.Immediate, newBlendState ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

            if (inDash) { RenderDash(spriteBatch, screenPos); }

            return false;
        }


        public void RenderDash(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            Vector2 testDashShaderPos = GetPositionFromCentered(dashTrailWP - screenPos, textureRect.Value);
            Vector2 testDashShaderSize = new Vector2(400, -200);
            
            // Should in theory flip the direction of the effect
            // if(NPC.spriteDirection == -1)
            // {
            //     testDashShaderSize.X *= -1;
            //     testDashShaderPos.X -= testDashShaderSize.X;
            // }


            // End current batch
            spriteBatch.End();
            // Update shader settings
            GameShaders.Misc["CalamityRelics:RocketRam"].Apply();
            // Begin new spritebatch referencing shader
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, effectRocketRam.Value, Main.GameViewMatrix.TransformationMatrix);
            // Draw elements with shader
            spriteBatch.Draw(textureRect.Value, new Rectangle((int)testDashShaderPos.X, (int)testDashShaderPos.Y, (int)testDashShaderSize.X, (int)testDashShaderSize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            // End shader batch
            spriteBatch.End();
            // Reinitialize the default settings
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public Vector2 GetPositionFromCentered(Vector2 position, Texture2D texture)
        {
            Vector2 size = texture.Size();
            position -= size/2;
            return position;
        }

        public void FacePlayer()
        {
            NPC.spriteDirection = dirToTarget.X > 0 ? -1 : 1;
        }

        public SpriteEffects SpriteEffect()
        {
            return NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        public override void AI()
        {
            if (facePlayer)
            {
                FacePlayer();
            }

            // Sets the initial state on spawn
            if(timer == 0){
                
                stateIndex = 0;
                stateTimer = 0;
                currentState = currentStatesList[stateIndex];
                BeginNextState();

                timer++;
                return;
            }

            // Fires a laser if this tick aligns with their fire rates
            if(currentStatesList == phaseAStatesList){

                if(timer % currentFireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    FireLaser();
                }

                // Fires a rolling bomb if this tick aligns with their fire rates
                if(timer % currentRollFireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    FireBomb();
                }
            }



            

            // Updates the active state machine
            timer++;

            if (newStateQueued)
            {
                stateTimer = 0;
                BeginNextState();
            }
            else
            {
                stateTimer++;
                UpdateCurrentState();
            }
        
        }
        public void FireLaser()
        {
            Vector2 velocity = Vector2.Normalize(targetPlayer.Center - laserBarrelPos) * laserVelocity;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), laserBarrelPos, velocity, ProjectileID.EyeLaser, 35, 5, -1);

            SoundEngine.PlaySound(laserShootSound, laserBarrelPos);
        }

        public void FireBomb()
        {
            Main.NewText("Bombs away!");
            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, ModContent.NPCType<RollingBomb>());
            SoundEngine.PlaySound(barrelShootSound, laserBarrelPos);
        }

        // Called by states which decide that they are finished with their process
        public void SetNewState()
        {
            Main.NewText("");
            
            // Second phase transition
            if(NPC.GetLifePercent() <= phaseBHealthThreshold && currentStatesList != phaseBStatesList)
            {
                stateIndex = 0;
                currentStatesList = phaseBStatesList;
                // NPC.mod
                Main.NewText("Im angry now >:(");
                
                drone = NPC.NewNPCDirect(NPC.GetSource_FromAI(), laserBarrelPos, ModContent.NPCType<WulfrumSentinelDrone>()).ModNPC as WulfrumSentinelDrone; 
                Main.NewText(drone);
                
            }
            else
            {
                // Standard indexing
                stateIndex++;
                if(currentStatesList.Length <= stateIndex){stateIndex = 0;}
            }

            SetNewState(currentStatesList[stateIndex]);
        }

        // A way to force the next state manually without depending on the current state list
        public void SetNewState(WulfrumBossStates newState)
        {
            // Ends the current state and sets the new one
            EndCurrentState();
            currentState = newState;
            newStateQueued = true;
        }

        // Calls the End() Method of the current state
        public void EndCurrentState()
        {
            
            switch (currentState)
            {
                case WulfrumBossStates.Chase:
                    ChaseEnd();
                    break;
                case WulfrumBossStates.Missile:
                    MissileEnd();
                    break;
                case WulfrumBossStates.Spawn:
                    SpawnEnd();
                    break;
                case WulfrumBossStates.Dash:
                    DashEnd();
                    break;
                case WulfrumBossStates.Stomp:
                    StompEnd();
                    break;
                case WulfrumBossStates.Barrage:
                    BarrageEnd();
                    break;
            }
        }

        // Calls the Start() Method of the current state
        public void BeginNextState()
        {
            newStateQueued = false;

            switch (currentState)
            {
                case WulfrumBossStates.Chase:
                    ChaseStart();
                    break;
                case WulfrumBossStates.Missile:
                    MissileStart();
                    break;
                case WulfrumBossStates.Spawn:
                    SpawnStart();
                    break;
                case WulfrumBossStates.Dash:
                    DashStart();
                    break;
                case WulfrumBossStates.Stomp:
                    StompStart();
                    break;
                case WulfrumBossStates.Barrage:
                    BarrageStart();
                    break;
            }
        }

        // Calls the Update() Method of the current state
        public void UpdateCurrentState()
        {
            switch (currentState)
            {
                case WulfrumBossStates.Chase:
                    ChasePeriodic();
                    break;
                case WulfrumBossStates.Missile:
                    MissilePeriodic();
                    break;
                case WulfrumBossStates.Spawn:
                    SpawnPeriodic();
                    break;
                case WulfrumBossStates.Dash:
                    DashPeriodic();
                    break;
                case WulfrumBossStates.Stomp:
                    StompPeriodic();
                    break;
                case WulfrumBossStates.Barrage:
                    BarragePeriodic();
                    break;
            }
        }

        public void Jump()
        {
            // if(!canJump) return;
            NPC.velocity.Y = -jumpForce;
        }

        // ------------------------ CHASE STATE --------------------------
        public void ChaseStart()
        {
            // Anything to initiate the state
            Main.NewText("Standard chase -_-");
        }

        public void ChasePeriodic()
        {
            // cancel and do next state
            if(stateTimer > 300){
                SetNewState();
                return;
            }



            // Main Grounded Run Cycle
            NPC.TargetClosest();
            
            Vector2 vel = NPC.velocity;
            Vector2 targetPos = Main.player[NPC.target].Center;
            Vector2 targetDir = Vector2.Normalize(targetPos - NPC.Center);
            
            float xDirectionFactor = targetDir.X >= 0 ? 1 : -1;
            float deacceleration = xDirectionFactor == -(vel.X / Math.Abs(vel.X)) ? deaccelerationFactor : 0;

            NPC.velocity.X = Math.Clamp(vel.X + (accelerationFactor + deacceleration) * xDirectionFactor, -maxSpeed, maxSpeed);
            StepUpGroundedMovement();     

            if(grounded && NPC.collideX){
                Jump();
            }
        }

        public void ChaseEnd()
        {
            // Stop Running
            NPC.velocity = Vector2.Zero;
        }

        // ------------------------ SHOOT MISSILE STATE --------------------------
        
        public void MissileStart()
        {
            
            // Start animation
            Main.NewText("Shooting Missiles!");
        }

        public void MissilePeriodic()
        {
            int missileCount = 5;
            int ticksBetweenMissiles = 5;

            if(timer % ticksBetweenMissiles == 0)
            {
                if(Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Summon Projectile
                    float velX = Math.Clamp(Main.player[NPC.target].Center.X - NPC.Center.X, -maxXVelMissile, maxXVelMissile) + Random.Shared.Next(-3, 3);
                    Vector2 velocity = new Vector2(
                        velX,
                        -YVelMissile
                    );

                    int projectileIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.UnitY * -100, velocity, ModContent.ProjectileType<WulfrumMissile>(), 35, 5, -1, NPC.target, NPC.whoAmI, (int)(stateTimer/ticksBetweenMissiles));
                }       
            }

            if(stateTimer >= missileCount * ticksBetweenMissiles) { 
                SetNewState();
            }
        }

        public void MissileEnd()
        {
            // idk
        }

        // ------------------------ SPAWN WULFRUM ENEMIES STATE --------------------------

        public void SpawnStart()
        {
            // Start animation
            // Spawn Wulfrum Enemies
            NPC.velocity = Vector2.Zero;
        }

        public void SpawnPeriodic()
        {
            // Do animation and stay still
        }

        public void SpawnEnd()
        {
            // idk
        }

        // ------------------------ DASH STATE --------------------------

        public void DashStart()
        {
            // Do back dash
            NPC.velocity.X = dirToTarget.X < 0 ? -dashPreVel : dashPreVel;
        }

        public void DashPeriodic()
        {
            StepUpGroundedMovement();
            
            if(stateTimer >= dashTime + dashWindupTime)
            {
                SetNewState();
            }
            else if(stateTimer > dashWindupTime)
            {
                NPC.velocity.X = activeDashVel;
            }
            else if(stateTimer == dashWindupTime)
            {
                activeDashVel = dirToTarget.X < 0 ? -dashVel : dashVel;
                NPC.velocity.X = activeDashVel;
                
                Main.NewText("Dashing!");
                inDash = true;
                
                facePlayer = false;
                FacePlayer();
            }
            else
            {
                NPC.velocity.X *= 0.98f;
            }

        }

        public void DashEnd()
        {
            inDash = false;
            facePlayer = true;
        }

         // ------------------------ STOMP STATE --------------------------

        public void StompStart()
        {
            NPC.noTileCollide = true;
        }

        public void StompPeriodic()
        {
            
            // StepUpGroundedMovement();
            // if(stateTimer >= dashTime + dashWindupTime)


            if(stateTimer > stompHoverTime)
            {
                NPC.velocity = activeStompVel;
                if(NPC.collideY) 
                {
                    SetNewState();
                }
            }
            else if(stateTimer == stompHoverTime)
            {
                NPC.noTileCollide = false;

                Vector2 targetStompPos = targetPlayer.Center + targetPlayer.velocity * stompSpeed * 10f;

                float XDif = Math.Abs(targetStompPos.X - NPC.Center.X);
                float YDif = Math.Abs(targetStompPos.Y - NPC.Center.Y);
                float factor = XDif / YDif;


                if(factor > maxStompXFactor)
                {
                    targetStompPos.X = NPC.Center.X + (YDif * maxStompXFactor * flipFactor);
                }

                activeStompVel = targetStompPos - NPC.Center;
                if(activeStompVel.Y < 0) {activeStompVel *= -1;}
                activeStompVel = Vector2.Normalize(activeStompVel) * stompSpeed;

                NPC.velocity = activeStompVel;
                NPC.rotation = Math.Clamp( -NPC.velocity.X , -30, 30);
            }
            else 
            {
                float Kp = 0.07f;
                Vector2 currentPos = NPC.Center;
                Vector2 targetPos = targetPlayer.Center + stompHoverOffset;
                Vector2 newPos = Vector2.Lerp(currentPos, targetPos, Kp);

                NPC.Center = newPos;
                NPC.rotation = Math.Clamp( currentPos.X - newPos.X , -30, 30);

            }
        }

        public void StompEnd()
        {
            NPC.rotation = 0;
            NPC.noTileCollide = false;
            NPC.velocity = postStompCorrectionalBounceVel;
            // idk
        }

         // ------------------------ BARRAGE STATE --------------------------

        public void BarrageStart()
        {
            // NPC.noTileCollide = true;
            Main.NewText("Hovering over player (Prepare for a stomp)");
            initialBarrageFlipFactor = flipFactor;
        }
        public double GetBarrageYVel(float normalizedTime)
        {
            // Main.NewText(-16 * barrageHoverFlyMaxSpeed * Math.Pow(normalizedTime - 0.5f, 4));
            // Main.NewText((float)(8 * barrageHoverFlyMaxSpeed * Math.Pow(normalizedTime - 0.5f, 3)));
            
            return 8 * barrageHoverFlyMaxSpeed * Math.Pow(normalizedTime - 0.5f, 3);
        }

        public void BarragePeriodic()
        {

            int missileCount = 40;
            int ticksBetweenOrbs = 7;
            float totalTime = missileCount * ticksBetweenOrbs;



            NPC.velocity.Y = (float)GetBarrageYVel(stateTimer/totalTime);
            NPC.velocity.X = barrageXVel * initialBarrageFlipFactor;

            if(timer % ticksBetweenOrbs == 0)
            {
                if(Main.netMode != NetmodeID.MultiplayerClient)
                {

                    float velX = Main.rand.NextFloat(-6, 6);

                    Vector2 velocity = new Vector2(
                        velX,
                        -YVelOrb
                    );

                    int projectileIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.UnitY * -100, velocity, ModContent.ProjectileType<WulfrumElectricOrb>(), 25, 5, -1);
                }       
            }

            NPC.rotation = Math.Clamp( -NPC.velocity.X , -30, 30);

            if(stateTimer >= totalTime) { 
                SetNewState();
            }
        }


        public void BarrageEnd()
        {
            NPC.rotation = 0;
            // NPC.noTileCollide = false;
            NPC.velocity = postStompCorrectionalBounceVel;
            
            // idk
        }

        public override bool? CanFallThroughPlatforms()
        {
            return targetPlayer.Center.Y >= NPC.Center.Y + 20 && currentState != WulfrumBossStates.Stomp;
        }
    }
}