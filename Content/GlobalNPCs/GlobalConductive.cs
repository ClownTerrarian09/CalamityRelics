using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod;

namespace CalamityRelics.Content.GlobalNPCs
{
    public class GlobalConductive : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool wulfrumConductiveDebuff;
        public int debuffOwner = 255;
        public int debuffDamage = 10;
        public int shootTimer = 0;

        public override void ResetEffects(NPC npc)
        {
            wulfrumConductiveDebuff = false;
        }

        public override void PostAI(NPC npc)
        {
            if (wulfrumConductiveDebuff)
            {
                shootTimer++;
                
                if (shootTimer >= 30)
                {
                    shootTimer = 0;
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC target = null;
                        float closestDist = 200f; 
                        
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC n = Main.npc[i];
                            if (n.active && !n.friendly && !n.dontTakeDamage && n.CanBeChasedBy() && n.whoAmI != npc.whoAmI)
                            {
                                float dist = Vector2.Distance(npc.Center, n.Center);
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    target = n;
                                }
                            }
                        }
                        
                        if (target != null)
                        {
                            Vector2 velocity = (target.Center - npc.Center).SafeNormalize(Vector2.Zero) * 12f;

                            int projIndex = Projectile.NewProjectile(
                                npc.GetSource_FromAI(),
                                npc.Center,
                                velocity,
                                ProjectileID.Spark, // TODO: Change this to your desired projectile ID
                                debuffDamage, 
                                0f,
                                debuffOwner 
                            );
                            
                            Main.projectile[projIndex].friendly = true;
                            Main.projectile[projIndex].hostile = false;
                            Main.projectile[projIndex].DamageType = ModContent.GetInstance<RogueDamageClass>();
                        }
                    }
                }
            }
            else
            {
                shootTimer = 0; 
            }
        }
    }
}