// #include "../HelperLib.hlsl" 
// HelperLib.hlsl is located in the Workspace folder, adding many helper functions for you to use here!
// You can open HelperLib to check out what it can offer in its description. 


// Variable declarations
float uTime; // uTime is built-in to the shader tool and does not need to be included in the .json
sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 mainImageSize; // the Size suffix is built into the shader tool for texture parameters. These do not need to be included in the .json

float4 mainColor;
float opacity;
bool showUV;
bool showTexture;


// The main function that returns the final color from the shader. 
float4 main(float4 inputColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // if(showUV) // try setting this to true in the .json!
    // { 
    //     return float4(uv.x, uv.y, 0, 1);
    // }


    // if(showTexture) // true by default in the .json!
    // {

	    
        float dist = distance(uv, float2(0.5f, 0.5f));
        float2 unitvector = uv-float2(0.5f, 0.5f);
        unitvector = normalize(unitvector);
        float2 distortion = 14*pow(dist, 5) * unitvector;

        float fade = 1;
        if (dist >= 0.4f){
            fade = 1/pow(dist + 0.6f, 50);
        }

        float4 lerping = lerp(float4(0, 1, 1, 1), float4(1, 1, 0, 1), dist);
        uv.x -= uTime * 0.3f;
        uv += distortion;
        
        float4 textureColor = tex2D(uImage1, uv);
        return textureColor * lerping * float4(1, 1, 1, fade);
    // }
    // else
    // {
    //     return mainColor;
    // }

}

// Initialization code for the compiler to understand what type of shader this is and its technique+pass 
technique Technique1
{
    pass Main
    {
        PixelShader = compile ps_2_0 main();
    }
}

