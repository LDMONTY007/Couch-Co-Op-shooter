using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class PixelateRenderPass : ScriptableRenderPass
{
    private PixelateSettings defaultSettings;
    private Material material;

    private TextureDesc pixelateTextureDescriptor;

    
    private const string k_PixelateTextureName = "_PixelateTexture";
    private const string k_PixelatePassName = "PixelateRenderPass";

    public PixelateRenderPass(Material material, PixelateSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing; // Or a later event
    }


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var volumeComponent = VolumeManager.instance.stack.GetComponent<CustomVolumeComponent>();

        //if our volume component disables the low res filter,
        //then we should return here.
        if (volumeComponent != null && volumeComponent.isActive.value == false)
        {
            //don't do this render pass.
            return;
        }

        //In the RecordRenderGraph method, create the variable for storing the UniversalResourceData instance from the frameData parameter. UniversalResourceData contains all the texture references used by URP, including the active color and depth textures of the camera.
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        //The destination texture is based on the camera color texture, so you can use the descriptor of the camera color texture as a starting point to define the destination texture. Using the same descriptor as the camera color texture ensures the source and destination textures will have the same size and color format (unless you choose to change the descriptor).
        TextureHandle srcCamColor = resourceData.activeColorTexture;
        pixelateTextureDescriptor = srcCamColor.GetDescriptor(renderGraph);
        pixelateTextureDescriptor.name = k_PixelateTextureName;
        pixelateTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(pixelateTextureDescriptor);

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // Only apply pixelation on the *final camera* in the stack
        if (!cameraData.resolveFinalTarget)
            return;

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Update the blur settings in the material
        UpdatePixelateSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        // The AddBlitPass method adds a vertical blur render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
        RenderGraphUtils.BlitMaterialParameters paraPixelate = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraPixelate, k_PixelatePassName);

        //copy dst back to the src cam color
        renderGraph.AddCopyPass(dst, srcCamColor);
    }

    //update shader values
    private void UpdatePixelateSettings()
    {
        if (material == null) return;

        // Use the Volume settings or the default settings if no Volume is set.
        var volumeComponent =
            VolumeManager.instance.stack.GetComponent<CustomVolumeComponent>();

        //use default settings if we use override state.
        material.SetVector("_PixelResolution", volumeComponent.pixelSize.overrideState ? volumeComponent.pixelSize.value : defaultSettings.pixelSize);
    }
}
