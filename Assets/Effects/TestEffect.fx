sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float uTime;

float4 TestEffect(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    return float4( + float2(uTime, uTime), 1, 1);
}

technique Technique1
{
    pass TestEffect
    {
        PixelShader = compile ps_2_0 TestEffect();
    }
}