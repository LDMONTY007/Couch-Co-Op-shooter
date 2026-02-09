using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Revolver : SecondaryWeapon
{
    //base damage, no damage type bonus applied here.
    float damage = 20f;
    int mask;
    float attackDist = 50f;

    public ParticleSystem muzzleFlashParticles;

    bool onCooldown = true;

    float attackCooldownTime = 0.34f;

    float stunTime = 1.5f; 

    private void Awake()
    {
        //Get everything except player and ignore raycast.
        mask = ~LayerMask.GetMask("Player", "IgnoreRaycast");
    }

    public override void Attack(Camera c, PlayerController player)
    {
        //if this weapon is on cooldown, then
        //don't execute this method.
        if (!onCooldown)
        {
            return;
        }

        //set the gun's animator trigger.
        animator.SetTrigger("Fire");

        //Play the muzzleFlashParticles
        //muzzleFlashParticles.Play();

        player.uiController.screenShakeController.AddGunshotRumble(0.9f);

        IDamageable bestTarget = parentPlayer.uiController.aimTargetController.GetBestTarget();

        //Added a check for the attack distance.
        if (bestTarget != null && Vector3.Distance((bestTarget as Component).transform.position, transform.position) > attackDist)
        {
            bestTarget = null;
        }

        if (bestTarget != null)
        {
            GameObject targetObject = (bestTarget as Component).gameObject;

            Debug.Log("HIT!!");
            //IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //create temp scoreData var
            List<ScoreData> scoreData = new();

            //if we actually hit a damageable.
            //Also pass the player as the other gameobject rather than this weapon.
            if (bestTarget != null)
                scoreData = bestTarget.TakeDamageScored(new DamageData() { damageType = damageType, damage = damage, stunTime = stunTime, other = player.gameObject, point = targetObject.transform.position, normal = (targetObject.transform.position - player.transform.position).normalized }).ToList();

            //add all the score data's from the damaged object to the player list.
            //this is so if they hit multiple enemies with a weapon or even if an explosive
            //hits multiple enemies.
            foreach (ScoreData sd in scoreData)
            {
                //Add the score data to the player.
                player.AddScoreData(sd);
            }
        }

        /*if (Physics.Raycast(c.transform.position, c.transform.forward, out var hitInfo, attackDist, mask))
        {
            Debug.Log("HIT!!");
            IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //create temp scoreData var
            List<ScoreData> scoreData = new();

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
        }*/

        //Start the cooldown.
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        //Don't allow attacks until cooldown is over.
        onCooldown = false;
        yield return new WaitForSeconds(attackCooldownTime);
        onCooldown = true;

        //Delete the muzzleFlashParticles
        //muzzleFlashParticles.Clear();
    }

    public override void CancelUse()
    {
        //Do nothing as holding the button down does nothing for this useable.
    }

    public override void Use(float useSpeed = 1f)
    {
        Attack(parentPlayer.cam, parentPlayer);
    }
}
