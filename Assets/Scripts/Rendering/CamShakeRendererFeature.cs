using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CamShakeRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private CamShakeSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private CamShakeRenderPass shakeRenderPass;

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        shakeRenderPass = new CamShakeRenderPass(material, settings);

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
public class CamShakeSettings
{
    [Range(-1f, 1f)] public float horizontalOffset;
    [Range(-1, 1f)] public float verticalOffset;
}