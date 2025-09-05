using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

//I have an idea, I think I need to make a serializable wrapper
//that does Serialization for every object with IDataPersistence
//attached when we are saving data. Hmm, hmm, hmm. 

[System.SerializableAttribute]
public class GameData 
{
    public long lastUpdated;
    public int slotIndex;
    public int deathCount;
    public Vector3 playerPos;
    public bool shouldLoadInAsVillain;
    public string sceneToLoadOnStart = "Character Select Scene";
    public CharacterType currentCharacter;
    public List<CharacterType> unlockedCharacters = new List<CharacterType>();
    public bool shouldDisableNewGameButton = false;
    public bool souseDidPossessedDialogue = false;

    //Used to store each player's guid and 
    //other data about their stats. 
    public PlayerInfo[] playerInfos = new PlayerInfo[4];

    //Audio settings
    public float masterVol = -1f;
    public float musicVol = -1f;
    public float sfxVol = -1f;

    //Dictionary of Dictionary
    //add/remove depending on if the data exists.
    //This was leftover from a previous project,
    //we don't need to use a serializable dictionary
    //as the data we are saving isn't going to be laid out like this.
    //public SerializableDictionary<string, SerializableDictionary<string, bool>> characters;

    #region Best Practice
    //can't save properties from the class because 
    //I would have to save each property and make it
    //serialized within the class using a loop.
    //this is apparently not best practice 
    //like straight up look at this: https://forum.unity.com/threads/using-unitys-serialization-as-saving-loading-system.1206115/
    #region their comment:
    //Short answer: You can't serialize monobehaviours or GameObjects. Primary reason is because these objects exist partly in C# land and part in the C++ managed game engine side.

    //Alternate answer: You can with addons like Easy Save or Odin but it's really not best practice. You get some pretty bloated save files and requires a whole gamut of work arounds and short cuts.

    //I see this question at least once a month and the answer generally is you have to work out an appropriate save system that works with your game.Everyone seems(at first) to think that serialising entire scenes and instantiated objects is easier, but it's not, it will actually be magnitudes more difficult than implementing a GUID system, a data base, and making your own serialisable surrogate classes that can be nicely written to a file with Newtonsoft JSON.net.
    #endregion

    //They say using ES3 isn't best practice which is odd.
    //like, why would using ES3 be bad practice?
    //I guess cus it saves everything and not just
    //the data you want to save.
    //so here I am only going to save the bool collected property
    //of the collectible.
    //public SerializableDictionary<string, Collectible> collectibles;
    #endregion

    //the bool is just determining if
    //the player collected them or not.
    //public SerializableDictionary<string, bool> chests;


    //Stores the positions of 
    //the characters
    //public SerializableDictionary<string, Vector2> characterPositions;

    //dictionary storing data for objects
    //which contain 1 dictionary within themselves 
    //that contain a string key which is the field's
    //property name and the value is the value of the
    //field. We can use reflection to set the values of
    //the object on load. 
    //public SerializableDictionary<string, SerializableDictionary<string, object>> savedObjects = new();

    //the default values the game starts with before ever saving data.


    public GameData()
    {
        deathCount = 0;
        playerPos = Vector3.zero;
        //chests = new SerializableDictionary<string, bool>();
        //characterPositions = new SerializableDictionary<string, Vector2>();
        //collectibles = new SerializableDictionary<string, bool>();
        //collectibles = new SerializableDictionary<string, Collectible>();
        //characters = new SerializableDictionary<string, SerializableDictionary<string, bool>>();
    }

    public int GetPercentageComplete()
    {
        //Calculate if we've done all the milestones
        //and just return the amount we've done
        //by the total as a percent


        int total = 5;
        int curAmount = 0;

        if (curAmount == 0)
        {
            return 0;
        }

        int percentageCompleted = (total * 100 / curAmount);
        return percentageCompleted;
    }

    public int GetScore(PlayerInfo p)
    {
        return p.zombieKillCount * 5;
    }

    public void UpdatePlayerInfo(PlayerInfo playerInfo)
    {
        playerInfos[playerInfo.playerIndex] = playerInfo;
    }
}

public enum CharacterType
{
    Snake,
    Souse,
    None
}

[Serializable]
public struct PlayerInfo
{
    public string guid;
    //Store the number of zombies killed by this player.
    public int zombieKillCount;

    public int deviceID;

    public string controlScheme;

    public int playerIndex;
    public int splitScreenIndex;

    public bool hasDevice;

    //The keys for our weapons and items
    //to load.
    public string primaryWeaponKey;
    public string secondaryWeaponKey;
    public string throwableKey;
    public string appliableKey;
    public string consumableKey;

    public bool isDead;
}
