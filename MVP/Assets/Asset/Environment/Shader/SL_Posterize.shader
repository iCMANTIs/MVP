Shader "Custom/Orange Posterize"
{
    Properties
    {
        [HDR] _DarkColor ("Dark Color", Color) = (0.035, 0.008, 0.004, 1)
        [HDR] _MidColor ("Mid Color", Color) = (0.28, 0.035, 0.012, 1)
        [HDR] _BrightColor ("Bright Color", Color) = (1.0, 0.16, 0.01, 1)
        [HDR] _HotColor ("Hot Color", Color) = (1.15, 0.32, 0.025, 1)

        _Exposure ("Exposure", Range(0.1, 4.0)) = 1.1
        _Contrast ("Contrast", Range(0.1, 5.0)) = 2.0
        _Steps ("Posterize Steps", Range(2, 12)) = 5

        _ShadowPoint ("Shadow Point", Range(0, 1)) = 0.22
        _MidPoint ("Mid Point", Range(0, 1)) = 0.50
        _HighlightPoint ("Highlight Point", Range(0, 1)) = 0.78

        _GrainStrength ("Grain Strength", Range(0, 0.15)) = 0.015
        _GrainScale ("Grain Scale", Range(1, 1000)) = 500

        _EffectStrength ("Effect Strength", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Orange Posterize"

            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _DarkColor;
                float4 _MidColor;
                float4 _BrightColor;
                float4 _HotColor;

                float _Exposure;
                float _Contrast;
                float _Steps;

                float _ShadowPoint;
                float _MidPoint;
                float _HighlightPoint;

                float _GrainStrength;
                float _GrainScale;
                float _EffectStrength;
            CBUFFER_END

            float RandomNoise(float2 uv)
            {
                float2 pixelUV = floor(uv * _GrainScale);

                return frac(
                    sin(
                        dot(
                            pixelUV + floor(_Time.y * 24.0),
                            float2(12.9898, 78.233)
                        )
                    ) * 43758.5453
                );
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                half4 sourceColor = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                );

                // 原画面亮度
                float luminance = dot(
                    sourceColor.rgb,
                    float3(0.2126, 0.7152, 0.0722)
                );

                // 比原版本温和得多的曝光与对比度
                luminance *= _Exposure;
                luminance = saturate(
                    (luminance - 0.5) * _Contrast + 0.5
                );

                // Posterize，但保留更多层级
                float steps = max(2.0, round(_Steps));
                float posterized = floor(luminance * steps) / steps;

                // 四段颜色映射
                float3 mappedColor;

                if (posterized < _ShadowPoint)
                {
                    float t = posterized / max(_ShadowPoint, 0.001);

                    mappedColor = lerp(
                        _DarkColor.rgb,
                        _MidColor.rgb,
                        t
                    );
                }
                else if (posterized < _MidPoint)
                {
                    float t = (
                        posterized - _ShadowPoint
                    ) / max(_MidPoint - _ShadowPoint, 0.001);

                    mappedColor = lerp(
                        _MidColor.rgb,
                        _BrightColor.rgb,
                        t
                    );
                }
                else
                {
                    float t = (
                        posterized - _MidPoint
                    ) / max(1.0 - _MidPoint, 0.001);

                    mappedColor = lerp(
                        _BrightColor.rgb,
                        _HotColor.rgb,
                        t
                    );
                }

                /*
                 * 关键修改：
                 * 不再直接用纯橙色覆盖画面。
                 * 使用原始亮度重新调制颜色，从而保留角色、
                 * 建筑、阴影和模型表面的明暗差异。
                 */
                float detailMultiplier = lerp(
                    0.35,
                    1.25,
                    luminance
                );

                mappedColor *= detailMultiplier;

                // 保留一部分原始画面，避免所有东西糊成一片
                float3 finalColor = lerp(
                    sourceColor.rgb,
                    mappedColor,
                    saturate(_EffectStrength)
                );

                return half4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }

    Fallback Off
}