using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


//This code was adopted from the following tutorial:
//https://www.youtube.com/watch?app=desktop&v=fyC8I0DaGgs&t=0s
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour, IDataPersistence
{
    public static PlayerManager instance;

    public GameObject playerPrefab;

    public PlayerInputManager inputManager;

    //Is this a level? if so we should auto-join the players
    //based on their saved data.
    public bool isLevel = false;

    public bool isCharacterSelectScene = false;

    public Material[] materials;

    public List<PlayerController> playerList = new List<PlayerController>();

    //Reference to the room in a level where
    //dead players can be rescued and will respawn.
    //Dead players will only spawn in this room
    //on the start of the level, dying in the level
    //means they will respawn in the next level.
    public RespawnRoom respawnRoom;

    private void Awake()
    {
        instance = this;

        inputManager = GetComponent<PlayerInputManager>();


    }

    private void Start()
    {

    }



   /* //TODO: Only allow players to join in the Character Select screen.
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


*//*        if (inputManager.playerCount <= 0)
        {
            Debug.Log("All players have left, return to title screen!");
            CustomSceneManager.LoadSceneAsync("Title Scene");
        }*//*
    }*/

    public void SpawnAllPlayers(GameData gameData)
    {
        Debug.LogWarning("SPAWNING ALL PLAYERS");

       

        //Spawn all players.
        for (int i = 0; i < gameData.playerInfos.Length; i++)
        {
            //Skip if there is no device assigned for this player.
            if (!gameData.playerInfos[i].hasDevice)
                continue;

            //if the player is dead, spawn them 
            //in the respawn room instead.
            if (gameData.playerInfos[i].isDead)
            {
                SpawnPlayerInRespawnRoom(gameData.playerInfos[i], gameData);
                Debug.LogWarning("RESPAWN");
                continue;
            }

            Debug.LogWarning("SPAWN");
            //Spawn the player.
            SpawnPlayer(gameData.playerInfos[i], gameData);
        }
    }

    public void SpawnPlayer(PlayerInfo p, GameData gameData)
    {
        //Create the player input object.
        //PlayerInput pi = PlayerInput.Instantiate(playerPrefab, p.playerIndex, p.controlScheme, p.splitScreenIndex);
        //Join the player and let the system auto-assign the screen.
        PlayerInput pi = inputManager.JoinPlayer(playerIndex: p.playerIndex, controlScheme: p.controlScheme, pairWithDevice: InputSystem.devices.First<InputDevice>(d => d.deviceId == p.deviceID));
        Debug.LogWarning("Player " + p.playerIndex + " Spawned!");

        //Get player controller.
        PlayerController controller = pi.GetComponent<PlayerController>();

        //Call init on the player.
        controller.init(Guid.Parse(p.guid), p.playerIndex, materials[p.playerIndex], p);

        //Add to our player list.
        playerList.Add(controller);

        //Call load data manually so that the player knows
        //it has been loaded and will instantiate
        //the weapon in it's game data.
        controller.LoadDataManually(gameData);
    }

    public void SpawnPlayerInRespawnRoom(PlayerInfo p, GameData gameData)
    {
        //Create the player input object.
        //PlayerInput pi = PlayerInput.Instantiate(playerPrefab, p.playerIndex, p.controlScheme, p.splitScreenIndex);
        //Join the player and let the system auto-assign the screen.
        PlayerInput pi = inputManager.JoinPlayer(playerIndex: p.playerIndex, controlScheme: p.controlScheme, pairWithDevice: InputSystem.devices.First<InputDevice>(d => d.deviceId == p.deviceID));
        Debug.LogWarning("Player " + p.playerIndex + " Spawned!");

        //Get player controller.
        PlayerController controller = pi.GetComponent<PlayerController>();

        //Call init on the player.
        controller.init(Guid.Parse(p.guid), p.playerIndex, materials[p.playerIndex], p);

        //Add to our player list.
        playerList.Add(controller);

        //Call load data manually so that the player knows
        //it has been loaded and will instantiate
        //the weapon in it's game data.
        controller.LoadDataManually(gameData);

        //Put this player at their respawn transform in the respawn room.
        controller.transform.position = respawnRoom.spawnTransforms[p.playerIndex].position;

        //Say the player is no longer dead.
        controller.isDead = false;
        //Say the player is in the respawn room.
        controller.inRespawnRoom = true;

        //Add the player to the list of players in the respawn room.
        respawnRoom.deadPlayers.Add(controller);
    }

    public void LoadData(GameData gameData)
    {
        //Spawn all players with their loaded data.
        SpawnAllPlayers(gameData);
    }

    public void SaveData(ref GameData gameData)
    {
        //Make sure if the respawn room hasn't
        //been unlocked that we set the dead players
        //back to being dead so they will show up in
        //the respawn room again in the next level.
        if (respawnRoom != null)
        respawnRoom.SetPlayersDeadIfNotFreed();

        //Call save data manually on players
        //so any private variables are properly
        //saved and we don't need to change access modifiers.
        foreach(var player in playerList)
        {
            player.SaveDataManually(ref gameData);
        }
    }
}
