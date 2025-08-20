using System;
using UnityEngine;
using UnityEngine.InputSystem;


//This code was adopted from the following tutorial:
//https://www.youtube.com/watch?app=desktop&v=fyC8I0DaGgs&t=0s
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public PlayerInputManager inputManager;

    public Material[] materials;

    private void Awake()
    {
        instance = this;

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

    public void OnPlayerLeave(PlayerInput input)
    {
        //Destroy this player.
        Destroy(input.gameObject);
        Debug.Log("Player " + input.playerIndex + " Left!");

        if (inputManager.playerCount <= 0)
        {
            Debug.Log("All players have left, return to title screen!");
            CustomSceneManager.LoadSceneAsync("Title Scene");
        }
    }
}
