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
/*
public class WulfrumBlaze : ModProjectile
{

public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Flames}";

public override void SetStaticDefaults()
{
    Main.projFrames[Projectile.type] = 7;
}

public override void SetDefaults()
{
    Projectile.CloneDefaults(ProjectileID.Flames);
    Projectile.aiStyle = 0;
}
public override void AI()
{
    Projectile.localAI[0] += 1f;

    if (Projectile.localAI[0] >= 50f)
        Projectile.Kill();

    if (Projectile.localAI[0] >= 60f)
        Projectile.velocity *= 0.95f;

    if (Projectile.localAI[0] > 25f && Main.rand.NextFloat() < 0.25f)
    {
        Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f),4, 4, DustID.MagicMirror,Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);

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

        dust.scale *= 1.3f;
        dust.velocity *= 1.2f;
        dust.customData = 1;
    }

    Projectile.rotation = Projectile.velocity.ToRotation();
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
    float scale = MathHelper.Clamp((float)(progress * Math.PI), 0.25f, 1f);

    Color colorStart = new Color(250, 127, 55, 255);
    Color colorEnd   = new Color(105, 90, 20, 255);
    Color drawColor  = Color.Lerp(colorStart, colorEnd, progress) * alpha;

    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    Vector2 offsetDrawPos = Projectile.Center - Main.screenPosition - (Projectile.velocity * 4f);

    float spin = Main.GlobalTimeWrappedHourly * 2f;
    float rot1 = Projectile.rotation + spin;
    float rot2 = Projectile.rotation - spin;

    Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor, rot1, origin, scale, SpriteEffects.None, 0);
    Main.EntitySpriteDraw(texture, offsetDrawPos, sourceRect, drawColor * 0.75f, rot2, origin, scale * 0.75f, SpriteEffects.None, 0);

    return false;
}


    
}
*/