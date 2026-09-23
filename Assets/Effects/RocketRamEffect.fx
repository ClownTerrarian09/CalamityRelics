sampler uImage0 : register(s0);
sampler baseMap       : register(s1);
sampler baseAlphaMap  : register(s2);
sampler noiseCell     : register(s3);
sampler noiseWeb      : register(s4);
sampler noisePerlin   : register(s5);
sampler noiseWebGlow  : register(s6);

float uTime;

float2 offsetFloat;
float2 webOffsetFloat;
float2 perlinEffectFloat;
float2 webTiling;
float maxAlpha;
float voronoiIntensity;
float webDistortionFactor;
float4 webColor;

// sampler baseSample : register(s1);


// SamplerState smp : register(s0);


float PerlinOffset(float perlinSample){
	return (perlinSample * (perlinEffectFloat)) - perlinEffectFloat/2;
}

float4 MainShaderFunction(float4 inputColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // float4 color = tex2D(baseMap, uv);// * uColor;
    // return color;

    float2 transformation = float2(1, 7);

 
    float pO = tex2D(noisePerlin, uv * transformation + offsetFloat * uTime); 
    float p1 = tex2D(noisePerlin, uv + offsetFloat * uTime / 5); 	
    
	float4 base = tex2D(baseMap, uv + PerlinOffset(pO));
    
    float4 voronoi = tex2D(noiseCell, float2(0.4 * uv.x * uv.x, uv.y)*1 + offsetFloat * uTime * 0.5);
    
    float2 webScaleTf = webTiling * float2(pow(uv.x, 1.5), (uv.y-0.5) * pow((uv.x+1), 5)) + float2(0, PerlinOffset(p1) * webDistortionFactor);
    

    float web = tex2D(noiseWeb, webScaleTf + webOffsetFloat * uTime).r;
    float webGlow = tex2D(noiseWebGlow, webScaleTf + webOffsetFloat * uTime).r;
	
	float alpha = tex2D(baseAlphaMap, uv + PerlinOffset(pO)).r;


	float voronoiAlpha = tex2D(baseAlphaMap, uv + float2(0.1, 0)).r;
	float webAlpha = tex2D(baseAlphaMap, (uv+0.05)*float2(1, .9) + float2(0.2, 0)).r;

	float4 webOut = webColor;
	webOut *= web + webGlow;
	
	webOut *= webAlpha;
	
	voronoi *= voronoiAlpha;
	voronoi *= voronoiIntensity;

	// webOut *= webColor.a;
	return webOut;
	
	if(alpha > maxAlpha){
		alpha = maxAlpha;
	}
	
	base *= alpha;
	
	float4 final = saturate(base + voronoi + webOut);
	return final;
	
	//return pO;
    // return float4(0.5f + 0.5f*cos(uTime+uv.yxy), 1.0f);
}

technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_3_0 MainShaderFunction();
    }
}


