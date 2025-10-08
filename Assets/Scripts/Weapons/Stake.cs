using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stake : PrimaryWeapon
{
    //base damage, no damage type bonus applied here.
    int damage = 5;
    int mask;
    int weakMask;
    float attackDist = 50f;

    float stunTime = 0.5f;

    private void Awake()
    {
        //Get everything except player and ignore raycast.
        mask = ~LayerMask.GetMask("Player", "IgnoreRaycast", "WeakPoint");
        weakMask = LayerMask.GetMask("WeakPoint");
    }

    public override void Attack(Camera c, PlayerController player)
    {
        RaycastHit hitInfo;

        //first raycast only weakpoints then raycast for any other type of collider if no weakpoints are hit.
        if (Physics.Raycast(c.transform.position, c.transform.forward, out hitInfo, attackDist, weakMask) || Physics.Raycast(c.transform.position, c.transform.forward, out hitInfo, attackDist, mask))
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

    public override void OnEquip(PlayerController p)
    {
        //Make the heart weak points on vampires visible.
        //Add weak points to the culling mask
        p.cam.cullingMask |= LayerMask.GetMask("WeakPoint");
    }

    public override void OnUnequip(PlayerController p)
    {
        //Make the heart weak points on vampires invisible.
        //remove weak points from the culling mask.
        p.cam.cullingMask &= ~(LayerMask.GetMask("WeakPoint"));
    }
}
