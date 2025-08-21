using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUIController : MonoBehaviour
{
    public GameObject playerObject;
    public PlayerInput playerInput;

    private bool _paused;
    public bool paused { get { return _paused; } set { _paused = value; OnPauseStateSwitched(); } }

    public GameObject pauseUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        paused = false;
        pauseUI.SetActive(paused);

        OnPauseStateSwitched();
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

    public void OnPauseStateSwitched()
    {
        //turn cursor off or on depending on if the game is paused.
        if (!paused) { 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
