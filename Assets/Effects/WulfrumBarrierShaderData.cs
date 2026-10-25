using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace CalamityRelics.Assets.Effects;

public class WulfrumBarrierShaderData : MiscShaderData
{
    public float timeAtLastDashStart;
    public Asset<Texture2D> texture;

    public WulfrumBarrierShaderData(Asset<Effect> effect, string passName, Asset<Texture2D> texture) : base( effect, passName)
    {
        this.texture = texture;
    }

    public void MarkNewDash()
    {
        timeAtLastDashStart = Main.GlobalTimeWrappedHourly;
    }

    public override void Apply(DrawData? drawData = null)
    {
        SamplerState sampleState = SamplerState.LinearWrap;

		
        base.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly - timeAtLastDashStart);
        SetTexture(texture,         1, sampleState);
    }

    public void UpdateTime()
    {
        Shader.Parameters["uTime"]?.SetValue((float)Main.GlobalTimeWrappedHourly);
    }

    public void SetTexture(Asset<Texture2D> asset, int register, SamplerState samplerState)
    {
        if(asset != null)
        {
            // Main.NewText("asset is being loaded");
            Main.graphics.GraphicsDevice.Textures[register] = asset.Value;
            Main.graphics.GraphicsDevice.SamplerStates[register] = samplerState;
            // Shader.Parameters[""]?.SetValue(new Vector2(_uImage0.Width(), _uImage0.Height()));
        }
    }
}