using System;
using System.Linq;
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

    //Is this a level? if so we should auto-join the players
    //based on their saved data.
    public bool isLevel = false;

    public bool isCharacterSelectScene = false;

    public Material[] materials;

    private void Awake()
    {
        instance = this;

        inputManager = GetComponent<PlayerInputManager>();


    }

    private void Start()
    {
        if (isLevel)
        SpawnAllPlayers();
    }



    //TODO: Only allow players to join in the Character Select screen.
    public void OnPlayerJoined(PlayerInput input)
    {
        //Don't allow joining unless we're in the character select scene.
        if (!isCharacterSelectScene)
        {
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
        if (!isCharacterSelectScene)
        {
            return;
        }

        //Destroy this player.
        Destroy(input.gameObject);
        Debug.Log("Player " + input.playerIndex + " Left!");

        //TODO:
        //We need to manually code a specific option for players actually leaving and redo this system quite a bit.
        //Right now it'll think a player leaves when scenes load which is a problem.
        //Say this player doesn't have a device.
        //DataPersistenceManager.instance.GetGameData().playerInfos[input.playerIndex].hasDevice = false;


/*        if (inputManager.playerCount <= 0)
        {
            Debug.Log("All players have left, return to title screen!");
            CustomSceneManager.LoadSceneAsync("Title Scene");
        }*/
    }

    public void SpawnAllPlayers()
    {
        Debug.LogWarning("SPAWNING ALL PLAYERS");

        Debug.LogWarning(GameManager.Instance.playerInfos.ToString());
        Debug.LogWarning(DataPersistenceManager.instance.GetGameData().playerInfos.ToString());

        //Spawn all players.
        for (int i = 0; i < DataPersistenceManager.instance.GetGameData().playerInfos.Length; i++)
        {
            //Skip if there is no device assigned for this player.
            if (!DataPersistenceManager.instance.GetGameData().playerInfos[i].hasDevice)
                continue;

            Debug.LogWarning("SPAWN");
            //Spawn the player.
            SpawnPlayer(DataPersistenceManager.instance.GetGameData().playerInfos[i]);
        }
    }

    public void SpawnPlayer(PlayerInfo p)
    {
        //Create the player input object.
        //PlayerInput pi = PlayerInput.Instantiate(playerPrefab, p.playerIndex, p.controlScheme, p.splitScreenIndex);
        //Join the player and let the system auto-assign the screen.
        inputManager.JoinPlayer(playerIndex: p.playerIndex, controlScheme: p.controlScheme, pairWithDevice: InputSystem.devices.First<InputDevice>(d => d.deviceId == p.deviceID));
        Debug.LogWarning("Player " + p.playerIndex + " Spawned!");
    }
}
