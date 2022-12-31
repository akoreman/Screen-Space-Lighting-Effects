Shader "Custom RP/Deferred"
{
	Properties{
		_BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		_Texture("Texture", 2D) = "white" {}

		[NoScaleOffset] _NormalMap("Normals", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range(0, 1)) = 1
	}
		SubShader{
			Pass {
				Tags {
					"LightMode" = "Geometry"
				}

				HLSLPROGRAM
				#pragma target 5.0
				#pragma multi_compile_instancing
				#pragma vertex GeometryPassVertex
				#pragma fragment GeometryPassFragment
				#include "GeometryPass.hlsl"
				ENDHLSL
			}
			
			Pass {
				Tags {
					"LightMode" = "SSAO"
				}

				HLSLPROGRAM
				#pragma target 5.0
				#pragma multi_compile_instancing
				#pragma vertex SSAOPassVertex
				#pragma fragment SSAOPassFragment
				#include "SSAOPass.hlsl"
				ENDHLSL
			}
		
			Pass {
				Tags {
					"LightMode" = "Lit"
				}

				HLSLPROGRAM
				#pragma target 5.0
				#pragma multi_compile_instancing
				#pragma vertex LitPassVertex
				#pragma fragment LitPassFragment
				#pragma shader_feature _RECEIVE_SHADOWS
				#include "LitPass.hlsl"
				ENDHLSL
			}
			
		}
}