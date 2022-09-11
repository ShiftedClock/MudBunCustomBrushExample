#define kLetter  (906)
// This disables an "unknown pragma" shader warning that spams the console
#pragma warning( disable : 3568 )
#pragma multi_compile __ MUD_FONT_ENABLED

#ifdef MUD_FONT_ENABLED
Texture2D ShadertoyFontTexture;
Texture2D ShadertoyFontFlippedTexture;
sampler ShadertoyFontTextureLinearClampSampler;
float4 sample_font_texture (float scale, float2 uv)
{
    return ShadertoyFontTexture.SampleLevel(ShadertoyFontTextureLinearClampSampler, uv*scale, 0);
}
float4 sample_flipped_font_texture (float scale, float2 uv)
{
    return ShadertoyFontFlippedTexture.SampleLevel(ShadertoyFontTextureLinearClampSampler, uv*scale, 0);
}
#endif


float sdf_letter(float3 p, float3 h, float scale, float2 offset, float thickness, float flipped=0.0)
{ 
    // float3 d = abs(p) - abs(float3(0.03125/scale,0.03125/scale, thickness));
    float3 d = abs(p) - abs(float3(0.025/scale, 0.025/scale, thickness));
    float box = length(max(d, 0.0f)) + min(max_comp(d), 0.0f);
    
    #ifdef MUD_FONT_ENABLED
    float letterDist = flipped > 0.5 ? (sample_flipped_font_texture(scale, p.xy + offset).w - 0.5+1.0/256.0) : (sample_font_texture(scale, p.xy + offset).w - 0.5+1.0/256.0);
    return max(letterDist, box);
    
    #else
    return box;
    #endif
}
