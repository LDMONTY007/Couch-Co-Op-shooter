using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUIController : MonoBehaviour
{
    public GameObject playerObject;
    public PlayerInput playerInput;

    [HideInInspector]
    public bool paused = false;

    public GameObject pauseUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        paused = false;
        pauseUI.SetActive(paused);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLeaveButtonPressed()
    {
        //playerObject.GetComponent<PlayerInput>().
        Destroy(playerObject);
    }

    //used for the "back" input in the control scheme.
    public void OnPauseResumeKeyPressed(InputAction.CallbackContext context)
    {
        //only when the key is initially pressed, not when it is held.
        if (context.started)
        {
            //resume when paused
            if (paused)
            {
                OnResumeButtonPressed();
            }
            //pause otherwise.
            else
            {
                OnPauseButtonPressed();
            }
        }
    }

    public void OnPauseButtonPressed()
    {
        paused = true;
        pauseUI.SetActive(paused);
        //Switch to the UI controls.
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void OnResumeButtonPressed()
    {
        paused = false;
        pauseUI.SetActive(paused);
        //Switch the current control scheme back to the normal game controls.
        playerInput.SwitchCurrentActionMap("Player");
    }

    private string GetCorrespondingControlScheme(InputDevice device)
    {
        if (device is Gamepad)
        {
            return "Gamepad";
        }
        if (device is Keyboard)
        {
            return "Keyboard&Mouse";
        }
        if (device is Mouse)
        {
            return "Keyboard&Mouse";
        }
        return null;
    }
}
