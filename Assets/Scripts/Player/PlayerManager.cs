using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


//This code was adopted from the following tutorial:
//https://www.youtube.com/watch?app=desktop&v=fyC8I0DaGgs&t=0s
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public GameObject playerPrefab;

    public PlayerInputManager inputManager;



    public Material[] materials;

    private void Awake()
    {
        instance = this;

        inputManager = GetComponent<PlayerInputManager>();

        SpawnAllPlayers();
    }

   

    //TODO: Only allow players to join in the Character Select screen.
    public void OnPlayerJoined(PlayerInput input)
    {
        //Don't allow joining unless we're in the character select scene.
        if (SceneManager.GetActiveScene().name != "Character Select Scene")
        {
            return;
        }

        if (GameManager.Instance.playerExists(input.GetDevice<InputDevice>()))
        {
            Debug.LogError("A duplicate Player was added when one already exists for this device!!!");
            return;
        }

        Guid id = Guid.NewGuid();
        GameObject player = input.gameObject;

        int curPlayerIndex = inputManager.playerCount - 1;

        //Set the player position to be the same position offset by the current player count.
        player.transform.position = new Vector3(curPlayerIndex, 1, 0);

        player.GetComponent<PlayerController>().init(id, curPlayerIndex, materials[curPlayerIndex]);
    }

    //TODO: Only allow players to leave in the Character Select screen.
    public void OnPlayerLeave(PlayerInput input)
    {
        //Don't allow leaving unless we're in the character select scene.
        if (SceneManager.GetActiveScene().name != "Character Select Scene")
        {
            return;
        }

        //Destroy this player.
        Destroy(input.gameObject);
        Debug.Log("Player " + input.playerIndex + " Left!");

        if (inputManager.playerCount <= 0)
        {
            Debug.Log("All players have left, return to title screen!");
            CustomSceneManager.LoadSceneAsync("Title Scene");
        }
    }



    //Called before our scene unloads.
    public void OnBeforeSceneUnloaded()
    {

    }

    public void SpawnAllPlayers()
    {
        //Spawn all players.
        for (int i = 0; i < GameManager.Instance.playerInfos.Count; i++)
        {
            //Spawn the player.
            SpawnPlayer(GameManager.Instance.playerInfos[i]);
        }
    }

    public void SpawnPlayer(PlayerInfo p)
    {
        //Create the player input object.
        PlayerInput pi = PlayerInput.Instantiate(playerPrefab, p.playerIndex, p.controlScheme, p.splitScreenIndex);
    }
}
