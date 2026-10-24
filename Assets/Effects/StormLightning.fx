// FROM TERRARIA 1.4.5 DECOMPILE modified to work in 1.4.4.9

sampler uImage0 : register(s0);
float uOpacity;
float uSaturation;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

VertexShaderOutput StormLightningVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    return output;
}

float4 StormLightningPS(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float streak = tex2D(uImage0, coords).r;
    float fade = 0.3 + 0.7 * uOpacity;
    return color * (streak * 8.0 * fade * uSaturation);
}

technique Technique1
{
    pass StormLightning
    {
        VertexShader = compile vs_2_0 StormLightningVS();
        PixelShader = compile ps_2_0 StormLightningPS();
    }
}