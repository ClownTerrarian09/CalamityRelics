using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Particles;
using JSubClasses.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Friendly
{
    // A* tracking around tile thingy for stuff. Idk how useful this will be for other stuff but its here now
    public static class TilePathfinder
    {
        static readonly Point[] Dirs = {
            new(1,0), new(-1,0), new(0,1), new(0,-1),
            new(1,1), new(1,-1), new(-1,1), new(-1,-1)
        };

        static bool SolidTile(int x, int y)
        {
            if (!Terraria.WorldGen.InWorld(x, y, 5)) return true;
            Tile t = Main.tile[x, y];
            return t.HasTile && !t.IsActuated && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType];
        }

        static void Footprint(int w, int h, out int x0, out int x1, out int y0, out int y1)
        {
            x0 = (int)MathF.Floor((8 - w / 2f) / 16f);
            x1 = (int)MathF.Floor((8 + w / 2f - 1) / 16f);
            y0 = (int)MathF.Floor((8 - h / 2f) / 16f);
            y1 = (int)MathF.Floor((8 + h / 2f - 1) / 16f);
        }

        static bool Blocked(int cx, int cy, int x0, int x1, int y0, int y1)
        {
            for (int x = cx + x0; x <= cx + x1; x++)
                for (int y = cy + y0; y <= cy + y1; y++)
                    if (SolidTile(x, y)) return true;
            return false;
        }

        public static List<Point> FindPath(Point start, Point goal, int width, int height, int maxExpanded = 1500)
        {
            Footprint(width, height, out int x0, out int x1, out int y0, out int y1);

            var open = new PriorityQueue<Point, int>();
            var came = new Dictionary<Point, Point>();
            var g = new Dictionary<Point, int> { [start] = 0 };
            open.Enqueue(start, 0);
            int expanded = 0;

            while (open.TryDequeue(out Point cur, out _))
            {
                if (Math.Abs(cur.X - goal.X) <= 1 && Math.Abs(cur.Y - goal.Y) <= 1)
                    return Rebuild(came, cur);
                if (++expanded > maxExpanded) return null;

                for (int i = 0; i < 8; i++)
                {
                    Point d = Dirs[i];
                    int nx = cur.X + d.X, ny = cur.Y + d.Y;
                    if (Blocked(nx, ny, x0, x1, y0, y1)) continue;
                    if (i >= 4 && (Blocked(cur.X + d.X, cur.Y, x0, x1, y0, y1) ||
                                   Blocked(cur.X, cur.Y + d.Y, x0, x1, y0, y1))) continue;

                    var next = new Point(nx, ny);
                    int cost = g[cur] + (i >= 4 ? 14 : 10);
                    if (g.TryGetValue(next, out int old) && cost >= old) continue;

                    g[next] = cost;
                    came[next] = cur;
                    int dx = Math.Abs(goal.X - nx), dy = Math.Abs(goal.Y - ny);
                    open.Enqueue(next, cost + 10 * (dx + dy) - 6 * Math.Min(dx, dy));
                }
            }
            return null;
        }

        static List<Point> Rebuild(Dictionary<Point, Point> came, Point cur)
        {
            var path = new List<Point> { cur };
            while (came.TryGetValue(cur, out cur)) path.Add(cur);
            path.Reverse();
            return path;
        }
    }

    public class WulfrumTrackerArrow : ModProjectile
    {
        private int TargetSlot
        {
            get => (int)Projectile.ai[2] - 1;
            set => Projectile.ai[2] = value + 1;
        }
        private NPC Target => TargetSlot >= 0 ? Main.npc[TargetSlot] : null;

        private float startAimTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private bool failed;
        private bool targetOnTarget;
        private List<Point> path;
        private int waypoint;

        private int repathTimer;
        private int repathCooldown;
        private Point lastTargetTile;
        private Vector2 aimTargetPosition;

        private int minRepathInterval = 10;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 1200;
        }

        bool IsOwner => Projectile.owner == Main.myPlayer;

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Vector3(0, 0.9f, 0.65f));
            NPC target = Target;

            if (target != null && (!target.active || target.life <= 0))
            {
                TargetSlot = -1;
                path = null;
                target = null;
                if (IsOwner)
                {
                    failed = true;
                    targetOnTarget = false;
                    Projectile.netUpdate = true;
                }
            }

            if (startAimTimer == 0)
                startAimTimer = 5;
            Projectile.ai[0] += 1f;

            if (target == null)
            {
                if (Projectile.ai[0] >= 15f)
                {
                    Projectile.ai[0] = 15f;
                    Projectile.velocity.Y += 0.1f;
                }

                if (IsOwner && startAimTimer <= 2 && !failed)
                {
                    float closest = 1000f;
                    NPC best = null;
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.CanBeChasedBy(Projectile))
                            continue;
                        float d = Vector2.Distance(Projectile.Center, npc.Center);
                        if (d < closest)
                        {
                            closest = d;
                            best = npc;
                        }
                    }

                    if (best != null)
                    {
                        TargetSlot = best.whoAmI;
                        aimTargetPosition = Projectile.Center;
                        Projectile.netUpdate = true;
                    }
                }
                else if (startAimTimer > 2)
                {
                    startAimTimer--;
                }

                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
            else
            {
                if (targetOnTarget)
                {
                    aimTargetPosition = target.Center;

                    if (Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height,
                                             target.position, target.width, target.height))
                    {
                        Vector2 dir = Projectile.DirectionTo(target.Center);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 24f, 0.2f);
                    }
                    else
                    {
                        FollowAStarPathToTarget(target);
                    }

                }
                else
                {
                    if (Projectile.ai[0] >= 15f)
                    {
                        Projectile.ai[0] = 15f;
                        Projectile.velocity.Y += 0.1f;
                    }
                    Projectile.velocity *= 0.95f;

                    if (Vector2.Distance(aimTargetPosition, target.Center) < 16f)
                    {
                        aimTargetPosition = target.Center;
                        if (IsOwner)
                        {
                            targetOnTarget = true;
                            Projectile.netUpdate = true;
                        }
                    }
                    else
                    {
                        Vector2 dir = Vector2.Normalize(target.Center - aimTargetPosition);
                        aimTargetPosition += (dir * 16f) +  new Vector2(MathF.Sin((Projectile.ai[0] + 65) * 0.04f) * 12f, MathF.Cos(Projectile.ai[0] * 0.08f) * 9f);
                    }
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        private void FollowAStarPathToTarget(NPC target)
        {
            Point targetTile = target.Center.ToTileCoordinates();
            if (repathCooldown > 0) repathCooldown--;

            bool targetMoved = Math.Abs(targetTile.X - lastTargetTile.X) + Math.Abs(targetTile.Y - lastTargetTile.Y) > 1;
            if (IsOwner && repathCooldown <= 0 && (path == null || targetMoved || --repathTimer <= 0))
            {
                Point from = path != null && path.Count > 0
                    ? path[Math.Min(waypoint, path.Count - 1)]
                    : Projectile.Center.ToTileCoordinates();

                path = TilePathfinder.FindPath(from, targetTile, Projectile.width, Projectile.height);
                waypoint = 0;
                lastTargetTile = targetTile;
                repathTimer = 45;
                repathCooldown = minRepathInterval;
                Projectile.netUpdate = true;
            }

            if (path == null || path.Count == 0)
            {
                Projectile.velocity *= 0.9f;
                return;
            }

            Vector2 wp = path[waypoint].ToWorldCoordinates();
            while (waypoint < path.Count - 1 && Vector2.DistanceSquared(Projectile.Center, wp) < 4f)
            {
                waypoint++;
                wp = path[waypoint].ToWorldCoordinates();
            }

            Vector2 goal = waypoint == path.Count - 1 ? target.Center : wp;

            Vector2 toGoal = goal - Projectile.Center;
            float dist = toGoal.Length();
            Projectile.velocity = dist < 24f ? toGoal : toGoal / dist * 24f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            default(WulfrumTrailDrawer).Draw(Projectile);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f,Projectile.scale, SpriteEffects.None);
            if (targetOnTarget)
            {
                for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;
 
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f;
                    Main.EntitySpriteDraw(tex, trailPos, null, Projectile.GetAlpha(lightColor), Projectile.oldRot[i], tex.Size() / 2f, Projectile.scale, SpriteEffects.None);
                }
            }
            if (IsOwner && Target != null)
            {
                Texture2D targetTex = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/WulfrumTrackerArrow/Target").Value;
                Main.EntitySpriteDraw(targetTex, aimTargetPosition - Main.screenPosition, null,
                    Color.White, 0f, targetTex.Size() / 2f,
                    1, SpriteEffects.None);
            }

            return false;
        }

        public override void SendExtraAI(BinaryWriter w)
        {
            w.Write(failed);
            w.Write(targetOnTarget);
            w.Write((short)waypoint);
            w.Write((short)(path?.Count ?? 0));
            if (path != null)
                foreach (var p in path) { w.Write((short)p.X); w.Write((short)p.Y); }
        }

        public override void ReceiveExtraAI(BinaryReader r)
        {
            failed = r.ReadBoolean();
            targetOnTarget = r.ReadBoolean();
            waypoint = r.ReadInt16();
            int n = r.ReadInt16();
            path = n == 0 ? null : new List<Point>(n);
            for (int i = 0; i < n; i++) path.Add(new Point(r.ReadInt16(), r.ReadInt16()));
            if (path != null) waypoint = Math.Clamp(waypoint, 0, path.Count - 1);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;
            }
        }
    }
}