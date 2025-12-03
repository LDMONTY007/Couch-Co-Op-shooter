using System.Collections;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{

    [Header("Damage Variables")]
    //the force at which we bounce off of the object that damaged us. 
    public float bounceForce = 5f;

    public bool invincible = false;

    //terraria uses this number for iframes as do most games.
    public float iFrameTime = 0.67f;

    public GameObject bloodParticlesPrefab;

    //private vars
    private Rigidbody rb;

    //coroutine references for ensuring no duplicates
    Coroutine iFramesRoutine = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(DamageData damageData)
    {
        //if we're invincible, 
        //then exit this method.
        if (invincible)
        {
            return;
        }

        //curHealth -= d;

        //Set to be low resolution for a small amount of time.
        StartLowResRoutine();

        //Start iFrames here.
        StartIFrames();

        //TODO:
        //change the layer of the visual model for the player to be
        //the lowres layer,
        //have the player get bounced away from the damaging object,
        //and then also give them invincibility frames where
        //they do the blinking in and out of existance thing.
        rb.linearVelocity += (transform.position - damageData.other.transform.position).normalized * bounceForce;

        //print out data about the player taking damage.
        Debug.Log("Dummy Took: ".Color("Orange") + damageData.damage.ToString().Color("Red") + " from " + damageData.other.transform.root.name.Color("Red"));

        //Spawn the particle system here with it's orientation
        //matching the damageData hit normal at the damageData hit point.
        Instantiate(bloodParticlesPrefab, damageData.point, Quaternion.LookRotation(damageData.normal));
        //When the particle system ends it will destroy itself.
    }

    public ScoreData[] TakeDamageScored(DamageData damageData)
    {
        //if we're invincible, 
        //then exit this method.
        if (invincible)
        {
            return null;
        }

        //curHealth -= d;

        //Set to be low resolution for a small amount of time.
        StartLowResRoutine();

        //Start iFrames here.
        StartIFrames();

        //TODO:
        //change the layer of the visual model for the player to be
        //the lowres layer,
        //have the player get bounced away from the damaging object,
        //and then also give them invincibility frames where
        //they do the blinking in and out of existance thing.
        rb.linearVelocity += (transform.position - damageData.other.transform.position).normalized * bounceForce;

        //print out data about the player taking damage.
        Debug.Log("Dummy Took: ".Color("Orange") + damageData.damage.ToString().Color("Red") + " from " + damageData.other.transform.root.name.Color("Red"));

        //Spawn the particle system here with it's orientation
        //matching the damageData hit normal at the damageData hit point.
        Instantiate(bloodParticlesPrefab, damageData.point, Quaternion.LookRotation(damageData.normal));
        //When the particle system ends it will destroy itself.

        return null;
    }

    bool isLowRes = false;

    Coroutine lowResRoutine = null;

    public void StartLowResRoutine()
    {
        if (isLowRes && lowResRoutine != null)
        {
            return;
        }

        //lowResRoutine = StartCoroutine(LowResCoroutine());
    }

    /*public IEnumerator LowResCoroutine()
    {
        //say we are in low resolution
        isLowRes = true;

        //store previous layer
        int prevLayer = gameObject.layer;

        //set to be low resolution
        gameObject.layer = LayerMask.NameToLayer("LowRes");

        //wait for .25 seconds
        yield return new WaitForSeconds(0.25f);

        //after waiting return to original layer
        gameObject.layer = prevLayer;

        //say we are no longer low res.
        isLowRes = false;

        //set back to null
        //to indicate the coroutine has ended.
        lowResRoutine = null;

        yield return null;
    }*/

    public void StartIFrames()
    {


        //if we're already invincible and
        //the iframes coroutine is currently
        //going, stop it, and create a new one.
        //Debug an error that this should never occur.
        if (invincible == true && iFramesRoutine != null)
        {
            StopCoroutine(iFramesRoutine);
            invincible = false;
            Debug.LogError("Player was damaged when in I-Frames, please check that enemies obey the rules of damage and only deal damage by calling TakeDamage.");
        }

        //start iframes coroutine
        iFramesRoutine = StartCoroutine(IFramesCoroutine());

    }

    public IEnumerator IFramesCoroutine()
    {
        invincible = true;
        //wait for 0.67 seconds while invincible.
        yield return new WaitForSeconds(iFrameTime);
        //after 0.67 seconds become hittable again.
        invincible = false;

        //set iframes routine to null 
        //to indicate we have finished
        //as this will not happen automatically.
        iFramesRoutine = null;

        //exit coroutine.
        yield break;
    }

    private void OnEnable()
    {
        OnDamageableEnabled();
    }

    private void OnDisable()
    {
        OnDamageableDisabled();
    }

    public void OnDamageableDisabled()
    {
        GameManager.Instance.damageables.Remove(this);
    }

    public void OnDamageableEnabled()
    {
        GameManager.Instance.damageables.Add(this);
    }
}
