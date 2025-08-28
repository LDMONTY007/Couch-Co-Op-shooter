using UnityEngine;

public class GarlicBomb : Throwable, IDamageable
{
    public float explosionRadius = 10f;
    bool explodeOnCollisionEnter = false;

    public float baseDamage = 50f;


    public override void OnThrown()
    {
        explodeOnCollisionEnter = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
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

                //Deal damage.
                damageable.TakeDamage(falloffDamage, 1f, gameObject);
            }
        }

        //Destroy after exploding
        Destroy(gameObject);
    }

    public void TakeDamage(float damage, float stunTime, GameObject other)
    {
        //When we take any amount of damage we should detonate.
        Detonate();
    }
}
