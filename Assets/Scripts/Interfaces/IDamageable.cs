using UnityEngine;

//Interface used for any object when we want an object
//to recieve damage.
public interface IDamageable
{
    public void TakeDamage(DamageData damageData);
}

public struct DamageData
{
    //TODO: Add the damage type in here as well.
    public float damage; //the amount of damage
    public float stunTime; //the time to stun
    public GameObject other; //The object dealing damage
    public Vector3 point; //The point where the damage was hit
    public Vector3 normal; //The normal of the hit point.
}

