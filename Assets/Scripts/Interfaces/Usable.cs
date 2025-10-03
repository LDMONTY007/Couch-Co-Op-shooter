using UnityEngine;

public abstract class Usable : MonoBehaviour 
{
    public enum DamageType
    {
        None,
        Silver,
        Stake,
        Garlic,
        Light,
        Healing,
    }

    //by default have the damage type be none.
    public DamageType damageType = DamageType.None;

    public Vector3 rotationWhenEquipped = Vector3.zero;
    public Vector3 rotationWhenUnequipped = Vector3.zero;

    public abstract void Use(float useSpeed = 1f);

    public abstract void CancelUse();

    public abstract void ReleaseUse();
}
