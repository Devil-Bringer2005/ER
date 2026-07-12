Shader "UI/HealthBarUIShader"
{
    Properties
    {
        [PerRendererData]_MainTex("Main Texture",2D)="white"{}
        [NoScaleOffset]_MaskTex("Mask Texture",2D)="white"{}

        [HDR]_MainColor1("Main Color A",Color)=(1,1,1,1)
        [HDR]_MainColor2("Main Color B",Color)=(1,1,1,1)

        _ColorStart("Color Start",Range(0,1))=0
        _ColorEnd("Color End",Range(0,1))=1

        _HealthValue("Health Value",Range(0,1))=1

        _AnimateXY("Animate XY",Vector)=(0,0,0,0)

        _HealthFlashValue("Health Flashing Value",Range(0,1))=1
        _FlickerSpeed("Flicker Speed",Float)=5

        //------------------------------------------------------
        // HDR Glow
        //------------------------------------------------------

        [HDR]_GlowColor("Glow Color",Color)=(1,0.2,0.2,1)
        _GlowIntensity("Glow Intensity",Range(0,20))=4
        _GlowPulse("Glow Pulse",Range(0,1))=0
        _GlowPulseSpeed("Glow Pulse Speed",Float)=3

        //------------------------------------------------------
        // Unity UI
        //------------------------------------------------------

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float)=0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Interpolators
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvGradient : TEXCOORD1;
                float2 uvMask : TEXCOORD2;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)

            float4 _MainTex_ST;
            float4 _MaskTex_ST;

            float4 _MainColor1;
            float4 _MainColor2;

            float _ColorStart;
            float _ColorEnd;

            float _HealthValue;

            float4 _AnimateXY;

            float _HealthFlashValue;
            float _FlickerSpeed;

            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowPulse;
            float _GlowPulseSpeed;

            CBUFFER_END

            float4 _ClipRect;

            Interpolators vert(MeshData IN)
            {
                Interpolators OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.vertex.xyz);

                OUT.worldPosition = IN.vertex;

                OUT.uv = TRANSFORM_TEX(IN.uv0,_MainTex);
                OUT.uvMask = TRANSFORM_TEX(IN.uv0,_MaskTex);

                OUT.uvGradient = IN.uv0;

                OUT.color = IN.color;

                OUT.uv += frac(_AnimateXY.xy * _MainTex_ST.xy * _Time.yy);

                return OUT;
            }

            float InverseLerp(float a,float b,float v)
            {
                return (v-a)/(b-a);
            }

            float4 frag(Interpolators IN) : SV_Target
            {
                float4 texColor = IN.color;

                float4 mainTexture = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float4 movingTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, IN.uvMask);

                float t = saturate(InverseLerp(_ColorStart, _ColorEnd, IN.uvGradient.x));

                float4 outputColor = lerp(_MainColor1, _MainColor2, t);
                float4 outputColorGradient = lerp(_MainColor1, _MainColor2, _HealthValue);

                float barRevealAmount = step(IN.uvGradient.x, _HealthValue);

                float4 finalOutput =
                    float4(mainTexture.rgb, mainTexture.a * barRevealAmount) *
                    outputColor *
                    texColor *
                    movingTex;

                if (_HealthValue <= _HealthFlashValue)
                {
                    float flickerAmount = saturate(0.8 + 0.15 * sin(_Time.y * _FlickerSpeed));
                    finalOutput *= flickerAmount;
                }

                return outputColorGradient * finalOutput;
            }
            ENDHLSL
        }
    }

    Fallback "UI/Default"
}