using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBarController : MonoBehaviour
{
    public Image fillImage;
    public Slider slider;
    public TMP_Text scoreText;
    public TMP_Text playerText;
    public int playerIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerText.text = "P" + playerIndex;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator AnimateScoreBarCoroutine(int highestScore, int endScore)
    {
        //Set slider max value so that
        //all score sliders will be 
        //different heights depending on score.
        slider.maxValue = highestScore;

        float totalTime = 0.5f;
        float curTime = 0f;

        //loop until we've finished animating.
        while (curTime <= totalTime)
        {
            //Smoothstep (slow down at beginning, quick in middle, slow down at end)
            //animate slider over time using our params.
            slider.value = Mathf.SmoothStep(0, endScore, curTime / totalTime);
            //set score text value.
            scoreText.text = ((int)slider.value).ToString();

            //Increment time
            curTime += Time.deltaTime;


            //wait till next frame to continue execution.
            yield return null;
        }
        
        //Set the final value to be our endscore
        //as interpolation sometimes doesn't quite
        //make it to the end.
        slider.value = endScore;
        //set score text value.
        scoreText.text = ((int)slider.value).ToString();

    }
}
