using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.UI.Image;

public class ScreenShakeController : MonoBehaviour
{
    public int playerIndex = -1;

    public AnimationCurve verticalShakeCurve;
    public AnimationCurve bounceCurve;

    public ScreenOffsetVolumeComponent screenOffsetVolumeComponent;

    public VolumeProfile[] playerVolumeProfiles = new VolumeProfile[4];

    public Volume volume;

    List<ShakeInstance> shakes = new List<ShakeInstance>();

    public void AddShake(ShakeInstance inst)
    {
        shakes.Add(inst);
    }

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
        //debug stuff.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddDirectionalBounce(new Vector2(1, 1), strength: 0.4f, duration: 0.45f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddVerticalShake(strength: 0.15f, frequency: 12f, duration: 0.8f);
        }

        //this is get key so we can see the buildup
        //of trauma.
        if (Input.GetKey(KeyCode.Alpha3))
        {
            AddTrauma(0.2f); // (Dead Cells / Doom style)
        }


        HandleShakeAnimation();
    }

    public void HandleShakeAnimation()
    {
        float dt = Time.deltaTime;

        Vector2 totalOffset = Vector2.zero;

        for (int i = shakes.Count - 1; i >= 0; i--)
        {
            var s = shakes[i];
            totalOffset += s.Evaluate(dt);
            s.UpdateAge(dt);

            if (s.IsDone)
                shakes.RemoveAt(i);
        }

        screenOffsetVolumeComponent.horizontalOffset.value = totalOffset.x;
        screenOffsetVolumeComponent.verticalOffset.value = totalOffset.y;
    }

    public void AddDirectionalBounce(Vector2 dir, float strength, float duration)
    {
        AddShake(new DirectionalBounceShake(dir, strength, duration));
    }

    public void AddVerticalShake(float strength, float frequency, float duration)
    {
        AddShake(new VerticalShake(strength, frequency, duration));
    }

    public void AddTrauma(float amount)
    {
        AddShake(new TraumaShake(amount, duration: 0.2f));
    }


    /* public void ShakeVertically()
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

     public IEnumerator DirBounceCoroutine(Vector2 dir, float totalTime = 1f)
     {
         float curTime = 0f;

         dir = dir.normalized;



         while (curTime < totalTime)
         {
             curTime += Time.deltaTime;

             //use the direction we need to bounce
             //and multiply by the bounce curve 
             //as a way to create a directional screen bounce back
             //effect.
             Vector2 curBounceValue = dir * bounceCurve.Evaluate(curTime / totalTime);

             //set shake value.
             screenOffsetVolumeComponent.verticalOffset.value = curBounceValue.y;
             screenOffsetVolumeComponent.horizontalOffset.value = curBounceValue.x;

             yield return null;
         }

         screenOffsetVolumeComponent.verticalOffset.value = 0;
     }*/
}

public abstract class ShakeInstance
{
    public float lifetime;
    public float age;

    public bool IsDone => age >= lifetime;

    public abstract Vector2 Evaluate(float dt);

    public virtual void UpdateAge(float dt) => age += dt;
}

//bounce shake in a direction.
public class DirectionalBounceShake : ShakeInstance
{
    Vector2 dir;
    float strength;

    public DirectionalBounceShake(Vector2 direction, float strength, float duration)
    {
        dir = direction.normalized;
        this.strength = strength;
        this.lifetime = duration;
    }

    public override Vector2 Evaluate(float dt)
    {
        float t = age / lifetime;

        // Fast hit --> slow return (spring-like)
        float falloff = 1f - t;                        // linear falloff
        float damper = Mathf.Exp(-6f * t);            // exponential spring damping

        float mag = strength * falloff * damper;

        return dir * mag;
    }
}

//Camera wobble, perfect for if a boss or tank
//throws a giant rock at you.
public class VerticalShake : ShakeInstance
{
    float strength;
    float frequency;

    public VerticalShake(float strength, float frequency, float duration)
    {
        this.strength = strength;
        this.frequency = frequency;
        this.lifetime = duration;
    }

    public override Vector2 Evaluate(float dt)
    {
        float t = age * frequency;
        float mag = Mathf.Sin(t * Mathf.PI * 2f) * strength * (1f - age / lifetime);

        return new Vector2(0, mag);
    }
}

//Dead cells style trauma shake
//small random bursts.
public class TraumaShake : ShakeInstance
{
    float power;

    public TraumaShake(float trauma, float duration)
    {
        power = trauma;
        lifetime = duration;
    }

    public override Vector2 Evaluate(float dt)
    {
        float amount = power * (1f - age / lifetime);

        return new Vector2(
            (Random.value * 2f - 1f) * amount,
            (Random.value * 2f - 1f) * amount
        );
    }
}


