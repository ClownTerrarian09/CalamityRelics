using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;


namespace CalamityRelics.Assets.Effects;

public class RocketRamShaderData : MiscShaderData
{
    public Vector2 offsetFloat;
    public Vector2 webOffsetFloat;
    public Vector2 perlinEffectFloat;
    public Vector2 webTiling;
    public float maxAlpha;
    public float timeAtLastDashStart;
    public float voronoiIntensity;
    public float webDistortionFactor;
    public Color webColor;

    public Asset<Texture2D> baseMap;       
    public Asset<Texture2D> baseAlphaMap;  
    public Asset<Texture2D> noiseCell;     
    public Asset<Texture2D> noiseWeb;      
    public Asset<Texture2D> noisePerlin;   
    public Asset<Texture2D> noiseWebGlow;  

    public RocketRamShaderData(
        Asset<Effect> effect, 
        string passName, 
        
        Asset<Texture2D> baseMap, 
        Asset<Texture2D> baseAlphaMap,
        Asset<Texture2D> noiseCell, 
        Asset<Texture2D> noiseWeb,
        Asset<Texture2D> noisePerlin, 
        Asset<Texture2D> noiseWebGlow,
        
        Vector2 offsetFloat,
        Vector2 webOffsetFloat,
        Vector2 perlinEffectFloat,
        Vector2 webTiling,
        Color webColor,
        float maxAlpha = 0.4f,
        float voronoiIntensity = 0.9f,
        float webDistortionFactor = 2.15f

    ) : base( effect, passName)
    {
        this.baseMap = baseMap;
        this.baseAlphaMap = baseAlphaMap;
        this.noiseCell = noiseCell;
        this.noiseWeb = noiseWeb;
        this.noisePerlin = noisePerlin;
        this.noiseWebGlow = noiseWebGlow;
        
        this.offsetFloat =  offsetFloat;
        this.webOffsetFloat =  webOffsetFloat;
        this.perlinEffectFloat =  perlinEffectFloat;
        this.webTiling =  webTiling;
        this.webColor =  webColor;
        this.maxAlpha =  maxAlpha;
        this.voronoiIntensity =  voronoiIntensity;
        this.webDistortionFactor =  webDistortionFactor;
    }

    public void MarkNewDash()
    {
        timeAtLastDashStart = Main.GlobalTimeWrappedHourly;
    }

    public override void Apply(DrawData? drawData = null)
    {
		SamplerState sampleState = SamplerState.LinearWrap;

		
        base.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly - timeAtLastDashStart);
        base.Shader.Parameters["offsetFloat"]?.SetValue(offsetFloat);
		base.Shader.Parameters["webOffsetFloat"]?.SetValue(webOffsetFloat);
		base.Shader.Parameters["perlinEffectFloat"]?.SetValue(perlinEffectFloat);
		base.Shader.Parameters["webTiling"]?.SetValue(webTiling);
		base.Shader.Parameters["maxAlpha"]?.SetValue(maxAlpha);
		base.Shader.Parameters["voronoiIntensity"]?.SetValue(voronoiIntensity);

        SetTexture(baseMap,         1, sampleState);
        SetTexture(baseAlphaMap,    2, sampleState);
        SetTexture(noiseCell,       3, sampleState);
        SetTexture(noiseWeb,        4, sampleState);
        SetTexture(noisePerlin,     5, sampleState);
        SetTexture(noiseWebGlow,    6, sampleState);
		
        // base.Apply(drawData);
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
