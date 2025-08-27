using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Appliable : MonoBehaviour, IUseable
{
    public float totalTimeToApply = 5f;

    //use this for updating the UI of the player.
    public UnityEvent<float> onValueChanged;

    //called before this gameobject is destroyed.
    public UnityEvent<IUseable> onBeforeDestroy;
    
    private float _curApplyTime = 0f;
    //call onValueChanged when setting.
    public float curApplyTime { get { return _curApplyTime; } set { _curApplyTime = value; onValueChanged.Invoke(value / totalTimeToApply); } }

    private bool didApply = false;

    public PlayerController targetPlayer;

    public PlayerController parentController;

    public Rigidbody rb;

    private void OnDisable()
    {
        //Make sure to remove all the listeners
        //as it could cause an error otherwise.
        onValueChanged.RemoveAllListeners();
    }

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

        //Show use slider for both player's UI
        parentController.uiController.ShowUseSlider(true);
        targetPlayer.uiController.ShowUseSlider(true);
        //Subscribe to the onvaluechanged methods so the sliders on both are updated
        //to match this appliable's progress.
        onValueChanged.AddListener(parentController.uiController.UpdateUseSlider);
        onValueChanged.AddListener(targetPlayer.uiController.UpdateUseSlider);
        applyCoroutine = StartCoroutine(ApplyCoroutine());
    }

    public void CancelApply()
    {
        //Reset back to beginning and don't apply when canceled.
        if (applyCoroutine != null)
        StopCoroutine(applyCoroutine);
        applyCoroutine = null;
        curApplyTime = 0f;

        //Make sure to remove all the listeners
        //as it could cause an error otherwise.
        onValueChanged.RemoveAllListeners();

        //Hide use slider for both player's UI
        parentController?.uiController.ShowUseSlider(false);
        targetPlayer?.uiController.ShowUseSlider(false);
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


        
        //Destroy this item.
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        //Make sure to remove all the listeners
        //as it could cause an error otherwise.
        onValueChanged.RemoveAllListeners();

        //Hide use slider for both player's UI
        parentController?.uiController.ShowUseSlider(false);
        targetPlayer?.uiController.ShowUseSlider(false);

        onBeforeDestroy?.Invoke(this);
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
