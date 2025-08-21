using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 5f;
    public float gravity = -9.81f;

    public TMP_Text idLabel;
    Guid guid;

    Vector2 moveInput;

    float mouseLookSpeed = 10f;
    float controllerLookSpeed = 100f;
    Vector2 curLook;

    public Renderer playerModel;

    private CharacterController controller;

    public Camera cam;

    private PlayerInput playerInput;
    private InputAction jumpAction;
    private InputAction lookAction;

    float yVel = 0f;

    bool grounded = false;

    public void init(Guid id, int index, Material mat)
    {
        idLabel.text = "Player " + index;

        guid = id;
        playerModel.material = mat;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
    }

    public void OnMove(InputAction.CallbackContext callback)
    {
        moveInput = callback.ReadValue<Vector2>();
    }

    private void HandleLook()
    {
        var mouseInput = lookAction.ReadValue<Vector2>();

        //apply mouse input to our current look vector using deltaTime and look speed.
        curLook.y -= mouseInput.y * GetCorrespondingLookSensitivity(playerInput.devices[0]) * Time.deltaTime;
        curLook.x += mouseInput.x * GetCorrespondingLookSensitivity(playerInput.devices[0]) * Time.deltaTime;

        //clamp to max and min look angle.
        curLook.y = Mathf.Clamp(curLook.y, -80f, 80f);

        //rotate the camera up and down
        cam.transform.localRotation = Quaternion.Euler(curLook.y, 0f, 0f);

        //set rotation for the body.
        transform.localRotation = Quaternion.Euler(0f, curLook.x, 0f);
    }
   


    private void Update()
    {
        HandleLook();

        grounded = controller.isGrounded;

        //if the player is grounded zero out their y velocity.
        if (grounded && yVel < 0)
        {
            yVel = 0f;
        }

        //Jump code ripped from Unity Manual
        //https://docs.unity3d.com/ScriptReference/CharacterController.Move.html
        //If the button is held, let the player jump.
        //This allows the player to hold space and jump the moment they are grounded again,
        //making bunny hopping easy.
        if (jumpAction.GetButton() && grounded)
        {
            yVel = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        //apply gravity.
        yVel += gravity * Time.deltaTime;

        //Axis aligned move, aligned with body axes via projection.
        Vector3 aaMove = (transform.forward.normalized * moveInput.y) + (transform.right.normalized * moveInput.x);

        //Add the y velocity for jumping to the movement.
        Vector3 finalMove = transform.up * yVel + aaMove.normalized * moveSpeed;

        //Move the character controller.
        controller.Move(finalMove * Time.deltaTime);

        
    }

    private float GetCorrespondingLookSensitivity(InputDevice device)
    {
        if (device is Gamepad)
        {
            return controllerLookSpeed;
        }
        if (device is Keyboard)
        {
            return mouseLookSpeed;
        }
        if (device is Mouse)
        {
            return mouseLookSpeed;
        }
        return mouseLookSpeed;
    }
}
