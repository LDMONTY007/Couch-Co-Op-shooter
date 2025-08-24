using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private string profileID = "";

    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;

    [SerializeField] private TextMeshProUGUI percentageCompleteText;
    [SerializeField] private TextMeshProUGUI deathCountText;

    [Header("Clear Data Button")]
    [SerializeField] private Button clearButton;

    public bool hasData { get; private set; } = false;

    private Button saveSlotButton;

    private void Awake()
    {
        saveSlotButton = GetComponent<Button>();
    }

    public void SetData(GameData data)
    {
        //There is no data for this profileID
        if (data == null)
        {
            hasData = false;
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            clearButton.gameObject.SetActive(false);
        }
        //There is data for this profileID
        else
        {
            hasData = true;
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            clearButton.gameObject.SetActive(true);

            percentageCompleteText.text = data.GetPercentageComplete() + "% COMPLETE";
            deathCountText.text = "DEATH COUNT: " + data.deathCount;
        }
    }

    public string GetProfileID() { return profileID; }

    public void SetInteractible(bool interactible)
    {
        saveSlotButton.interactable = interactible;
        clearButton.interactable = interactible;
    }
}
