using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PixelateRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private PixelateSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private PixelateRenderPass pixelateRenderPass;

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        pixelateRenderPass = new PixelateRenderPass(material, settings);

        pixelateRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        //nqueue the render pass with the EnqueuePass method.
        if (pixelateRenderPass == null)
        {
            return;
        }
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(pixelateRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }

}
[Serializable]
public class PixelateSettings
{
     public float pixelSize;
}


