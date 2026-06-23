Shader "Custom/ExpressionistToon"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 0.28, 0.02, 1)
        _ShadowColor ("Shadow Color", Color) = (0.02, 0.008, 0.002, 1)

        _ShadowThreshold ("Shadow Threshold", Range(-1, 1)) = 0.25
        _ShadowSoftness ("Shadow Softness", Range(0.001, 0.5)) = 0.04

        _GrainTex ("Paper Grain Texture", 2D) = "white" {}
        _GrainStrength ("Grain Strength", Range(0, 1)) = 0.6
        _GrainScale ("Grain Scale", Float) = 30
        _GrainOverlayStrength ("Grain Overlay Strength", Range(0, 1)) = 0.5

        _HatchTex ("Hatch Texture", 2D) = "white" {}
        _HatchStrength ("Hatch Strength", Range(0, 1)) = 0.6
        _HatchScale ("Hatch Scale", Float) = 30

        _PosterizeSteps ("Posterize Steps", Range(2, 12)) = 4

        _WobbleMask ("Wobble Mask", 2D) = "white" {}
        _WobbleStrength ("Wobble Strength", Range(0, 1)) = 0.03
        _WobbleSpeed ("Wobble Speed", Float) = 1.5
        _WobbleScale ("Wobble Scale", Float) = 2.0
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
            Name "ExpressionistToon"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ShadowColor;

                float _ShadowThreshold;
                float _ShadowSoftness;

                float4 _GrainTex_ST;
                float _GrainStrength;
                float _GrainScale;
                float _GrainOverlayStrength;

                float4 _HatchTex_ST;
                float _HatchStrength;
                float _HatchScale;

                float _PosterizeSteps;

                float4 _WobbleMask_ST;
                float _WobbleStrength;
                float _WobbleSpeed;
                float _WobbleScale;
            CBUFFER_END

            TEXTURE2D(_GrainTex);
            SAMPLER(sampler_GrainTex);

            TEXTURE2D(_HatchTex);
            SAMPLER(sampler_HatchTex);

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

                OUT.positionHCS = TransformObjectToHClip(posOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light mainLight = GetMainLight();

                float3 normalWS = normalize(IN.normalWS);
                float3 lightDir = normalize(mainLight.direction);

                float ndotl = dot(normalWS, -lightDir);

                float shadowMask = smoothstep(
                    _ShadowThreshold - _ShadowSoftness,
                    _ShadowThreshold + _ShadowSoftness,
                    ndotl
                );

                float3 litColor = _BaseColor.rgb * mainLight.color;
                float3 shadowColor = _ShadowColor.rgb;

                float3 finalColor = lerp(shadowColor, litColor, shadowMask);

                // Posterization
                float steps = max(_PosterizeSteps, 2.0);
                finalColor = floor(finalColor * steps + 0.5) / steps;

                // Screen-space grain
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                float2 grainUV = screenUV * _GrainScale;
                float grain = SAMPLE_TEXTURE2D(_GrainTex, sampler_GrainTex, grainUV).r;
                grain = saturate((grain - 0.5) * 1.6 + 0.5);

                float grainContrast = (grain - 0.5) * 2.0;

                float3 overlayColor = finalColor < 0.5
                    ? 2.0 * finalColor * (1.0 - grainContrast)
                    : 1.0 - 2.0 * (1.0 - finalColor) * (1.0 - grainContrast);

                finalColor = lerp(
                    finalColor,
                    overlayColor,
                    _GrainOverlayStrength * _GrainStrength
                );

                // Hatch texture
                float2 hatchUV = screenUV * _HatchScale;
                float hatch = SAMPLE_TEXTURE2D(_HatchTex, sampler_HatchTex, hatchUV).r;

                // Hatch mostly appears in shadow areas
                float hatchMask = 1.0 - shadowMask;

                float hatchAmount = lerp(
                    1.0,
                    hatch,
                    hatchMask * _HatchStrength
                );

                finalColor *= hatchAmount;

                return half4(saturate(finalColor), 1);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

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
                float4 _BaseColor;
                float4 _ShadowColor;

                float _ShadowThreshold;
                float _ShadowSoftness;

                float4 _GrainTex_ST;
                float _GrainStrength;
                float _GrainScale;
                float _GrainOverlayStrength;

                float4 _HatchTex_ST;
                float _HatchStrength;
                float _HatchScale;

                float _PosterizeSteps;

                float4 _WobbleMask_ST;
                float _WobbleStrength;
                float _WobbleSpeed;
                float _WobbleScale;
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

                OUT.positionHCS = TransformObjectToHClip(posOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }
}