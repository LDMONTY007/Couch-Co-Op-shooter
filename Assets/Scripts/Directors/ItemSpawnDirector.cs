using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class ItemSpawnDirector : MonoBehaviour
{
    //TODO:
    //Calculate a "Desperation" value for items
    //that increases the density of their spawning
    //and the chances of spawning.
    //So get all player health values and see if any
    //players are in need of temp health or full health.
    //then higher chances mean more likely for more 
    //valuable healing items to spawn,
    //with the lowest value being for speed potions and consumeable temp health potions,
    //and more valuable being appliable health packs.

    //So density is how close together
    //we're allowed to spawn multiple items.
    //so for example if an item was already
    //spawned somewhere and the density radius is 5
    //then we can only spawn a new item greater than a distance
    //of 5 away.

    //but lets start by just coding a slider for density where a higher density means
    //there's a higher chance that nearby items will spawn near each other and fill up
    //like a whole shelf of spawners rather than just one item on said shelf.

    //Density = 1: allow all spawners to activate.
    //Density = 0: only 1 spawner is chosen (regardless of how many exist).

    [Range(0f, 1f)]
    public float density = 0.1f;
    //At 1f density:
    public float minSpacing = 2f;
    //At 0f density:
    public float maxSpacing = 50f;

    public float rarityBias = 1f;
    // >1 makes rare stuff more common, <1 makes common stuff even more common


    float lastDensity = 0f;

    private List<ItemSpawner> spawners = new List<ItemSpawner>();

    private void OnValidate()
    {
        //only when the game is playing call this.
        if (Application.isPlaying)
        //whenever the editor has values changed,
        //try to spawn the items again
        SpawnItems();
    }

    private void Start()
    {
        lastDensity = density;
        SpawnItems();
    }

    private void Update()
    {
        //When density changes recalculate
        //the spawning algorithm.
        if (density != lastDensity) 
        {
            SpawnItems();
            lastDensity = density;
        }
    }

    public void RegisterSpawner(ItemSpawner spawner)
    {
        spawners.Add(spawner);
    }

    //I know I'm looping a lot here but I only run this once right now.
    //TODO: Make it so that we run this once per player
    //so that every player's individual "luck"
    //can have an effect on item chances and more players
    //means more possible items. 
    //so we run it once for one player and then run it again for a diff player
    //so it should fill in every possible spot that can fit at our current density.
    //After testing it, it looks like maybe we shouldn't run this more than once
    //as running it once is pretty balanced.
    public void SpawnItems()
    {
        float minDistance = Mathf.Lerp(maxSpacing, minSpacing, density);

        List<ItemSpawner> selectedSpawners = new List<ItemSpawner>();
        List<ItemSpawner> disabledSpawners = new List<ItemSpawner>();

        foreach (ItemSpawner spawner in spawners.OrderBy(x => Random.value)) //shuffle spawners randomly.
        {
            bool tooClose = false;

            foreach (ItemSpawner s in selectedSpawners)
            {
                float dist = Vector3.Distance(spawner.transform.position, s.transform.position);
                if (dist < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                selectedSpawners.Add(spawner);
            }
            else
            {
                disabledSpawners.Add(spawner);
            }
        }

        foreach (var spawner in selectedSpawners)
        {
            spawner.Spawn(rarityBias);
        }

        foreach (var spawner in disabledSpawners)
        {
            spawner.Despawn();
        }
    }
}
