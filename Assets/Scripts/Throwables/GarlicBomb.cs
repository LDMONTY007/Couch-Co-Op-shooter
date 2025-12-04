using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class GarlicBomb : Throwable
{
    public float explosionRadius = 10f;
    bool explodeOnCollisionEnter = false;

    public float baseDamage = 50f;

    public GameObject garlicParticles;

    public override void OnThrown()
    {
        explodeOnCollisionEnter = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //ignore if it hits a player.
        if (collision.collider.CompareTag("Player"))
        {
            return;
        }

        if (explodeOnCollisionEnter)
        {
            Detonate();
            
        }
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

                //calc the normal
                //Should be the direction facing the explosion source
                Vector3 normal = (cols[i].transform.position - transform.position).normalized;
                //TODO: Try a raycast here to get the hitinfo of the raycast
                //But for now just say the point we hit is the position of the collider object
                Vector3 point = cols[i].transform.position;

                //deal damage to player
                if (damageable is PlayerController)
                {
                    damageable.TakeDamage(new DamageData() { damageType = DamageType.Garlic, damage = falloffDamage, stunTime = 1f, other = gameObject, point = point, normal = normal });
                }
                //deal scored damage
                else
                {
                    //Deal damage.
                    scores.AddRange(damageable.TakeDamageScored(new DamageData() { damageType = DamageType.Garlic, damage = falloffDamage, stunTime = 1f, other = gameObject, point = point, normal = normal }).ToList());
                }

                
            }
        }

        //create garlic particles
        Instantiate(garlicParticles, transform.position, Quaternion.identity);

        //Destroy after exploding
        Destroy(gameObject);
    }

    public void TakeDamage(DamageData damageData)
    {
        //When we take any amount of damage we should detonate.
        Detonate();
    }

    List<ScoreData> scores = new();

    //When a player hits this object it needs to be scored so we
    //can return the score values to the player for all the enemies
    //they hit/killed.
    public ScoreData[] TakeDamageScored(DamageData damageData)
    {

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
}
