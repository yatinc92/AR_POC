Shader "Custom/URP/MilkyWayGalaxySprite" // Renamed for clarity
{
    Properties
    {
        _MainTex ("Galaxy Texture", 2D) = "white" {} // No longer "Pano"
        _Exposure ("Exposure Multiplier", Range(0.1, 5.0)) = 1.0
        _Color ("Tint Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" } // Important for transparency
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForwardOnly" }
            Cull Off // Render both sides of the quad
            ZWrite Off // Don't write to depth buffer to allow transparency sorting
            Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST; 

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Exposure;
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

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                half4 galaxyColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Use the alpha channel from the texture for transparency
                half3 finalColor = galaxyColor.rgb * _Color.rgb * _Exposure;
                
                return half4(finalColor, galaxyColor.a); // Pass through alpha
            }
            ENDHLSL
        }
    }
}