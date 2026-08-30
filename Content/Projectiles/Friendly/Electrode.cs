using System;
using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Magic;
using CalamityRelics.Content.Items;
using CalamityRelics.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Friendly;

public class Electrode : ModProjectile
{
    protected override bool CloneNewInstances => true;

    private NPC stuckToNPC = null;
    private Vector2 offset = Vector2.Zero;
    private bool triggered = false;
    private int seed;
    private int orbitTimer;
    
    internal Color PrimColorMult = Color.White;
    
    
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1400;
        Projectile.extraUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        seed = Main.rand.Next(0, 360);
    }

    public override void AI()
    {
        foreach (Projectile projectile in Main.projectile)
        {
            if (projectile.type != ModContent.ProjectileType<WulfrumBlaze>() || !projectile.active)
                continue;
            if (Vector2.Distance(projectile.Center, Projectile.Center) < 100f)
            {
                triggered = true;
                Projectile.timeLeft += Main.rand.Next(-10, -2);
            }
        }
        Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.X);

        if (stuckToNPC != null)
        {
            if (!stuckToNPC.active)
            {
                Projectile.Kill();
                return;
            }

            orbitTimer++;
            float seedValue = (seed / 360f - 0.5f) * 1.5f;
            float timeX = orbitTimer / 10f;
            float timeY = orbitTimer / 7f;
            float scaleX = 25f + seedValue * 10;
            float scaleY = 15f + seedValue * 5;

            Vector2 spinOffset = new Vector2(MathF.Sin(timeX) * scaleX, MathF.Cos(timeY) * scaleY)
                .RotatedBy(MathHelper.ToRadians(seed));

            Projectile.velocity = Vector2.Zero;
            Projectile.Center = Vector2.Lerp(Projectile.Center, stuckToNPC.Center + spinOffset, 0.35f);
            Projectile.rotation += MathHelper.ToRadians(6f);
            return;
        }

        Projectile.velocity.X *= 0.98f;
        Projectile.velocity.Y += 0.07f;

    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (stuckToNPC == null)
        {
            offset = Projectile.position - target.position;
            Projectile.tileCollide = false;
            stuckToNPC = target;
        }
    }

    public override bool? CanHitNPC(NPC target)
    {
        if(stuckToNPC != null)
            return false;
        return base.CanHitNPC(target);
    }

    public override void OnKill(int timeLeft)
    {
        if (stuckToNPC != null)
        {
            int damage = triggered ? 25 : 5;
            stuckToNPC.StrikeNPC(new NPC.HitInfo(){Damage = damage,Knockback = 0,DamageType = DamageClass.Ranged});
            Main.player[Projectile.owner].dpsDamage += damage;
        }
        SoundEngine.PlaySound(WulfrumProsthesis.HitSound, Projectile.Center);
        for (int d = 0; d < 15; d++)
        {
            Dust chust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror);
            chust.noGravity = true;
        }
        ElectroblazerPlayer ePlayer = Main.player[Projectile.owner].GetModPlayer<ElectroblazerPlayer>();
        if(ePlayer is { electrodeCount: > 0 })
            ePlayer.electrodeCount--;
        base.OnKill(timeLeft);
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 origin = frame.Size() / 2f;

        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
            float progress = 1f - (i / (float)Projectile.oldPos.Length);
            Color trailColor = Color.Cyan * progress * 0.5f;
            trailColor.A = 0;

            Main.EntitySpriteDraw(texture, drawPos, frame, trailColor,
                Projectile.oldRot[i], origin, Projectile.scale * progress,
                SpriteEffects.None, 0);
        }

        // the projectile itself
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame,
            Projectile.GetAlpha(lightColor), Projectile.rotation, origin,
            Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
}