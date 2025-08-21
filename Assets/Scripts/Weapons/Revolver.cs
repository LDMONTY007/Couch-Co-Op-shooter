using System.Collections;
using UnityEngine;

public class Revolver : Weapon
{
    //base damage, no damage type bonus applied here.
    float damage = 20f;
    int mask;
    float attackDist = 50f;

    public ParticleSystem muzzleFlashParticles;

    bool onCooldown = true;

    float attackCooldownTime = 0.3f;

    float stunTime = 1.5f;

    private void Awake()
    {
        //Get everything except player and ignore raycast.
        mask = ~LayerMask.GetMask("Player", "IgnoreRaycast");
    }

    public override void Attack(Camera c)
    {
        //if this weapon is on cooldown, then
        //don't execute this method.
        if (!onCooldown)
        {
            return;
        }

        //Play the muzzleFlashParticles
        muzzleFlashParticles.Play();

        if (Physics.Raycast(c.transform.position, c.transform.forward, out var hitInfo, attackDist, mask))
        {
            Debug.Log("HIT!!");
            IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //if we actually hit a damageable.
            if (damageable != null)
            damageable.TakeDamage(damage, stunTime, gameObject);
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
        muzzleFlashParticles.Clear();
    }
}
