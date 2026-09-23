// MEANT TO BE CODE FROM ANOTHER PROGRAM I STILL HAVE TO CONVERT



cbuffer vars : register(b0)
{
	float2 uResolution;
	float uTime;
	
	float2 offsetFloat;
	float2 webOffsetFloat;
	float2 perlinEffectFloat;
	float2 webTiling;
	float maxAlpha;
	float voronoiIntensity;
	float webDistortionFactor;
	float4 webColor;
};

Texture2D baseAlphaMap : register(t1);
Texture2D noiseCell : register(t2);
Texture2D noiseWeb : register(t3);
Texture2D noisePerlin : register(t4);
Texture2D background : register(t0);
Texture2D noiseWebGlow : register(t5);
Texture2D grad : register(t6);



SamplerState smp : register(s0);
sampler baseSample : register(s1);

float PerlinOffset(float perlinSample){
	return (perlinSample * (perlinEffectFloat)) - perlinEffectFloat/2;
}

float4 main(float4 fragCoord : SV_POSITION) : SV_TARGET
{
    float2 uv = fragCoord.xy/uResolution;
    float2 transformation = float2(1, 7);

 
    float pO = noisePerlin.Sample(smp, uv * transformation + offsetFloat * uTime); 
    float p1 = noisePerlin.Sample(smp, uv + offsetFloat * uTime / 5); 	
    
    float4 voronoi = noiseCell.Sample(smp, float2(0.4 * uv.x * uv.x, uv.y)*1 + offsetFloat * uTime * 0.5);
    
    float2 webScaleTf = webTiling * float2(pow(uv.x, 1.5), (uv.y-0.5) * pow((uv.x+1), 5)) + float2(0, PerlinOffset(p1) * webDistortionFactor);
    

    float web = noiseWeb.Sample(smp, webScaleTf + webOffsetFloat * uTime);
    float webGlow = noiseWebGlow.Sample(smp, webScaleTf + webOffsetFloat * uTime);

    
    float alpha = baseAlphaMap.Sample(smp, uv + PerlinOffset(pO)).r;
    
    //float4 base = baseMap.Sample(smp, uv + PerlinOffset(pO));
	float4 base = grad.Sample(smp, float2(alpha-0.01f, 0));    
	
	float4 bg = background.Sample(smp, uv);
	//base /=0.8;
	
	
	float voronoiAlpha = baseAlphaMap.Sample(smp, uv + float2(0.1, 0)).r;
	float webAlpha = baseAlphaMap.Sample(smp, (uv+0.05)*float2(1, .9) + float2(0.05f, 0)).r;
	
	float4 webOut = webColor;
	webOut *= web + webGlow;
	webOut *= webAlpha;
	webOut *= webColor.a;
	
	voronoi = grad.Sample(smp, float2(voronoi.r, 0));
	//float alpha = alphaMap.a;
	voronoi *= voronoiAlpha;
	voronoi *= voronoiIntensity;
	
	if(alpha > maxAlpha){
		alpha = maxAlpha;
	}
	
	base *= alpha;
	
	float4 final = saturate(bg + base + voronoi + webOut);
	
	//return pO;
	return final;
    return float4(0.5f + 0.5f*cos(uTime+uv.yxy), 1.0f);
}
