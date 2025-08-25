using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class ScoreBarManager : MonoBehaviour
{
    public GameObject scoreBarPrefab;

    public Transform scoreBarPanel;

    public TMP_Text continueText;

    public Image continueBackgroundPanel;
    
    List<ScoreBarController> scoreBars = new List<ScoreBarController>();

    List<int> endScores = new List<int>();

    
    
    GameData gameData;

    int highScore = 0;

    System.IDisposable AnyButtonPressed;

    bool hasFinishedAnimation = false;


    private void OnEnable()
    {
        //Setup the check for a player input device pressing any button.
        //use a disposeable so it can be disposed before this scene ends.
        AnyButtonPressed = InputSystem.onEvent.Where(e => e.HasButtonPress()).Call(eventPtr =>
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
            if (Application.isPlaying)
            {
                if (hasFinishedAnimation)
                {
                    //Delete all player info.
                    DeleteAllPlayerInfos();

                    //Transition back to character select.
                    DataPersistenceManager.instance.LoadSceneAsync("Character Select Scene");
                }
            }
        });
    }

    private void OnDisable()
    {
        //Dispose this callback.
        AnyButtonPressed.Dispose();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set the continue text to be invisible.
        continueText.color = new Color(continueText.color.r, continueText.color.g, continueText.color.b, 0);
        continueBackgroundPanel.color = new Color(continueBackgroundPanel.color.r, continueBackgroundPanel.color.g, continueBackgroundPanel.color.b, 0);

        gameData = DataPersistenceManager.instance.GetGameData();

        //use player infos to get the highest score 
        //and instantiate the score bars.
        foreach (PlayerInfo p in gameData.playerInfos)
        {
            if (!p.hasDevice)
                continue;


            //Create the new score bar.
            GameObject go = Instantiate(scoreBarPrefab, scoreBarPanel);
            //assign the score bar controller reference.
            scoreBars.Add(go.GetComponent<ScoreBarController>());
            //assign player index to the score bar.
            scoreBars[scoreBars.Count - 1].playerIndex = p.playerIndex;

            //Score is calculated here, based on the different kills
            //and kill types the player got.
            int score = gameData.GetScore(p);

            //add the score to our list of scores.
            endScores.Add(score);

            //set highest score.
            if (score > highScore)
            {
                highScore = score;
            }
        }

        //Start the animate all scores coroutine.
        StartCoroutine(AnimateAllScores());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator AnimateAllScores()
    {
        //loop through each score bar and one by one
        //animate them in sequence.
        for (int i = 0; i < scoreBars.Count; i++)
        {
            yield return scoreBars[i].AnimateScoreBarCoroutine(highScore, endScores[i]);
        }

        //After the animation wait 5 seconds and then
        //transition to the character selection scene.
        yield return new WaitForSeconds(5);

        hasFinishedAnimation = true;

        //Fade the text in.
        yield return FadeTextCoroutine();
    }

    private void DeleteAllPlayerInfos()
    {
        //Replace all player infos with blank ones. 
        //This effectively removes all references to 
        //all controllers and players.
        //Note: We are only deleting player controller
        //and device data along with their score as this level
        //is over once the score bar scene is reached.
        //We do not however delete any characters that have been unlocked
        //or any levels that have been unlocked.
        for (int i = 0; i < gameData.playerInfos.Length; i++)
        {
            gameData.playerInfos[i] = new PlayerInfo();
        }
    }

    private IEnumerator FadeTextCoroutine()
    {
        float currentTime = 0f;
        float timeToFade = 1f;
        while (currentTime != timeToFade)
        {
            currentTime += Time.deltaTime;
            continueText.color = new Color(continueText.color.r, continueText.color.g, continueText.color.b, Mathf.Lerp(0f, 1f, currentTime / timeToFade));

            continueBackgroundPanel.color = new Color(continueBackgroundPanel.color.r, continueBackgroundPanel.color.g, continueBackgroundPanel.color.b, Mathf.Lerp(0f, 1f, currentTime / timeToFade));

            //Exit the coroutine if it's 
            //been enough time.
            if (timeToFade - currentTime < 0.01f)
            {
                //Set to be one alpha when done lerping
                continueText.color = new Color(continueText.color.r, continueText.color.g, continueText.color.b, 1f);
                continueBackgroundPanel.color = new Color(continueBackgroundPanel.color.r, continueBackgroundPanel.color.g, continueBackgroundPanel.color.b, 1f);


                

                break;
            }

            yield return null;
        }
    }
}
