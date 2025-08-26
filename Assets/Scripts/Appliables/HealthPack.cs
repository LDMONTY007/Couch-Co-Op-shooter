using UnityEngine;

public class HealthPack : Appliable
{
    public float HealthToRestore = 50f;

    //Require the didApply bool so that
    //didApply gets updated properly.
    public override void OnApply(PlayerController playerController)
    {
        //Add to the player's health.
        playerController.curHealth += HealthToRestore;
    }
}
