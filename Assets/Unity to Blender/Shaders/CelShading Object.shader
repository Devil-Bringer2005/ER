Shader "Custom/CelShadingObject"
{
     Properties
    {   
        [Header(Main Textures)]
        [Space]
        [Space]
        [HDR] [MainColor] _MainColor("Main Color", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex("Main Texture" , 2D) = "white" {}
        _ShadeTex1("Shaded Tex" ,2D) = "white" {}
        _ShadeStrength("Shade Strength" ,Range(0,1)) = 1
        
        [Space]
        [Space]
        [Header(Diffuse properties)]
        [Space]
        [Space]
        _DiffuseColor("Diffuse Color" , Color) = (1,1,1,1)
        _DiffuseStrength("Diffuse Strength" , Float) = 1
        _DiffuseThreshold("Diffuse Threshold", Float) = 0.01
        _DiffuseSmoothness("Diffuse Smoothness" , Float) = 0.01
       
        [Space]
        [Space]
        [Header(Specular properties)]
        [Space]
        [Space]
        [ToggleUI] _UseNormal ("Use Normal", Float) = 0
        [ToggleUI] _UseBlingPhong("Bling Phong", Float) = 0
        [NoScaleOffset][Normal] _NormalTex("Normal Texture" , 2D) = "bump" {}
        _NormalStrength("Normal Strength" , Range(-2,2)) = 1

        [Space]
        [Space]
        [Header(Gloss properties)]
        [Space]
        [Space]
        _SpecularColor("Specular Color" , Color) = (1,1,1,1)
        _Gloss("Glossiness" , Range(0,1)) = 0.5
        _SpecularThreshold("Specular Threshold" , Float) = 0.1
        _SpecularSmoothness("Specular Smoothness" , Float) = 0.1

        [Space]
        [Space]
        [Header(Fresnel properties)]
        [Space]
        [Space]
        [ToggleUI] _UseFresnel ("Use Fresnel", Float) = 0
        _FresnelColor("Fresnel Color" , Color) = (1,1,1,1)
        _FresnelIntensity("Fresnel Intensity" , Float) = 1
        _FresnelThreshold("Fresnel Threshold" , Float) = 0.1
        _FresnelSmoothness("Fresnel Smoothness" , Float) = 0.1

        [Space]
        [Space]
        [Header(Ambient properties)]
        [Space]
        [Space]
        [ToggleUI] _UseAmbient ("Use Ambient", Float) = 0
        _AmbientStrength("Ambient Strength" , Float) = 1

        [Space]
        [Space]
        [Header(Emission properties)]
        [Space]
        [Space]
        [ToggleUI] _UseEmission ("Use Emission", Float) = 0
        _EmissionTex("Emission Texture" , 2D) = "" {}
        _EmissionColor("Emission Color" , Color) = (1,1,1,1)
        _EmissionIntensity("Emission Intensity" , Float) = 1
        
        [Space]
        [Space]
        [Header(Hatching properties)]
        [Space]
        [Space]
        [ToggleUI] _UseHatching ("Use Hatching", Float) = 0
        [HDR] _HatchColor("Hatch Color" , Color) = (1,1,1,1)
        _HatchTex("Hatch Texture" , 2D) = "white" {}
        _HatchMask("Hatch mask" ,2D) = "white" {}
        _HatchThreshold("Hatch Threshold", Float) = 0.01
        _HatchSmoothness("Hatch Smoothness" , Float) = 0.01
        _HatchOpacity("Hatch Opacity" , Float) = 0.5
 
        [Space]
        [Space]
        [ToggleUI] _UseAdditionalLights("Use Additional Lights" , Float) = 0

        // [Space]
        // [Space]
        // [Header(Blending)]
        // [Space]
        // [Space]
        // [Enum(UnityEngine.Rendering.BlendMode)]
        //     _SrcFactor("Src Factor", Float) = 5

        // [Enum(UnityEngine.Rendering.BlendMode)]
        //     _DstFactor("Dst Factor" , Float) = 10

        // [Enum(UnityEngine.Rendering.BlendOp)]
        //     _Opp("Operation" , Float) = 0     
        
    }

    SubShader
    {   
        Name "CelShadingPass"

        Tags { "RenderType" = "Opaque" 
                "Queue" = "Geometry"
                "RenderPipeline" = "UniversalPipeline" 
             }

        Blend One Zero
        BlendOp Add
        ZWrite On

        // BlendOp [_Opp]
             
        Pass
        {   
            Name "Forward Pass"
            Tags { 
                "RenderType" = "Opaque" 
                "Queue" = "Geometry"
                "LightMode" = "UniversalForward" }


            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
        

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
         

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _LIGHTS_PER_OBJECT
            
            // forward +
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            // #pragma multi_compile_instancing

            
            
            // #pragma shader_feature_fragment _Main

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct MeshData
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD11;
                float3 normalWS: TEXCOORD1;
                float3 wPos : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float3 normalOS : TEXCOORD4;
                float3 positionOS : TEXCOORD6;
                float2 hatchUV : TEXCOORD7; 
                float  fogCoord	: TEXCOORD8;
                float3 tangentWS : TEXCOORD9;
                float3 bitangentWS : TEXCOORD10;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _MainTex_ST;
                float4 _HatchTex_ST;
                float4 _ShadeTex1_ST;
                float4 _HatchMask_ST;

                float _ShadeStrength;

                float4 _DiffuseColor;
                float4 _SpecularColor;
                float4 _FresnelColor;
                float4 _EmissionColor;
                float4 _HatchColor;

                float _DiffuseStrength;
                float _DiffuseThreshold;
                float _DiffuseSmoothness;

                float _UseNormal;
                float _UseBlingPhong;
                float _NormalStrength;
                float _Gloss;
                float _SpecularThreshold;
                float _SpecularSmoothness;

                float _UseFresnel;
                float _FresnelIntensity;
                float _FresnelThreshold;
                float _FresnelSmoothness;

                float _UseAmbient;
                float _AmbientStrength;

                float _UseEmission;
                float _EmissionIntensity;

                float _UseHatching;
                float _HatchThreshold;
                float _HatchSmoothness;
                float _HatchOpacity;
           
                float _UseAdditionalLights;
            CBUFFER_END

            sampler2D _MainTex;
            sampler2D _ShadeTex1;
            sampler2D _EmissionTex;
            sampler2D _NormalTex;
            sampler2D _HatchTex;
            sampler2D _DissolveTex;
            sampler2D _HatchMask;
            sampler2D _HeightTex;
            // sampler2D _MetallicMap;      

            Interpolators vert(MeshData IN)
            {   
                Interpolators OUT = (Interpolators)0;

                // UNITY_SETUP_INSTANCE_ID(IN);
                // UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 positionOS = IN.positionOS.xyz;
                OUT.positionHCS = TransformObjectToHClip(positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.uv1 = TRANSFORM_TEX(IN.uv, _ShadeTex1);
                OUT.hatchUV = TRANSFORM_TEX(IN.uv, _HatchTex);

                OUT.normalOS = IN.normal;
                OUT.positionOS = positionOS;
                OUT.wPos = TransformObjectToWorld(positionOS);

                float3 normalWS = TransformObjectToWorldNormal(IN.normal);
                float3 tangentWS = TransformObjectToWorldDir(IN.tangent.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangent.w;

                OUT.normalWS = normalWS;
                OUT.tangentWS = tangentWS;
                OUT.bitangentWS = bitangentWS;

                //OUT.shadowCoord = TransformWorldToShadowCoord(OUT.wPos);
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag(Interpolators IN) : SV_Target
            {  
                // UNITY_SETUP_INSTANCE_ID(IN);

                // Lighting Data
                float4 shadowCoord = TransformWorldToShadowCoord(IN.wPos);
                Light mainLight = GetMainLight(shadowCoord);
                float3 mlDirection = mainLight.direction;
                float3 mlColor = mainLight.color;
                float mldistanceAtt = mainLight.distanceAttenuation;
                float mlshadowAtt = mainLight.shadowAttenuation;
                float shadow = mlshadowAtt * mldistanceAtt;
               
                half3 N = normalize(IN.normalWS); //NORMAL
                half3 L = mlDirection; // Light direction
                half3 V = normalize(_WorldSpaceCameraPos - IN.wPos); // View Angle
                
                // Diffuse Lighting (CelShading)
                float4 texColor = tex2D(_MainTex,IN.uv);
                half diffuseLight = saturate(dot(N,L)) * 0.5 + 0.5;
                half diffuseSmooth = smoothstep(_DiffuseThreshold , (_DiffuseThreshold + _DiffuseSmoothness) , diffuseLight);
                half maxDiffuse = max(diffuseSmooth, _DiffuseStrength);

                // Shaded Texture Blend
                half3 shadedAlbedo = texColor.rgb;

                if(_ShadeStrength > 0.001)
                {
                    float3 shadeTex = tex2D(_ShadeTex1,IN.uv1).rgb;
                    shadedAlbedo = lerp(texColor.rgb,texColor.rgb * shadeTex.rgb,_ShadeStrength);
                }
                shadedAlbedo *= _MainColor.rgb;

                half3 finalDiffuse = maxDiffuse * mlColor * shadow * shadedAlbedo * _DiffuseColor.rgb ;

                // Specular Lighting
                half specularLight = 0;
                half specularExponent = 0; 
                half specularSmooth = 0;
                half3 finalSpecular = 0;

                if(_UseNormal > 0.5)
                {
                    if (_UseBlingPhong > 0.5)
                    {
                        // Specualar Lighting (Realistic) (Bling Phong)
                        // Texture to normal map convertion 
                        float3 normalTS = UnpackNormal(tex2D(_NormalTex,IN.uv)); 

                        // Normal and Tangent
                        normalTS.xy *= _NormalStrength;  // normal intensity
                        normalTS = normalize(normalTS);
               
                        float3x3 TBN = float3x3(
                        normalize(IN.tangentWS),
                        normalize(IN.bitangentWS),
                        normalize(IN.normalWS)
                        );

                        half3 H = normalize(L + V); // Half Vector
                        float3 Nt = normalize(mul(normalTS,TBN));
                        specularLight = saturate(dot(H,Nt)) * (finalDiffuse.x > 0);
                        specularExponent = exp2(_Gloss * 6);
                        specularLight = pow(specularLight , specularExponent);
                        specularSmooth = smoothstep(_SpecularThreshold , (_SpecularThreshold + _SpecularSmoothness) , specularLight);
                        finalSpecular = specularSmooth * mlColor * shadow * _SpecularColor.rgb;   
                    }
                    else
                    {
                        // Specular Lighting (CelShading) (Phong)
                        half3 R = reflect(-L,N); // Relect Angle
                        specularLight = saturate(dot(V,R)) * (finalDiffuse.x > 0);
                        specularExponent = exp2(_Gloss * 6);
                        specularLight = pow(specularLight , specularExponent);
                        specularSmooth = smoothstep(_SpecularThreshold , (_SpecularThreshold + _SpecularSmoothness) , specularLight);
                        finalSpecular = specularSmooth * mlColor * _SpecularColor.rgb;    
                    } 
                }
                             
                // Fresnel Lighting (CelShading)
                half3 finalFresnel = 0;
                if (_UseFresnel > 0.5)
                {
                    half fresnelLight =  1.0 - saturate(dot(N, V));
                    fresnelLight = pow(fresnelLight , _FresnelIntensity);
                    half fresnelSmooth = smoothstep(_FresnelThreshold , (_FresnelThreshold + _FresnelSmoothness) , fresnelLight);
                    finalFresnel = fresnelSmooth * _FresnelColor.rgb;
                }
                // Ambient Lighting
                half3 finalAmbient = 0;

                if (_UseAmbient > 0.5)
                {
                    half ambientLight = saturate(dot(N,L));
                    half3 maxAmbient = max(ambientLight , _AmbientStrength);
                    finalAmbient = maxAmbient * SampleSH(N) * texColor.rgb * _DiffuseColor.rgb;
                }
                

                //Object-space Triplanar Hatching
                half3 finalHatch = 0;

                if (_UseHatching > 0.5)
                {
                    float hatchMask = tex2D(_HatchMask,IN.uv).r;
                    float3 p = IN.positionOS * _HatchTex_ST.x;
                    float3 p1 = p;
                    float3 nOS = normalize(IN.normalOS);
                    float3 blend = abs(nOS);
                    blend /= (blend.x + blend.y + blend.z + 1e-5);

                    float3 hatchX = tex2D(_HatchTex, p1.yz).rgb;
                    float3 hatchY = tex2D(_HatchTex, p1.xz).rgb;
                    float3 hatchZ = tex2D(_HatchTex, p1.xy).rgb;
                    float3 hatchTex1 = hatchX * blend.x + hatchY * blend.y + hatchZ * blend.z;

                    half hatchLight = saturate(1.0 - dot(N, L));
                    half hatchSmooth = smoothstep(_HatchThreshold - _HatchSmoothness,_HatchThreshold + _HatchSmoothness,hatchLight);
                    half hatchOpacitySmooth = smoothstep(0,_HatchOpacity,1.0 - mlshadowAtt); 
                    // hatchSmooth *= shadow;

                    half hatch = 1.0 - dot(hatchTex1, float3(0.333,0.333,0.333));
                    finalHatch = -hatch * hatchMask * hatchSmooth * _HatchColor.rgb * hatchOpacitySmooth;   
                }

                //Emission 
                half3 finalEmission = 0;

                if(_UseEmission > 0.5)
                {
                    float4 emissionTex = tex2D(_EmissionTex, IN.uv);
                    half mask = dot(emissionTex.rgb, half3(0.299, 0.587, 0.114)); // Use texture brightness as mask
                    finalEmission = emissionTex.rgb * _EmissionColor.rgb * _EmissionIntensity * mask;
                }

                half3 additionalDiffuse = 0;
                half3 additionalSpecular = 0;

                if(_UseAdditionalLights > 0.5)
                {
                    // Additional Light Calculations
                    InputData inputData = (InputData)0;
                    inputData.positionWS = IN.wPos;
                    inputData.normalWS = N;
                    inputData.viewDirectionWS = V;
                    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);

                    #if defined(_ADDITIONAL_LIGHTS)

                        uint pixelLightCount = GetAdditionalLightsCount();

                        LIGHT_LOOP_BEGIN(pixelLightCount)
                            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));

                            half3 L = normalize(light.direction);
                            half3 R = reflect(-L, N);

                            float att = light.distanceAttenuation * light.shadowAttenuation;

                            // Toon diffuse (Additional)
                            half diff = saturate(dot(N, L)) * 0.5 + 0.5;
                            diff = smoothstep(_DiffuseThreshold,_DiffuseThreshold + _DiffuseSmoothness,diff);

                            additionalDiffuse += diff * att * light.color * shadedAlbedo * _DiffuseColor.rgb;

                            // Toon specular (Additional)
                            half spec = pow(saturate(dot(V, R)), _Gloss);
                            half specStep = smoothstep(_SpecularThreshold,_SpecularThreshold + _SpecularSmoothness,spec);
                            additionalSpecular += specStep * att * light.color;

                        LIGHT_LOOP_END
                    #endif
                }

                // Final Output (CelShading)         
                half3 mixedColor = finalDiffuse + additionalDiffuse + finalSpecular + finalFresnel + finalAmbient + finalEmission + finalHatch;
                half4 finalOutput = half4(mixedColor,1);
                finalOutput.rgb = MixFog(finalOutput.rgb, IN.fogCoord);
                return finalOutput;   
            }
            ENDHLSL
        }

        
        Pass 
        {
            Name "ShadowCaster"
            
            Tags{ "LightMode" = "ShadowCaster"}

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
               
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct MeshData
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Interpolators
            {
                float4 positionHCS : SV_POSITION;
            };
            
            float4 GetShadowPositionHClip(MeshData IN)
            {   
                float3 lightDirectionWS = _MainLightPosition.xyz;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(posWS,normalWS,lightDirectionWS));
                positionCS = ApplyShadowClamping(positionCS);

                return positionCS;
            };

            Interpolators vert (MeshData IN)
            {
                Interpolators OUT;

                OUT.positionHCS = GetShadowPositionHClip(IN);

                return OUT;
            }

            half4 frag (Interpolators IN) : SV_Target
            {   
                return 0;
            }

            ENDHLSL
        }
        //UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
