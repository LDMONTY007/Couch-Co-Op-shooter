using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ScreenOffsetRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private ScreenOffsetSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private ScreenOffsetRenderPass shakeRenderPass;

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        shakeRenderPass = new ScreenOffsetRenderPass(material, settings);

        shakeRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        //nqueue the render pass with the EnqueuePass method.
        if (shakeRenderPass == null)
        {
            return;
        }
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(shakeRenderPass);
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
public class ScreenOffsetSettings
{
    [Range(-1f, 1f)] public float horizontalOffset;
    [Range(-1f, 1f)] public float verticalOffset;
}