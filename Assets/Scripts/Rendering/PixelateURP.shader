Shader "CustomEffects/Pixelate"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float2 _PixelResolution; // Target resolution, e.g. (320, 240)


        float4 FragPixelate (Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 screenSize = _ScreenParams.xy;

            // Step size in UV space for each pixel
            float2 pixelStep = 1.0 / _PixelResolution;

            // Snap UVs to nearest pixel center
            float2 uv = floor(input.texcoord / pixelStep) * pixelStep;
            //Use point clamp so there are no blurred edges/pixels.
            float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, uv);
            return col;
        }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "PixelatePass"
            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragPixelate
            ENDHLSL
        }
    }
}
