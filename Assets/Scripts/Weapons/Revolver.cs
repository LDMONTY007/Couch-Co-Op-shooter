using System.Collections;
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

        if (Physics.Raycast(c.transform.position, c.transform.forward, out var hitInfo, attackDist, mask))
        {
            Debug.Log("HIT!!");
            IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //if we actually hit a damageable.
            //Also pass the player as the other gameobject rather than this weapon.
            if (damageable != null)
            damageable.TakeDamage(new DamageData() { damage = damage, stunTime = stunTime, other = player.gameObject, point = hitInfo.point, normal = hitInfo.normal });
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

    public override void CancelUse()
    {
        //Do nothing as holding the button down does nothing for this useable.
    }

    public override void Use(float useSpeed = 1f)
    {
        Attack(parentPlayer.cam, parentPlayer);
    }
}
