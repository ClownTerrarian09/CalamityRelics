using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityRelics.Content.Items;
using SteelSeries.GameSense.DeviceZone;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.NPCs.Bosses.Cnidrion
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class Cnidrion : ModNPC
	{
        Player player;

        int timere;

        SpriteEffects effect = SpriteEffects.None;
        SpriteEffects effectB = SpriteEffects.None;

        int effectDir;

        int frame = 0;
        int frameCounter = 0;
        int frameB = 6;
        int frameCounterB = 0;
        int frameC = 5;
        int frameCounterC = 0;

        float rawHeadRot = -115;
        float headRot;

        Vector2 playerPos;

        Vector2 actualOffset = new Vector2(-15, 0);

        float bodyPosOffset = 0;
        float bodyRotOffset = 0;

        int idleSpeed = 50;

        float shieldOpacity = 0;
        int shieldHeath = 10;

        int state = 0;
        int stateVar1 = 0;

        int dir = -1;

        float waveSize = 1;
        float waveOpacity = 0;

        Vector2 mouthPos;

        bool allPlayersDead;

        float waterVortexOpacity = 0;

        bool phasingThroughTiles = false;
        int phasingThroughTilesTime = 0;
        float phasingThroughTilesSpeed = 0;

        float CnidrionOpacity = 1;
        public override void SetStaticDefaults()
        {
			Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 3000;
            NPC.width = 86;
            NPC.height = 170;
            NPC.noTileCollide = false;
            NPC.knockBackResist = 0;
            NPC.defense = 16;
            NPC.ShowNameOnHover = false;
            NPC.takenDamageMultiplier = 0;

            Music = MusicLoader.GetMusicSlot(Mod, "Music/PetalDance");
        }

        public void animation()
        {
            frameCounter++;

            if (frameCounter == idleSpeed)
            {
                frame++;
                frameCounter = 0;

                if (frame == 4)
                {
                    frame = 0;
                }
            }

            // - - - - -

            if (Math.Abs(NPC.velocity.X) > 0.1f)
            {
                frameCounterB++;

                if (frameCounterB == 8)
                {
                    frameCounterB = 0;
                    frameB++;

                    if (frameB >= 5)
                    {
                        frameB = 0;
                    }
                }
            }
            else
            {
                frameB = 5;
            }
        }

        public void phaseTiles()
        {
            //NPC.noTileCollide = !phasingThroughTiles;

            if (NPC.collideX)
            {
                phasingThroughTiles = true;
            }

            if (phasingThroughTiles)
            {
                phasingThroughTilesTime++;

                NPC.noTileCollide = true;

                if (phasingThroughTilesSpeed < 4)
                {
                    phasingThroughTilesSpeed += 0.2f;
                }

                NPC.position.Y -= phasingThroughTilesSpeed;

                NPC.noGravity = true;

                if (!Collision.SolidCollision(NPC.position, NPC.width, NPC.height) && phasingThroughTilesTime > 10)
                {
                    phasingThroughTilesTime = 0;
                    phasingThroughTiles = false;
                    phasingThroughTilesSpeed = 0;
                    NPC.noGravity = true;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;
                }
            }
        }
        public void misc()
        {
            if (state != 4)
            {
                if (shieldOpacity > 0.02f)
                {
                    shieldOpacity -= 0.02f;
                }
            }

            if (dir == -1)
            {
                effectDir = 0;
                effect = SpriteEffects.None;
                effectB = SpriteEffects.None;
            }
            else
            {
                effectDir = 1;
                effect = SpriteEffects.FlipHorizontally;
                effectB = SpriteEffects.FlipVertically;
            }

            headRot = MathHelper.ToRadians((-rawHeadRot * dir) + 90);

            if (state != 2)
            {
                bodyRotOffset += ((NPC.velocity.X * 5f) - bodyRotOffset) / 10f;
            }

            bodyPosOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 5;

            mouthPos = actualOffset + NPC.Center - new Vector2(33 - ((25 + 66) * effectDir), 64 + bodyPosOffset).RotatedBy(MathHelper.ToRadians(bodyRotOffset)) + new Vector2(-30, 25 * -dir).RotatedBy(headRot);

            if (state > 1)
            {
                if (player.Center.X > NPC.Center.X)
                {
                    dir = 1;
                }
                else
                {
                    dir = -1;
                }

                allPlayersDead = true;

                foreach (Player player in Main.ActivePlayers)
                {
                    if (!player.dead)
                    {
                        allPlayersDead = false;
                        break;
                    }
                }

                if (allPlayersDead)
                {
                    if (state != -1)
                    {
                        state = -1;
                        timere = 0;
                        NPC.velocity.Y = 0;
                    }
                }
            }
        }















        public void bossAI()
        {
            timere++;

            if (state == -2)
            {
                NPC.velocity.X *= 0.94f;

                if (timere == 1)
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/CnidrionRoar"), NPC.Center);
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 8f, 12, 140, 1000f));
                    frameC = 3;
                }

                NPC.damage = 0;

                if (timere < 200)
                {
                   rawHeadRot += (-30 - rawHeadRot) / 10;
                }

                if (timere > 200)
                {
                    frameC = 4;
                    rawHeadRot += (-115 - rawHeadRot) / 25;
                }

                if (timere > 210)
                {
                    frameC = 1;
                }

                if (timere > 550)
                {
                    frameC = 0;
                    rawHeadRot += (-85 - rawHeadRot) / 20;
                }

                if (timere > 625 && timere < 725)
                {
                    frameC = 1;
                    rawHeadRot += (-145 - rawHeadRot) / 80;
                    bodyRotOffset += ((90 * dir) - bodyRotOffset) / 80;
                }

                if (timere > 725 && timere < 750)
                {
                    frameC = 0;
                    rawHeadRot += (-90 - rawHeadRot) / 80;
                    bodyRotOffset += (0 - bodyRotOffset) / 80;
                }

                if (timere > 800 && timere < 900)
                {
                    rawHeadRot += (-40 - rawHeadRot) / 80;
                }

                if (timere == 880)
                {
                    rawHeadRot = -35;
                    Item.NewItem(NPC.GetSource_FromThis(), mouthPos, 0, 0, ModContent.ItemType<CaudalGeode>(), 1, false, 0, false);
                }

                if (timere > 1000)
                {
                    rawHeadRot += (-90 - rawHeadRot) / 80;
                }

                if (timere == 1150)
                {
                    timere = 0;
                    stateVar1 = 0;
                    state = -1;
                }
            }

            if (state == -1)
            {
                NPC.velocity.X *= 0.94f;

                rawHeadRot += (-135 - rawHeadRot) / 25;

                NPC.dontTakeDamage = true;
                NPC.noTileCollide = true;
                NPC.noGravity = true;

                CnidrionOpacity -= 0.05f;



                NPC.position.Y += 2;

                if (timere > 200)
                {
                    NPC.active = false;
                }
            }

            if (state == 1)
            {
                NPC.damage = 30;

                idleSpeed = 12;
                if (timere == 1)
                {
                    NPC.HitSound = new Terraria.Audio.SoundStyle("CalamityRelics/Assets/Sounds/NPC/CnidrionHit");
                    NPC.dontTakeDamage = true;
                    NPC.takenDamageMultiplier = 1;
                }

                if (timere == 100)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 3f, 10, 15, 1000f));
                    frameC = 3;
                }
                if (timere == 110)
                {
                    frameC = 4;
                }
                if (timere == 120)
                {
                    frameC = 0;
                }

                if (timere > 220 && timere < 280)
                {
                    rawHeadRot += (-95 - rawHeadRot) / 20;
                }

                if (timere == 380)
                {
                    BossTitlecard titlecard = ModContent.GetInstance<BossTitlecard>();
                    titlecard.timere = 0;
                    titlecard.name = "Cnidrion";
                    titlecard.title = "Dormant Destrier";
                    titlecard.fontColorA = new Color(0, 221, 255);
                    titlecard.fontColorB = new Color(171, 210, 255);
                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/CnidrionRoar"), NPC.Center);
                    NPC.boss = true;
                    NPC.takenDamageMultiplier = 1;
                    NPC.ShowNameOnHover = true;
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 8f, 12, 140, 1000f));
                }

                if (timere > 380 && timere < 500)
                {
                    rawHeadRot += (-30 - rawHeadRot) / 3;
                    frameC = 3;

                    if (timere % 20 == 1)
                    {
                        waveOpacity = 1;
                        waveSize = 0;
                    }
                }

                if (timere == 500)
                {
                    NPC.dontTakeDamage = false;
                }

                if (timere > 500)
                {
                    rawHeadRot += (-90 - rawHeadRot) / 15;
                    frameC = 0;
                }

                if (timere == 550)
                {
                    state = 2;
                    timere = 0;
                }
            }

            if (state == 2)
            {
                idleSpeed = 12;

                rawHeadRot += (-15 - rawHeadRot) / 15;

                if (Math.Abs(NPC.velocity.X) < 2)
                {
                    if (dir > 0)
                    {
                        NPC.velocity.X += 0.02f;
                    }
                    else
                    {
                        NPC.velocity.X -= 0.02f;
                    }
                }

                if (timere > 40)
                {
                    if (timere % 5 == 1)
                    {
                        Dust.NewDustPerfect(mouthPos, ModContent.DustType<WaterBoltSteam>(), new Vector2(0, -Main.rand.Next(0, 2)));
                    }

                    if (timere % 60 == 1)
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/CnidrionShoot"), NPC.Center);
                        for (int i = 0; i < 5; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), mouthPos + new Vector2(Main.rand.Next(-4, 4), Main.rand.Next(-4, 4)), (headRot + MathHelper.ToRadians(180 + Main.rand.Next(-20, 20))).ToRotationVector2() * 10, ModContent.ProjectileType<WaterBolt>(), 5, 2, -1);
                            rawHeadRot = -5 * dir;
                        }

                        stateVar1++;
                        if (stateVar1 == 8)
                        {
                            stateVar1 = 0;
                            state = 3;
                            timere = 0;
                        }
                    }
                }
            }

            if (state == 3)
            {
                if (timere < 300)
                {
                    NPC.velocity.X *= 0.95f;

                    rawHeadRot += (-80 - rawHeadRot) / 15;

                    if (timere > 50)
                    {
                        if (timere % 5 == 1)
                        {
                            Dust.NewDustPerfect(mouthPos, ModContent.DustType<WaterBoltSteam>(), new Vector2(0, -Main.rand.Next(0, 2)));
                        }
                    }

                    if (timere > 80)
                    {
                        if (timere % 15 == 1)
                        {
                            SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/CnidrionShoot"), NPC.Center);
                        }

                        if (timere % 10 == 1)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), mouthPos + new Vector2(Main.rand.Next(-4, 4), Main.rand.Next(-4, 4)), (headRot + MathHelper.ToRadians(180 + Main.rand.Next(-10, 10))).ToRotationVector2() * ((timere - 80) / 15), ModContent.ProjectileType<WaterBolt>(), 5, 2, -1);
                        }
                    }
                }
                else
                {
                    rawHeadRot += (-90 - rawHeadRot) / 15;
                }

                if (timere > 350)
                {
                    frameC = 1;
                }
                if (timere > 360)
                {
                    frameC = 2;
                }
                if (timere == 390)
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/CnidrionShoot"), NPC.Center);
                    for (int i = 0; i < 6; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), mouthPos, (headRot + MathHelper.ToRadians(180)).ToRotationVector2() * (5 + (5 * i)) , ModContent.ProjectileType<CnidrionBubble>(), 5, 2, -1);
                    }
                }
                if (timere > 390)
                {
                    frameC = 3;
                }
                if (timere > 400)
                {
                    frameC = 4;
                }

                if (timere == 410)
                {
                    stateVar1 = dir;
                }

                if (timere > 410 && timere < 470)
                {
                    frameC = 0;
                    NPC.velocity.X = stateVar1 * -1;
                }

                if (timere > 470 && timere < 520)
                {
                    if (Math.Abs(NPC.velocity.X) < 9)
                    {
                        NPC.velocity.X += 0.3f * dir;
                    }
                }

                if (timere > 600)
                {
                    NPC.velocity.X *= 0.94f;
                }

                if (timere == 670)
                {
                    if (NPC.life < NPC.lifeMax / 2)
                    {
                        state = 4;
                    }
                    else
                    {
                        state = 2;
                    }
                    stateVar1 = 0;
                    timere = 0;
                }
            }

            if (state == 4)
            {
                NPC.dontTakeDamage = true;

                NPC.velocity.X *= 0.94f;

                if (timere > 25)
                {
                    rawHeadRot += (-125 - rawHeadRot) / 25;
                }

                if (timere > 50 && timere < 200)
                {
                    if (shieldOpacity < 1.02f)
                    {
                        shieldOpacity += 0.02f;
                    }
                    Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-100, 100)), ModContent.DustType<ShieldParticle>(), new Vector2(Main.rand.Next(-3, 3), -3), 0);
                }

                if (timere == 50)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 2f, 12, 50, 1000f));
                }

                if (timere < 300)
                {
                    if (timere > 75)
                    {
                        if (waterVortexOpacity < 1.05f)
                        {
                            waterVortexOpacity += 0.05f;
                        }
                    }
                    if (timere == 100)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie104, NPC.Center);
                        Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 10f, 15, 400, 1000f));
                    }
                    if (timere > 100)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - new Vector2(0, 100) + new Vector2(Main.rand.Next(-25, 25), 0), new Vector2((Main.rand.Next(-500, 500) / 100), -15), ModContent.ProjectileType<WaterBolt>(), 5, 2, -1);
                    }
                }
                if (timere > 400)
                {
                    if (waterVortexOpacity > -0.05f)
                    {
                        waterVortexOpacity -= 0.05f;
                    }
                    if (shieldOpacity > -0.02f)
                    {
                        shieldOpacity -= 0.02f;
                    }
                    rawHeadRot += (-90 - rawHeadRot) / 35;
                }
                if (timere > 475)
                {
                    state = 2;
                    stateVar1 = 0;
                    timere = 0;
                    NPC.dontTakeDamage = false;
                }
            }
        }






























        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            player = Main.player[NPC.target];

            animation();

            misc();

            bossAI();

            if (state > 0)
            {
                phaseTiles();
            }

            float stepSpeed = 0f;

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref stepSpeed, ref NPC.gfxOffY);
        }

        public override bool CheckDead()
        {
            if (NPC.life <= 0)
            {
                NPC.life = 1;

                state = -2;
                timere = 0;
                stateVar1 = 0;
                NPC.dontTakeDamage = true;

                return false;
            }

            return true;
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (state == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-100, 100), Main.rand.Next(-100, 100)), ModContent.DustType<ShieldParticle>(), new Vector2(0, -1), 0);
                }

                NPC.life = NPC.lifeMax;
                shieldOpacity = 1.5f;
                SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/ShieldCrach"), NPC.Center);
                shieldHeath--;

                if (shieldHeath < 0)
                {
                    for (int i = 0; i < 35; i++)
                    {
                        Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-100, 100)), ModContent.DustType<ShieldParticle>(), new Vector2(Main.rand.Next(-3, 3), -3), 0);
                    }

                    timere = 0;
                    shieldOpacity = 0;
                    state = 1;
                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/ShieldBreak"), NPC.Center);
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 8f, 15, 25, 1000f));
                }
            }
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (state == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-100, 100), Main.rand.Next(-100, 100)), ModContent.DustType<ShieldParticle>(), new Vector2(0, -1), 0);
                }

                NPC.life = NPC.lifeMax;
                shieldOpacity = 1.5f;
                SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/ShieldCrach"), NPC.Center);
                shieldHeath--;

                if (shieldHeath < 0)
                {
                    for (int i = 0; i < 35; i++)
                    {
                        Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-100, 100)), ModContent.DustType<ShieldParticle>(), new Vector2(Main.rand.Next(-3, 3), -3), 0);
                    }

                    timere = 0;
                    shieldOpacity = 0;
                    state = 1;
                    SoundEngine.PlaySound(new SoundStyle("CalamityRelics/Assets/Sounds/NPC/ShieldBreak"), NPC.Center);
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 8f, 15, 45, 1000f));
                }
            }
        }

        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D shieldText = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/Shield").Value;
            Texture2D waveText = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/ChromaticBurst").Value;
            Texture2D vortexText = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/WaterVortex").Value;

            Texture2D bodyText = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/Cnidrion").Value;
            Rectangle bodyRect = new Rectangle(0, (170 * frame), 172, 170);

            Texture2D legFText = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/CnidrionLegFront").Value;
            Texture2D legBText = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/CnidrionLegBack").Value;
            Rectangle legsRect = new Rectangle(0, (170 * frameB), 172, 170);

            Texture2D headText = ModContent.Request<Texture2D>("CalamityRelics/Content/NPCs/Bosses/Cnidrion/CnidrionHead").Value;
            Rectangle headRect = new Rectangle(0, (52 * frameC), 70, 52);

            spriteBatch.Draw(legBText, actualOffset + NPC.Center - Main.screenPosition, legsRect, drawColor, NPC.rotation, new Vector2(68, 85), 1f, effect, 1);

            spriteBatch.Draw(bodyText, actualOffset + NPC.Center - Main.screenPosition - new Vector2(0, bodyPosOffset).RotatedBy(MathHelper.ToRadians(bodyRotOffset)), bodyRect, drawColor, NPC.rotation + MathHelper.ToRadians(bodyRotOffset), new Vector2(68, 85), 1f, effect, 1);

            spriteBatch.Draw(legFText, actualOffset + NPC.Center - Main.screenPosition, legsRect, drawColor, NPC.rotation, new Vector2(68, 85), 1f, effect, 1);

            spriteBatch.Draw(headText, actualOffset + NPC.Center - Main.screenPosition - new Vector2(33 - ((25 + 66) * effectDir), 59 + bodyPosOffset).RotatedBy(MathHelper.ToRadians(bodyRotOffset)), headRect, drawColor, NPC.rotation + headRot, new Vector2(35, 26), 1f, effectB, 1);

            spriteBatch.Draw(shieldText, actualOffset + NPC.Center - Main.screenPosition + new Vector2(5, 0), null, new Color(28, 224, 255, 0) * (0.7f * shieldOpacity), MathHelper.ToRadians(Main.GlobalTimeWrappedHourly * 15), shieldText.Size() / 2, 0.35f, SpriteEffects.None, 1);
            spriteBatch.Draw(shieldText, actualOffset + NPC.Center - Main.screenPosition + new Vector2(5, 0), null, new Color(28, 224, 255, 0) * (0.7f * shieldOpacity), MathHelper.ToRadians(Main.GlobalTimeWrappedHourly * -15), shieldText.Size() / 2, 0.35f, SpriteEffects.FlipHorizontally, 1);

            if (waveOpacity > 0)
            {
                waveOpacity -= 0.04f;
                waveSize += 0.06f;
                spriteBatch.Draw(waveText, actualOffset + NPC.Center - Main.screenPosition - new Vector2(35 - ((25 + 65) * effectDir), 64 + bodyPosOffset).RotatedBy(MathHelper.ToRadians(bodyRotOffset)), null, new Color(255, 255, 255, 0) * waveOpacity, 0, waveText.Size() / 2, new Vector2(0.7f, 0.3f) * waveSize, SpriteEffects.None, 1);
            }

            if (waterVortexOpacity > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    spriteBatch.Draw(vortexText, NPC.Center - Main.screenPosition - new Vector2(0, 100), null, new Color(158, 246, 255, 0) * (0.7f * waterVortexOpacity), MathHelper.ToRadians(Main.GlobalTimeWrappedHourly * 120 + (i * 120)), vortexText.Size() / 2, 0.5f, SpriteEffects.None, 1);
                }

                Dust.NewDustPerfect(NPC.Center - new Vector2(0, 100) + new Vector2(Main.rand.Next(-50, 50), 0), ModContent.DustType<ShieldParticle>(), new Vector2(0, -1), 0);
                spriteBatch.Draw(vortexText, NPC.Center - Main.screenPosition - new Vector2(0, 100), null, new Color(28, 224, 255, 0) * (0.7f * waterVortexOpacity), MathHelper.ToRadians(Main.GlobalTimeWrappedHourly * 60), vortexText.Size() / 2, 0.8f, SpriteEffects.None, 1);
            }

            return false;
        }
	}
}
