using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraWeapon : PrimaryWeapon
{
    public Light cameraLight;

    //base damage, no damage type bonus applied here.
    float damage = 20f;
    int mask;
    float attackDist = 50f;

    bool onCooldown = true;

    float attackCooldownTime = 0.34f;

    float stunTime = 1.5f;

    private void Awake()
    {
        //Turn off the camera flash light.
        cameraLight.enabled = false;
        //Get only the enemy and vampire mask, ignore all other layers.
        mask = LayerMask.GetMask("Enemy", "Vampire");
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
        //animator.SetTrigger("Fire");

        //Play the muzzleFlashParticles
        //muzzleFlashParticles.Play();

        Vector3 camWorldExtents = LDUtil.PerspectiveCameraFrustumSize(c, attackDist / 2f);

        Vector3 halfExtents = new Vector3(camWorldExtents.x / 2f, camWorldExtents.y / 2f, attackDist);
        //Do the overlapbox so that it extends from the head's position to the camShootDist
        Collider[] cols = Physics.OverlapBox(c.transform.position + c.transform.forward.normalized * attackDist / 2f, halfExtents, Quaternion.identity, mask);

        //Check all colliders that were in the box check
        //and see if any of them are in the current camera's view.
        for (int i  = 0; i < cols.Length; i++)
        {

            //Check that the collider's parent gameobject is within the view of the camera.
            if (LDUtil.IsInView(c, c.transform.position, cols[i].transform.root.gameObject))
            {
                Debug.Log("HIT!!");
                IDamageable damageable = cols[i].gameObject.GetComponent<IDamageable>();

                //create temp scoreData var
                List<ScoreData> scoreData = new();

                //if we actually hit a damageable.
                //Also pass the player as the other gameobject rather than this weapon.
                if (damageable != null)
                    scoreData = damageable.TakeDamageScored(new DamageData() { damageType = damageType, damage = damage, stunTime = stunTime, other = player.gameObject, point = cols[i].transform.position, normal = (cols[i].transform.position - c.transform.position).normalized }).ToList();

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

    public override void OnEquip(PlayerController p)
    {
        //Make vampires invisible to the player while using
        //the camera.
        p.cam.cullingMask |= LayerMask.GetMask("Vampire");
    }

    public override void OnUnequip(PlayerController p)
    {
        //Make vampires visible to the player --+
        p.cam.cullingMask &= ~(LayerMask.GetMask("Vampire"));
    }

    public override void Use(float useSpeed = 1f)
    {
        Attack(parentPlayer.cam, parentPlayer);
    }
}
