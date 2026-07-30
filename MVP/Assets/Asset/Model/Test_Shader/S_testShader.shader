Shader "Custom/Unreal Packed Mask"
{
    Properties
    {
        _Color ("Color Tint", Color) = (1,1,1,1)

        _MainTex ("Base Color", 2D) = "white" {}

        [Normal]
        _NormalMap ("Normal Map", 2D) = "bump" {}

        [NoScaleOffset]
        _PackedMask ("Packed Mask (R: AO, G: Roughness, B: Metallic)", 2D) = "white" {}

        _NormalStrength ("Normal Strength", Range(0, 2)) = 1

        _AOIntensity ("AO Intensity", Range(0, 1)) = 1

        _MetallicMultiplier ("Metallic Multiplier", Range(0, 1)) = 1

        _SmoothnessMultiplier ("Smoothness Multiplier", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        LOD 300

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _PackedMask;

        fixed4 _Color;

        half _NormalStrength;
        half _AOIntensity;
        half _MetallicMultiplier;
        half _SmoothnessMultiplier;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float2 uv_PackedMask;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Base Color
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = baseColor.rgb;
            o.Alpha = baseColor.a;

            // Normal
            fixed3 normal = UnpackNormal(
                tex2D(_NormalMap, IN.uv_NormalMap)
            );

            normal.xy *= _NormalStrength;
            o.Normal = normalize(normal);

            // Packed Mask
            fixed4 packedMask = tex2D(
                _PackedMask,
                IN.uv_PackedMask
            );

            // R = Ambient Occlusion
            half ao = packedMask.r;
            o.Occlusion = lerp(1.0, ao, _AOIntensity);

            // G = Roughness
            half roughness = packedMask.g;
            half smoothness = 1.0 - roughness;
            o.Smoothness = saturate(
                smoothness * _SmoothnessMultiplier
            );

            // B = Metallic
            o.Metallic = saturate(
                packedMask.b * _MetallicMultiplier
            );
        }

        ENDCG
    }

    FallBack "Standard"
}