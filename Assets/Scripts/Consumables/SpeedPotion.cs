using System.Collections;
using UnityEngine;

public class SpeedPotion : Consumable
{
    public override void Consume()
    {
        //In l4d2 adrenaline lasts for 15 seconds
        //so we'll have it last that long.
        
        //Move Speed multiplier
        parentPlayer.statManager.AddStat(
            new StatModifier
            {
                type = ModifiedStat.MoveSpeed,
                multiplier = 0.5f, //50%, we add 1 automatically when calculating multipliers later.
                duration = 15f
            });

        //Add use speed multiplier,
        //makes picking up other players faster
        //and makes appliables get applied faster.
        parentPlayer.statManager.AddStat(
        new StatModifier
        {
            type = ModifiedStat.UseSpeed,
            multiplier = 2f, //50%, we add 1 automatically when calculating multipliers later.
            duration = 15f
        });

        //We'll also give a small amount of degrading health.
        parentPlayer.degradingHealth += 5f;

        //TODO:
        //The player should start the animation for consuming and then
        //call this method at the end of the animation
        //so this is applied and destroyed after it's animation plays.
        Destroy(gameObject);
    }
}
