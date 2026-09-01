using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityRelics.Content.Buffs;
using CalamityRelics.Content.GlobalNPCs;


namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class WulfrumSpikyBallsProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30; 
            Projectile.height = 30; 
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
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
	            target.AddBuff(ModContent.BuffType<ConductiveBuff>(), 240);
	            
	            var globalNPC = target.GetGlobalNPC<GlobalConductive>();
	            globalNPC.debuffOwner = Projectile.owner;
	            globalNPC.debuffDamage = Projectile.damage;
	            bool isStealth = Projectile.Calamity().stealthStrike;

            	if (isStealth)
            	{
		            Vector2 spawnPosition = target.Center - new Vector2(Main.rand.NextFloat(-50f, 50f), 400f);
		            
		            Vector2 direction = target.Center - spawnPosition;
		            direction.Normalize();
		            float speed = 15f; 
		            Vector2 velocity = direction * speed;
		            
		            int lightningDamage = Projectile.damage;

		            int projIndex = Projectile.NewProjectile(
			            Projectile.GetSource_FromThis(),
			            spawnPosition,
			            velocity,
			            ProjectileID.VortexLightning, 
			            lightningDamage,
			            0f, 
			            Projectile.owner,
			            velocity.ToRotation(), // ai[0]: The target angle in radians for vanilla lightning
			            Main.rand.Next(100)    // ai[1]: A random seed to generate the lightning zig-zag
		            );
		            
		            Main.projectile[projIndex].friendly = true;
		            Main.projectile[projIndex].hostile = false;
                
               
		            Main.projectile[projIndex].DamageType = ModContent.GetInstance<RogueDamageClass>(); 
	            }
	        }
	    }     
	}
}
