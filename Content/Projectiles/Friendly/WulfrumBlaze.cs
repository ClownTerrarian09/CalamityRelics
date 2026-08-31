using System;
using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Magic;
using CalamityRelics.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Projectiles.Friendly;

public class WulfrumBlaze : ModProjectile
{

public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Flames}";

public override void SetStaticDefaults()
{
    Main.projFrames[Projectile.type] = 7;
    ProjectileID.Sets.TrailCacheLength[Type] = 20;
    ProjectileID.Sets.TrailingMode[Type] = 3;
}

public override void SetDefaults()
{
    Projectile.CloneDefaults(ProjectileID.Flames);
    Projectile.timeLeft = (int)(Projectile.timeLeft * 0.25f);
    Projectile.aiStyle = 0;
}
public override void AI()
{
    Projectile.localAI[0] += 1f;

    if (Projectile.localAI[0] >= 50f)
        Projectile.Kill();

    if (Projectile.localAI[0] >= 60f)
        Projectile.velocity *= 0.95f;

    SpawnDust(DustID.GreenTorch, 0.5f);
    SpawnDust(DustID.GreenFairy,0.2f);
    SpawnDust(DustID.GreenMoss,0.3f);

    Projectile.rotation = Projectile.velocity.ToRotation();
}

private void SpawnDust(int type, float scaling = 1)
{
    if (Projectile.localAI[0] > 10f && Main.rand.NextFloat() < 0.25f)
    {
        Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f),4, 4, type,Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100,default, 2f * scaling);

        if (Main.rand.Next(4) == 0)
        {
            dust.noGravity = true;
            dust.scale *= 1.2f;
            dust.velocity *= 2f;
        }
        else
        {
            dust.scale *= 0.8f;
        }

        dust.customData = 1;
    }
    
}

public override bool OnTileCollide(Vector2 oldVelocity)
{
    Projectile.velocity = Vector2.Zero;
    return false;
}

public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = TextureAssets.Projectile[ProjectileID.Flames].Value;
    float age = Projectile.localAI[0];

    Rectangle sourceRect = texture.Frame(1, 7, 0, 3);
    Vector2 origin = sourceRect.Size() / 2f;
    float progress = MathHelper.Clamp(age / 50f, 0f, 1f);
    float alpha = (float)Math.Sin(progress * Math.PI);
    float scale = MathHelper.Clamp((float)(progress * Math.PI), 0, 1f);
    
    Color colorStart = new Color(40, 255, 25, 255);
    Color colorEnd   = new Color(120, 127, 55, 105);
    Color drawColor  = Color.Lerp(colorStart, colorEnd, progress) * alpha;


    float spin = Main.GlobalTimeWrappedHourly * 8f;
    float rot1 = Projectile.rotation + spin;
    float rot2 = Projectile.rotation - spin;
    Vector2 drawPos = Projectile.position + Projectile.Size / 2f - Main.screenPosition;

    Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor, rot1, origin, scale, SpriteEffects.None, 0);
    Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor, rot2, origin, scale, SpriteEffects.None, 0);
    float scalePercentagePerFlame = 0.8f;
    float scaled = scale;
    for (int i = 0; i < Projectile.oldPos.Length; i++)
    {
        if (Projectile.oldPos[i] == Vector2.Zero || i % 5 != 0)
            continue;
        
        scaled *= scalePercentagePerFlame;
        
        drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

        Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor,
            Projectile.oldRot[i] + spin, origin, scaled,
            SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor,
            Projectile.oldRot[i] - spin, origin, scaled,
            SpriteEffects.None, 0);
    }
    return false;
}


    
}
