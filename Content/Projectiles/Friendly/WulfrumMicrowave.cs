using System;
using CalamityRelics.Content.NPCs.Wulfrum;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class WulfrumMicrowave : ModProjectile
    {
        public const float MaxLength = 1200f;
        public const int Lifetime = 150;
        public float ChargeTime
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float Length
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
    
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.knockBack = 0.1f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.width = 16;
            Projectile.height = 24;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.Kill();
                return;
            }
            Projectile.rotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
            ChargeTime = 50;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.Kill();
                return;
            }
            
            Projectile.rotation = Projectile.rotation.AngleLerp((Main.MouseWorld - player.Center).ToRotation(), 0.08f);
            Projectile.Center = player.Center + Projectile.rotation.ToRotationVector2() * 30f;
            if (ChargeTime > 0)
            {
                ChargeTime--;
                if (Main.rand.NextFloat(2f) <= 1f)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 0f, 0f, 100, default, 1f);
                    dust.color = Color.Green;
                    dust.scale = 0.675f;
                }
                return;
            }
            else if(ChargeTime == 0)
            {
                SoundEngine.PlaySound(SoundID.Item12, Projectile.position);
                ChargeTime = -1;
            }
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            float length = MaxLength;
            Collision.LaserScan(Projectile.Center, dir, Projectile.width * 0.5f, MaxLength, new[] { length });
            Length = MathHelper.Lerp(Length, FindLength(dir, out bool hitTile), 0.5f);
            
            Vector2 endPos = Projectile.Center + dir * Length;
            
            Particle startbeamParticle = new BloomParticle(Projectile.Center, Projectile.velocity * 0.5f, Color.White, 0.3f, 0.3f, 4);
            GeneralParticleHandler.SpawnParticle(startbeamParticle);
            
            
            if (hitTile)
            {
                Particle hitTileParticle = new HeavySmokeParticle(endPos, Projectile.velocity * 0.5f, Color.White, 40, 0.5f * Main.rand.NextFloat(0.8f, 1f), 0.7f, glowing: true);
                GeneralParticleHandler.SpawnParticle(hitTileParticle);
            }
            else
            {
                
                Particle endbeamParticle = new BloomParticle(endPos, Projectile.velocity * 0.5f, Color.White, 0.4f, 0.3f, 4);
                GeneralParticleHandler.SpawnParticle(endbeamParticle);
            }
            
            float spacing = 20f;
            for (float d = 0f; d < Length; d += spacing)
            {
                if (!Main.rand.NextBool(6))
                    continue;

                Vector2 pos = Projectile.Center + dir * d;
                Vector2 vel = dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1f, 1f);
                Particle beamParticle = new HeavySmokeParticle(pos, vel, Color.Cyan, 10,
                    0.5f * Main.rand.NextFloat(0.8f, 1f), 0.6f, glowing: true);
                GeneralParticleHandler.SpawnParticle(beamParticle);
            }
            Projectile.scale = GetScale();

            SpawnDust(dir);
        }

        private float FindLength(Vector2 dir, out bool hitTile)
        {
            hitTile = false;
            float step = 8f;
            for (float d = 0f; d < MaxLength; d += step)
            {
                Vector2 point = Projectile.Center + dir * d;
                Point tile = point.ToTileCoordinates();

                if (!Terraria.WorldGen.InWorld(tile.X, tile.Y, 1))
                {
                    hitTile = true;
                    return d;
                }

                Tile t = Main.tile[tile.X, tile.Y];
                if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
                {
                    hitTile = true;
                    return d;
                }
            }
            return MaxLength;
        }

        private float GetScale()
        {
            int age = Lifetime - Projectile.timeLeft;
            if (age < 15)
                return MathHelper.Lerp(0.1f, 1f, age / 15f);
            if (Projectile.timeLeft < 20)
                return MathHelper.Lerp(0.1f, 1f, Projectile.timeLeft / 20f);
            return 1f;
        }

        private void SpawnDust(Vector2 dir)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 end = Projectile.Center + dir * Length;
                Dust dust = Dust.NewDustDirect(end - Vector2.One * 6f, 12, 12, DustID.Vortex,
                    0f, 0f, 100, default, 1f);
                dust.velocity = -dir.RotatedByRandom(0.6f) * Main.rand.NextFloat(1f, 4f);
                dust.color = Color.Green;
                dust.noGravity = true;
                dust.scale = 0.9f;
            }

            if (Main.rand.NextBool(3))
            {
                Vector2 point = Projectile.Center + dir * Main.rand.NextFloat(Length);
                Dust dust = Dust.NewDustDirect(point - Vector2.One * 4f, 8, 8, DustID.Vortex,
                    0f, 0f, 100, default, 1f);
                dust.velocity = dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2f, 2f);
                dust.color = Color.Green;
                dust.noGravity = true;
                dust.scale = 0.6f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.scale < 0.5f)
                return false;

            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * Length;
            float _ = 0f;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, Projectile.width * Projectile.scale, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            Vector2 origin = new Vector2(0f, tex.Height * 0.5f);
            Color color = Color.White * 0.9f;

            float drawn = 0f;
            while (drawn < Length)
            {
                float segment = MathHelper.Min(tex.Width, Length - drawn);
                Rectangle source = new Rectangle(0, 0, (int)segment, tex.Height);
                Vector2 pos = Projectile.Center + dir * drawn - Main.screenPosition;

                Main.EntitySpriteDraw(tex, pos, source, color,
                    Projectile.rotation, origin, new Vector2(1f, Projectile.scale),
                    SpriteEffects.None, 0);

                drawn += segment;
            }

            return false;
        }

        public override void CutTiles()
        {
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + dir * Length,
                Projectile.width * Projectile.scale, DelegateMethods.CutTiles);
        }
    }
}