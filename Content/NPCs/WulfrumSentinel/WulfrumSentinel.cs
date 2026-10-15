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
        public static float distanceUntilMustJump => 200f; 

        public static float phaseBHealthThreshold => 0.5f; 
        public static float dashVel => 9f; 
        public static float dashFlyingVel => 14f; 
        public static float dashPreVel => -4f; 
        public static float dashWindupTime => 60; 
        public static float dashFlyingWindupTime => 45; 
        public static float stompHoverTime => 240; 
        public static float dashTime => 60; 
        public static float laserVelocity => 10f;
        public static float spriteScaleFactor => 2;


        public static int minFireRate => 120;
        public static int maxFireRate => 50;
        public static int rollingRateMin => 150;
        public static int rollingRateMax => 480;
        public static int stompSpeed => 10; 
        public static int maxTurretAngle => 17; 
        public static float maxStompXFactor => 1.65f; 
        public float flyingStandardXOffset => 400;
        public float flyingBarrageXOffset => 650;
        public float flyingXOffset = 400;

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
        public Vector2 barrageFlyingPos => NPC.Center + new Vector2( NPC.spriteDirection == 1 ? -70 : 70,30);
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
        public int timer = 0, lastJumpTimer = 0; 
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

        private bool inDash, inFloat, flying, jumping;

        private Vector2 spriteTarget => NPC.position + new Vector2(48, 36) + new Vector2(8, 0) * NPC.spriteDirection;
        private Vector2 currentWheelOffset;
        private Vector2 wheelKp => new Vector2(0.2f, 0.2f);
        private Vector2 wheelClampRanges => new Vector2(20, 8);
        private Vector2 mainBodyWP => NPC.Center + new Vector2(0, 0);

        private Vector2 turretWP => NPC.Center + new Vector2(0, -40);
        private Vector2 turretSize => textureTurret.Value.Size();
        private Vector2 turretPos => turretWP - (Vector2.UnitX * turretSize.X*3/5 * (NPC.spriteDirection == 1 ? -1 : 1));
        private Vector2 turretDir => Vector2.Normalize(targetPlayer.Center - turretPos);
        private bool turretInRange => MathF.Abs(MathF.Sin(float.DegreesToRadians(Origin.RotationFromVector(turretDir)))) < MathF.Sin(float.DegreesToRadians(maxTurretAngle));
        
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
            NPC.width = (int)(178*spriteScaleFactor);
            NPC.height= (int)(99*spriteScaleFactor);
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
                WulfrumBossStates.Missile,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Chase,  
                WulfrumBossStates.Barrage,
                WulfrumBossStates.Dash
                // WulfrumBossStates.Stomp
            };
            phaseBStatesList = new[]
            {
                WulfrumBossStates.FlyChase,
                // WulfrumBossStates.Missile,
                // WulfrumBossStates.Missile,
                WulfrumBossStates.FlyLaser,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Barrage,
                // WulfrumBossStates.Stomp
            };

            currentStatesList = phaseAStatesList;

            // Banner = Type;
            // BannerItem = 
            
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            gfxOffY = 0;
            stateIndex = -1;
            turretOrigin = new Origin(turretPos, turretDir);
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
            // ported line from some place i cant remember but will be important for shader development: spriteBatch.Begin(SpriteSortMode.Immediate, newBlendState ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
        
            // DrawMainLayers(spriteBatch, screenPos);
            if (doLaser)
            {    
                Rectangle laserRect = new Rectangle((int)(laserRenderStart.X - screenPos.X), (int)(laserRenderStart.Y - screenPos.Y), (int)laserLength+1, (int)laserWidth );
                spriteBatch.Draw(textureRect.Value, laserRect, null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            }
            if (inDash)  { RenderDash(spriteBatch, screenPos); }

            if (flying) 
                spriteBatch.Draw( textureBodyFlying.Value, RectangleHelper.CreateAnchored( RectangleHelper.Anchor.CENTER, NPC.Center + new Vector2(0, 6) - screenPos, NPC.Size), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            else        
                spriteBatch.Draw( textureMainBody.Value, RectangleHelper.CreateAnchored( RectangleHelper.Anchor.CENTER, NPC.Center - screenPos, NPC.Size), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            
            DrawTurret(spriteBatch, screenPos);
            return false;
        }
        
        void DrawMainLayers(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            // Old code for old tank sprites. Still might be applicable if we get moving wheel components similar to how it was before.

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
            if (turretInRange){
                turretOrigin = new Origin(turretPos, turretDir);
            }
            else
            {
                if( 
                    turretDir.X <= 0 && turretOrigin.unitVector.X > 0 ||
                    turretDir.X > 0 && turretOrigin.unitVector.X <= 0
                ) turretOrigin.Mirror(turretWP);
            }

            Vector2 pivotOrigin = new Vector2(
                0.25f,
                0.5f
            ) * turretSize;
            
            Rectangle baseAABB = RectangleHelper.CreateAnchored(RectangleHelper.Anchor.BOT_LEFT, turretPos - screenPos, turretSize*spriteScaleFactor);

            // Drawing element  
            spriteBatch.Draw(
                textureTurret.Value, 
                baseAABB, 
                null, 
                Color.White, 
                turretOrigin.rotationRad, 
                pivotOrigin, 
                dirToTarget.X >= 0 ?  SpriteEffects.None : SpriteEffects.FlipVertically, 
                0);
        }

        public void FireMissile()
        {
            Vector2 velocity = Vector2.Normalize(targetPlayer.Center - turretWP) * laserVelocity;
            int newMissileIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), turretWP, velocity, ModContent.ProjectileType<WulfrumSineMissile>(), 35, 5, -1, velocity.ToRotation());
            SoundEngine.PlaySound(laserShootSound, turretWP);
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

            // Phase One Exclusive Logic
            if(currentStatesList == phaseAStatesList){
                JumpLogic();
                UpdatePersistentProjectiles();

                // Fires a rolling bomb if this tick aligns with their fire rates
                if(timer % currentRollFireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    FireBomb();
                }
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

            // Updates the active state machine
            timer++;
            lastJumpTimer++;

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

        public void UpdatePersistentProjectiles()
        {
            if (turretInRange)
            {
                if(timer % currentFireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    FireLaser();
                }
                
            }
            else
            {   
                if(timer % (currentFireRate/3) == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    FireMissile();
                }
            }
        }

        public void JumpLogic()
        {
            // Jump controls for the first phase, when the player gets too high
            if(lastJumpTimer % 90 == 0 && ((targetPlayer.Center - NPC.Center).Y < -distanceUntilMustJump))
            {
                jumping = true;
                lastJumpTimer = 0;
            }

            if (jumping)
            {
                NPC.velocity.Y = -jumpForce;
                if ((bool)CanFallThroughPlatforms())
                {
                    jumping = false;
                    lastJumpTimer = 0;
                }
            }
        }

        public void FireLaser()
        {
            Origin origin = turretOrigin;
            // origin.position -=
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



        // ------------------------ CHASE STATE --------------------------
        public void FlyChaseStart()
        {
            // Anything to initiate the state
            Main.NewText("flying chase >_<");
            NPC.noTileCollide = true;
            flyingXOffset = flyingStandardXOffset;
        }

        public void FlyChasePeriodic(bool fromState = false)
        {
            // cancel and do next state
            if(stateTimer > 180 && !fromState){
                SetNewState();
                return;
            }

            if(timer % currentFireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient && !fromState)
            {
                FireLaser();
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

            flyingXOffset = flyingBarrageXOffset;

            if(Main.netMode != NetmodeID.MultiplayerClient) 
            {
                drone.Detach();
            }
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
            }
            if(stateTimer == 45)
            {
                doLaser = true;
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

            if(jumping){NPC.velocity.X = NPC.spriteDirection * -maxSpeed*2/3;}
            else{NPC.velocity.X = Math.Clamp(vel.X + (accelerationFactor + deacceleration) * xDirectionFactor, -maxSpeed, maxSpeed);}
            StepUpGroundedMovement();     

            if(grounded && NPC.collideX){
                JumpLogic();
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
            if(!flying) NPC.velocity.X = dirToTarget.X < 0 ? -dashPreVel : dashPreVel;
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
                    dirToTarget * dashFlyingVel :
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
            flyingXOffset = flyingBarrageXOffset;
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
            int windupTicks = 30;
            int ticksBetweenOrbs = flying ? 12 : 7;
            float totalTime = missileCount * ticksBetweenOrbs + windupTicks;

            if(flying){FlyChasePeriodic(true);}
            else
            {
                NPC.velocity.Y = (float)GetBarrageYVel(stateTimer/totalTime);
                NPC.velocity.X = barrageXVel * initialBarrageFlipFactor;
            }

            if(timer % ticksBetweenOrbs == 0 && stateTimer >= windupTicks)
            {
                if(Main.netMode != NetmodeID.MultiplayerClient)
                {

                    Vector2 velocity;

                    if (flying)
                    {
                        float velY = Main.rand.NextFloat(-6, 6);
                        velocity = new Vector2(
                            NPC.spriteDirection == 1 ? -10 : 10,
                            velY
                        );
                        
                    }
                    else
                    {
                        float velX = Main.rand.NextFloat(-6, 6);
                        velocity = new Vector2(
                            velX,
                            -YVelOrb
                        );
                        
                    }

                    int projectileIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), flying ? barrageFlyingPos : NPC.Center + Vector2.UnitY * -100, velocity, ModContent.ProjectileType<WulfrumElectricOrb>(), 25, 5, -1);
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
            return targetPlayer.BottomLeft.Y >= NPC.BottomLeft.Y + 20 && currentState != WulfrumBossStates.Stomp;
        }

        public override void OnKill()
        {
            if(drone != null && Main.netMode != NetmodeID.MultiplayerClient)
            {
                drone.NPC.immortal = false;
                drone.NPC.StrikeInstantKill();
            }
        }
    }
}