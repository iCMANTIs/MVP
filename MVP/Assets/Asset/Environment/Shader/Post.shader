Shader "Custom/Post"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (1.0, 0.35, 0.05, 1)
        _Contrast ("Contrast", Range(0, 3)) = 1.6
        _Brightness ("Brightness", Range(-1, 1)) = 0
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.45
        _VignetteRadius ("Vignette Radius", Range(0, 1)) = 0.65
        _PosterizeSteps ("Posterize Steps", Range(2, 16)) = 6

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineStrength ("Outline Strength", Range(0, 1)) = 1
        _OutlineThickness ("Outline Thickness", Range(0.5, 5)) = 1

        _DepthThreshold ("Depth Threshold", Range(0.0001, 0.2)) = 0.03
        _NormalThreshold ("Normal Threshold", Range(0.001, 1)) = 0.25
        _NormalOutlineStrength ("Normal Outline Strength", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "Post"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor;
                float _Contrast;
                float _Brightness;
                float _VignetteStrength;
                float _VignetteRadius;
                float _PosterizeSteps;

                float4 _OutlineColor;
                float _OutlineStrength;
                float _OutlineThickness;
                float _DepthThreshold;
                float _NormalThreshold;
                float _NormalOutlineStrength;
            CBUFFER_END

            float IsSky(float rawDepth)
            {
                return rawDepth >= 0.9999;
            }

            float GetLinearDepth(float2 uv)
            {
                float rawDepth = SampleSceneDepth(uv);

                if (IsSky(rawDepth))
                    return -1.0;

                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            float GetDepthEdge(float2 uv, float2 offset)
            {
                float centerDepth = GetLinearDepth(uv);

                if (centerDepth < 0)
                    return 0;

                float dL = GetLinearDepth(uv + float2(-offset.x, 0));
                float dR = GetLinearDepth(uv + float2( offset.x, 0));
                float dD = GetLinearDepth(uv + float2(0, -offset.y));
                float dU = GetLinearDepth(uv + float2(0,  offset.y));

                if (dL < 0) dL = centerDepth;
                if (dR < 0) dR = centerDepth;
                if (dD < 0) dD = centerDepth;
                if (dU < 0) dU = centerDepth;

                float diff = 0;
                diff += abs(centerDepth - dL);
                diff += abs(centerDepth - dR);
                diff += abs(centerDepth - dD);
                diff += abs(centerDepth - dU);

                diff /= max(centerDepth, 0.0001);

                return smoothstep(_DepthThreshold, _DepthThreshold * 2.0, diff);
            }

            float GetNormalEdge(float2 uv, float2 offset)
            {
                float centerDepth = GetLinearDepth(uv);

                if (centerDepth < 0)
                    return 0;

                float3 nC = normalize(SampleSceneNormals(uv));
                float3 nL = normalize(SampleSceneNormals(uv + float2(-offset.x, 0)));
                float3 nR = normalize(SampleSceneNormals(uv + float2( offset.x, 0)));
                float3 nD = normalize(SampleSceneNormals(uv + float2(0, -offset.y)));
                float3 nU = normalize(SampleSceneNormals(uv + float2(0,  offset.y)));

                float diff = 0;
                diff += 1.0 - saturate(dot(nC, nL));
                diff += 1.0 - saturate(dot(nC, nR));
                diff += 1.0 - saturate(dot(nC, nD));
                diff += 1.0 - saturate(dot(nC, nU));

                return smoothstep(_NormalThreshold, _NormalThreshold * 2.0, diff);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float3 color = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                ).rgb;

                // Contrast / Brightness
                color = (color - 0.5) * _Contrast + 0.5;
                color += _Brightness;
                color = saturate(color);

                // Orange-black tint
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                float3 grayscale = float3(luminance, luminance, luminance);
                float3 tintedColor = color * _TintColor.rgb;
                color = lerp(grayscale, tintedColor, 0.75);

                // Posterize
                float steps = max(_PosterizeSteps, 2.0);
                color = floor(color * steps + 0.5) / steps;

                // Sobel / Edge
                float2 pixelSize = 1.0 / _ScaledScreenParams.xy;
                float2 offset = pixelSize * _OutlineThickness;

                float depthEdge = GetDepthEdge(uv, offset);
                float normalEdge = GetNormalEdge(uv, offset) * _NormalOutlineStrength;

                float edge = saturate(max(depthEdge, normalEdge));
                edge *= _OutlineStrength;

                color = lerp(color, _OutlineColor.rgb, edge);

                // Vignette
                float dist = distance(uv, float2(0.5, 0.5));
                float vignette = smoothstep(_VignetteRadius, 1.0, dist);
                color *= 1.0 - vignette * _VignetteStrength;

                return half4(saturate(color), 1);
            }

            ENDHLSL
        }
    }
}