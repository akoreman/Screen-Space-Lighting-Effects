#ifndef CUSTOM_SSAO_PASS_INCLUDED
#define CUSTOM_SSAO_PASS_INCLUDED

#include "../Auxiliary/Common.hlsl"
#include "../Auxiliary/Random.hlsl"

// This pass is to sample the textures and sample them to the part of a triangle.
TEXTURE2D(_NormalBuffer);
SAMPLER(sampler_NormalBuffer);

TEXTURE2D(_AlbedoBuffer);
SAMPLER(sampler_AlbedoBuffer);

TEXTURE2D(_ViewPositionBuffer);
SAMPLER(sampler_ViewPositionBuffer);

struct vertexOutput {
	float4 positionClipSpace : SV_POSITION;
	float2 coordsUV : VAR_SCREEN_UV;
};

// Assign the correct clip-space coordinates and UV coordinates to the rendered triangle.
vertexOutput SSAOPassVertex (uint vertexID : SV_VertexID) {
	vertexOutput output;
	
	output.positionClipSpace = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? 3.0 : -1.0,
		0.0, 1.0
	);
	output.coordsUV = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? -1.0 : 1.0
	);

	return output;
}

float4 SSAOPassFragment(vertexOutput input) : SV_TARGET
{
    float4 albedo = SAMPLE_TEXTURE2D(_AlbedoBuffer, sampler_AlbedoBuffer, input.coordsUV);
	float4 normal = SAMPLE_TEXTURE2D(_NormalBuffer, sampler_NormalBuffer, input.coordsUV);

	//return float4(1.0f, 0.0f,0.0f, 0.5f);

    return albedo + float4(RandomRange_float(input.coordsUV, 0.0, 1.0), RandomRange_float(input.coordsUV, 0.0, 1.0) ,RandomRange_float(input.coordsUV, 0.0, 1.0) ,RandomRange_float(input.coordsUV, 0.0, 1.0));
}

#endif