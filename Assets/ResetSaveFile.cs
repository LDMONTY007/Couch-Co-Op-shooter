using UnityEngine;

public class ResetSaveFile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateNewSaveFile()
    {
        DataPersistenceManager.instance.NewGame();
        DataPersistenceManager.instance.SaveGame();
    }
}
