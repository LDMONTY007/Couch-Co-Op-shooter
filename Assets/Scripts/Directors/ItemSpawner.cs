using UnityEngine;
using System.Collections.Generic;



public class ItemSpawner : MonoBehaviour
{
    public Color rarityColor = Color.red;

    [Range(0f, 1f)]
    public float baseWeight = 1f; //1f = common, 0.1f = rare

    Renderer r;

    ItemSpawnDirector spawnDirector;

    private void Awake()
    {
        r = GetComponent<Renderer>();
        r.material.color = Color.blue;
        spawnDirector = FindAnyObjectByType<ItemSpawnDirector>();
        spawnDirector.RegisterSpawner(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawn()
    {
        r.material.color = rarityColor;
    }

    public void Despawn()
    {
        r.material.color = Color.blue;
    }
}
