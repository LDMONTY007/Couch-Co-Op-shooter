using UnityEngine;

public class Weapon : MonoBehaviour, IUseable
{
    public PrefabData prefabData;

    public Rigidbody rb;

    public PlayerController parentPlayer;

    public Sprite icon;

    public Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Attack(Camera cam, PlayerController player)
    {

    }
    public virtual void CancelUse()
    {

    }
    public virtual void Use(float useSpeed = 1f)
    {
        
    }

    public void ReleaseUse()
    {
        
    }
}
