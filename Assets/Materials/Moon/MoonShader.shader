// URP Moon Phase Unlit Shader
Shader "Custom/URP/MoonPhaseUnlitGlow" // Renamed for clarity
{
    Properties
    {
        _MainTex ("Moon Texture", 2D) = "white" {}
        _Illumination ("Illumination", Range(0,1)) = 0.5
        _Phase ("Phase", Range(0,1)) = 0.5
        _GlowMultiplier ("HDR Glow Multiplier", Range(1, 10)) = 2.0 // New Property
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "UniversalForwardOnly" }
            Blend Off
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            // ------------------------------------
            // Texture and Sampler Declaration
            // ------------------------------------
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST; 

            CBUFFER_START(UnityPerMaterial)
            float _Illumination;
            float _Phase;
            float _GlowMultiplier; // New variable
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // ------------------------------------
            // Vertex Shader
            // ------------------------------------
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            // ------------------------------------
            // Fragment Shader (Modified for Glow)
            // ------------------------------------
            half4 frag(Varyings input) : SV_TARGET
            {
                // 1. Sample the texture
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // 2. Calculate the phase effect
                float phaseEffect = 1.0 - abs(_Phase - 0.5) * 2.0;
                
                // 3. Apply illumination and phase
                color.rgb *= _Illumination * phaseEffect;

                // 4. Apply the HDR Glow Multiplier (makes it self-illuminate strongly)
                // This makes the output color HDR (> 1.0), triggering URP Bloom.
                color.rgb *= _GlowMultiplier;

                return color;
            }
            ENDHLSL
        }
    }
}