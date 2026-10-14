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
using Terraria.GameContent.Bestiary;
using CalamityRelics.HelperScripts;

namespace CalamityRelics.Content.NPCs.WulfrumSentinel{

    [AutoloadBossHead]
    public class WulfrumSentinel : ModNPC
    {
        // Static variable fields
        public static float deaccelerationFactor => 0.25f; 
        public static float accelerationFactor => 0.1f; 
        public static float YVelMissile => 11f; 
        public static float YVelOrb=> 6f; 
        public static float maxXVelMissile => 6f; 
        public static float barrageHoverFlyMaxSpeed => 9f; 
        public static float barrageXVel => 3; 
        public static float jumpForce => 7.5f; 

        public static float phaseBHealthThreshold => 0.5f; 
        public static float dashVel => 7f; 
        public static float dashPreVel => -4f; 
        public static float dashWindupTime => 60; 
        public static float dashFlyingWindupTime => 30; 
        public static float stompHoverTime => 240; 
        public static float dashTime => 60; 
        public static float laserVelocity => 10f;


        public static int minFireRate => 120;
        public static int maxFireRate => 50;
        public static int rollingRateMin => 150;
        public static int rollingRateMax => 480;
        public static int stompSpeed => 10; 
        public static float maxStompXFactor => 1.65f; 
        public static float flyingXOffset => 400;

        public static SoundStyle laserShootSound => SoundID.DD2_BetsyFireballShot;
        public static SoundStyle barrelShootSound => SoundID.DD2_DrakinShot;
        public static SoundStyle hitSound => SoundID.NPCHit1;
        public static SoundStyle hurtSound => SoundID.NPCDeath1;

        public float maxSpeed => 3.5f * float.Clamp(distToTarget / 300, 0, 1); 

        // Like WOF, fire rates should scale similarily
        public int currentFireRate => (int)float.Lerp(maxFireRate, minFireRate, NPC.GetLifePercent());
        public int currentRollFireRate => (int)float.Lerp(rollingRateMin, rollingRateMax, NPC.GetLifePercent());


        // I am probably leaving out a built in variable or method that does this already that would be able to replace this later
        public bool flipped => targetPlayer.Center.X - NPC.Center.X < 0;
        public int flipFactor => flipped ? -1 : 1;
        public bool grounded => NPC.collideY && NPC.velocity.Y == 0;
        public int initialBarrageFlipFactor;

        // Laser Related Variables
        public bool doLaser;
        public float laserWidth => 10;
        public float laserStartX => NPC.spriteDirection == 1 ? Main.screenPosition.X : laserBarrelPos.X;
        public float laserStartY => laserBarrelPos.Y - laserWidth/2;
        public Vector2 laserRenderStart => new Vector2( laserStartX, laserStartY);
        public float laserEndPosX => NPC.spriteDirection == 1 ? laserBarrelPos.X : Main.screenPosition.X + Main.screenWidth;
        public float laserLength => MathF.Abs(laserEndPosX - laserStartX);

        // public WulfrumSentinelDrone drone;

        public Player targetPlayer => Main.player[NPC.target]; 
        public float distToTarget => (targetPlayer.Center - NPC.Center).Length() ; 
        public Vector2 dirToTarget => NPC.DirectionTo(targetPlayer.Center); 
        public Vector2 rocketFirePos => NPC.Center + new Vector2(0, -100); 
        public Vector2 laserBarrelPos => NPC.Center + new Vector2(0, -30); 
        public Vector2 droneOffset => new Vector2(0, -50); 
        public Vector2 stompHoverOffset => new Vector2(0, -500);
        public Vector2 postStompCorrectionalBounceVel => new Vector2(0, -5);

