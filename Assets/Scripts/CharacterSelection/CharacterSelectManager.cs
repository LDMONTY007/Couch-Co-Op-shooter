using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
using System.Collections.Generic;

public class CharacterSelectManager : MonoBehaviour
{
    public GameObject cursorPrefab;

    public Canvas characterSelectCanvas;

    public PlayerInputManager inputManager;

    List<PlayerInput> inputList = new List<PlayerInput>();

    private void Awake()
    {



        // Wait for first button press on a gamepad.
        /*InputSystem.onEvent
            .ForDevice<Keyboard>()
            .Where(e => e.HasButtonPress())
            .CallOnce(ctrl => Debug.Log($"Button {ctrl} pressed"));*/

        InputSystem.onEvent.Where(e => e.HasButtonPress()).Call(eventPtr =>
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
            if (Application.isPlaying)
            {
                foreach (var control in eventPtr.EnumerateChangedControls())
                {

                    string scheme = GetCorrespondingControlScheme(control.device);

                    if (!GameManager.Instance.inputDevices.Contains(control.device))
                    {
                        //Assign this device to any empty input slot in our 4 input array.
                        for (int i = 0; i < GameManager.Instance.inputDevices.Length; i++)
                        {
                            if (GameManager.Instance.inputDevices[i] == null)
                            {
                                GameManager.Instance.inputDevices[i] = control.device;
                                //Join this player.
                                inputList.Add(PlayerInput.Instantiate(cursorPrefab, playerIndex: i, pairWithDevice: control.device));
                                //Set the parent to be the canvas.
                                inputList[inputList.Count - 1].gameObject.transform.SetParent(characterSelectCanvas.transform);

                                Debug.Log("PLAYER JOINED!!!");
                                break;
                            }
                        }
                    }

                    

                    
                }
            }
        });

        InputSystem.onDeviceChange +=
        (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    // New Device.
                    Debug.Log(device + " was Added!");
                    break;
                case InputDeviceChange.Disconnected:
                    //On disconnect we want to delete the cursor and their playerInfo
                    //BUT ONLY IN THE SELECTION SCENE
                    //If a controller is disconnected during gameplay it should auto pause
                    //and then let the user reconnect it. THIS IS REALLY IMPORTANT.

                    //if this device exists in our input array, remove it when it disconnects.
                    //remove this device from our 4 input array.
                    for (int i = 0; i < GameManager.Instance.inputDevices.Length; i++)
                    {
                        if (GameManager.Instance.inputDevices[i] == device)
                        {
                            GameManager.Instance.inputDevices[i] = null;
                            break;
                        }
                    }

                    int index = inputList.FindIndex(0, inputList.Count, p => p.GetDevice<InputDevice>() == device);

                    //Destroy this cursor.
                    Destroy(inputList[index].gameObject);

                    //Remove the input device from our list.
                    inputList.RemoveAt(index);



                    // Debug the disconnect.
                    Debug.Log(device + " was Disconnected!");
                    break;
                case InputDeviceChange.Reconnected:
                    // Plugged back in.
                    Debug.Log(device + " was Reconnected!");
                    break;
                case InputDeviceChange.Removed:
                    // Remove from Input System entirely; by default, Devices stay in the system once discovered.
                    Debug.Log(device + " was Removed!");
                    break;
                default:
                    // See InputDeviceChange reference for other event types.
                    break;
            }
        };
        //GameManager.instance.characterManager = this;
    }

/*    public void OnPlayerJoined(PlayerInput input)
    {
        //Assign this device to any empty input slot in our 4 input array.
        for (int i = 0; i < GameManager.Instance.inputDevices.Length; i++)
        {
            if (GameManager.Instance.inputDevices[i] != null)
            {
                GameManager.Instance.inputDevices[i] = input.GetDevice<InputDevice>();
                //Join this player.
                inputList.Add(PlayerInput.Instantiate(cursorPrefab, playerIndex: i, pairWithDevice: input.GetDevice<InputDevice>()));
                //Set the parent to be the canvas.
                inputList[inputList.Count - 1].gameObject.transform.SetParent(characterSelectCanvas.transform);

                Debug.Log("PLAYER JOINED!!!");
                break;
            }
        }
    }*/

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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

    public void SpawnCursor()
    {

    }
}
