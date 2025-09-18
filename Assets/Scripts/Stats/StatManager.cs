using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StatManager : MonoBehaviour
{
    
    public List<StatModifier> modifiers = new List<StatModifier>();

    public void AddStat(StatModifier stat)
    { 
        modifiers.Add(stat);
        //if a stat duration is zero,
        //that means it doesn't "wear off" 
        if (stat.duration != 0)
        {
            //Start the duration coroutine for this stat.
            StartCoroutine(StatDurationCoroutine(stat));
        }
    }

    IEnumerator StatDurationCoroutine(StatModifier stat)
    {
        float curTime = 0f;

        //Wait until we reach the stat's
        //duration and then remove it from
        //the modifiers.
        while (curTime < stat.duration)
        {
            curTime += Time.deltaTime;
            yield return null;
        }

        //Remove this stat from the modifiers list.
        modifiers.Remove(stat);

        Debug.Log("REMOVED");
    }

}
