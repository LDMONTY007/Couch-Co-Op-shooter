using System.Collections.Generic;
using UnityEngine;

public class Saferoom : MonoBehaviour
{
    public PlayerManager playerManager;
    List<PlayerController> playersInRoom = new List<PlayerController>();

    public string sceneToLoad;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (sceneToLoad == string.Empty)
        {
            Debug.LogError("There is no assigned scene for the current saferoom object! please make sure to assign a scene!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandlePlayerCountReached()
    {
        Debug.Log("HERE 1");

        //verify that all players are in the room
        //and we don't have duplicate players.
        for (int i = 0; i < playerManager.playerList.Count; i++)
        {
            //if a player is dead or if they're still in the respawn room, skip them when
            //checking if they've made it into the room.
            if (playerManager.playerList[i].isDead || playerManager.playerList[i].inRespawnRoom)
            {
                continue;
            }

            //if our players in room contains a matching player,
            //continue executing this method.
            if (playersInRoom.Contains(playerManager.playerList[i]))
            {
                continue;
            }
            else
            {
                //otherwise return and stop executing this method.
                return;
            }
        }

        Debug.Log("HERE 3");

        //When all conditions are met we will load into the next level.

        //freeze all players.
        foreach (PlayerController player in playersInRoom)
        {
            
            player.canMove = false;
        }

        //Start loading the next scene.
        DataPersistenceManager.instance.LoadSceneAsync(sceneToLoad);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        
        //if the player isn't already marked as being in the room:
        //Add the player to the list of players that are in the room.
        if (player != null && !playersInRoom.Contains(player))
        {
            playersInRoom.Add(player);
        }

        HandlePlayerCountReached();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        //if the player is marked as being in the room,
        //remove them from the list of players in the room.
        if (player != null && playersInRoom.Contains(player))
        {
            playersInRoom.Remove(player);
        }
    }
}
