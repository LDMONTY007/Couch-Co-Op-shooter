using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Appliable : MonoBehaviour, IUseable
{
    public float totalTimeToApply = 5f;

    //use this for updating the UI of the player.
    public UnityEvent<float> onValueChanged;

    
    private float _curApplyTime = 0f;
    //call onValueChanged when setting.
    public float curApplyTime { get { return _curApplyTime; } set { _curApplyTime = value; onValueChanged.Invoke(value); } }

    private bool didApply = false;

    public PlayerController targetPlayer;

    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //Require the didApply bool so that
    //didApply gets updated properly.
    public virtual void OnApply(PlayerController playerController/*ref bool didApply*/)
    { 
        //TODO: When you make an appliable 
        //override this method and code
        //what occurs when this item is applied.
    }

    public void StartApply()
    {
        if (applyCoroutine != null)
        {
            Debug.LogError("Appliable didn't stop applying before starting to apply again!");
            return;
        }
        applyCoroutine = StartCoroutine(ApplyCoroutine());
    }

    public void CancelApply()
    {
        StopCoroutine(applyCoroutine);
        applyCoroutine = null;
    }

    Coroutine applyCoroutine;

    private IEnumerator ApplyCoroutine()
    {
        while (curApplyTime < totalTimeToApply)
        {
            curApplyTime += Time.deltaTime;
            yield return null;
        }

        //Call OnApply before the coroutine ends.
        OnApply(targetPlayer/*ref didApply*/);

/*        //Wait until this item is applied before destroying it.
        while (!didApply)
        {
            yield return null;
        }*/

        applyCoroutine = null;

        //Destory this item.
        Destroy(gameObject);
    }

    public void Use()
    {
        StartApply();
    }

    public void CancelUse()
    {
        CancelApply();
    }
}
