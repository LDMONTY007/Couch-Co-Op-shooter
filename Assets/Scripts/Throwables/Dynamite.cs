using System.Collections;
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

    public void TakeDamage(float damage, float stunTime, GameObject other)
    {
        //When we take any amount of damage we should detonate.
        Detonate();
    }

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

                //Deal damage.
                damageable.TakeDamage(falloffDamage, 1f, gameObject);
            }
        }

        //Destroy after exploding
        Destroy(gameObject);
    }
}
