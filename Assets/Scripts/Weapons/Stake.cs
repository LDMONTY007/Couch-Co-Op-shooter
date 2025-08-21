using UnityEngine;

public class Stake : Weapon
{
    //base damage, no damage type bonus applied here.
    int damage = 5;
    int mask;
    float attackDist = 50f;

    private void Awake()
    {
        //Get everything except player and ignore raycast.
        mask = ~LayerMask.GetMask("Player", "IgnoreRaycast");
    }

    public override void Attack(Camera c)
    {
        if (Physics.Raycast(c.transform.position, c.transform.forward, out var hitInfo, attackDist, mask))
        {
            Debug.Log("HIT!!");
            IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //if we actually hit a damageable.
            if (damageable != null)
            damageable.TakeDamage(damage, gameObject);
        }
    }
}
