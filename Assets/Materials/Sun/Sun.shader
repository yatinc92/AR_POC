Shader "Custom/URP/RealisticSun"
{
    Properties
    {
        _SunColor ("Sun Color (HDR)", Color) = (1.0, 0.7, 0.2, 1.0)
        _CoreRadius ("Core Radius", Range(0.0, 1.0)) = 0.5
        _GlowIntensity ("Glow Intensity", Range(0.0, 10.0)) = 2.0
        _GlowFalloff ("Glow Falloff", Range(1.0, 20.0)) = 5.0
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 1.0
        _NoiseSpeed ("Noise Speed", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "UniversalForwardOnly" }
            Cull Back
            ZWrite On
            Blend Off // Render as opaque, relying on HDR and bloom for glow

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            // ------------------------------------
            // Texture and Properties
            // ------------------------------------
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            float4 _NoiseTex_ST; 

            CBUFFER_START(UnityPerMaterial)
            float4 _SunColor;
            float _CoreRadius;
            float _GlowIntensity;
            float _GlowFalloff;
            float _NoiseScale;
            float _NoiseSpeed;
            CBUFFER_END

            // ------------------------------------
            // Structs
            // ------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL; // Object space normal
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0; // World space normal
                float3 viewDirWS : TEXCOORD1; // World space view direction
                float2 uv : TEXCOORD2;
            };

            // ------------------------------------
            // Vertex Shader
            // ------------------------------------
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = _WorldSpaceCameraPos.xyz - TransformObjectToWorld(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _NoiseTex);
                return output;
            }

            // ------------------------------------
            // Fragment Shader
            // ------------------------------------
            half4 frag(Varyings input) : SV_TARGET
            {
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);

                // Calculate view-dependent glow (facing camera)
                // dot(normal, viewDir) gives 1 when facing away, -1 when facing towards.
                // We want glow when facing towards camera or glancing angle.
                float dotNV = 1.0 - saturate(dot(normalWS, viewDirWS)); // 1 at glancing, 0 at straight on

                // Add some subtle noise for atmospheric variation
                float2 noiseUV = input.uv * _NoiseScale + _Time.y * _NoiseSpeed;
                float noiseVal = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;
                dotNV += noiseVal * 0.1; // Small influence from noise

                // Core glow
                float coreGlow = smoothstep(0.0, _CoreRadius, dotNV); 
                
                // Outer glow falloff
                float outerGlow = pow(dotNV, _GlowFalloff) * _GlowIntensity;

                // Combine and apply color
                half3 finalColor = (_SunColor.rgb * coreGlow + _SunColor.rgb * outerGlow);
                
                // Return HDR color (important for bloom)
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}