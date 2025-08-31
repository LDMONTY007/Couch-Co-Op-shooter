using UnityEngine;
using UnityEngine.Events;

public class Consumable : MonoBehaviour, IUseable
{
    public PrefabData prefabData;

    public Rigidbody rb;

    public Sprite icon;

    public PlayerController parentPlayer;

    //called before this gameobject is destroyed.
    public UnityEvent<IUseable, int> onBeforeDestroy;

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

    public void ReleaseUse()
    {
        throw new System.NotImplementedException();
    }
}
