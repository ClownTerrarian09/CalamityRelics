
using System;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Hostile
{

    public class WulfrumMissile : ModProjectile
    {

        public static int timeUntilDive = 50;
        public static int diveTime = 50;
        public static float explosionRange => 50;
        public static int explosionDmg => 50;



        public static float startDiveSpeed => 1;
        public static float diveAcceleration => 0.25f;
        public float currentMagnitude;
        public int currentTime = 0;
        public bool diving;

        private Curve curveX;
        private Curve curveY;
        private Vector2 posFinalTarget;


        public override void SetStaticDefaults() {
			ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true; // Deals double damage on direct hits.
			ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true; // Damage dealt to players does not scale with difficulty in vanilla.
			
			// This set handles some things for us already:
			// Sets the timeLeft to 3 and the projectile direction when colliding with an NPC or player in PVP (so the explosive can detonate).
			// Explosives also bounce off the top of Shimmer, detonate with no blast damage when touching the bottom or sides of Shimmer, and damage other players in For the Worthy worlds.
			ProjectileID.Sets.Explosive[Type] = true;
			// ProjectileID.Sets. = true;

			// This set makes it so the rocket doesn't deal damage to players. Only used for vanilla rockets.
			// Simply remove the Projectile.HurtPlayer() part to stop the projectile from damaging its user.
		}
		public override void SetDefaults() {
			Projectile.width = 50;
			Projectile.height = 50;
			Projectile.friendly = false;
			Projectile.penetrate = -1; // Infinite penetration so that the blast can hit all enemies within its radius.
			Projectile.DamageType = DamageClass.Ranged;
            Projectile.scale = 1.2f;
            Projectile.hostile = true;
            

			// Rockets use explosive AI, ProjAIStyleID.Explosive (16). You could use that instead here with the correct AIType.
			// But, using our own AI allows us to customize things like the dusts that the rocket creates.
			// Projectile.aiStyle = ProjAIStyleID.Explosive;
		}

        public override void AI()
        {
            currentTime++;
            
            Projectile.rotation = Projectile.velocity.ToRotation();// + MathHelper.PiOver2;

            if(currentTime >= timeUntilDive)
            {
                if(diving) DivePeriodic();
                else DiveStart();
            }
            else
            {
                Projectile.velocity.Y += 0.1f;
            }
        }

        public void DiveStart()
        {
            int playerIndex = (int)Projectile.ai[0];
            diving = true;

            Vector2 posCurrent = Projectile.position;
            Vector2 velCurrent = Projectile.velocity;
            
            Random random = Random.Shared;

            bool isFirstMissile = Projectile.ai[2] == 1; // If this is the first missile fired, do not apply RNG spread to the target
            Main.NewText(Projectile.ai[2]);
            posFinalTarget = Main.player[playerIndex].Center + Vector2.UnitX * (isFirstMissile ? 0 : random.Next(-250, 250));
            
            Projectile.velocity = Vector2.Normalize(posFinalTarget - posCurrent) * startDiveSpeed;
            currentMagnitude = startDiveSpeed;
        }

        public void DivePeriodic()
        {
            int diveTick = currentTime - timeUntilDive;
            int diveTargetTick = timeUntilDive + diveTime;
            float normalizedTime = (float)diveTick / diveTime;

            
            Vector2 currentDir = Vector2.Normalize(Projectile.velocity);

            currentMagnitude += diveAcceleration;
            Projectile.velocity = currentDir * currentMagnitude;
        }


        public void DoExplosion(Player playerDirectlyHit = null)
        {

            // Player hurt logic, only for valid netmode environments
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Player[] players = Main.player;

                for (int i = 0; i < players.Length; i++)
                {
                    if(playerDirectlyHit == players[i]) continue;
                    
                    float dist = Vector2.Distance(players[i].Center, Projectile.Center);
                    if(dist < explosionRange)
                    {
                        int dir = (players[i].Center - Projectile.Center).X > 0 ? 1 : -1;

                        Player.HurtInfo hurtInfo = new Player.HurtInfo
                        {
                            Damage = explosionDmg,
                            HitDirection = dir,
                            CooldownCounter = ImmunityCooldownID.Bosses,
                            PvP = false,
                            DamageSource = PlayerDeathReason.ByNPC((int)Projectile.ai[1])
                        };

                        players[i].Hurt(hurtInfo);
                    }
                }
            }

            // Effects
            Dust.NewDust(Projectile.position, 100, 100, DustID.TerraBlade);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            DoExplosion();
            Projectile.Kill();
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            DoExplosion(target);
            // Main.NewText("HitPlayer");
            // target.Hurt(info)
            // Projectile.Kill();
        }
    } 

}