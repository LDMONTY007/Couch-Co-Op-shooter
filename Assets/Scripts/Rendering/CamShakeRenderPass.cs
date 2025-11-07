using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class CamShakeRenderPass : ScriptableRenderPass
{
    private CamShakeSettings defaultSettings;
    private Material material;

    private TextureDesc CamShakeTextureDescriptor;

    private static readonly int horizontalCamShakeId = Shader.PropertyToID("_HorizontalOffset");
    private static readonly int verticalCamShakeId = Shader.PropertyToID("_VerticalOffset");
    private const string k_CamShakeTextureName = "_CamShakeTexture";
    private const string k_OffsetPassName = "VerticalCamShakeRenderPass";

    public CamShakeRenderPass(Material material, CamShakeSettings defaultSettings)
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
        CamShakeTextureDescriptor = srcCamColor.GetDescriptor(renderGraph);
        CamShakeTextureDescriptor.name = k_CamShakeTextureName;
        CamShakeTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(CamShakeTextureDescriptor);

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // Only apply pass on the *final camera* in the stack
        if (!cameraData.resolveFinalTarget)
            return;

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Update the CamShake settings in the material
        UpdateCamShakeSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        // The AddBlitPass method adds a vertical CamShake render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
        RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraVertical, k_OffsetPassName);

        //copy dst back to the src cam color
        renderGraph.AddCopyPass(dst, srcCamColor);
    }

    //update shader values
    private void UpdateCamShakeSettings()
    {
        if (material == null) return;

        material.SetFloat(horizontalCamShakeId, defaultSettings.horizontalOffset);
        material.SetFloat(verticalCamShakeId, defaultSettings.verticalOffset);
    }
}
