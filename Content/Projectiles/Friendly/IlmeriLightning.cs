using System;
using System.Collections.Generic;
using System.IO;
using CalamityRelics.ModUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.Utilities.Terraria.Utilities;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    public class IlmeriLightning : ModProjectile
    {
        public Vector2[] Waypoints = Array.Empty<Vector2>();

        private Vector2[] _path;
        private float[] _rotations;
        private Vector2[] _curve; 

        private float PointSpacing = 10f;
        private int SmoothIterations = 3;
        private float BendAmount = 14f;
        private int BendEvery = 5;
        private float JagAmount = 3f;
        private int SweepTicksPerSegment = 1;
        private int FadeTicks = 40;

        private int Segments => Math.Max(1, Waypoints.Length - 1);
        private int SweepTicks => SweepTicksPerSegment * Segments;
        private int Lifetime => SweepTicks + FadeTicks;
        private float Progress => Projectile.localAI[0] / Lifetime;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.timeLeft = 600;
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
            
            
            BendAmount *= Main.rand.NextFloat(0.9f, 0.1f);
            BendEvery = (int)MathF.Round(BendEvery * Main.rand.NextFloat(0.9f, 0.1f));
            JagAmount *= Main.rand.NextFloat(0.9f, 0.1f);
            FadeTicks = (int)MathF.Round(FadeTicks * Main.rand.NextFloat(0.8f, 0.2f));
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)Waypoints.Length);
            foreach (Vector2 p in Waypoints)
                writer.WriteVector2(p);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            int count = reader.ReadByte();
            Waypoints = new Vector2[count];
            for (int i = 0; i < count; i++)
                Waypoints[i] = reader.ReadVector2();
            _path = null;
        }

        public override void AI()
        {
            if (Waypoints.Length < 2)
                return;

            _path ??= BuildPath();

            Projectile.localAI[0]++;
            if (Projectile.localAI[0] >= Lifetime)
                Projectile.Kill();
        }

        private Vector2[] BuildPath()
        {
            _curve = Resample(Chaikin(Waypoints, SmoothIterations), PointSpacing);
            int n = _curve.Length;
            var rand = new UnifiedRandom((int)Projectile.ai[0]);

            float[] bends = new float[n / BendEvery + 2];
            for (int k = 0; k < bends.Length; k++)
                bends[k] = rand.NextFloat(-1f, 1f);

            Vector2[] path = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)BendEvery;
                int k0 = (int)k;
                float f = k - k0;
                f = f * f * (3f - 2f * f);
                float bend = MathHelper.Lerp(bends[k0], bends[k0 + 1], f) * BendAmount;
                float jag = rand.NextFloat(-1f, 1f) * JagAmount;

                float edge = Math.Min(1f, Math.Min(i, n - 1 - i) / 4f);

                Vector2 normal = Tangent(_curve, i).RotatedBy(MathHelper.PiOver2);
                path[i] = _curve[i] + normal * (bend + jag) * edge;
            }

            _rotations = new float[n];
            for (int i = 0; i < n; i++)
                _rotations[i] = Tangent(path, i).ToRotation();

            return path;
        }
        private static Vector2 Tangent(Vector2[] p, int i)
        {
            Vector2 prev = p[Math.Max(i - 1, 0)];
            Vector2 next = p[Math.Min(i + 1, p.Length - 1)];
            return (next - prev).SafeNormalize(Vector2.UnitX);
        }
        private static List<Vector2> Chaikin(IList<Vector2> pts, int iterations)
        {
            var current = new List<Vector2>(pts);
            for (int it = 0; it < iterations && current.Count >= 3; it++)
            {
                var next = new List<Vector2> { current[0] };
                for (int i = 0; i < current.Count - 1; i++)
                {
                    next.Add(Vector2.Lerp(current[i], current[i + 1], 0.25f));
                    next.Add(Vector2.Lerp(current[i], current[i + 1], 0.75f));
                }
                next.Add(current[^1]);
                current = next;
            }
            return current;
        }
        private static Vector2[] Resample(List<Vector2> pts, float spacing)
        {
            var result = new List<Vector2> { pts[0] };
            float carry = 0f;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                Vector2 a = pts[i], b = pts[i + 1];
                float len = Vector2.Distance(a, b);
                float d = spacing - carry;
                while (d <= len)
                {
                    result.Add(Vector2.Lerp(a, b, d / len));
                    d += spacing;
                }
                carry = len - (d - spacing);
            }
            if (result[^1] != pts[^1])
                result.Add(pts[^1]);
            return result.ToArray();
        }
        private StormLightningDrawer.AnimParams GetAnim()
        {
            float sweepFrac = SweepTicks / (float)Lifetime;
            var anim = StormLightningDrawer.DefaultAnim;
            anim.WaveTransitionInDuration = sweepFrac;
            anim.WaveTransitionOutDuration = 1f - sweepFrac;
            anim.FadeStartProgress = 0f;
            anim.FadeMidProgress = sweepFrac;
            anim.FadeEndProgress = 1f;
            return anim;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (_curve == null)
                return false;

            float revealed = Utils.Remap(Projectile.localAI[0], 0f, SweepTicks, 0f, 1f) * (_curve.Length - 1);
            for (int s = 0; s < _curve.Length - 1 && s < revealed; s++)
            {
                float _ = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                        _curve[s], _curve[s + 1], 16f, ref _))
                    return true;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_path == null)
                return false;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            new StormLightningDrawer().Draw(_path, _rotations, 16f, new Color(80, 220, 220),
                Progress, true, new FloatRange(0f, 1f), 1f, GetAnim());

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}