        private Vector2 activeDashVel; 
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
        private static Asset<Texture2D> textureMainBody, textureFull, textureWheelTreadBack, textureWheelTreadFront, textureTurret, textureBodyFlying;
        private static Asset<Texture2D> textureBaseMap;       
        private static Asset<Texture2D> textureBaseAlphaMap;  
        private static Asset<Texture2D> textureNoiseCell;     
        private static Asset<Texture2D> textureNoiseWeb;      
        private static Asset<Texture2D> textureNoisePerlin;
        private static Asset<Texture2D> textureNoiseWebGlow;  
        private static Asset<Texture2D> textureRect;  
        private static Asset<Effect> effectRocketRam;

        private bool inDash, inFloat, flying;

        private Vector2 spriteTarget => NPC.position + new Vector2(48, 36) + new Vector2(8, 0) * NPC.spriteDirection;
        private Vector2 currentWheelOffset;
        private Vector2 wheelKp => new Vector2(0.2f, 0.2f);
        private Vector2 wheelClampRanges => new Vector2(20, 8);
        private Vector2 mainBodyWP => NPC.Center + new Vector2(0, 0);
        private Vector2 turretWP => NPC.Center + new Vector2(0, -16);
        private float turretMaxAngle => 30 * 180 / MathF.PI;
        private Vector2 dashTrailWP => NPC.Center+ new Vector2(-201 + -201 * -NPC.spriteDirection, 110);
        private Vector2 backWheelOffset => new Vector2(-33, 0) * NPC.spriteDirection;
        private Vector2 frontWheelOffset => new Vector2(33, 0) * NPC.spriteDirection;
        private Vector2 wheelSize => new Vector2(108, 38);
        private WulfrumSentinelDrone drone;
        private Origin turretOrigin;

        // public bool canJump;

        public enum WulfrumBossStates
        {
            Chase,
            Missile,
            Spawn,
            Dash,
            Stomp,
            Barrage,
            FlyChase,
            FlyLaser
        }
        
        public WulfrumBossStates currentState;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1; 
            NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.ShimmerSlime;    
        }


