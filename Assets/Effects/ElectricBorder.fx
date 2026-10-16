sampler uImage0 : register(s0); // the screen
sampler uImage1 : register(s1); // noise texture
float3 uColor;
float uOpacity;
float uIntensity;
float uTime;
float2 uScreenResolution;

float4 ElectricBorder(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);

    // Distance to nearest edge in pixels, so the band is even on all sides
    float2 px = coords * uScreenResolution;
    float2 toEdge = min(px, uScreenResolution - px);
    float edgeDist = min(toEdge.x, toEdge.y);
    float border = 1 - saturate(edgeDist / 120.0); // 120 px band

    // Two noise layers scrolling against each other
    float n1 = tex2D(uImage1, coords * 2.0 + float2(uTime * 0.6, uTime * 0.2)).r;
    float n2 = tex2D(uImage1, coords * 3.0 - float2(uTime * 0.3, uTime * 0.7)).r;
    float n = (n1 + n2) * 0.5;

    // Thin bright lines where the noise crosses 0.5
    float arc = 1 - saturate(abs(n - 0.5) * 25.0);
    arc = arc * arc * arc;

    float glow = arc * border + border * border * 0.25; // arcs plus a soft edge haze
    color.rgb += uColor * glow * uIntensity * uOpacity;
    return color;
}

technique Technique1
{
    pass ElectricPass
    {
        PixelShader = compile ps_3_0 ElectricBorder();
    }
}