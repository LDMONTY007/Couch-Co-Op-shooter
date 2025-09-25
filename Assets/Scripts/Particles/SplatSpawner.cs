using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//Where I learned this:
//https://docs.unity3d.com/6000.2/Documentation/ScriptReference/MonoBehaviour.OnParticleCollision.html
public class SplatSpawner : MonoBehaviour
{
    public List<GameObject> splatDecals = new List<GameObject>();
    public ParticleSystem splatSpawner;
    public List<ParticleCollisionEvent> collisionEvents = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnParticleCollision(GameObject other)
    {

        int colCount = splatSpawner.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < colCount; i++)
        {
            //get collision position
            Vector3 collisionPoint = collisionEvents[i].intersection;
            //get collision normal
            Vector3 collisionNormal = collisionEvents[i].normal;

            //select random splat decal.
            int r = Random.Range(0, splatDecals.Count);

            //This script is from some of the unity examples.
            //Instantiate the decal at the collision point, aligned with the surface normal
            //Make sure that we parent this to the object it landed on, so that it moves with any non-static objects.
            Instantiate(splatDecals[r], collisionPoint, Quaternion.LookRotation(collisionNormal), other.transform);
        }
    }
}
