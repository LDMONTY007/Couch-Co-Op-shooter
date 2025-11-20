using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenShakeController : MonoBehaviour
{
    public int playerIndex = -1;

    public AnimationCurve verticalShakeCurve;

    public ScreenOffsetVolumeComponent screenOffsetVolumeComponent;

    public VolumeProfile[] playerVolumeProfiles = new VolumeProfile[4];

    public Volume volume;

    public void AssignRendererFeatures()
    {
        //Get all renderer features at runtime
        //https://discussions.unity.com/t/how-to-get-access-to-renderer-features-at-runtime/861114/8
        var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
        var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

        List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

        foreach (ScriptableRendererFeature feature in features)
        {
            if (feature is ScreenOffsetRendererFeature)
            {

            }
            else
            {
                continue;
            }

            ScreenOffsetRendererFeature soFeature = feature as ScreenOffsetRendererFeature;

            if (soFeature.settings.player_index == playerIndex)
            {
                ScreenOffsetVolumeComponent component = null;
                //find the ScreenOffsetVolumeComponent 
                //on the volume.
                foreach (VolumeComponent vc in volume.profile.components)
                {
                    if (vc is ScreenOffsetVolumeComponent)
                    {
                        component = vc as ScreenOffsetVolumeComponent;
                        break;
                    }
                    
                }
                //tell the renderer feature
                //which volume component to use.
                soFeature.settings.volumeComponent = component;
                screenOffsetVolumeComponent = component;
            }
            
        }
    }

    private void Awake()
    {
        //Using the player index set the correct volume profile for this
        //player.
        volume.profile = playerVolumeProfiles[playerIndex];
        //the only volume component on here will be the first one,
        //sure this isn't the best in case we add more components here
        //but I'll know immediately what the issue is.

        AssignRendererFeatures();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShakeVertically();
        }
    }

    public void ShakeVertically()
    {
        StartCoroutine(VerticalShakeCoroutine());
    }

    public IEnumerator VerticalShakeCoroutine(float totalTime = 1f)
    {
        float curTime = 0f;

        

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            //set shake value.
            screenOffsetVolumeComponent.verticalOffset.value = verticalShakeCurve.Evaluate(curTime / totalTime);

            yield return null;
        }

        screenOffsetVolumeComponent.verticalOffset.value = 0;
    }
}
