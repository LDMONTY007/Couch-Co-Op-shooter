using UnityEngine;

public class Weapon : Usable
{

    public PrefabData prefabData;

    public Rigidbody rb;

    public Collider col;

    public PlayerController parentPlayer;

    public Sprite icon;

    public Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Attack(Camera cam, PlayerController player)
    {

    }
    public override void CancelUse()
    {

    }
    public override void Use(float useSpeed = 1f)
    {
        
    }

    public override void ReleaseUse()
    {
        
    }
}
