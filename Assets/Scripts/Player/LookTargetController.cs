using UnityEngine;

public class LookTargetController : MonoBehaviour
{
    public Transform lookTarget;

    public float maxLookDistance = 30f;

    public PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleLookTargeting();
    }

    public IInteractible lastFocusInteractible = null;

    public void HandleLookTargeting()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxLookDistance, playerController.playerMask))
        {
            //handle focus on interactible.
            IInteractible interactible = hit.collider.GetComponent<IInteractible>();
            if (interactible != null)
            {
                //say we're looking at an interactible.
                interactible.OnFocusEnter();
                lastFocusInteractible = interactible;

                //if the last interactible was in focus before,
                //tell it that it is no longer in focus.
                if (lastFocusInteractible != null && lastFocusInteractible != interactible)
                {
                    //say we lost focus on the interactible.
                    lastFocusInteractible.OnFocusLeave();
                    lastFocusInteractible = null;
                }
            }
            //if our interactible is null.
            else if (lastFocusInteractible != null)
            {
                //say we lost focus on the interactible.
                lastFocusInteractible.OnFocusLeave();
                lastFocusInteractible = null;
            }

            //Set the look target position to be where we hit.
            lookTarget.position = hit.point;
        }
        else
        {
            if (lastFocusInteractible != null)
            {
                //say we lost focus on the interactible.
                lastFocusInteractible.OnFocusLeave();
                lastFocusInteractible = null;
            }

            //Set the look target position to be where we're currently looking.
            lookTarget.position = transform.position + transform.forward.normalized * maxLookDistance;
        }
    }
}
