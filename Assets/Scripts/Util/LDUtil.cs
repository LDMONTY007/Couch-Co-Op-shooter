using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains various methods 
/// that can be helpful during game design.
/// </summary>
public class LDUtil : MonoBehaviour
{
    /// <summary>
    /// Wait <paramref name="time"> seconds before calling <paramref name="action">.
    /// </summary>
    /// <param name="action">the action to call</param>
    /// <param name="time">time until we call the action</param>
    /// <returns></returns>
    public static IEnumerator Wait(Action action, float time)
    {

        //wait for 0.5 seconds
        yield return new WaitForSecondsRealtime(time);
        //call the given action.
        action();
    }

    //overload that allows a parameter to be passed with the wait method.
    public static IEnumerator Wait<T1>(Action<T1> action, T1 parameter, float time)
    {

        //wait for 0.5 seconds
        yield return new WaitForSeconds(time);
        //call the given action.
        action(parameter);
    }

    /// <summary>
    /// Waits <paramref name="frames"/> frames until it calls the provided <paramref name="action"/>
    /// </summary>
    /// <param name="action"></param>
    /// <param name="frames"></param>
    /// <returns></returns>
    public static IEnumerator WaitFrames(Action action, int frames)
    {
        while (frames > 0)
        {
            frames--;
            yield return null;
        }
        action();
    }

    //use this for slowing down time during things like impacts and right as you die before we switch scenes. Basically it'll do impact frames.
    public static IEnumerator SlowTime(float scale, float duration)
    {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public static float LookRotation2D(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Returns a vector pointing in angle direction. 
    /// </summary>
    /// <param name="angleDeg"></param>
    /// <returns></returns>
    public static Vector2 AngleToDir2D(float angleDeg)
    {
        return new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Sets a mesh renderer's material color to be a given color for some amount of time. Returns
    /// to initial color when the coroutine ends.
    /// </summary>
    /// <param name="m"></param>
    /// <param name="c"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static IEnumerator SetColorForDuration(MeshRenderer m, Color c, float time)
    {
        Color prevColor = m.material.color;
        m.material.color = c;
        //wait for time
        yield return new WaitForSeconds(time);
        //Reset to previous color
        m.material.color = prevColor;
    }

    /// <summary>
    /// Coroutine that helps
    /// for waiting for an animation to 
    /// finish.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator WaitForAnimationFinish(Animator animator)
    {
        //For some reason we need
        //to wait while the normalized time is > 1
        //because if an animation plays multiple times
        //the normalized time will be greater than 1. 
        //This is dumb.
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 || animator.IsInTransition(0))
        {
            yield return null;
        }

        //Wait until the animation is done and we aren't transitioning
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0))
        {
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine that helps
    /// for waiting for an animation to 
    /// finish.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator WaitForAnimationFinish(Animator animator, string animation)
    {
        //Wait until the animation is done
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(animation))
        {
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine that helps
    /// for waiting for an animation to 
    /// finish.
    /// </summary>
    /// <returns></returns>
    /// Extra Note: For some reason some animations
    /// need to ignore checking the transition and I don't know why.
    public static IEnumerator WaitForAnimationFinishIgnoreTransition(Animator animator)
    {
        //For some reason we need
        //to wait while the normalized time is > 1
        //because if an animation plays multiple times
        //the normalized time will be greater than 1. 
        //This is dumb.
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            yield return null;
        }

        //Wait until the animation is done
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }
    }

    public static bool IsLayerInMask(LayerMask mask, int layer)
    {
        //where I learned how to do this check: https://discussions.unity.com/t/checking-if-a-layer-is-in-a-layer-mask/860331/2
        return (mask.value & (1 << layer)) != 0;
    }

    public static Vector3 RotateVectorAroundAxis(Vector3 vectorToRotate, Vector3 axis, float angleDeg)
    {
        //multiplying a quaternion by a vector gives us the vector rotated by that quaternion.
        return Quaternion.AngleAxis(angleDeg, axis) * vectorToRotate;
    }


    //I got this from here:
    //https://easings.net/#easeInOutBack
    //https://discussions.unity.com/t/smooth-function-with-overshooting/241010
    public static float EaseInOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;
        float t2 = t - 1f;
        return t < 0.5
            ? t * t * 2 * ((c2 + 1) * t * 2 - c2)
            : t2 * t2 * 2 * ((c2 + 1) * t2 * 2 + c2) + 1;
    }

    //This is also from the easing site.
    public static float EaseOutElastic(float x) {
    const float c4 = (2 * Mathf.PI) / 3;

    return x == 0
      ? 0
      : x == 1
      ? 1
      : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
    }

    //this is also from the easing site. 
    //I love this one.
    public static float EaseOutBack(float x) {
    const float c1 = 1.70158f;
    const float c3 = c1 + 1;

        return 1 + c3* Mathf.Pow(x - 1, 3) + c1* Mathf.Pow(x - 1, 2);
    }

    public static Color GetHexColor(string s)
    {
        ColorUtility.TryParseHtmlString(s, out var color);
        return color;
    }

    //Where I learned this:
    //https://docs.unity3d.com/6000.2/Documentation/Manual/FrustumSizeAtDistance.html
    public static Vector2 PerspectiveCameraFrustumSize(Camera c, float distance)
    {
        var frustumHeight = 2.0f * distance * Mathf.Tan(c.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * c.aspect;

        return new Vector2(frustumWidth, frustumHeight);
    }

    //Where I learned this:
    //https://discussions.unity.com/t/how-can-i-know-if-a-gameobject-is-seen-by-a-particular-camera/248/9
    //checks if an object is in view and not blocked by another object.
    public static bool IsInView(Camera c, Vector3 origin, GameObject toCheck)
    {
        if (toCheck.GetComponentInChildren<Renderer>() == null)
        {
            Debug.LogError("HERE");
        }

        Vector3 pointOnScreen = c.WorldToScreenPoint(toCheck.GetComponentInChildren<Renderer>().bounds.center);

        //Is in front
        if (pointOnScreen.z < 0)
        {
            Debug.Log("Behind: " + toCheck.name);
            return false;
        }

        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            Debug.Log("OutOfBounds: " + toCheck.name);
            return false;
        }

        RaycastHit hit;
        Vector3 heading = toCheck.transform.position - origin;
        Vector3 direction = heading.normalized;// / heading.magnitude;

        if (Physics.Linecast(c.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, out hit))
        {
            if (hit.transform.name != toCheck.name)
            {
                /* -->
                Debug.DrawLine(cam.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, Color.red);
                Debug.LogError(toCheck.name + " occluded by " + hit.transform.name);
                */
                Debug.Log(toCheck.name + " occluded by " + hit.transform.name);
                return false;
            }
        }
        return true;
    }
}
