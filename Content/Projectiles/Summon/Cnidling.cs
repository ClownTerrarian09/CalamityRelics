using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Buffs.SummonWeapons;
using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace CalamityRelics.Content.Projectiles.Summon
{
    public class Cnidling : ModProjectile
    {
        // 
        private SoundStyle shootSound = new("CalamityMod/Sounds/Item/CursedDaggerThrow"){Volume = 0.3f};
        private SoundStyle hitSound = new("CalamityMod/Sounds/NPCHit/PolterghastHit");
        
        private int targetWhoAmI = -1;

        private bool lastSpinRight = true;
        private Vector2 squashScale = Vector2.One;
        private float squashPulse;

        private enum State
        {
            Charging,
            Shooting,
            Spin,
            Protect
        }

        private State AIState
        {
            get => (State)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        private float StateTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        private float AttackTimer
        {
            get => Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true; 
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; 
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 36;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true; 
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = .25f; 
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
            return AIState != State.Shooting; 
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(hitSound, Projectile.position);
            squashPulse = 0.6f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (AIState == State.Charging)
            {
                Projectile.velocity = -oldVelocity * 0.5f;
                squashPulse = 1f;
            }
            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = (int)Projectile.ai[0];
        }

        public override void AI()
        {
            Projectile.frame = (int)AIState;
            Player owner = Main.player[Projectile.owner]; 

            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CnidlingBuff>());
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<CnidlingBuff>())) 
            {
                Projectile.timeLeft = 2; 
            }

            AIGeneral(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
            AISearchForTarget(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            
            AIMovement(owner, foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
            if (foundTarget) 
            {
                Projectile.spriteDirection = (targetCenter.X > Projectile.Center.X) ? 1 : -1;
            } 
            else 
            {
                Projectile.spriteDirection = owner.direction;
            }

            UpdateSquashStretch();
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
            targetWhoAmI = -1;
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
                            targetWhoAmI = npc.whoAmI;
                        }
                    }
                }
            }
        }

        private void AIMovement(Player owner, bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            if (foundTarget)
            {
                switch (AIState)
                {
                    case State.Charging:
                    {
                        
                        float chargeSpeed = 16f; 
                        float chargeInertia = 15f; 

                        Vector2 direction = targetCenter - Projectile.Center;
                        direction.Normalize();
                        direction *= chargeSpeed;
                        AttackTimer++;
                        if (AttackTimer >= 30)
                        {
                            if (Vector2.Distance(targetCenter, Projectile.Center) > 100f)
                            {
                                Projectile.velocity = (Projectile.velocity * (chargeInertia - 1) + direction) / chargeInertia;
                            }
                            else
                            {
                                Projectile.velocity = direction * 1.6f;
                                AttackTimer = 0;
                                squashPulse = 0.8f;
                            }
                            
                        }
                        else
                            Projectile.velocity *= 0.92f;
                        

                        Projectile.tileCollide = Projectile.velocity.Length() > 8f;
                        
                        break;
                    }
                    case State.Shooting:
                    {

                        float speed = distanceToIdlePosition > 200f ? 10f : 4f;
                        float inertia = distanceToIdlePosition > 200f ? 20f : 40f;

                        if (distanceToIdlePosition > 20f)
                        {
                            vectorToIdlePosition.Normalize();
                            vectorToIdlePosition *= speed;
                            Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                        } 
                        else if (Projectile.velocity == Vector2.Zero)
                        {
                            Projectile.velocity.X = -0.15f;
                            Projectile.velocity.Y = -0.05f;
                        }
                        
                        Projectile.ai[2]++;

                        if (Projectile.ai[2] >= 40)
                        {
                            if (Main.myPlayer == Projectile.owner)
                            {
                                Vector2 shootVelocity = (targetCenter - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                                Projectile.velocity -= shootVelocity * 0.5f;
                                Projectile.rotation -= MathHelper.ToRadians(shootVelocity.X * 2.5f);
                                SoundEngine.PlaySound(shootSound, Projectile.position);
                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(), 
                                    Projectile.Center, 
                                    shootVelocity, 
                                    ModContent.ProjectileType<CnidarianMist>(),
                                    Projectile.damage, 
                                    Projectile.knockBack, 
                                    Projectile.owner
                                );
                            }
                            squashPulse = 1f;
                            Projectile.ai[2] = 0; 
                        }
                        Projectile.rotation = Projectile.rotation.AngleLerp(0, 0.1f);

                        break;
                    }
                    case State.Spin:
                    {
                        
                        float chargeSpeed = 16f; 
                        float chargeInertia = 15f; 

                        Vector2 direction = targetCenter - Projectile.Center;
                        direction.Normalize();
                        direction *= chargeSpeed;
                        if (Vector2.Distance(targetCenter, Projectile.Center) > 20f)
                        {
                            Projectile.velocity = (Projectile.velocity * (chargeInertia - 1) + direction) / chargeInertia;
                            lastSpinRight = Projectile.velocity.X >= 0;
                        }
                        else
                            Projectile.velocity *= 0.95f;

                        float nextAngle = Projectile.rotation.AngleLerp(MathHelper.ToRadians(Projectile.velocity.X * 4), 0.3f);
                        float nextAnglePos = MathF.Max(nextAngle, 4);
                        float nextAngleNeg = MathF.Max(nextAngle, -4);
                        Projectile.rotation = AttackTimer += (lastSpinRight ? nextAnglePos : nextAngleNeg);
                        break;
                    }
                    case State.Protect:
                    {
                        AttackTimer++;
                        Projectile.tileCollide = false;
                        Vector2 idlePos = owner.Center + new Vector2(MathF.Sin(AttackTimer * 0.1f) * 40f, MathF.Cos(AttackTimer * 0.1f) * 40f);
                        float distance = Vector2.Distance(idlePos, Projectile.Center);
                        float speed = distance > 300f ? 12f : 6f;
                        float inertia = distance > 300f ? 20f : 40f;
                        Vector2 spinDir = Vector2.Normalize(idlePos - Projectile.Center);
                        
                        float chargeSpeed = 16f; 
                        float chargeInertia = 15f; 

                        Vector2 direction = targetCenter - Projectile.Center;
                        direction.Normalize();
                        direction *= chargeSpeed;
                        
                        if (Vector2.Distance(owner.Center, Projectile.Center) < 120f)
                        {
                            Projectile.velocity = (Projectile.velocity * (chargeInertia - 1) + direction) / chargeInertia;
                        }
                        else
                        {
                            spinDir *= speed;
                            Projectile.velocity = (Projectile.velocity * (inertia - 1) + spinDir) / inertia;
                        }
                        
                        break;
                    }
                }
            }
            else
            {
                Projectile.tileCollide = false;
                float speed = distanceToIdlePosition > 600f ? 17f : 4f;
                float inertia = distanceToIdlePosition > 600f ? 20f : 40f;

                if (distanceToIdlePosition > 20f)
                {
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                } 
                else if (Projectile.velocity == Vector2.Zero)
                {
                    Projectile.velocity.X = -0.15f;
                    Projectile.velocity.Y = -0.05f;
                }
                Projectile.rotation = Projectile.rotation.AngleLerp(0, 0.2f);
            }
            

        }
        
        private void UpdateSquashStretch()
        {
            Vector2 target = Vector2.One;
            float speed = Projectile.velocity.Length();

            if (speed > 0.5f)
            {
                Vector2 local = Projectile.velocity.RotatedBy(-Projectile.rotation) / speed;
                float amount = MathHelper.Clamp(speed / 16f, 0f, 1f) * 0.18f;
                float bias = Math.Abs(local.Y) - Math.Abs(local.X);

                target.X -= amount * bias;
                target.Y += amount * bias;
            }
            float pulseStrength = 0.25f;
            target.X += squashPulse * pulseStrength;
            target.Y -= squashPulse * pulseStrength;
            squashPulse *= 0.82f;

            squashScale = Vector2.Lerp(squashScale, target, 0.25f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawOffset = new Vector2(0f, Projectile.gfxOffY) - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            Vector2 scale = squashScale * Projectile.scale;
 
            bool drawTrail = (AIState == State.Charging || AIState == State.Spin) && Projectile.velocity.Length() > 6f;
            if (drawTrail)
            {
                for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;
 
                    float progress = 1f - i / (float)Projectile.oldPos.Length;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f + drawOffset;
                    Color trailColor = drawColor * (progress * 0.5f);
                    Vector2 trailScale = scale * MathHelper.Lerp(0.8f, 1f, progress);
 
                    Main.EntitySpriteDraw(texture, trailPos, frame, trailColor, Projectile.oldRot[i], origin, trailScale, effects);
                }
            }
 
            Main.EntitySpriteDraw(texture, Projectile.Center + drawOffset, frame, drawColor, Projectile.rotation, origin, scale, effects);
            return false;
        }

    }
}