using UnityEngine;
using UnityEngine.Events;

public class Consumable : Usable
{

    public PrefabData prefabData;

    public Rigidbody rb;

    public Collider col;

    public Sprite icon;

    public PlayerController parentPlayer;

    //called before this gameobject is destroyed.
    public UnityEvent<Usable, int> onBeforeDestroy;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void CancelUse()
    {
        throw new System.NotImplementedException();
    }

    public override void Use(float useSpeed = 1f)
    {
        Consume();
    }

    public virtual void Consume()
    {

    }

    private void OnDestroy()
    {
        //invoke onBeforeDestroy.
        onBeforeDestroy?.Invoke(this, 4);
    }

    public override void ReleaseUse()
    {
        throw new System.NotImplementedException();
    }
}
