using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace JSubClasses.Effects
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct WulfrumTrailDrawer
	{
        private static VertexStrip _vertexStrip = new VertexStrip();

        public void Draw(Projectile proj)
        {

            MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(4f);
            miscShaderData.Apply();
            _vertexStrip.PrepareStripWithProceduralPadding(proj.oldPos, proj.oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        private Color StripColors(float progressOnStrip)
        {

            Color value = Main.hslToRgb((progressOnStrip * 1.6f - Main.GlobalTimeWrappedHourly) % 1f, 1f, 0.5f);
            Color result = Color.Lerp(Color.MediumAquamarine, Color.MediumSpringGreen, Utils.GetLerpValue(-0.2f, 0.5f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
            result.A = 0;
            return result;
        }

        private float StripWidth(float progressOnStrip)
        {
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 32f, num);
        }

    }
}