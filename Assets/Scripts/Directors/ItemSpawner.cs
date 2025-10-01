using UnityEngine;
using System.Collections.Generic;

//Used for storing a weight + prefab reference for spawning behavior in various scripts.
[System.Serializable]
public class WeightedReference<T> where T : UnityEngine.Object
{
    public T reference;
    public float baseWeight;
    public Color rarityColor;
}

public class ItemSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    public List<WeightedReference<GameObject>> weightedItems = new List<WeightedReference<GameObject>>();

    ItemSpawnDirector spawnDirector;

    Color curColor = Color.white;

    GameObject curSpawnedItem = null;

    

    private void Awake()
    {
        spawnDirector = FindAnyObjectByType<ItemSpawnDirector>();
        spawnDirector.RegisterSpawner(this);
    }

    public void Spawn(float rarityBias)
    {
        // Pick item based on weights
        WeightedReference<GameObject> weightedReference = ChooseWeightedItem(rarityBias);
        GameObject prefab = weightedReference.reference;

        //create the object
        if (prefab != null)
        {
            curSpawnedItem = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            if (curSpawnedItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
            { 
                //Freeze the rigidbody so it doesn't fall.
                rb.isKinematic = true;
            }

        }


        if (weightedReference != null)
        {
           curColor = weightedReference.rarityColor; // debug
            
        }
        else
        {
            Debug.LogWarning("WE didn't select an item!!!");
        }

    }

    public void Despawn()
    {
        curColor = Color.white;
        Destroy(curSpawnedItem);
    }

    //borrowed this from a previous project.
    private WeightedReference<GameObject> ChooseWeightedItem(float rarityBias)
    {
        if (weightedItems == null || weightedItems.Count == 0)
            return null;

        // Step 1: calculate adjusted total weight
        float totalWeight = 0f;
        foreach (var entry in weightedItems)
        {
            // Apply rarity bias curve
            float adjusted = Mathf.Pow(entry.baseWeight, 1f / rarityBias);
            totalWeight += adjusted;
        }

        if (totalWeight <= 0f)
            return weightedItems[0]; // fallback

        // Step 2: roll in [0, totalWeight)
        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        // Step 3: walk through and find which entry roll lands in
        foreach (var entry in weightedItems)
        {
            float adjusted = Mathf.Pow(entry.baseWeight, 1f / rarityBias);
            cumulative += adjusted;
            if (roll <= cumulative)
                return entry;
        }

        // Fallback: due to float imprecision, return last
        return weightedItems[weightedItems.Count - 1];
    }

    private void OnDrawGizmos()
    {
        //Draw the spawn area for the spawner
        //usiong the current rarity color.
        Color temp = Gizmos.color;
        Gizmos.color = curColor;
        Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.1f, 0.5f));
        //Draw where the spawn point is.
        if (spawnPoint != null)
        Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
        Gizmos.color = temp;
    }
}
