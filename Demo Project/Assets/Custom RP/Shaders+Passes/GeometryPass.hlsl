#ifndef CUSTOM_GEOMETRY_PASS_INCLUDED
#define CUSTOM_GEOMETRY_PASS_INCLUDED

#include "../Auxiliary/Common.hlsl"

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);

TEXTURE2D(_Texture);
SAMPLER(sampler_Texture);

// Unity buffer to keep track of per material properties.
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

// Input struct for the vertex shader.
struct vertexInput
{
    float3 positionObjectSpace : POSITION;
    float3 normalObjectSpace : NORMAL;
    float2 coordsUV : TEXCOORD0;
    float4 tangentObjectSpace : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Output struct for the vertex shader.
struct vertexOutput
{
    float4 positionClipSpace : SV_POSITION;
    float3 positionViewSpace : VAR_POSITION;
    float3 normalWorldSpace : VAR_NORMAL;
    float2 coordsUV : TEXCOORD0;
    float4 tangentWorldSpace : VAR_TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Output struct for the fragment shader using the 3 different render targets.
struct fragmentOutput
{
    float4 normalBuffer : SV_TARGET0;
    float4 albedoBuffer : SV_TARGET1;
    float4 viewPositionBuffer : SV_TARGET2;
};

float3 GetNormalTS(float2 coordsUV)
{
    float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, coordsUV);
    float3 normal = DecodeNormal(map, 1.0);
    
    return normal;
}

vertexOutput GeometryPassVertex(vertexInput input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
   
	// Transform from object space to world space.
    float3 positionWorldSpace = TransformObjectToWorld(input.positionObjectSpace);
    float3 positionViewSpace = TransformWorldToView(positionWorldSpace);

	// Transform from world space to clip space.
    vertexOutput output;

    output.positionClipSpace = TransformWorldToHClip(positionWorldSpace);
    output.normalWorldSpace = TransformObjectToWorldNormal(input.normalObjectSpace);
    output.coordsUV = input.coordsUV;
    
    output.positionViewSpace = positionViewSpace;
    output.tangentWorldSpace = float4(TransformObjectToWorldDir(input.tangentObjectSpace.xyz), input.tangentObjectSpace.w);
    
    return output;
}

fragmentOutput GeometryPassFragment(vertexOutput input) 
{
    float3 normal = NormalTangentToWorld(GetNormalTS(input.coordsUV), input.normalWorldSpace, input.tangentWorldSpace);
    float4 textureSampleColor = SAMPLE_TEXTURE2D(_Texture, sampler_Texture, input.coordsUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    
    fragmentOutput output;

    output.normalBuffer = float4(normal, 1.0);
    output.albedoBuffer = textureSampleColor * baseColor;
    output.viewPositionBuffer = float4(input.positionViewSpace, 1.0);
     
    return output;
}

#endif