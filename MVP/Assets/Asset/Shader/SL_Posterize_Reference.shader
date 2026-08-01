Shader "Custom/Orange Posterize Reference"
{
    Properties
    {
        [Header(Palette)]
        [HDR] _ShadowColor ("Shadow Color", Color) = (0.055, 0.010, 0.006, 1)
        [HDR] _DarkColor ("Dark Color", Color) = (0.035, 0.008, 0.004, 1)
        [HDR] _MidColor ("Mid Color", Color) = (0.28, 0.035, 0.012, 1)
        [HDR] _BrightColor ("Bright Color", Color) = (1.0, 0.16, 0.01, 1)
        [HDR] _HotColor ("Hot Color", Color) = (1.15, 0.32, 0.025, 1)

        [Header(Tone)]
        _Exposure ("Exposure", Range(0.1, 3.0)) = 0.85
        _Contrast ("Contrast", Range(0.2, 3.0)) = 1.15
        _BlackLift ("Black Lift", Range(0.0, 0.4)) = 0.045
        _WhitePoint ("White Point", Range(0.4, 2.0)) = 1.0

        [Header(Palette Positions)]
        _ShadowPoint ("Shadow Point", Range(0.0, 0.5)) = 0.16
        _DarkPoint ("Dark Point", Range(0.05, 0.7)) = 0.34
        _MidPoint ("Mid Point", Range(0.2, 0.9)) = 0.62
        _HighlightPoint ("Highlight Point", Range(0.5, 1.0)) = 0.84
        _TransitionSoftness ("Transition Softness", Range(0.001, 0.25)) = 0.065

        [Header(Posterize)]
        _Steps ("Posterize Steps", Range(2, 16)) = 8
        _PosterizeStrength ("Posterize Strength", Range(0, 1)) = 0.28
        _DitherStrength ("Dither Strength", Range(0, 0.08)) = 0.008

        [Header(Detail)]
        _SourceDetail ("Source Detail", Range(0, 1)) = 0.26
        _ShadowDetail ("Shadow Detail", Range(0, 1)) = 0.55
        _Saturation ("Orange Saturation", Range(0.5, 2.0)) = 1.0

        [Header(Grain)]
        _GrainStrength ("Grain Strength", Range(0, 0.08)) = 0.006
        _GrainScale ("Grain Scale", Range(64, 1200)) = 480

        [Header(Final)]
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
            Name "Orange Posterize Reference"

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
                float4 _ShadowColor;
                float4 _DarkColor;
                float4 _MidColor;
                float4 _BrightColor;
                float4 _HotColor;

                float _Exposure;
                float _Contrast;
                float _BlackLift;
                float _WhitePoint;

                float _ShadowPoint;
                float _DarkPoint;
                float _MidPoint;
                float _HighlightPoint;
                float _TransitionSoftness;

                float _Steps;
                float _PosterizeStrength;
                float _DitherStrength;

                float _SourceDetail;
                float _ShadowDetail;
                float _Saturation;

                float _GrainStrength;
                float _GrainScale;

                float _EffectStrength;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float3 AdjustSaturation(float3 color, float saturation)
            {
                float gray = dot(color, float3(0.2126, 0.7152, 0.0722));
                return lerp(gray.xxx, color, saturation);
            }

            float SoftBand(float value, float edge, float softness)
            {
                return smoothstep(edge - softness, edge + softness, value);
            }

            float3 PaletteMap(float value)
            {
                float s = max(_TransitionSoftness, 0.001);

                float w1 = SoftBand(value, _ShadowPoint, s);
                float w2 = SoftBand(value, _DarkPoint, s);
                float w3 = SoftBand(value, _MidPoint, s);
                float w4 = SoftBand(value, _HighlightPoint, s);

                float3 c = lerp(_ShadowColor.rgb, _DarkColor.rgb, w1);
                c = lerp(c, _MidColor.rgb, w2);
                c = lerp(c, _BrightColor.rgb, w3);
                c = lerp(c, _HotColor.rgb, w4);

                return c;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                half4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                );

                float luminance = dot(
                    source.rgb,
                    float3(0.2126, 0.7152, 0.0722)
                );

                // Tone shaping. Black lift prevents crushed shadows.
                luminance = max(0.0, luminance * _Exposure);
                luminance = (luminance + _BlackLift)
                            / max(_WhitePoint + _BlackLift, 0.001);

                luminance = saturate(
                    (luminance - 0.5) * _Contrast + 0.5
                );

                // Very small screen-space dithering avoids large blocky bands.
                float2 pixelPos = uv * _ScreenParams.xy;
                float dither = Hash21(floor(pixelPos)) - 0.5;
                float ditheredLum = saturate(
                    luminance + dither * _DitherStrength
                );

                // Blend quantized and continuous luminance.
                // This keeps the graphic look without making shadows pixelated.
                float steps = max(2.0, round(_Steps));
                float quantized = floor(ditheredLum * steps) / steps;
                float paletteLum = lerp(
                    luminance,
                    quantized,
                    saturate(_PosterizeStrength)
                );

                float3 mapped = PaletteMap(paletteLum);
                mapped = AdjustSaturation(mapped, _Saturation);

                // Preserve original material detail, especially inside shadows.
                float shadowMask = 1.0 - smoothstep(
                    _ShadowPoint,
                    _MidPoint,
                    luminance
                );

                float detailAmount = saturate(
                    _SourceDetail
                    + shadowMask * _ShadowDetail
                );

                float sourceLumSafe = max(
                    dot(source.rgb, float3(0.2126, 0.7152, 0.0722)),
                    0.02
                );

                float3 normalizedSource = source.rgb / sourceLumSafe;
                normalizedSource = clamp(normalizedSource, 0.35, 2.2);

                mapped *= lerp(
                    1.0.xxx,
                    normalizedSource,
                    detailAmount
                );

                // Subtle animated grain.
                float grain = Hash21(
                    floor(pixelPos / max(_GrainScale / 500.0, 0.25))
                    + floor(_Time.y * 18.0)
                ) - 0.5;

                mapped += grain * _GrainStrength;

                float3 finalColor = lerp(
                    source.rgb,
                    mapped,
                    saturate(_EffectStrength)
                );

                return half4(max(finalColor, 0.0), 1.0);
            }

            ENDHLSL
        }
    }

    Fallback Off
}
