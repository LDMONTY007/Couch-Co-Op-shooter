using System.Collections.Generic;
using UnityEngine;

public class RespawnRoom : MonoBehaviour
{
    public Transform[] spawnTransforms = new Transform[4];

    bool wasRespawnRoomUnlocked = false;

    public List<PlayerController> deadPlayers = new List<PlayerController>();

    public void FreePlayers()
    {
        wasRespawnRoomUnlocked = true;
        foreach (PlayerController player in deadPlayers)
        {
            //Say the player is no longer in the respawn room.
            player.inRespawnRoom = false;
        }
    }

    public void SetPlayersDeadIfNotFreed()
    {
        if (!wasRespawnRoomUnlocked)
        {
            foreach (PlayerController player in deadPlayers)
            {
                player.isDead = true;
            }
        }
    }

}
