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
using CalamityRelics.HelperScripts;

namespace CalamityRelics.Content.NPCs.WulfrumSentinel{

    public class WulfrumSentinelDrone : ModNPC
    {

        public static float laserVelocity => 10f;
        public static int attachThreshold => 10;
        
        public static SoundStyle laserShootSound => SoundID.DD2_BetsyFireballShot;
        public static SoundStyle hitSound => SoundID.NPCHit1;
        public static SoundStyle hurtSound => SoundID.NPCDeath1;


        // I am probably leaving out a built in variable or method that does this already that would be able to replace this later
        public bool flipped => targetPlayer.Center.X - NPC.Center.X < 0;
        public int flipFactor => flipped ? -1 : 1;
        public bool attached = false;
        public bool attaching = false;

        // public WulfrumSentinelDrone drone;

        public Player targetPlayer => Main.player[NPC.target]; 
        public Vector2 dirToTarget => Vector2.Normalize(NPC.DirectionTo(targetPlayer.Center)); 
        public Vector2 launcherBarrelPos => NPC.Center; 

        public Vector2 targetAttachPos => host.NPC.Center + attachedOffset; 
        private static float attachKp => 0.125f; 
        private Vector2 attachedOffset; 
        private WulfrumSentinel host;
        public bool facePlayer = true;
        private static Asset<Texture2D> textureMainBody;
        

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1; 
        }

        public override void SetDefaults()
        {
            NPC.width = 102 * 2;
            NPC.height= 39 * 2;
            NPC.aiStyle = -1;
            // NPC.aiStyle = NPCAIStyleID.Fighter;
            
            // NPC.dontTakeDamage = true;
            // NPC.noTileCollide = true;

            // NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = 5000;
            NPC.HitSound = hitSound;
            NPC.HitSound = hurtSound;
            NPC.value = 25f;
            NPC.knockBackResist = 0.1f;
            NPC.noGravity = true;
            NPC.boss = false;
            NPC.despawnEncouraged = false;
            // NPC.;

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
            attached = false;
            NPC.TargetClosest();

            Main.NewText("Drone exists");
        }

        public override void Load()
        {
            textureMainBody = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/WulfrumSentinelDrone");
            // textureWheelTreadFront = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/FrontWheel");
            // textureWheelTreadBack = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumSentinel/BackWheel");
        }

        // ------------------------- STATE MACHINE --------------------------
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            spriteBatch.Draw(textureMainBody.Value, RectangleHelper.CreateAnchored(RectangleHelper.Anchor.CENTER, NPC.Center - screenPos, NPC.Size), null, Color.White, 0, Vector2.Zero, SpriteEffect(), 0);
            return false;
        }

        public void FacePlayer()
        {
            NPC.spriteDirection = dirToTarget.X > 0 ? -1 : 1;
        }

        public void FireMissile()
        {
            Vector2 velocity = Vector2.Normalize(targetPlayer.Center - launcherBarrelPos) * laserVelocity;
            int newMissileIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), launcherBarrelPos, velocity, ModContent.ProjectileType<WulfrumSineMissile>(), 35, 5, -1, velocity.ToRotation());
            SoundEngine.PlaySound(laserShootSound, launcherBarrelPos);
        }

        public void Detach()
        {
            attached = false;
            attaching = false;
        }

        public void SetHost(WulfrumSentinel wulfrumSentinel)
        {
            host = wulfrumSentinel;
        }

        public void Attach(Vector2 offset)
        {
            attaching = true;
            attachedOffset = offset;
        }

        public void ProportionalMove(Vector2 target, float Kp)
        {
            if(attached || host == null) return;
            Vector2 difference = target - NPC.Center;
            NPC.Center += difference * Kp;

            // Main.NewText("Exists, "+ NPC.Center);
            // NPC.velocity = (target - NPC.Center) * Kp;
            // Main.NewText("moving to target + " + NPC.active);
        }

        public SpriteEffects SpriteEffect()
        {
            return NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        public override void AI()
        {
            if(host == null || Main.netMode == NetmodeID.MultiplayerClient) return;

            if (facePlayer)
            {
                FacePlayer();
            }

            if (attached)
            {
                NPC.Center = targetAttachPos;
            }
            else if (attaching)
            {
                ProportionalMove(targetAttachPos, attachKp);
                if( (targetAttachPos-NPC.Center).Length() <= attachThreshold)
                {
                    attached = true;
                    attaching = false;
                }
            }
        }
    }
}