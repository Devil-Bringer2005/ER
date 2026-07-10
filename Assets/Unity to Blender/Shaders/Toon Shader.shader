Shader "Custom/ToonShader"
{
    Properties
    {   
        [Header(Main Maps)]
        [Space]
        [Space]
        [MainColor] _MainColor("Main Color", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex("Main Tex", 2D) = "white"

        [Header(Band Settings)]
        [Space]

        // 8 Level Banding 
        [Space]
        _WhiteThreshold("White Threshold", Float) = 8
        _WhiteTint("White Tint", Float) = 4
        [Space]
        _HighlightThreshold("Highlight Threshold", Float) = 4.4
        _HighlightTint("Highlight Tint", Float) = 2
        [Space]
        _NeutralThreshold("Neutral Threshold", Float) = 1.3
        _NeutralTint("Neutral Tint", Float) = 1.25
        [Space]
        _ShadowThreshold("Shadow Threshold", Float) = 0.7
        _ShadowTint("Shadow Tint", Float) = 0.75
        [Space]
        _DeepShadowThreshold("Deep Shadow Threshold", Float) = 0.3
        _DeepShadowTint("Deep Shadow Tint", Float) = 0.5
        [Space]
        _DeeperShadowThreshold("Deeper Shadow Threshold", Float) = 0.06
        _DeeperShadowTint("Deeper Shadow Tint", Float) = 0.35
        [Space]
        _DeepestShadowThreshold("Deepest Shadow Threshold", Float) = 0.03
        _DeepestShadowTint("Deepest Shadow Tint", Float) = 0.125
        [Space]
        _BlackThreshold("Black Threshold", Float) = 0.01
        _BlackTint("Black Tint", Float) = 0
        // [Space]
        // _BandBlendThreshold("Band Blend Threshold", Float) = 0.5
        // _BandBlendSmoothness("Band Blend Smoothness", Float) = 0.03

    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _LIGHTS_PER_OBJECT
            
            // forward +
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct MeshData
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS: TEXCOORD1;
                float3 wPos : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float3 normalOS : TEXCOORD4;
                float3 positionOS : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex; 

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _MainTex_ST;

                float  _WhiteThreshold;
                float  _WhiteTint;

                float  _HighlightThreshold;
                float  _HighlightTint;

                float  _NeutralThreshold;
                float  _NeutralTint;

                float  _ShadowThreshold;
                float  _ShadowTint;

                float  _DeepShadowThreshold;
                float  _DeepShadowTint;

                float  _DeeperShadowThreshold;
                float  _DeeperShadowTint;

                float  _DeepestShadowThreshold;
                float  _DeepestShadowTint;

                float  _BlackThreshold;
                float  _BlackTint;

                // float _BandBlendThreshold;
                // float _BandBlendSmoothness;

            CBUFFER_END

            Interpolators vert(MeshData IN)
            {
                Interpolators OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal);
                OUT.normalOS = IN.normal;
                OUT.positionOS = IN.positionOS.xyz;
                OUT.wPos = mul(unity_ObjectToWorld , IN.positionOS).xyz;
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.wPos);
                return OUT;
            }

            float4 frag(Interpolators IN) : SV_Target
            {   
                // Lighting Data
                Light mainLight = GetMainLight(IN.shadowCoord);
                float3 mlDirection = mainLight.direction;
                float3 mlColor = mainLight.color;
                float mldistanceAtt = mainLight.distanceAttenuation;
                float mlshadowAtt = mainLight.shadowAttenuation;

                float3 N = normalize(IN.normalOS); // NORMAL
                float3 L = normalize(mlDirection); // Light direction
                float3 V = normalize(_WorldSpaceCameraPos - IN.wPos); // View Angle
                float3 R = reflect(-L,N); // Relect Angle
                
                // Diffuse Banding
                float diffuseColor = saturate(dot(N,L));
                float band = 0;
                float smoothBand = 0;

                // Hard Bandings
                band = step(_WhiteThreshold, diffuseColor) * _WhiteTint +
                       step(_HighlightThreshold, diffuseColor) *  _HighlightTint +
                       step(_NeutralThreshold, diffuseColor) *  _NeutralTint +
                       step(_ShadowThreshold, diffuseColor) * _ShadowTint +
                       step(_DeepShadowThreshold, diffuseColor) *  _DeepShadowTint +
                       step(_DeeperShadowThreshold, diffuseColor) *  _DeeperShadowTint +
                       step(_DeepestShadowThreshold, diffuseColor) * _DeepestShadowTint +
                       (1 - step(_DeepestShadowThreshold, diffuseColor)) * _BlackTint;

                // smoothBand = smoothstep(_BandBlendThreshold,_BandBlendSmoothness,band);
                
                // diffuseColor = smoothBand;
                diffuseColor = band;
                float4 mainTex = tex2D(_MainTex,IN.uv);

                float4 finalOutput = mainTex * _MainColor * diffuseColor * float4(mlColor.rgb,1);
                return finalOutput;
            }
            ENDHLSL
        }
    }
}
