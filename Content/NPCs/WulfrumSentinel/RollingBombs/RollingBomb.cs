using Terraria;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader.Utilities;

namespace CalamityRelics.Content.NPCs.WulfrumSentinel.RollingBombs{

    public class RollingBomb : ModNPC
    {


        public static float startDashVel => 2f; 
        public static float dashVelMultiplier => 1.01f; 
        public static float maxRollTime => 300; 
        public static float explosionThreshold => 1.5f; 
        public static float explosionRange => 150; 
        public static int explosionDamage => 50; 
        public static int directHitDamage => 100; 


        public Player targetPlayer => Main.player[NPC.target]; 
        public Vector2 dirToTarget => Vector2.Normalize(NPC.DirectionTo(targetPlayer.position)); 
        public float startingDashVel; 
        

        public float stepSpeed = 1; 
        public int timer = 0; 
        public float gfxOffY = 0;   

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4; 
            NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.ShimmerSlime;    
            
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height= 40;
            NPC.aiStyle = -1;
            // NPC.aiStyle = NPCAIStyleID.Fighter;s
            NPC.damage = 100;
            NPC.defense = 100;
            NPC.lifeMax = 9999;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.HitSound = SoundID.NPCDeath1;
            NPC.value = 0f;
            NPC.knockBackResist = 0.1f;
            NPC.dontTakeDamage = true;

            
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.OverworldDaySlime.Chance * 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.TargetClosest(false);
            
            gfxOffY = 0;
            startingDashVel = dirToTarget.X < 0 ? -startDashVel : startDashVel;
            NPC.velocity.X = startingDashVel;
            return;
            
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

        public override void AI()
        {
            timer++;

            StepUpGroundedMovement();

            
            if(timer >= maxRollTime || Math.Abs(NPC.velocity.X) < explosionThreshold)
            {
                Explode();
            }
            else
            {
                NPC.velocity.X *= dashVelMultiplier;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            Explode(false);
        }
        public void Explode(bool damage = true)
        {

            if(damage && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < Main.player.Length; i++)
                {
                    Player player = Main.player[i];
                    if(Vector2.Distance(player.position, NPC.position) <= explosionRange)
                    {
                        int dir = (player.position - NPC.position).X > 0 ? 1 : -1;

                        Player.HurtInfo hurtInfo = new Player.HurtInfo
                        {
                            Damage = explosionDamage,
                            HitDirection = dir,
                            CooldownCounter = ImmunityCooldownID.Bosses,
                            PvP = false,
                            DamageSource = PlayerDeathReason.ByNPC(NPC.whoAmI)
                        };

                        player.Hurt(hurtInfo);
                    }       
                }
            }

            Dust.NewDust(NPC.position, 100, 100, DustID.Smoke);
            
            NPC.HitInfo hitInfo = new NPC.HitInfo { InstantKill = true };
            NPC.StrikeNPC(hitInfo);
            
        }








        
    }
}