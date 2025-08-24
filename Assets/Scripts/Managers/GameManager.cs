using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : IDataPersistence
{
    //The players in the game.
    //Save this data on scene end,
    //but we still want to persist it here
    //in this singleton.
    //These may not be updated correctly rn.
    public PlayerInfo[] playerInfos = new PlayerInfo[4];
    //Used to store the 4 input devices for the game.
    //updated when a player joins in the character select screen.
    //These 4 indexes represent the index of each display/player
    //and the device that should be used.
    public InputDevice[] inputDevices = new InputDevice[4];

    public string sceneToLoadOnStart = "Title Scene";
    public bool shouldDisableNewGameAndLoadGameButtons = false;

    private bool _inCutscene;

    //used for the player to check if they
    //should be able to move or not.
    //Made this have get and set
    //so I can debug and see all places where
    //this is modified, because sometimes
    //characters aren't able to move and I
    //don't know why.
    public bool inCutscene
    {
        get { return _inCutscene; }
        set { Debug.Log("GameManager: Changed inCutscene from [" + _inCutscene + "] to [" + value + "]"); _inCutscene = value; }
    }

    private GameManager() { }

    private static GameManager _instance;

    public static GameManager Instance
    {
        get //Whenever the Instance is referenced the following code is called. 
        {
            //Create instance if it doesn't exist
            if (_instance == null)
            {
                _instance = new GameManager();
            }
            //return instance.
            return _instance;
        }
    }

    //Audio
    public float AudioSetting = 0.2f;
    public float AudioSlider = 0.2f;
    public AudioMixer audioMasterMixer;


    public void LoadData(GameData gameData)
    {
        sceneToLoadOnStart = gameData.sceneToLoadOnStart;
        shouldDisableNewGameAndLoadGameButtons = gameData.shouldDisableNewGameButton;
        playerInfos = gameData.playerInfos;

        //If this scene has a player input manager spawn all players.
        if (PlayerInputManager.instance != null)
        {

        }
    }

    public void SaveData(ref GameData gameData)
    {
        //save the scene to load on start in case it 
        //has changed.
        Debug.Log("Saving GameManager!");
        gameData.sceneToLoadOnStart = sceneToLoadOnStart;
        gameData.shouldDisableNewGameButton = shouldDisableNewGameAndLoadGameButtons;
        gameData.playerInfos = playerInfos;
    }


    public void SetAudioSetting(float volume)
    {
        audioMasterMixer.SetFloat("Master", volume);
        AudioSetting = volume;
        //Debug.Log("Volume is at: " + volume);
    }

    public void SetAudioSlider(float vol)
    {
        AudioSlider = vol;
    }

    public float GetAudioSetting()
    {
        return AudioSetting;
    }

    public float GetAudioSlider()
    {
        return AudioSlider;
    }

    public void SetAudioMixerMaster(AudioMixer Master)
    {
        audioMasterMixer = Master;
    }

    public AudioMixer GetAudioMixer()
    {
        return audioMasterMixer;
    }
}
