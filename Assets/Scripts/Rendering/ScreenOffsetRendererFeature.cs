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
            //only enqueue the rendering pass if this camera is owned by the same player that owns
            //this render feature.
            ScreenShakeController shakeController = renderingData.cameraData.camera.GetComponent<ScreenShakeController>();
            if (shakeController != null && renderingData.cameraData.camera.GetComponent<ScreenShakeController>().playerIndex == settings.player_index)
            {
                renderer.EnqueuePass(shakeRenderPass);
            }
            
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
    //0-3 for all 4 players,
    //this is how we know which screen offset to use
    //so each player has an individual pass.
    [Range(0, 3)] public int player_index = 0;
}