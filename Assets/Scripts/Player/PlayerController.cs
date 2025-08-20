using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    public TMP_Text idLabel;
    Guid guid;

    Vector2 moveInput;

    public Renderer playerModel;

    public void init(Guid id, int index, Material mat)
    {
        idLabel.text = "Player " + index;

        guid = id;
        playerModel.material = mat;
    }

    public void OnMove(InputAction.CallbackContext callback)
    {
        moveInput = callback.ReadValue<Vector2>();
    }

    private void Update()
    {
        transform.Translate(moveSpeed * Time.deltaTime * moveInput);
    }
}
