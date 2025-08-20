using System;
using UnityEngine;
using UnityEngine.InputSystem;


//This code was adopted from the following tutorial:
//https://www.youtube.com/watch?app=desktop&v=fyC8I0DaGgs&t=0s
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    PlayerInputManager inputManager;

    public Material[] materials;

    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        Guid id = Guid.NewGuid();
        GameObject player = input.gameObject;

        int curPlayerIndex = inputManager.playerCount - 1;

        //Set the player position to be the same position offset by the current player count.
        player.transform.position = new Vector3(curPlayerIndex, 1, 0);

        player.GetComponent<PlayerController>().init(id, curPlayerIndex, materials[curPlayerIndex]);
    }
}