        public override void SetDefaults()
        {
            NPC.width = 178*2;
            NPC.height= 99*2;
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
                WulfrumBossStates.FlyChase,
                // WulfrumBossStates.Missile,
                // WulfrumBossStates.Missile,
                WulfrumBossStates.FlyLaser,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Dash,
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
                textureFull = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/WulfrumSentinel");
                textureTurret = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/WulfrumTurret");
                textureWheelTreadFront = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/FrontWheel");
                textureWheelTreadBack = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/BackWheel");
                textureBodyFlying = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/WulfrumSentinelFlying");
                
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

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // bestiaryEntry.
            
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
            if(Main.netMode == NetmodeID.Server) return false;
            // ported line for smth: spriteBatch.Begin(SpriteSortMode.Immediate, newBlendState ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
        
            DrawTurret(spriteBatch, screenPos);
            // DrawMainLayers(spriteBatch, screenPos);
            if (doLaser)
            {    
                Rectangle laserRect = new Rectangle((int)(laserRenderStart.X - screenPos.X), (int)(laserRenderStart.Y - screenPos.Y), (int)laserLength+1, (int)laserWidth );
                spriteBatch.Draw(textureRect.Value, laserRect, null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            }
            if (inDash)  { RenderDash(spriteBatch, screenPos); }

            if (flying) 
                spriteBatch.Draw( textureBodyFlying.Value, RectangleHelper.CreateAnchored( RectangleHelper.Anchor.CENTER, NPC.Center - screenPos, NPC.Size), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            else        
                spriteBatch.Draw( textureBodyFlying.Value, RectangleHelper.CreateAnchored( RectangleHelper.Anchor.CENTER, NPC.Center - screenPos, NPC.Size), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            return false;
        }
        
        void DrawMainLayers(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            // Calculating new shared offsets            
            Vector2 newVal = Vector2.Zero;
            newVal.X = float.Lerp(currentWheelOffset.X, spriteTarget.X, wheelKp.X);
            newVal.Y = float.Lerp(currentWheelOffset.Y, spriteTarget.Y, wheelKp.Y);
            
            Vector2 relativeOffset = spriteTarget - newVal;
            relativeOffset.X = Math.Clamp(relativeOffset.X,  -wheelClampRanges.X, wheelClampRanges.X);
            relativeOffset.Y = Math.Clamp(relativeOffset.Y, -wheelClampRanges.Y, wheelClampRanges.Y);

            currentWheelOffset = spriteTarget - relativeOffset;
            
            // Generating wheel positions from shared offset      
            Vector2 wheelOffsetPos = currentWheelOffset - screenPos;

            Vector2 backWheelPos = wheelOffsetPos + backWheelOffset;
            Vector2 frontWheelPos = wheelOffsetPos + frontWheelOffset;

            Vector2 mainBodyPos = GetPositionFromCentered(mainBodyWP - screenPos, textureMainBody.Value);
            Vector2 mainBodySize = textureMainBody.Value.Size();


            // Drawing elements  
            if (!flying) spriteBatch.Draw(textureWheelTreadBack.Value, new Rectangle((int)backWheelPos.X, (int)backWheelPos.Y, (int)wheelSize.X, (int)wheelSize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            spriteBatch.Draw(textureMainBody.Value, new Rectangle((int)mainBodyPos.X, (int)mainBodyPos.Y, (int)mainBodySize.X, (int)mainBodySize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            if (!flying) spriteBatch.Draw(textureWheelTreadFront.Value,new Rectangle((int)frontWheelPos.X, (int)frontWheelPos.Y, (int)wheelSize.X, (int)wheelSize.Y), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);

        }

        void DrawTurret(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            // Calculating origin (position and rotation target pointing towards the player)
            Vector2 dir = Vector2.Normalize(targetPlayer.Center - turretWP);

            // float maxX = MathF.Cos(Origin.ToDegrees(5));
            // float minX = MathF.Cos(Origin.ToDegrees(-turretMaxAngle));

            // dir.Y = float.Clamp(dir.X, maxX, minX);
            // dir = Vector2.Normalize(dir);
            
            // float.Clamp(angle, 0, turretMaxAngle);
            // if(NPC.spriteDirection > 0)
            // {
            //     angle = float.Clamp(angle, -turretMaxAngle, 10);
            // }
            // else
            // {
            //     if(angle > 0) angle = float.Clamp(angle, 180+turretMaxAngle, -180);
            //     else angle = float.Clamp(angle, 170, 180);
            // }
            turretOrigin = new Origin(turretWP, dir);

            // Positioning Elements      

            // Vector2 turretPos = GetPositionFromCentered(turretWP - screenPos, textureTurret.Value);
            Vector2 turretSize = textureTurret.Value.Size();
            Vector2 turretPos = turretOrigin.position - Vector2.UnitX * turretSize.X/2 * -NPC.spriteDirection - screenPos;// - Vector2.UnitY * turretSize.Y/2 - screenPos;
            
            // Drawing elements      
            spriteBatch.Draw(
                textureTurret.Value, 
                new Rectangle((int)turretPos.X, (int)turretPos.Y, (int)turretSize.X, (int)turretSize.Y), 
                null, 
                Color.White, 
                turretOrigin.rotationRad, 
                new Vector2(0, turretSize.Y/2), 
                NPC.spriteDirection == 1 ? SpriteEffects.FlipVertically: SpriteEffects.None, 
                0);
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
            if (facePlayer) FacePlayer();
            if(doLaser) LaserLogic();

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
                    // FireLaser();
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
            Vector2 velocity = turretOrigin.unitVector * laserVelocity;
            Vector2 pos = turretOrigin.LocalToWorldSpace(Vector2.UnitX * 100);

            if(Main.netMode != NetmodeID.MultiplayerClient) Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, velocity, ProjectileID.EyeLaser, 35, 5, -1);

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
                StartSecondPhase();
            }
            else
            {
                // Standard indexing
                stateIndex++;
                if(currentStatesList.Length <= stateIndex){stateIndex = 0;}
            }

            SetNewState(currentStatesList[stateIndex]);
        }

        public void StartSecondPhase()
        {
            // Logic code
            stateIndex = 0;
            currentStatesList = phaseBStatesList;
            flying = true;
            NPC.noGravity = true;

            // Sprite Change code


            // Drone code
            Vector2 droneSpawn = NPC.Center + droneOffset;
            int droneInt = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, ModContent.NPCType<WulfrumSentinelDrone>()); 
            Main.NewText("DroneInt: "+ droneInt + " \n MaxINT: " + Main.maxNPCs);
            if( droneInt >= 0 && droneInt < Main.maxNPCs && Main.npc[droneInt].ModNPC is WulfrumSentinelDrone)
            {
                Main.NewText("Chad logic" + Main.npc[droneInt].ModNPC);
                NPC entity = Main.npc[droneInt];
                drone = entity.ModNPC as WulfrumSentinelDrone;

                Main.NewText(drone);
                drone.SetHost(this);
                drone.Attach(droneOffset);
            }
            else
            {
                Main.NewText("Chud logic");
            }
            // }
            // Main.NewText("Im angry now >:(");

            // NPC.mod
            //Main.NewText(drone);
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
                case WulfrumBossStates.FlyChase:
                    FlyChaseEnd();
                    break;
                case WulfrumBossStates.FlyLaser:
                    FlyLaserEnd();
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
                case WulfrumBossStates.FlyChase:
                    FlyChaseStart();
                    break;
                case WulfrumBossStates.FlyLaser:
                    FlyLaserStart();
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
                case WulfrumBossStates.FlyChase:
                    FlyChasePeriodic();
                    break;
                case WulfrumBossStates.FlyLaser:
                    FlyLaserPeriodic();
                    break;
            }
        }

        public void Jump()
        {
            // if(!canJump) return;
            NPC.velocity.Y = -jumpForce;
        }

        // ------------------------ CHASE STATE --------------------------
        public void FlyChaseStart()
        {
            // Anything to initiate the state
            Main.NewText("flying chase >_<");
            NPC.noTileCollide = true;
        }

        public void FlyChasePeriodic()
        {
            // cancel and do next state
            if(stateTimer > 180){
                SetNewState();
                return;
            }

            // Main Grounded Run Cycle
            NPC.TargetClosest();
            
            Vector2 vel = NPC.velocity;
            Vector2 targetDir = Main.player[NPC.target].Center - NPC.Center;
            float xDirectionFactor = targetDir.X >= 0 ? 1 : -1;
            
            Vector2 targetPos = Main.player[NPC.target].Center;
            targetPos.X += flyingXOffset * -xDirectionFactor;
            targetDir = targetPos - NPC.Center;
            // float targetDist = targetDir.Length();
            
            NPC.velocity = targetDir * 0.04f;
        }

        public void FlyChaseEnd()
        {
            // Stop Running
            NPC.velocity = Vector2.Zero;
        }

        public void FlyLaserStart()
        {
            // Anything to initiate the state
            Main.NewText("flying chase >_<");
            NPC.noTileCollide = true;

            if(Main.netMode != NetmodeID.MultiplayerClient) 
            {
                drone.Detach();
            }
            doLaser = true;
        }

        public void FlyLaserPeriodic()
        {
            // cancel and do next state
            if(stateTimer > 360){
                SetNewState();
                return;
            }

            if(stateTimer > 60)
            {
                if(Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if(stateTimer % 20 == 0)
                    {
                        drone.FireMissile();
                    }
                }

                if(stateTimer % 5 == 0)
                {
                    FireLaser();
                }
            }

            // Main Grounded Run Cycle
            NPC.TargetClosest();
            
            Vector2 targetDir = Main.player[NPC.target].Center - NPC.Center;
            float xDirectionFactor = targetDir.X >= 0 ? 1 : -1;
            
            Vector2 targetPos = Main.player[NPC.target].Center;
            Vector2 targetDronePos = targetPos;
            
            targetDronePos.X += flyingXOffset * xDirectionFactor;
            targetPos.X += flyingXOffset * -xDirectionFactor;
            
            targetDir = targetPos - NPC.Center;
            // float targetDist = targetDir.Length();

            drone.ProportionalMove(targetDronePos, 0.02f);
            
            NPC.velocity = targetDir * 0.04f;
            NPC.velocity.Y = float.Clamp(NPC.velocity.Y, 2, 8) * NPC.velocity.Y > 0? 1 : -1;
        }
        public void FlyLaserEnd()
        {
            drone.Attach(droneOffset);
            doLaser = false;
        }




        public void LaserRender(SpriteBatch spriteBatch, Vector2 screenPos)
        {   
            // Rendering
            // Vector2 testDashShaderPos = GetPositionFromCentered(dashTrailWP - screenPos, textureRect.Value);
            // Vector2 testDashShaderSize = new Vector2(400, -200);

            Rectangle laserRect = new Rectangle((int)(laserRenderStart.X - screenPos.X), (int)(laserRenderStart.Y - screenPos.Y), (int)laserLength+1, (int)laserWidth );
            // Main.NewText(NPC.spriteDirection);
            
            // Should in theory flip the direction of the effect
            // if(NPC.spriteDirection == -1)
            // {
            //     testDashShaderSize.X *= -1;
            //     testDashShaderPos.X -= testDashShaderSize.X;
            // }


            // End current batch
            // spriteBatch.End();
            // Update shader settings
            // GameShaders.Misc["CalamityRelics:LaserShader"].Apply();
            // Begin new spritebatch referencing shader
            // spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, effectRocketRam.Value, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw elements with shader
            spriteBatch.Draw(textureRect.Value, laserRect, null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            
            
            // End shader batch
            // spriteBatch.End();
            // Reinitialize the default settings
            // spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
        }

        public void LaserLogic()
        {   
            // Logic
            if(Main.netMode == NetmodeID.MultiplayerClient) return;

            Rectangle laserRect = new Rectangle((int)laserRenderStart.X, (int)laserRenderStart.Y, (int)laserLength+1, (int)laserWidth );
            bool hittingPlayer = targetPlayer.Hitbox.Intersects(laserRect);
            if (hittingPlayer)
            {
                Player.HurtInfo info = new()
                {
                    HitDirection = NPC.spriteDirection,
                    SourceDamage = 20,
                    Knockback = 3,
                    CooldownCounter = ImmunityCooldownID.Bosses,
                    PvP = false,
                    DamageSource = PlayerDeathReason.ByNPC(NPC.whoAmI)
                };

                targetPlayer.Hurt(info);
            }
        }


        public void ChaseStart()
        {
            // Anything to initiate the state
            Main.NewText("Standard chase -_-");
        }

        public void ChasePeriodic()
        {
            // cancel and do next state
            if(stateTimer > (flying ? 200 : 300)){
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
            
            if(stateTimer >= dashTime + (flying ? dashFlyingWindupTime : dashWindupTime) )
            {
                SetNewState();
            }
            else if(stateTimer > (flying ? dashFlyingWindupTime : dashWindupTime) )
            {
                NPC.velocity.X = activeDashVel.X;
                if(flying) NPC.velocity.Y = activeDashVel.Y;
            }
            else if(stateTimer == (flying ? dashFlyingWindupTime : dashWindupTime) )
            {
                activeDashVel = 
                flying ? 
                    dirToTarget * dashVel :
                    Vector2.UnitX * (dirToTarget.X < 0 ? -dashVel : dashVel);
                
                // Setting velocity
                NPC.velocity.X = activeDashVel.X;
                if (flying)
                {
                    NPC.velocity.Y = activeDashVel.Y;
                }
                
                Main.NewText("Dashing!");
                inDash = true;
                
                facePlayer = false;
                FacePlayer();
            }
            else
            {
                // preDash velocity management
                NPC.velocity.X *= 0.98f;
                if (flying)
                {
                    NPC.velocity.Y = (targetPlayer.Center.Y - NPC.Center.Y) * 0.1f;   
                }
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