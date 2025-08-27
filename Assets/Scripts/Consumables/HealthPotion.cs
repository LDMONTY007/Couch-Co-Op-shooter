using UnityEngine;

public class HealthPotion : Consumable
{
    public override void Consume()
    {
        //In l4d2 pills give the player 50 temp health,
        //so we'll do the same.
        parentPlayer.degradingHealth += 50f;

        //Destroy this consumable as it takes no time to consume.
        Destroy(gameObject);
    }
}
