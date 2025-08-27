using UnityEngine;

public class Consumable : MonoBehaviour, IUseable
{
    public Rigidbody rb;

    public Sprite icon;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void CancelUse()
    {
        throw new System.NotImplementedException();
    }

    public void Use()
    {
        throw new System.NotImplementedException();
    }
}
