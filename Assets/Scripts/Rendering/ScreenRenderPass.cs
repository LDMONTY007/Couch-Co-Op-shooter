using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class ScreenRenderPass : ScriptableRenderPass
{
    private ScreenSettings defaultSettings;
    private Material material;

    private TextureDesc screenTextureDescriptor;

    private static readonly int horizontalScreenId = Shader.PropertyToID("_HorizontalScreen");
    private static readonly int verticalScreenId = Shader.PropertyToID("_VerticalScreen");
    private const string k_ScreenTextureName = "_ScreenTexture";
    private const string k_VerticalPassName = "VerticalScreenRenderPass";
    private const string k_HorizontalPassName = "HorizontalScreenRenderPass";

    public ScreenRenderPass(Material material, ScreenSettings defaultSettings)
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
        screenTextureDescriptor = srcCamColor.GetDescriptor(renderGraph);
        screenTextureDescriptor.name = k_ScreenTextureName;
        screenTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(screenTextureDescriptor);

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Update the Screen settings in the material
        UpdateScreenSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        // The AddBlitPass method adds a vertical Screen render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
        RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraVertical, k_VerticalPassName);

        // The AddBlitPass method adds a horizontal Screen render graph pass that blits from the texture written by the vertical Screen pass to the camera color texture. The method uses the second shader pass.
        RenderGraphUtils.BlitMaterialParameters paraHorizontal = new(dst, srcCamColor, material, 1);
        renderGraph.AddBlitPass(paraHorizontal, k_HorizontalPassName);
    }

    //update shader values
    private void UpdateScreenSettings()
    {
        if (material == null) return;

        //material.SetFloat(horizontalScreenId, defaultSettings.horizontalScreen);
        //material.SetFloat(verticalScreenId, defaultSettings.verticalScreen);
    }
}
