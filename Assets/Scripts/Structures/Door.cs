using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractible
{
    public Transform doorTransform;

    bool isDoorOpen = false;
    bool inAnimation = false;

    bool isLocked = false;

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

        Quaternion startRotation = doorTransform.rotation;

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            //Lerp using smoothstep
            doorTransform.rotation = Quaternion.Lerp(startRotation, Quaternion.AngleAxis(90f, doorTransform.up), Mathf.SmoothStep(0, 1, curTime / totalTime));

            yield return null;
        }
        isDoorOpen = true;

        inAnimation = false;
    }

    public IEnumerator OpenDoorBackwardCoroutine()
    {
        inAnimation = true;

        float totalTime = 1f;
        float curTime = 0f;

        Quaternion startRotation = doorTransform.rotation;

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            //Lerp using smoothstep
            doorTransform.rotation = Quaternion.Lerp(startRotation, Quaternion.AngleAxis(-90f, doorTransform.up), Mathf.SmoothStep(0, 1, curTime / totalTime));

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

        Quaternion startRotation = doorTransform.rotation;

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            //Lerp using smoothstep
            doorTransform.rotation = Quaternion.Lerp(startRotation, Quaternion.AngleAxis(0f, doorTransform.up), Mathf.SmoothStep(0, 1, curTime / totalTime));

            yield return null;
        }

        isDoorOpen = false;

        inAnimation = false;
    }

    public void CloseDoorInstant()
    {
        doorTransform.rotation = Quaternion.AngleAxis(0f, doorTransform.up);
        isDoorOpen = false;
    }

    public void OpenDoorForwardInstant()
    {
        doorTransform.rotation = Quaternion.AngleAxis(90f, doorTransform.up);
        isDoorOpen = true;
    }

    public void OpenDoorBackwardInstant()
    {
        doorTransform.rotation = Quaternion.AngleAxis(-90f, doorTransform.up);
        isDoorOpen = true;
    }


    public void OnFocusEnter()
    {
        //Do nothing.
    }

    public void Interact(GameObject other)
    {
        //if locked don't do the interact logic.
        if (isLocked)
        {
            //TODO: Play sound of door handle locked
            //and not turning.
            return;
        }

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

    public void InteractHold()
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
