using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public GameObject playerObject;
    public PlayerController playerController;
    public PlayerInput playerInput;
    public RectTransform canvasRect;
    public GameObject scorePrefab;
    public RectTransform scoreTarget;
    public ScoreUIController scoreUIController;

    private bool _paused;
    public bool paused { get { return _paused; } set { _paused = value; GameManager.Instance.isPaused = value; OnPauseStateSwitched(); } }

    public GameObject pauseUI;

    public Slider healthBarSlider;
    public Slider degradingHealthBarSlider;
    public Slider reviveSlider;
    public Slider useSlider;
    public Image revivePanel;
    public Image usePanel;
    public Image deadPanel;

    public TMP_Text zombieKillsLabel;
    public TMP_Text idLabel;

    public RectTransform slotSelectionTransform;
    //These are assigned in the inspector.
    public RectTransform[] slotTransforms = new RectTransform[5];

    public Image[] slotIcons = new Image[5];

    public void UpdateZombieKills(int kills)
    {
        zombieKillsLabel.text = "Zombies Killed: " + kills;
    }

    public void UpdateIDLabel(int index)
    {
        idLabel.text = "Player " + index;
    }

    public void UpdateReviveSlider(float value)
    {
        reviveSlider.value = value;
    }

    //Used to turn on/off the revive UI.
    public void ShowRevivePanel(bool  value)
    {
        revivePanel.gameObject.SetActive(value);
    }

    public void UpdateUseSlider(float value)
    {
        useSlider.value = value;
    }

    public void ShowUseSlider(bool value)
    {
        usePanel.gameObject.SetActive(value);

    }

    public void ShowDeadPanel(bool value)
    {
        deadPanel.gameObject.SetActive(value);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        paused = false;
        pauseUI.SetActive(paused);

        playerController = playerObject.GetComponent<PlayerController>();

        //assign UI controller.
        playerController.uiController = this;

        OnPauseStateSwitched();

        healthBarSlider.maxValue = PlayerController.maxHealth;
        healthBarSlider.value = playerController.curHealth;

        //Don't show the revive slider.
        ShowRevivePanel(false);
        //Don't show the use slider on start
        ShowUseSlider(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLeaveButtonPressed()
    {
        //playerObject.GetComponent<PlayerInput>().
        //Destroy(playerObject);
        //Exit back to the title screen.
        DataPersistenceManager.instance.LoadSceneAsync("Title Screen");
    }

    public void OnPauseStateSwitched()
    {
        //turn cursor off or on depending on if the game is paused.
        if (!paused) { 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    //used for the "back" input in the control scheme.
    public void OnPauseResumeKeyPressed(InputAction.CallbackContext context)
    {
        //only when the key is initially pressed, not when it is held.
        if (context.started)
        {
            //resume when paused
            if (paused)
            {
                OnResumeButtonPressed();
            }
            //pause otherwise.
            else
            {
                OnPauseButtonPressed();
            }
        }
    }

    public void OnPauseButtonPressed()
    {
        paused = true;
        Time.timeScale = 0f;
        pauseUI.SetActive(paused);
        //Switch to the UI controls.
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void OnResumeButtonPressed()
    {
        paused = false;
        Time.timeScale = 1f;
        pauseUI.SetActive(paused);
        //Switch the current control scheme back to the normal game controls.
        playerInput.SwitchCurrentActionMap("Player");
    }

    public void UpdateHealthBar(float baseHealth, bool hasDegradingHealth, float degradingHealth, float maxHealth)
    {
        healthBarSlider.value = baseHealth;
        healthBarSlider.maxValue = maxHealth;
        degradingHealthBarSlider.maxValue = maxHealth;

        //Set degrading bar slider so it is longer
        //than base health and shows the visual area
        //that is occupied by degrading health in 
        //the health bar.
        if (hasDegradingHealth)
        {
            degradingHealthBarSlider.value = baseHealth + degradingHealth;
        }
        else
            //otherwise set the value to zero so that
            //it doesn't clip the normal health at all.
            degradingHealthBarSlider.value = 0;
    }

    private string GetCorrespondingControlScheme(InputDevice device)
    {
        if (device is Gamepad)
        {
            return "Gamepad";
        }
        if (device is Keyboard)
        {
            return "Keyboard&Mouse";
        }
        if (device is Mouse)
        {
            return "Keyboard&Mouse";
        }
        return null;
    }

    public void OnUpdateSlotIcon(Sprite s, int slotIndex)
    {
        //if this is an empty
        //icon, then disable the image.
        if (s == null)
        {
            slotIcons[slotIndex].enabled = false;
        }
        else
        {
            //make sure the icon is visible.
            slotIcons[slotIndex].enabled = true;

            //Set the sprite. 
            slotIcons[slotIndex].sprite = s;
        }
    }

    public void OnSlotSwitched(int  slot)
    {
        //stop coroutine so we can instead
        //start a new animation coroutine.
        if (slotSwitchCoroutine != null)
        {
            StopCoroutine(slotSwitchCoroutine);

            //Go back to the original position before 
            //animating again.
            /*slotSelectionTransform.localPosition = slotStartPos;
            slotSelectionTransform.sizeDelta = slotStartSize;*/
            curSlotSwitchTime = 0f;
        }
        Debug.Log("SLOT: " + slot);

        slotSwitchCoroutine = StartCoroutine(SlotSwitchAnimation(slotTransforms[slot]));
    }

    float curSlotSwitchTime = 0f;
    float totalSlotSwitchTime = 0.2f;

    Coroutine slotSwitchCoroutine = null;

    Vector3 slotStartPos;
    Vector2 slotStartSize;

    public IEnumerator SlotSwitchAnimation(RectTransform toTransform)
    {
        slotStartPos = slotSelectionTransform.localPosition;
        slotStartSize = slotSelectionTransform.sizeDelta;


        while (curSlotSwitchTime < totalSlotSwitchTime)
        {
            curSlotSwitchTime += Time.deltaTime;
            slotSelectionTransform.localPosition = new Vector3(Mathf.SmoothStep(slotStartPos.x, toTransform.localPosition.x, curSlotSwitchTime / totalSlotSwitchTime), Mathf.SmoothStep(slotStartPos.y, toTransform.localPosition.y, curSlotSwitchTime / totalSlotSwitchTime), 0f);

            slotSelectionTransform.sizeDelta = new Vector2(Mathf.SmoothStep(slotStartSize.x, toTransform.sizeDelta.x, curSlotSwitchTime / totalSlotSwitchTime), Mathf.SmoothStep(slotStartSize.y, toTransform.sizeDelta.y, curSlotSwitchTime / totalSlotSwitchTime));

            

            yield return null;
        }

        //Set exact values at end of coroutine.
        slotSelectionTransform.localPosition = toTransform.localPosition;
        slotSelectionTransform.sizeDelta = toTransform.sizeDelta;

        curSlotSwitchTime = 0f;

        slotSwitchCoroutine = null;
    }

    public void CreateScorePopup(ScoreData scoreData)
    {
        Vector3 worldPos = scoreData.spawnPos;
        float score = scoreData.baseScore + scoreData.calcBonus();

        Vector3 screenPos = playerController.cam.WorldToScreenPoint(worldPos);
        Vector2 canvasPos = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, playerController.cam, out canvasPos))
        {
            Debug.Log("Spawning score at " + canvasPos);
            //Spawn the score.
            GameObject temp = Instantiate(scorePrefab, canvasRect);

            RectTransform scoreRect = temp.GetComponent<RectTransform>();
            scoreRect.anchoredPosition = canvasPos;

            ScoreUIEntity scoreEntity = temp.GetComponent<ScoreUIEntity>();

            //Set the score controller score.
            scoreEntity.score = (int)score;
            //assign the score target.
            scoreEntity.target = scoreTarget;
            //Assign the score UI controller.
            scoreEntity.scoreUIController = scoreUIController;
            scoreEntity.worldStartPos = worldPos;
            scoreEntity.cam = playerController.cam;
            scoreEntity.canvasRect = canvasRect;
        }
    }
}
