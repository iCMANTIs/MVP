Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.2)) = 0.04

        _WobbleStrength ("Wobble Strength", Range(0, 1)) = 0.03
        _WobbleSpeed ("Wobble Speed", Float) = 1.5
        _WobbleScale ("Wobble Scale", Float) = 2.0
        _WobbleMask ("Wobble Mask", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "InvertedHullOutline"
            Tags { "LightMode"="UniversalForward" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;

                float _WobbleStrength;
                float _WobbleSpeed;
                float _WobbleScale;
                float4 _WobbleMask_ST;
            CBUFFER_END

            TEXTURE2D(_WobbleMask);
            SAMPLER(sampler_WobbleMask);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 posOS = IN.positionOS.xyz;

                float noise =
                    sin((posOS.x + _Time.y * _WobbleSpeed) * _WobbleScale) *
                    sin((posOS.y + _Time.y * _WobbleSpeed * 0.7) * _WobbleScale);

                float2 maskUV = TRANSFORM_TEX(IN.uv, _WobbleMask);
                float wobbleMask = SAMPLE_TEXTURE2D_LOD(_WobbleMask, sampler_WobbleMask, maskUV, 0).r;

                
                posOS += IN.normalOS * noise * _WobbleStrength * wobbleMask;

                
                posOS += IN.normalOS * _OutlineWidth;

                OUT.positionHCS = TransformObjectToHClip(posOS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }
    }
}