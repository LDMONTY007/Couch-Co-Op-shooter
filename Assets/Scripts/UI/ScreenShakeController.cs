using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public Gamepad currentGamepad;
    float rumbleDecaySpeed = 6f;

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
            AddDirectionalBounce(new Vector2(1, 1), strength: 0.4f, duration: 0.45f, rumble:0.9f);
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
        float totalRumble = 0f;

        for (int i = shakes.Count - 1; i >= 0; i--)
        {
            var s = shakes[i];

            totalOffset += s.Evaluate(dt);
            totalRumble = Mathf.Max(totalRumble, s.EvaluateRumble());

            s.UpdateAge(dt);
            if (s.IsDone)
                shakes.RemoveAt(i);
        }

        screenOffsetVolumeComponent.horizontalOffset.value = totalOffset.x;
        screenOffsetVolumeComponent.verticalOffset.value = totalOffset.y;

        ApplyRumble(totalRumble);
    }

    void ApplyRumble(float intensity)
    {
        //if the player doesn't
        //have a gamepad, then 
        //don't apply rumble.
        if (currentGamepad == null)
        {    
            return;
        }

        // Map shake intensity to motor power
        float low = intensity * 0.6f;
        float high = intensity;

        currentGamepad.SetMotorSpeeds(low, high);
    }



    public void AddDirectionalBounce(Vector2 dir, float strength, float duration, float rumble = 0.5f)
    {
        AddShake(new DirectionalBounceShake(dir, strength, rumble, duration));
    }


    public void AddVerticalShake(float strength, float frequency, float duration)
    {
        AddShake(new VerticalShake(strength, frequency, duration));
    }

    public void AddTrauma(float amount)
    {
        AddShake(new TraumaShake(amount, duration: 0.2f));
    }

    public void AddGunshotRumble(float strength = 1f, float duration = 0.12f)
    {
        AddShake(new GunshotRumbleShake(strength, duration));
    }

    public void AddThrownImpactRumble(float strength = 1f, float duration = 0.35f)
    {
        AddShake(new ThrownImpactRumbleShake(strength, duration));
    }

    public void AddRumbleOnly(float strength, float duration)
    {
        AddShake(new RumbleOnlyShake(strength, duration));
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

    // Returns a 0–1 rumble intensity
    public virtual float EvaluateRumble() => 0f;

    public abstract Vector2 Evaluate(float dt);

    public virtual void UpdateAge(float dt) => age += dt;
}

//bounce shake in a direction.
public class DirectionalBounceShake : ShakeInstance
{
    Vector2 dir;
    float strength;
    float rumbleStrength;

    public DirectionalBounceShake(Vector2 direction, float strength, float rumble, float duration)
    {
        dir = direction.normalized;
        this.strength = strength;
        this.rumbleStrength = rumble;
        lifetime = duration;
    }

    public override Vector2 Evaluate(float dt)
    {
        float t = age / lifetime;
        float mag = strength * (1f - t) * Mathf.Exp(-6f * t);
        return dir * mag;
    }

    public override float EvaluateRumble()
    {
        float t = age / lifetime;

        // Same exponential falloff as the screen bounce
        float falloff = Mathf.Exp(-6f * t);

        return rumbleStrength * falloff;
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

    public override float EvaluateRumble()
    {
        float normalizedAge = age / lifetime;

        float sin = Mathf.Sin(age * frequency * Mathf.PI * 2f);
        float mag = Mathf.Abs(sin) * strength * (1f - normalizedAge);

        return Mathf.Clamp01(mag * 2f); // boost slightly for feel
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

    public override float EvaluateRumble()
    {
        float normalizedAge = age / lifetime;

        float amount = power * (1f - normalizedAge);

        // Random burst rumble each frame
        return Random.Range(0f, amount);
    }
}

public class RumbleOnlyShake : ShakeInstance
{
    float rumbleStrength;
    AnimationCurve rumbleCurve;

    public RumbleOnlyShake(float strength, float duration, AnimationCurve curve = null)
    {
        rumbleStrength = strength;
        lifetime = duration;

        // If no curve is provided, use a simple fade-out curve
        rumbleCurve = curve != null ? curve : AnimationCurve.EaseInOut(0, 1, 1, 0);
    }

    public override Vector2 Evaluate(float dt)
    {
        // No camera movement
        return Vector2.zero;
    }

    public override float EvaluateRumble()
    {
        float t = Mathf.Clamp01(age / lifetime);
        return rumbleStrength * rumbleCurve.Evaluate(t);
    }
}

public class GunshotRumbleShake : RumbleOnlyShake
{
    static AnimationCurve gunCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.1f, 0.8f),
        new Keyframe(1f, 0f)
    );

    public GunshotRumbleShake(float strength, float duration = 0.12f)
        : base(strength, duration, gunCurve)
    { }
}

public class ThrownImpactRumbleShake : RumbleOnlyShake
{
    static AnimationCurve impactCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.4f, 0.8f),
        new Keyframe(0.55f, 1f),
        new Keyframe(1f, 0f)
    );

    public ThrownImpactRumbleShake(float strength, float duration = 0.35f)
        : base(strength, duration, impactCurve)
    { }
}


