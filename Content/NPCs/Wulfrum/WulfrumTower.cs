using System;
using CalamityRelics.Content.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.NPCs.NormalNPCs;
using System.Collections;
using System.Collections.Generic;
using CalamityMod.Particles;

namespace CalamityRelics.Content.NPCs.Wulfrum
{
    public class WulfrumTower : ModNPC
    {
        public bool isChargeState;
        private float stateTimer{
                get => NPC.ai[0];
                set => NPC.ai[0] = value;
            }
        private float fireRateTimer{
                get => NPC.ai[1];
                set => NPC.ai[1] = value;
            }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 58;
            NPC.height = 136;
            NPC.knockBackResist = 0f;
            NPC.lifeMax = 1000;
            NPC.damage = 0;
            NPC.noGravity = true;
        }

        public override void AI()
        {
            if (NPC.target == 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active){
                NPC.TargetClosest(true);
            }
            Player player = Main.player[NPC.target];
            if(player.dead || !player.active){
                NPC.EncourageDespawn(10);
                return;
            }
            StateMachine(player);
            
        }

        public void StateMachine(Player player)
        {
            if (isChargeState)
            {
                ChargeState(player, 60f*1.5f, 60f*9f);
            }
            else
            {
                SpawnState(player, 60f*1.1f, 800, 60f*10f);
            }
        }

        public void SpawnState(Player player, float fireRate, int distance, float timer)
        {
            stateTimer ++; fireRateTimer ++;
            if (fireRateTimer > fireRate)
            {
                int tries = 0;
                float randomAngle;
                Vector2 pos;
                do {
                    randomAngle = Main.rand.NextFloat(0, 3.14f);
                    pos = new Vector2(MathF.Cos(randomAngle) * distance, -MathF.Sin(randomAngle) * distance) + player.Center;
                    tries ++;
                } while (Terraria.WorldGen.SolidTile(CalamityMod.CalamityUtils.ToSafeTileCoordinates(pos)) || tries < 100);

                SpawnNPC(pos);
                fireRateTimer = 0;
            }
            if (stateTimer >= timer)
            {
                stateTimer = 0;
                fireRateTimer = 0;
                isChargeState = !isChargeState;
            }
            
        }

        public void SpawnNPC(Vector2 position)
        {
            int randomNPC = Main.rand.Next(4);
                switch (randomNPC)
                {
                    case 0:
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)position.X, (int)position.Y, ModContent.NPCType<WulfrumGyrator>());
                    break;
                    case 1:
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)position.X, (int)position.Y, ModContent.NPCType<WulfrumDrone>());
                    break;
                    case 2:
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)position.X, (int)position.Y, ModContent.NPCType<WulfrumHovercraft>());
                    break;
                    case 3:
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)position.X, (int)position.Y, ModContent.NPCType<WulfrumRover>());
                    break;
                }
        }
        List<int> WulfrumEnemies = [ModContent.NPCType<WulfrumGyrator>(), ModContent.NPCType<WulfrumDrone>(), ModContent.NPCType<WulfrumHovercraft>(), ModContent.NPCType<WulfrumRover>()];
        public void ChargeState(Player player, float fireRate, float timer)
        {
            fireRateTimer ++;
            NPC.SuperArmor = true;
            stateTimer ++;

            if (fireRateTimer >= fireRate)
            {
                Particle pulseRing = new CustomPulse(NPC.Center, Vector2.Zero, new Color(0, 255, 249), "CalamityMod/Particles/HighResHollowCircleHardEdge", 
                Vector2.One, 0, 0f, 1.5f, 80);
                GeneralParticleHandler.SpawnParticle(pulseRing);
                foreach (NPC npcType in Main.ActiveNPCs)
                    {
                        if (WulfrumEnemies.Contains(npcType.type) && NPC.Distance(npcType.Center) < 800f)
                        {
                            npcType.ai[3] = 360;
                        }
                    }
                fireRateTimer = 0;
            }
            
            if (stateTimer >= timer)
            {
                stateTimer = 0;
                fireRateTimer = 0;
                NPC.SuperArmor = false;
                isChargeState = !isChargeState;
            }
        }
    }
}