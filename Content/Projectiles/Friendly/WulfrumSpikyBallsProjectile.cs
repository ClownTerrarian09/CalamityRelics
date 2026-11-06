using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityRelics.Content.Buffs;
using CalamityRelics.Content.GlobalNPCs;
using Terraria.Audio;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class WulfrumSpikyBallsProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 38; 
            Projectile.height = 38; 
            Projectile.friendly = true; 
            Projectile.hostile = false; 
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            
            Projectile.penetrate = 5; 
            
            Projectile.timeLeft = 600; 
            
            Projectile.aiStyle = ProjAIStyleID.GroundProjectile; 
            AIType = ProjectileID.SpikyBall;
			
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }
        public override void PostAI()
        {
            if (Main.rand.NextBool(3) && Projectile.Calamity().stealthStrike)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center, 5, 5, DustID.Electric, 0f, 0f, 0, default(Color), 1f);
				dust.noGravity = true;
			}
		}
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
	            target.AddBuff(ModContent.BuffType<ConductiveBuff>(), 60);
	            
	            var globalNPC = target.GetGlobalNPC<GlobalConductive>();
	            globalNPC.debuffOwner = Projectile.owner;
	            globalNPC.debuffDamage = Projectile.damage / 2;
				bool isStealth = Projectile.Calamity().stealthStrike;

            	if (isStealth)
            	{
		            Vector2 spawnPosition = target.Center - new Vector2(Main.rand.NextFloat(-50f, 50f), 800f);
		            
		            Vector2 direction = target.Center - spawnPosition;
		            direction.Normalize();
		            float speed = 80f; 
		            Vector2 velocity = direction * speed;
		            
		            int lightningDamage = Projectile.damage;

		            // int projIndex = Projectile.NewProjectile(
			        //     Projectile.GetSource_FromThis(),
			        //     spawnPosition,
			        //     velocity,
			        //     ProjectileID.VortexLightning, 
			        //     lightningDamage,
			        //     0f, 
			        //     Projectile.owner,
			        //     velocity.ToRotation(), // ai[0]: The target angle in radians for vanilla lightning
			        //     Main.rand.Next(100)    // ai[1]: A random seed to generate the lightning zig-zag
		            // );
		            
		            // Main.projectile[projIndex].friendly = true;
		            // Main.projectile[projIndex].hostile = false;
					int projIndex = Projectile.NewProjectile(
			            Projectile.GetSource_FromThis(),
			            spawnPosition,
			            velocity,
			            ModContent.ProjectileType<LightningBolt>(), 
			            lightningDamage,
			            0f, 
			            Projectile.owner
			            // velocity.ToRotation(), // ai[0]: The target angle in radians for vanilla lightning
			            // Main.rand.Next(100)    // ai[1]: A random seed to generate the lightning zig-zag
		            );
					SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ArcFlash"), spawnPosition);
                
               
		            // Main.projectile[projIndex].DamageType = ModContent.GetInstance<RogueDamageClass>(); 
	            }
	        }
	    }     
	}
}
