using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class ScreenOffsetRenderPass : ScriptableRenderPass
{
    private ScreenOffsetSettings defaultSettings;
    private Material material;

    private TextureDesc ScreenOffsetTextureDescriptor;

    private static readonly int horizontalScreenOffsetId = Shader.PropertyToID("_HorizontalOffset");
    private static readonly int verticalScreenOffsetId = Shader.PropertyToID("_VerticalOffset");
    private const string k_ScreenOffsetTextureName = "_ScreenOffsetTexture";
    private const string k_OffsetPassName = "VerticalScreenOffsetRenderPass";

    public ScreenOffsetRenderPass(Material material, ScreenOffsetSettings defaultSettings)
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
        ScreenOffsetTextureDescriptor = srcCamColor.GetDescriptor(renderGraph);
        ScreenOffsetTextureDescriptor.name = k_ScreenOffsetTextureName;
        ScreenOffsetTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(ScreenOffsetTextureDescriptor);

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // Only apply pass on the *final camera* in the stack
        if (!cameraData.resolveFinalTarget)
            return;

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Update the ScreenOffset settings in the material
        UpdateScreenOffsetSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        // The AddBlitPass method adds a vertical ScreenOffset render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
        RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraVertical, k_OffsetPassName);

        //copy dst back to the src cam color
        renderGraph.AddCopyPass(dst, srcCamColor);
    }

    //update shader values
    private void UpdateScreenOffsetSettings()
    {
        if (material == null) return;

        // Use the Volume settings or the default settings if no Volume is set.
        //var volumeComponent = VolumeManager.instance.stack.GetComponent<ScreenOffsetVolumeComponent>();
        

        //Get the volume component from our default settings.
        //because we have one specifically assigned for each
        //player.
        var volumeComponent = defaultSettings.volumeComponent;

        if (volumeComponent == null)
            return;

        material.SetFloat(horizontalScreenOffsetId, volumeComponent.horizontalOffset.overrideState ? volumeComponent.horizontalOffset.value : defaultSettings.horizontalOffset);
        material.SetFloat(verticalScreenOffsetId, volumeComponent.verticalOffset.overrideState ? volumeComponent.verticalOffset.value : defaultSettings.verticalOffset);
    }
}
