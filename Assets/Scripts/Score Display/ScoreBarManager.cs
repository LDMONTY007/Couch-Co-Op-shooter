using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBarManager : MonoBehaviour
{
    public GameObject scoreBarPrefab;

    public Transform scoreBarPanel;

    List<ScoreBarController> scoreBars = new List<ScoreBarController>();

    List<int> endScores = new List<int>();

    GameData gameData;

    int highScore = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

            //add the score to our list of scores.
            endScores.Add(p.score);

            //set highest score.
            if (p.score > highScore)
            {
                highScore = p.score;
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
    }
}
