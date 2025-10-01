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
    public List<WeightedReference<GameObject>> weightedItems = new List<WeightedReference<GameObject>>();

    Renderer r;

    ItemSpawnDirector spawnDirector;

    private void Awake()
    {
        r = GetComponent<Renderer>();
        r.material.color = Color.blue;
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
            Instantiate(prefab, transform.position, Quaternion.identity);
        }


        if (weightedReference != null)
        {
            r.material.color = weightedReference.rarityColor; // debug
            
        }
        else
        {
            Debug.LogWarning("WE didn't select an item!!!");
        }

    }

    public void Despawn()
    {
        r.material.color = Color.white;
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
}
