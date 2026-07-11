#ifndef UNIVERSAL_PARTICLES_GBUFFER_SIMPLE_LIT_PASS_INCLUDED
#define UNIVERSAL_PARTICLES_GBUFFER_SIMPLE_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Particles.hlsl"

void InitializeInputData(VaryingsParticle input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = input.positionWS.xyz;

#ifdef _NORMALMAP

    half3 viewDirWS = half3(
        input.normalWS.w,
        input.tangentWS.w,
        input.bitangentWS.w);

    half3x3 tangentToWorld = half3x3(
        input.tangentWS.xyz,
        input.bitangentWS.xyz,
        input.normalWS.xyz);

    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);

#else

    half3 viewDirWS = input.viewDirWS;
    inputData.normalWS = input.normalWS;

#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = SafeNormalize(viewDirWS);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)

    inputData.shadowCoord = input.shadowCoord;

#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)

    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);

#else

    inputData.shadowCoord = float4(0,0,0,0);

#endif

    inputData.fogCoord = 0.0h;
    inputData.vertexLighting = half3(0,0,0);
    inputData.bakedGI = SampleSHPixel(input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.clipPos);

#if defined(DEBUG_DISPLAY) && !defined(PARTICLES_EDITOR_META_PASS)

    inputData.vertexSH = input.vertexSH;

#endif
}

inline void InitializeParticleSimpleLitSurfaceData(VaryingsParticle input, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    ParticleParams particleParams;
    InitParticleParams(input, particleParams);

    outSurfaceData.normalTS = SampleNormalTS(
        particleParams.uv,
        particleParams.blendUv,
        TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));

    half4 albedo = SampleAlbedo(
        TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap),
        particleParams);

    outSurfaceData.albedo = AlphaModulate(albedo.rgb, albedo.a);
    outSurfaceData.alpha = albedo.a;

#if defined(_EMISSION)

    outSurfaceData.emission =
        BlendTexture(
            TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap),
            particleParams.uv,
            particleParams.blendUv) * _EmissionColor.rgb;

#else

    outSurfaceData.emission = half3(0.0h, 0.0h, 0.0h);

#endif

    half4 specularGloss = SampleSpecularSmoothness(
        particleParams.uv,
        particleParams.blendUv,
        albedo.a,
        _SpecColor,
        TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));

    outSurfaceData.specular = specularGloss.rgb;
    outSurfaceData.smoothness = specularGloss.a;

#if defined(_DISTORTION_ON)

    outSurfaceData.albedo = Distortion(
        half4(outSurfaceData.albedo, outSurfaceData.alpha),
        outSurfaceData.normalTS,
        _DistortionStrengthScaled,
        _DistortionBlend,
        input.projectedPosition);

#endif

    outSurfaceData.metallic = 0.0h;   // Unused by Simple Lit
    outSurfaceData.occlusion = 1.0h;
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

VaryingsParticle ParticlesLitGBufferVertex(AttributesParticle input)
{
    VaryingsParticle output = (VaryingsParticle)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
    #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
        CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(input.positionOS, input.normalOS, input.tangentOS)
    #else
        CURVEDWORLD_TRANSFORM_VERTEX(input.positionOS)
    #endif
#endif

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

#ifdef _NORMALMAP

    output.normalWS = half4(
        NormalizeNormalPerVertex(normalInput.normalWS),
        viewDirWS.x);

    output.tangentWS = half4(
        NormalizeNormalPerVertex(normalInput.tangentWS),
        viewDirWS.y);

    output.bitangentWS = half4(
        NormalizeNormalPerVertex(normalInput.bitangentWS),
        viewDirWS.z);

#else

    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDirWS = viewDirWS;

#endif

    OUTPUT_SH(normalInput.normalWS, output.vertexSH);

    output.positionWS.xyz = vertexInput.positionWS;
    output.positionWS.w = 0.0h;
    output.clipPos = vertexInput.positionCS;
    output.color = GetParticleColor(input.color);

#if defined(_FLIPBOOKBLENDING_ON)

    #if defined(UNITY_PARTICLE_INSTANCING_ENABLED)

        GetParticleTexcoords(
            output.texcoord,
            output.texcoord2AndBlend,
            input.texcoords.xyxy,
            0.0);

    #else

        GetParticleTexcoords(
            output.texcoord,
            output.texcoord2AndBlend,
            input.texcoords,
            input.texcoordBlend);

    #endif

#else

    GetParticleTexcoords(
        output.texcoord,
        input.texcoords.xy);

#endif

#if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON) || defined(_DISTORTION_ON)

    output.projectedPosition = vertexInput.positionNDC;

#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)

    output.shadowCoord = GetShadowCoord(vertexInput);

#endif

    return output;
}


FragmentOutput ParticlesLitGBufferFragment(VaryingsParticle input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeParticleSimpleLitSurfaceData(input, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

#if defined(DEBUG_DISPLAY)
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.texcoord, _BaseMap);
#endif

    half4 color = half4(
        inputData.bakedGI * surfaceData.albedo + surfaceData.emission,
        surfaceData.alpha);

    return SurfaceDataToGbuffer(
        surfaceData,
        inputData,
        color.rgb,
        kLightingSimpleLit);
}

#endif // UNIVERSAL_PARTICLES_GBUFFER_SIMPLE_LIT_PASS_INCLUDED