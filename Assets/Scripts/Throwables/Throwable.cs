using UnityEngine;

public class Throwable : MonoBehaviour, IUseable
{
    public PrefabData prefabData;

    public Rigidbody rb;

    public Collider col;

    public Sprite icon;

    private bool _readyToThrow = false;
    //is this throwable ready to be thrown
    //when the player releases it?
    public bool readyToThrow { get { return _readyToThrow; } set { _readyToThrow = value; } }

    public bool wasThrown = false;

    public Transform launchTransform;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb);
    }

    public void CancelUse()
    {
        //Don't throw the throwable if it's canceled.
        readyToThrow = false;
    }

    public void Use(float useSpeed = 1f)
    {
        Debug.Log("HERE");
        readyToThrow = true;
    }

    public void ReleaseUse()
    {
        //TODO:
        //Throw the throwable.
        if (readyToThrow)
        {
            //Remove all parents
            transform.parent = null;
            //Set our position to be the launch transform
            transform.position = launchTransform.position;
            //Set our rotation to match the launch transform
            transform.rotation = launchTransform.rotation;

            //Make sure we have a rigidbody
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            //Launch the throwable with an impulse force
            //impulse means instant change in velocity.
            rb.AddForce(launchTransform.forward.normalized * 5f, ForceMode.Impulse);

            //say we were thrown.
            wasThrown = true;
            //call OnThrown()
            OnThrown();
        }
    }

    public virtual void OnThrown()
    {
        //Todo: you would start your timer
        //for your bomb or start waiting until
        //colliding with an object
        //to explode here.
        //Override this in your child class.
    }

}
