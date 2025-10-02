using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    public TMP_Text scoreText;

    private int _score = 0;
    public int score { get { return _score; } set { _score = value; scoreText.text = value.ToString(); } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int s)
    {
        score += s;

        StartCoroutine(scaleCoroutine());
    }

    public IEnumerator scaleCoroutine()
    {
        float totalTime = 0.15f;
        float curTime = 0f;

        float startFontSize = scoreText.fontSize;
        float MidFontSize = scoreText.fontSize + 16;

        bool decreasing = false;
        bool sentinel = true;

        while (sentinel)
        {
            curTime += Time.deltaTime;

            if (!decreasing)
                scoreText.fontSize = Mathf.SmoothStep(startFontSize, MidFontSize, curTime / totalTime);
            else
                scoreText.fontSize = Mathf.SmoothStep(MidFontSize, startFontSize, curTime / totalTime);

            //if we've reached the end of the lerp.
            if (curTime >= totalTime)
            {
                if (decreasing == false)
                {
                    //start decreasing.
                    decreasing = true;
                    curTime = 0f;
                }
                else
                {
                    //stop the loop.
                    sentinel = false;
                }
            }

            yield return null;
        }

        //set back to start font size.
        scoreText.fontSize = startFontSize;
    }
}
