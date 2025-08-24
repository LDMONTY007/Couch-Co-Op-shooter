using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    public bool disableDataPersistence = false;
    public bool initializeDataIfNull = false;
    public bool overrideSelectedProfileID = false;
    public string testSelectedProfileID = "test";

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    public ScriptableObjectList weaponDataList;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    private string selectedProfileID = "";

    public static DataPersistenceManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene. Destroying newest one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);

        if (disableDataPersistence)
        {
            Debug.LogWarning("Data Persistence is disabled!");
        }

        //we need to create in awake
        //so that this exists
        //when OnSceneLoaded() is called.
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

        InitializeSelectedProfileId();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CustomSceneManager.beforeSceneUnload += OnBeforeSceneUnload;
        //SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        CustomSceneManager.beforeSceneUnload -= OnBeforeSceneUnload;
        //SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    //find the prefab stored in a dropData in our global dropData scriptable object list.
    public GameObject FindWeaponPrefab(string key)
    {
        PrefabData temp = (weaponDataList.soList.Find(d => (d as PrefabData).key == key) as PrefabData);
        if (temp == null)
        {
            Debug.LogError("DROP DATA WAS NULL FOR THIS SEARCH: " + key);
        }
        GameObject temp1 = temp.prefab;
        //return the dropped object specifically.
        //return (dropDataList.soList.Find(d => d.name == key) as DropData).droppedObject;
        return temp1;
    }

    //Didn't work
    //correctly if saving
    //referenced a object in the current
    //scene as this would only call
    //after loading into the new scene.
    /*    public void OnSceneUnloaded(Scene scene)
        {
            SaveGame();
        }*/

    //Called right before we load a scene
    public void OnBeforeSceneUnload()
    {
        Debug.LogWarning("SAVE GAME BEFORE UNLOAD");
        //we do this as 
        //sometimes the game doesn't pick up
        //all of them when we load into a scene.
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        SaveGame();
    }



    public void ChangeSelectedProfileID(string newProfileID)
    {
        //Update the profile to use for saving and loading.
        selectedProfileID = newProfileID;

        //load the game, which will use that profile, updating our game data accordingly
        LoadGame();
    }    

    public GameData GetGameData()
    {
        return gameData;
    }


    public void DeleteProfileData(string profileId)
    {
        //delete the data for this profile id
        dataHandler.Delete(profileId);
        //Initialize Selected Profile ID in case we just deleted it.
        InitializeSelectedProfileId();
        //reload the game so that our data matches the newly selected profile id
        LoadGame();

    }

    private void InitializeSelectedProfileId()
    {
        //set the current profileID
        //to be the one that was
        //most recently updated.
        //AKA, the player's most
        //recent used save file.
        selectedProfileID = dataHandler.GetMostRecentlyUpdatedProfileID();

        if (overrideSelectedProfileID)
        {
            selectedProfileID = testSelectedProfileID;
            Debug.LogWarning("Overrode selected profile id with test id: " + testSelectedProfileID);
        }
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        //Return right away if data persistence is disabled
        if (disableDataPersistence)
        {
            return;
        }

        // load any saved data from a file using the data handler
        this.gameData = dataHandler.Load(selectedProfileID);

        if (initializeDataIfNull && gameData == null)
        {
            NewGame();
        }

        // if no data can be loaded, don't continue.
        if (this.gameData == null)
        {
            Debug.Log("No data was found. A new game needs to be started before data can be loaded!");
            return;
        }

        // push the loaded data to all other scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        Debug.Log("Save step 1");

        //Return right away if data persistence is disabled
        if (disableDataPersistence)
        {
            return;
        }

        Debug.Log("Save step 2");

        //If we don't have any data to save, Log a warning here.
        if (this.gameData == null)
        {
            Debug.LogWarning("No Data was found. A new game needs to be created before data can be saved.");
            return;
        }

        Debug.Log("Save step 3");

        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        Debug.Log("Save step 4");

        //timestamp the data so we know when it was last saved
        gameData.lastUpdated = System.DateTime.Now.ToBinary();

        Debug.Log("Save step 5");

        // save that data to a file using the data handler
        dataHandler.Save(gameData, selectedProfileID);

        Debug.LogWarning("Saved Game File!");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        //IT IS EXTREMELY IMPORTANT THAT WE INCLUDE INACTIVE OBJECTS HERE!
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>();

        //Convert to a list so we can 
        //add the game manager to it.
        List<IDataPersistence> data = new List<IDataPersistence>(dataPersistenceObjects)
        {
            //make sure we add the game manager as it isn't a monobehavior
            //so it won't be added by the above line.
            GameManager.Instance
        };

        return new List<IDataPersistence>(data);
    }

    public bool HasGameData()
    {
        return gameData != null;
    }

    public Dictionary<string, GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }

    public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, mode));
        //CustomSceneManager.LoadSceneAsync(sceneName, mode);
    }

    public IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        Task load = CustomSceneManager.LoadSceneAsync(sceneName, mode);
        while (!load.IsCompleted)
        {
            yield return null;
        }

        Debug.Log("FINISHED LOADING!!!");
    }
}