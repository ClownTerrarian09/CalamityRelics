using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Buffs.SummonWeapons;
using CalamityRelics.Content.Projectiles.Friendly;

namespace CalamityRelics.Content.Projectiles.Summon
{
    public class WulfrumOrbProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true; 
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; 
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 88;
            Projectile.height = 84;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true; 
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = .5f; 
            Projectile.penetrate = -1;
            
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20; 
        }

        public override bool? CanCutTiles() 
        {
            return false; 
        }

        public override bool MinionContactDamage()
        {
            return true; 
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner]; 

            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<WulfrumOrbBuff>());
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<WulfrumOrbBuff>())) 
            {
                Projectile.timeLeft = 2; 
            }

            AIGeneral(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
            AISearchForTarget(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            AIMovement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
            if (foundTarget) 
            {
                Projectile.spriteDirection = (targetCenter.X > Projectile.Center.X) ? 1 : -1;
            } 
            else 
            {
                Projectile.spriteDirection = owner.direction;
            }
        }

        private void AIGeneral(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
        {
            Vector2 idlePosition = owner.Center;
            idlePosition.Y -= 48f; 

            float minionPositionOffset = (10 + Projectile.minionPos * 40) * -owner.direction; 
            idlePosition.X += minionPositionOffset;

            vectorToIdlePosition = idlePosition - Projectile.Center;
            distanceToIdlePosition = vectorToIdlePosition.Length();

            if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f)
            {
                Projectile.position = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            float overlapVelocity = 0.04f;

            for (int i = 0; i < Main.maxProjectiles; i++) 
            {
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;

                    if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }
        }
   
        private void AISearchForTarget(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = 700f;
            targetCenter = Projectile.position;
            foundTarget = false;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                if (between < 1200f)
                {
                    distanceFromTarget = between; 
                    targetCenter = npc.Center;
                    foundTarget = true; 
                }
            }

            if (!foundTarget)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        bool closeThroughWall = between < 100f;

                        if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
        }

        private void AIMovement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            float TileGap = 80f; 

            if (foundTarget)
            {
                if (distanceFromTarget <= TileGap)
                {
                    
                    // 1. Close range
                    
                    // Rapidly decelerate to a stop
                    Projectile.velocity *= 0.1f; 

                    // Increment close-range attack timer
                    Projectile.ai[0]++;
                    int blastFireRate = 10; // slow down when balancing

                    if (Projectile.ai[0] >= blastFireRate)
                    {
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(), 
                                Projectile.Center, 
                                Vector2.Zero,
                                ModContent.ProjectileType<WulfrumOrbProjectileSecondary>(), 
                                Projectile.damage, 
                                Projectile.knockBack, 
                                Projectile.owner,
                                Projectile.whoAmI
                            );
                        }
                        Projectile.ai[0] = 0; 
                    }
                }
                else
                {
                    // 2. RANGED MODE (Hover & Shoot Primary)
                    
                    // Move towards the target
                    float hoverSpeed = 3f; 
                    float hoverInertia = 40f; 

                    Vector2 direction = (targetCenter - Projectile.Center).SafeNormalize(Vector2.Zero) * hoverSpeed;
                    Projectile.velocity = (Projectile.velocity * (hoverInertia - 1) + direction) / hoverInertia;
                    
                    Projectile.ai[1]++;
                    int primaryFireRate = 30; 

                    if (Projectile.ai[1] >= primaryFireRate)
                    {
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Vector2 shootVelocity = (targetCenter - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                            
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(), 
                                Projectile.Center, 
                                shootVelocity, 
                                ProjectileID.PurpleLaser, 
                                Projectile.damage, 
                                Projectile.knockBack, 
                                Projectile.owner
                            );
                        }
                        Projectile.ai[1] = 0; 
                    }
                }
                return; 
            }
            // 3. Idle (Return to Player)
            Projectile.ai[0] = 0;
            Projectile.ai[1] = 0;

            float speed = distanceToIdlePosition > 600f ? 17f : 4f;
            float inertia = distanceToIdlePosition > 600f ? 20f : 40f;

            if (distanceToIdlePosition > 20f)
            {
                vectorToIdlePosition = vectorToIdlePosition.SafeNormalize(Vector2.Zero) * speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
            } 
            else if (Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity.X = -0.15f;
                Projectile.velocity.Y = -0.05f;
            }
        }
    }
}