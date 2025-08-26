using UnityEngine;

//used for interactible objects.
public interface IInteractible
{
    //Use raycasts on the player to detect if they look at an interactible.
    void OnFocusEnter();

    void Interact();

    void InteractHold();

    void InteractStopHold();

    //Use raycasts on the player to detect if they stop looking at an interactible.
    void OnFocusLeave();
}
