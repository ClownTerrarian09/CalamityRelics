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
        public bool flipped => targetPlayer.position.X - NPC.position.X < 0;
        public int flipFactor => flipped ? -1 : 1;
        public bool grounded => NPC.collideY && NPC.velocity.Y == 0;
        public int initialBarrageFlipFactor;


        // public WulfrumSentinelDrone drone;

        public Player targetPlayer => Main.player[NPC.target]; 
        public Vector2 dirToTarget => Vector2.Normalize(NPC.DirectionTo(targetPlayer.position)); 
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
            NPC.width = 18;
            NPC.height= 40;
            NPC.aiStyle = -1;
            // NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.damage = 50;
            NPC.defense = 10;
            NPC.lifeMax = 5000;
            NPC.HitSound = hitSound;
            NPC.HitSound = hurtSound;
            NPC.value = 25f;
            NPC.knockBackResist = 0.1f;

            phaseAStatesList = new[]
            {
                WulfrumBossStates.Chase,  
                WulfrumBossStates.Dash,
                WulfrumBossStates.Chase,  
                WulfrumBossStates.Stomp
            };
            phaseBStatesList = new[]
            {
                WulfrumBossStates.Barrage,
                WulfrumBossStates.Missile,
                WulfrumBossStates.Chase,
                WulfrumBossStates.Dash,
                WulfrumBossStates.Missile,
                WulfrumBossStates.Stomp
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

        public override void OnSpawn(IEntitySource source)
        {
            gfxOffY = 0;
            stateIndex = -1;
            SetNewState();

            Main.NewText("i exist.");
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
            Vector2 dir = targetPlayer.position - laserBarrelPos;
            Vector2 imagePos = laserBarrelPos - screenPos;
            float rotBarrelToTarget = (float)Math.Atan2(dir.Y, dir.X);

            spriteBatch.Draw((Texture2D)TextureAssets.DukeFishron, imagePos, null, Color.White, rotBarrelToTarget, imagePos, 1, SpriteEffects.None, 0);
            return true;
        }

        public void FacePlayer()
        {
            NPC.spriteDirection = dirToTarget.X > 0 ? -1 : 1;
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
            if(timer % currentFireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FireLaser();
            }

            // Fires a rolling bomb if this tick aligns with their fire rates
            if(timer % currentRollFireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FireBomb();
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
                Main.NewText("Im angry now >:(");
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
            Vector2 targetPos = Main.player[NPC.target].position;
            Vector2 targetDir = Vector2.Normalize(targetPos - NPC.position);
            
            float xDirectionFactor = targetDir.X >= 0 ? 1 : -1;
            float deacceleration = xDirectionFactor == -(vel.X / Math.Abs(vel.X)) ? deaccelerationFactor : 0;

            NPC.velocity.X = Math.Clamp(vel.X + (accelerationFactor + deacceleration) * xDirectionFactor, -maxSpeed, maxSpeed);
            StepUpGroundedMovement();     

            if(grounded && NPC.collideX){
                Jump();
                Main.NewText("Jumping!");
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
                    float velX = Math.Clamp(Main.player[NPC.target].position.X - NPC.position.X, -maxXVelMissile, maxXVelMissile) + Random.Shared.Next(-3, 3);
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
            facePlayer = true;
        }

         // ------------------------ STOMP STATE --------------------------

        public void StompStart()
        {
            NPC.noTileCollide = true;
            Main.NewText("Hovering over player (Prepare for a stomp)");
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

                Vector2 targetStompPos = targetPlayer.position + targetPlayer.velocity * stompSpeed * 10f;

                float XDif = Math.Abs(targetStompPos.X - NPC.position.X);
                float YDif = Math.Abs(targetStompPos.Y - NPC.position.Y);
                float factor = XDif / YDif;


                if(factor > maxStompXFactor)
                {
                    targetStompPos.X = NPC.position.X + (YDif * maxStompXFactor * flipFactor);
                }

                activeStompVel = targetStompPos - NPC.position;
                if(activeStompVel.Y < 0) {activeStompVel *= -1;}
                activeStompVel = Vector2.Normalize(activeStompVel) * stompSpeed;

                NPC.velocity = activeStompVel;
                NPC.rotation = Math.Clamp( -NPC.velocity.X , -30, 30);
                Main.NewText("Stomping!");
            }
            else 
            {
                float Kp = 0.07f;
                Vector2 currentPos = NPC.position;
                Vector2 targetPos = targetPlayer.position + stompHoverOffset;
                Vector2 newPos = Vector2.Lerp(currentPos, targetPos, Kp);

                NPC.position = newPos;
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
            return targetPlayer.position.Y >= NPC.position.Y && currentState != WulfrumBossStates.Stomp;
        }
    }
}