using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
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
        r.material.color = Color.red;
    }

    public void Despawn()
    {
        r.material.color = Color.blue;
    }
}
