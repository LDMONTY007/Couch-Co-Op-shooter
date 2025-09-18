using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotsMenu : Menu
{
    [Header("Menu Navigation")]
    [SerializeField] private MainMenu mainMenu;

    [Header("Menu Buttons")]
    [SerializeField] private Button backButton;

    [Header("Confirmation Popup")]
    [SerializeField] private ConfirmationPopupMenu confirmationPopupMenu;

    private SaveSlot[] saveSlots;

    private bool isLoadingGame = false;

    private void Awake()
    {
        saveSlots = GetComponentsInChildren<SaveSlot>();
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //disable all buttons
        DisableMenuButtons();

        /*//Update the selected profile ID to be used for data persistence.
        DataPersistenceManager.instance.ChangeSelectedProfileID(saveSlot.GetProfileID());

        if (!isLoadingGame )
        {
            //Create a new game which will initialize our data to a clean slate
            DataPersistenceManager.instance.NewGame();
            //Reset the GameManager's scene to load on start
            //because it could have been set already.
            GameManager.Instance.sceneToLoadOnStart = DataPersistenceManager.instance.GetGameData().sceneToLoadOnStart;
        }*/

        //Case - Loading game
        if (isLoadingGame)
        {
            DataPersistenceManager.instance.ChangeSelectedProfileID(saveSlot.GetProfileID());
            SaveGameAndLoadScene();
        }
        //Case - new game, but the save slot has data.
        else if (saveSlot.hasData)
        {
            confirmationPopupMenu.ActivateMenu
            (
                "Starting a new game with this slot will override the currently saved data. Are you sure?",
                //Function to execute if we slect 'yes'.
                () =>
                {

                    DataPersistenceManager.instance.ChangeSelectedProfileID(saveSlot.GetProfileID());
                    DataPersistenceManager.instance.NewGame();
                    //Reset the GameManager's scene to load on start
                    //because it could have been set already.
                    GameManager.Instance.sceneToLoadOnStart = DataPersistenceManager.instance.GetGameData().sceneToLoadOnStart;
                    SaveGameAndLoadScene();
                },
                //Function to execute if we select 'cancel'.
                () =>
                {
                    this.ActivateMenu(isLoadingGame);
                }
           );
        }
        //Case - new game, and the save slot has no data
        else
        {
            DataPersistenceManager.instance.ChangeSelectedProfileID(saveSlot.GetProfileID());
            DataPersistenceManager.instance.NewGame();
            //Reset the GameManager's scene to load on start
            //because it could have been set already.
            GameManager.Instance.sceneToLoadOnStart = DataPersistenceManager.instance.GetGameData().sceneToLoadOnStart;
            SaveGameAndLoadScene();
        }

    }

    private void SaveGameAndLoadScene()
    {
        //Save game before loading scene.
        DataPersistenceManager.instance.SaveGame();

        //Load the scene which will in turn save the game because of OnSceneUnloaded() in DataPersistenceManager.

        DataPersistenceManager.instance.LoadSceneAsync(GameManager.Instance.sceneToLoadOnStart);
    }

    public void OnBackClicked()
    {
        mainMenu.ActivateMenu();
        DeactivateMenu();
    }

    public void OnClearClicked(SaveSlot saveSlot)
    {
        DisableMenuButtons();

        confirmationPopupMenu.ActivateMenu(
            "Are you sure you want to delete this saved data?",
            //Function to execute if we select 'yes'
            () =>
            {
                //Delete profile data.
                DataPersistenceManager.instance.DeleteProfileData(saveSlot.GetProfileID());
                //Activate menu to refresh it after we delete profile data.
                ActivateMenu(isLoadingGame);
            },
            //Function to execute if we select 'cancel'
            () =>
            {
                ActivateMenu(isLoadingGame);
            }
            );


    }

    public void ActivateMenu(bool isLoadingGame)
    {
        //Set this menu to be active
        gameObject.SetActive(true);

        //Set mode
        this.isLoadingGame = isLoadingGame;

        //Load all of the profiles that exist
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.instance.GetAllProfilesGameData();

        //Ensure the back button is interactible
        backButton.interactable = true;

        //Loop through each save slot in the UI and set the content appropriately.
        GameObject firstSelected = backButton.gameObject;
        foreach (SaveSlot saveSlot in saveSlots)
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileID(), out profileData);
            saveSlot.SetData(profileData);
            if (profileData == null && isLoadingGame)
            {
                saveSlot.SetInteractible(false);
            }
            else
            {
                saveSlot.SetInteractible(true);
                if (firstSelected.Equals(backButton.gameObject))
                {
                    firstSelected = saveSlot.gameObject;
                }
            }
        }

        //set the first selected button
        StartCoroutine(SetFirstSelected(firstSelected));
    }

    public void DeactivateMenu()
    {
        gameObject.SetActive(false);
    }

    public void DisableMenuButtons()
    {
        foreach (SaveSlot saveSlot in saveSlots)
        {
            saveSlot.SetInteractible(false);
        }
        backButton.interactable = false;
    }
}
