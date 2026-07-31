Shader "Custom/URP Unreal Packed Mask Lit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Color", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color Tint", Color) = (1,1,1,1)

        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 2)) = 1

        [NoScaleOffset] _PackedMask(
            "Packed Mask (R=AO, G=Roughness, B=Metallic)", 2D
        ) = "white" {}

        _AOIntensity("AO Intensity", Range(0, 1)) = 1
        _MetallicMultiplier("Metallic Multiplier", Range(0, 1)) = 1
        _SmoothnessMultiplier("Smoothness Multiplier", Range(0, 1)) = 1

        [Toggle(_SPECULARHIGHLIGHTS_OFF)]
        _SpecularHighlights("Specular Highlights", Float) = 1

        [Toggle(_ENVIRONMENTREFLECTIONS_OFF)]
        _EnvironmentReflections("Environment Reflections", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
        }

        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            // Main light and shadows
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // Additional lights
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            // URP lighting variants
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            TEXTURE2D(_PackedMask);
            SAMPLER(sampler_PackedMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _NormalStrength;
                half _AOIntensity;
                half _MetallicMultiplier;
                half _SmoothnessMultiplier;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;

                float3 positionWS : TEXCOORD1;
                half3 normalWS   : TEXCOORD2;
                half4 tangentWS  : TEXCOORD3;

                float4 shadowCoord : TEXCOORD4;
                half fogFactor     : TEXCOORD5;

                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 6);

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                output.normalWS = normalInputs.normalWS;
                output.tangentWS = half4(
                    normalInputs.tangentWS,
                    input.tangentOS.w * GetOddNegativeScale()
                );

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                OUTPUT_LIGHTMAP_UV(
                    input.lightmapUV,
                    unity_LightmapST,
                    output.lightmapUV
                );
                OUTPUT_SH(output.normalWS, output.vertexSH);

                return output;
            }

            void BuildSurfaceData(
                Varyings input,
                out SurfaceData surfaceData
            )
            {
                surfaceData = (SurfaceData)0;

                half4 baseSample =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv)
                    * _BaseColor;

                half4 packed =
                    SAMPLE_TEXTURE2D(_PackedMask, sampler_PackedMask, input.uv);

                // Unreal-style packed mask:
                // R = Ambient Occlusion
                // G = Roughness
                // B = Metallic
                half ao = packed.r;
                half roughness = packed.g;
                half metallic = packed.b;

                surfaceData.albedo = baseSample.rgb;
                surfaceData.alpha = 1.0h;

                surfaceData.metallic =
                    saturate(metallic * _MetallicMultiplier);

                surfaceData.specular = half3(0.0h, 0.0h, 0.0h);

                surfaceData.smoothness =
                    saturate((1.0h - roughness) * _SmoothnessMultiplier);

                surfaceData.normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(
                        _NormalMap,
                        sampler_NormalMap,
                        input.uv
                    ),
                    _NormalStrength
                );

                surfaceData.occlusion =
                    lerp(1.0h, ao, _AOIntensity);

                surfaceData.emission = half3(0.0h, 0.0h, 0.0h);
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;
            }

            void BuildInputData(
                Varyings input,
                half3 normalTS,
                out InputData inputData
            )
            {
                inputData = (InputData)0;

                half3 bitangentWS =
                    input.tangentWS.w
                    * cross(input.normalWS, input.tangentWS.xyz);

                half3x3 tangentToWorld = half3x3(
                    input.tangentWS.xyz,
                    bitangentWS,
                    input.normalWS
                );

                half3 normalWS =
                    TransformTangentToWorld(normalTS, tangentToWorld);

                normalWS = NormalizeNormalPerPixel(normalWS);

                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS =
                    GetWorldSpaceNormalizeViewDir(input.positionWS);

                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting =
                    VertexLighting(input.positionWS, normalWS);

                inputData.bakedGI = SAMPLE_GI(
                    input.lightmapUV,
                    input.vertexSH,
                    normalWS
                );

                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(input.positionCS);

                inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                SurfaceData surfaceData;
                BuildSurfaceData(input, surfaceData);

                InputData inputData;
                BuildInputData(input, surfaceData.normalTS, inputData);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = 1.0h;

                return color;
            }

            ENDHLSL
        }

        // Reuse URP Lit passes so the object casts shadows and participates
        // in depth rendering correctly.
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
