using UnityEngine;

public class Weapon : MonoBehaviour, IUseable
{
    public Rigidbody rb;

    public PlayerController parentPlayer;

    public Sprite icon;

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
    public virtual void Use()
    {
        
    }
}
