using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

//Resource: https://docs.unity3d.com/6000.2/Documentation/Manual/urp/renderer-features/create-custom-renderer-feature.html#all-complete-code-for-the-scripts-in-this-example
public class BlurRenderPass : ScriptableRenderPass
{
    private BlurSettings defaultSettings;
    private Material material;

    private TextureDesc blurTextureDescriptor;

    private static readonly int horizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
    private static readonly int verticalBlurId = Shader.PropertyToID("_VerticalBlur");
    private const string k_BlurTextureName = "_BlurTexture";
    private const string k_VerticalPassName = "VerticalBlurRenderPass";
    private const string k_HorizontalPassName = "HorizontalBlurRenderPass";

    public BlurRenderPass(Material material, BlurSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;
    }


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        //In the RecordRenderGraph method, create the variable for storing the UniversalResourceData instance from the frameData parameter. UniversalResourceData contains all the texture references used by URP, including the active color and depth textures of the camera.
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        //The destination texture is based on the camera color texture, so you can use the descriptor of the camera color texture as a starting point to define the destination texture. Using the same descriptor as the camera color texture ensures the source and destination textures will have the same size and color format (unless you choose to change the descriptor).
        TextureHandle srcCamColor = resourceData.activeColorTexture;
        blurTextureDescriptor = srcCamColor.GetDescriptor(renderGraph);
        blurTextureDescriptor.name = k_BlurTextureName;
        blurTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(blurTextureDescriptor);

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Update the blur settings in the material
        UpdateBlurSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        // The AddBlitPass method adds a vertical blur render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
        RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraVertical, k_VerticalPassName);

        // The AddBlitPass method adds a horizontal blur render graph pass that blits from the texture written by the vertical blur pass to the camera color texture. The method uses the second shader pass.
        RenderGraphUtils.BlitMaterialParameters paraHorizontal = new(dst, srcCamColor, material, 1);
        renderGraph.AddBlitPass(paraHorizontal, k_HorizontalPassName);
    }

    //update shader values
    private void UpdateBlurSettings()
    {
        if (material == null) return;

        material.SetFloat(horizontalBlurId, defaultSettings.horizontalBlur);
        material.SetFloat(verticalBlurId, defaultSettings.verticalBlur);
    }
}
