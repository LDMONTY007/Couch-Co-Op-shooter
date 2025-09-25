using UnityEngine;
using UnityEngine.Events;

public class InteractionPropegator : MonoBehaviour, IInteractible
{
    //called when this object is interacted with,
    //used to forward the interaction data to 
    //a parent interactible script.
    public UnityEvent<GameObject> OnInteract;

    public void Interact(GameObject other)
    {
        OnInteract?.Invoke(other);
    }

    public void InteractHold(float useSpeed = 1)
    {
        
    }

    public void InteractStopHold()
    {
        
    }

    public void OnFocusEnter()
    {
        
    }

    public void OnFocusLeave()
    {
        
    }
}
