using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public class Dynamite : Throwable, IDamageable
{
    //small radius
    public float explosionRadius = 10f;
    //huge base damage so if the player
    //is too close, it will knock them.
    //Tweak this later, it may need to drop down to just 100.
    public float baseDamage = 200f;

    float curTime = 0;
    float totalTime = 6f;

    public override void OnThrown()
    {
        //Start countdown.
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;
            //TODO: 
            //Set the animator value for the 
            //fuse on the dynamite. 
            yield return null;
        }

        //Detonate the Dynamite
        Detonate();
    }

    //used so we know to apply
    //the indirect damage
    //score modifier.
    public bool wasDamaged = false;

    public void TakeDamage(DamageData damageData)
    {
        wasDamaged = true;

        //When we take any amount of damage we should detonate.
        Detonate();
    }

    //When a player hits this object it needs to be scored so we
    //can return the score values to the player for all the enemies
    //they hit/killed.
    public ScoreData[] TakeDamageScored(DamageData damageData)
    {
        wasDamaged = true;

        //When we take any amount of damage we should detonate.
        Detonate();

        //tell the game we
        //should get the x2 bonus for shooting the dynamite.
        foreach (ScoreData sd in scores)
        {
            sd.wasIndirectDamage = true;
        }

        //return the score array.
        return scores.ToArray();
    }

    List<ScoreData> scores = new List<ScoreData>();

    public void Detonate()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, explosionRadius);

        //Deal damage to all damageables within radius of blast.
        for (int i = 0; i < cols.Length; i++)
        {
            IDamageable damageable = cols[i].GetComponent<IDamageable>();

            if (damageable != null)
            {
                //for fallof factor it's (radius - distance) / radius
                //so for a distance of 7 it would be 10 - 7 = 3 / 10 = 0.3f 
                //so we'd only deal 30% of the base damage at that distance.
                float falloffFactor = (explosionRadius - Vector3.Distance(cols[i].transform.position, transform.position)) / explosionRadius;
                float falloffDamage = baseDamage * falloffFactor;

                //calc the normal
                //Should be the direction facing the explosion source
                Vector3 normal = (cols[i].transform.position - transform.position).normalized;
                //TODO: Try a raycast here to get the hitinfo of the raycast
                //But for now just say the point we hit is the position of the collider object
                Vector3 point = cols[i].transform.position;

                //Deal damage.
                scores.AddRange(damageable.TakeDamageScored(new DamageData() {damageType = DamageType.None, damage = falloffDamage, stunTime = 1f, other = gameObject, point = point, normal = normal}).ToList());
            }
        }

        //Destroy after exploding
        Destroy(gameObject);
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
