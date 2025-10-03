using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stake : PrimaryWeapon
{
    //base damage, no damage type bonus applied here.
    int damage = 5;
    int mask;
    float attackDist = 50f;

    float stunTime = 0.5f;

    private void Awake()
    {
        //Get everything except player and ignore raycast.
        mask = ~LayerMask.GetMask("Player", "IgnoreRaycast");
    }

    public override void Attack(Camera c, PlayerController player)
    {
        if (Physics.Raycast(c.transform.position, c.transform.forward, out var hitInfo, attackDist, mask))
        {
            Debug.Log("HIT!!");
            IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //Store score data here
            List<ScoreData> scoreData = new List<ScoreData>();

            //if we actually hit a damageable.
            //Also pass the player as the other gameobject rather than this weapon.
            if (damageable != null)
                scoreData = damageable.TakeDamageScored(new DamageData() { damageType = damageType, damage = damage, stunTime = stunTime, other = player.gameObject, point = hitInfo.point, normal = hitInfo.normal }).ToList();

            //add all the score data's from the damaged object to the player list.
            //this is so if they hit multiple enemies with a weapon or even if an explosive
            //hits multiple enemies.
            foreach (ScoreData sd in scoreData) 
            {
                //Add the score data to the player.
                player.AddScoreData(sd);
            }

            
        }
    }

    public override void CancelUse()
    {
        //This is not a hold down to use weapon so this goes unused.
    }

    public override void Use(float useSpeed = 1f)
    {
        Attack(parentPlayer.cam, parentPlayer);
    }
}
