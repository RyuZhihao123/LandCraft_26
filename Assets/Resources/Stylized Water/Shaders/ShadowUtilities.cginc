#ifndef SHADOW_UTILITIES
#define SHADOW_UTILITIES


float4 GetShadowCoordinate(float3 positionWS, float4 weights)
{

    float4 sc0 = mul(unity_WorldToShadow[0], float4(positionWS, 1)); 
    float4 sc1 = mul(unity_WorldToShadow[1], float4(positionWS, 1));
    float4 sc2 = mul(unity_WorldToShadow[2], float4(positionWS, 1));
    float4 sc3 = mul(unity_WorldToShadow[3], float4(positionWS, 1));

    // Get the final shadow coordinate by multiplying by the weights.
    return sc0 * weights.x + sc1 * weights.y + sc2 * weights.z + sc3 * weights.w;
}

float GetLightVisibility(sampler2D shadowMap, float3 positionWS, float maxDistance)
{
    // Calculate the weights for each shadow cascade.
    float distFromCam = length(positionWS - _WorldSpaceCameraPos.xyz);

    if (distFromCam > maxDistance)
    {
        return 1.0;
    }

    float4 near = float4 (distFromCam >= _LightSplitsNear); 
    float4 far = float4 (distFromCam < _LightSplitsFar);
    float4 cascadeWeights = near * far;

    float4 shadowCoord = GetShadowCoordinate(positionWS, cascadeWeights);
    // shadowCoord /= shadowCoord.w;

    return tex2Dproj(shadowMap, shadowCoord) < shadowCoord.z/shadowCoord.w;
}



#endif  // SHADOW_UTILITIES