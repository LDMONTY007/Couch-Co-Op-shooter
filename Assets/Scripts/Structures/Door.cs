using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour, IInteractible
{
    public Transform doorTransform;

    bool _isDoorOpen = false;

    bool isDoorOpen {  get { return _isDoorOpen; } set { _isDoorOpen = value; 
            if (_isDoorOpen)
            {        
                //invoke onOpen.
                OnDoorOpen.Invoke();
            }
            else
            {
                //invoke onClose.
                OnDoorClose.Invoke();
            }
        } 
    }

    bool inAnimation = false;

    public bool isLocked = false;

    public bool unlockFromFront = false;

    public UnityEvent OnDoorOpen;

    public UnityEvent OnDoorClose;

    public UnityEvent OnUnlocked;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator OpenDoorForwardCoroutine()
    {
        inAnimation = true;

        float totalTime = 1f;
        float curTime = 0f;

        Quaternion startRotation = doorTransform.localRotation;

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            //Lerp using smoothstep
            doorTransform.localRotation = Quaternion.Lerp(startRotation, Quaternion.AngleAxis(90f, doorTransform.up), Mathf.SmoothStep(0, 1, curTime / totalTime));

            yield return null;
        }
        isDoorOpen = true;

        OnDoorOpen.Invoke();

        inAnimation = false;
    }

    public IEnumerator OpenDoorBackwardCoroutine()
    {
        inAnimation = true;

        float totalTime = 1f;
        float curTime = 0f;

        Quaternion startRotation = doorTransform.localRotation;

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            //Lerp using smoothstep
            doorTransform.localRotation = Quaternion.Lerp(startRotation, Quaternion.AngleAxis(-90f, doorTransform.up), Mathf.SmoothStep(0, 1, curTime / totalTime));

            yield return null;
        }
        isDoorOpen = true;

        inAnimation = false;
    }

    public IEnumerator CloseDoorCoroutine()
    {
        inAnimation = true;

        float totalTime = 1f;
        float curTime = 0f;

        Quaternion startRotation = doorTransform.localRotation;

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            //Lerp using smoothstep
            doorTransform.localRotation = Quaternion.Lerp(startRotation, Quaternion.AngleAxis(0f, doorTransform.up), Mathf.SmoothStep(0, 1, curTime / totalTime));

            yield return null;
        }

        isDoorOpen = false;

        inAnimation = false;
    }

    public void CloseDoorInstant()
    {
        doorTransform.localRotation = Quaternion.AngleAxis(0f, doorTransform.up);
        isDoorOpen = false;
    }

    public void OpenDoorForwardInstant()
    {
        doorTransform.localRotation = Quaternion.AngleAxis(90f, doorTransform.up);
        isDoorOpen = true;
    }

    public void OpenDoorBackwardInstant()
    {
        doorTransform.localRotation = Quaternion.AngleAxis(-90f, doorTransform.up);
        isDoorOpen = true;
    }


    public void OnFocusEnter()
    {
        //Do nothing.
    }

    public void Interact(GameObject other)
    {


        bool openForward = false;

        //Decide which way we need the door to open
        Vector3 dir = transform.position - other.transform.position;
        if (Vector3.Dot(dir.normalized, transform.forward.normalized) > 0)
        {
            openForward = true;
        }
        else
        {
            openForward = false;
        }

        //if locked don't do the interact logic.
        if (isLocked)
        {
            //if a player opens the door from
            //the front this door will become unlocked.
            if (unlockFromFront && !openForward)
            {
                //TODO: Play sound of unlocking.
                isLocked = false;
                OnUnlocked.Invoke();
            }
            else
            {
                //TODO: Play sound of door handle locked
                //and not turning.
                return;
            }

                
        }

        //If we're in an animation
        //stop the door animation
        //and instantly set it's rotation.
        //that way, the player can double press
        //E to quickly "throw" the door open or closed.
        if (inAnimation)
        {
            StopAllCoroutines();

            if (isDoorOpen)
            {
                inAnimation = false;
                CloseDoorInstant();
                //exit this method so we don't start an animation.
                return;

            }
            else
            {
                inAnimation = false;

                if (openForward)
                    OpenDoorForwardInstant();
                else
                    OpenDoorBackwardInstant();

                    //exit this method so we don't start an animation.
                    return;
            }
        }

        //Open or close the door.
        if (isDoorOpen)
        {
            StartCoroutine(CloseDoorCoroutine());
        }
        else
        {
            if (openForward)
                StartCoroutine(OpenDoorForwardCoroutine());
            else
                StartCoroutine(OpenDoorBackwardCoroutine());
        }
    }

    public void InteractHold(float useSpeed = 1f)
    {
        //Do nothing.
    }

    public void InteractStopHold()
    {
        //Do nothing.
    }

    public void OnFocusLeave()
    {
        //Do nothing.
    }
}
