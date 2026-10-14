
using System;
using CalamityRelics.HelperScripts;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Hostile
{

    public class WulfrumSineMissile : ModProjectile
    {

        public float explosionRange = 50;
        public int explosionDmg = 50;
        public float angleFactor = 0.5f;
        public float angleDifMax = 20f;
        public int currentTime = 0;
        public float sinePhaseFactor => 0.1f;
        public int phaseOffset = 0;
        public float startAngle = 0;
        public float speed => 3 + 0.1f * MathF.Pow(currentTime, 1.05f);

        public Asset<Texture2D> particleTexAsset;

        public float currentAngle => startAngle + MathF.Sin(phaseOffset + currentTime * sinePhaseFactor) * float.Clamp(angleFactor * currentTime, -angleDifMax, angleDifMax);
        
        public override void SetStaticDefaults() {
			ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true; // Deals double damage on direct hits.
			ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true; // Damage dealt to players does not scale with difficulty in vanilla.
			ProjectileID.Sets.Explosive[Type] = true;
		}

        public override void Load()
        {
            particleTexAsset = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/WulfrumWavewireProjH"); 
        }

		public override void SetDefaults() {
			Projectile.width = 50;
			Projectile.height = 50;
			Projectile.friendly = false;
			Projectile.penetrate = -1; // Infinite penetration so that the blast can hit all enemies within its radius.
			Projectile.DamageType = DamageClass.Ranged;
            Projectile.scale = 1.2f;
            Projectile.hostile = true;
		}

        public override void AI()
        {
            if(currentTime == 0)
            {
                startAngle = float.RadiansToDegrees(Projectile.velocity.ToRotation());
                phaseOffset = Main.rand.Next(300);
                Main.NewText(startAngle);
            }

            if(currentTime % 5 == 0)
            {
                ParticleSystemManager.Instance.CreateParticle(
                    new BaseParticle(particleTexAsset, Projectile.Center, 45, scale: Vector2.One * 50, drawLocation: IntendedDrawHook.PRE_DRAW)
                );
            }

            Projectile.velocity = new Vector2(
                MathF.Cos(float.DegreesToRadians(currentAngle)),
                MathF.Sin(float.DegreesToRadians(currentAngle))
            ) * speed;

            Projectile.rotation = Projectile.velocity.ToRotation();// + MathHelper.PiOver2;

            if(currentTime > 300)
            {
                DoExplosion();
            }
            currentTime++;

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