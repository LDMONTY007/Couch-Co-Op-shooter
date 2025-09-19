Shader "CustomEffects/Pixelate"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float _PixelSize; // how many vertical pixels to snap to

        float4 FragPixelate (Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            // Get screen resolution
            float2 screenSize = _ScreenParams.xy;

            // Determine "pixel step" in UV space
            float2 pixelStep = _PixelSize / screenSize;

            // Snap UVs to nearest pixel grid
            float2 uv = input.texcoord;
            uv = floor(uv / pixelStep) * pixelStep;

            // Sample once with snapped UV
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
