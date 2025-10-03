using UnityEngine;
using static Usable;

//Interface used for any object when we want an object
//to recieve damage.
public interface IDamageable
{
    public void TakeDamage(DamageData damageData);

    public ScoreData[] TakeDamageScored(DamageData damageData);
}

public struct DamageData
{
    public DamageType damageType; //Damage type
    public float damage; //the amount of damage
    public float stunTime; //the time to stun
    public GameObject other; //The object dealing damage
    public Vector3 point; //The point where the damage was hit
    public Vector3 normal; //The normal of the hit point.

}

