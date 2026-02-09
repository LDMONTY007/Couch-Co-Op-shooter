using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : Menu
{
    [Header("Menu Navigation")]
    [SerializeField] private SaveSlotsMenu saveSlotsMenu;


    [Header("Menu Buttons")]
    public Button newGameButton;
    public Button continueGameButton;
    public Button loadGameButton;


    // Start is called before the first frame update
    void Start()
    {
        DisableButtonsDependingOnData();

        //Disable the new game button if we're supposed to.
        if (DataPersistenceManager.instance.HasGameData())
        {
            if (GameManager.Instance.shouldDisableNewGameAndLoadGameButtons)
            {
                newGameButton.interactable = false;
                loadGameButton.interactable = false;
            }
        }

        Time.timeScale = 1.0f;
        //Debug.Log(Time.timeScale);
    }

    public void DisableButtonsDependingOnData()
    {
        //if we don't have game data
        //disable the continue button.
        if (!DataPersistenceManager.instance.HasGameData())
        {
            continueGameButton.interactable = false;
            loadGameButton.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnNewGameClicked()
    {
        saveSlotsMenu.ActivateMenu(false);
        DeactivateMenu();
    }

    public void OnLoadGameClicked()
    {
        saveSlotsMenu.ActivateMenu(true);
        DeactivateMenu();
    }

    public void OnContinueButtonClicked()
    {
        DisableMenuButtons();
        Debug.Log("Continue Game Clicked");
        //Load the desired scene, the OnSceneLoaded() 
        //in DataPersistenceManager
        //will load the pre-existing data.
        DataPersistenceManager.instance.LoadSceneAsync(GameManager.Instance.sceneToLoadOnStart);
    }

    private void DisableMenuButtons()
    {
        newGameButton.interactable = false;
        continueGameButton.interactable = false;
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
        //Disable buttons if they are supposed to be disabled.
        DisableButtonsDependingOnData();
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}
