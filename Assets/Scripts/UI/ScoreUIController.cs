using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    public TMP_Text scoreText;

    public ScreenShakeController shakeController;

    private int _score = 0;
    public int score { get { return _score; } set { _score = value; scoreText.text = value.ToString(); } }

    Color neonCyan { get { return LDUtil.GetHexColor("#5fffe4"); } }
    Color neonLime { get { return LDUtil.GetHexColor("#a6fd29"); } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText.color = neonCyan;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int s)
    {
        

        score += s;

        shakeController.AddHeavyCatchRumble(0.9f, 0.25f);

        StartCoroutine(AddScoreAnimation());
    }

    public IEnumerator AddScoreAnimation()
    {
        float totalTime = 0.15f;
        float curTime = 0f;

        float startFontSize = scoreText.fontSize;
        float MidFontSize = scoreText.fontSize + 16;

        float startRotation = 0f;
        float endRotation = -45f;

        bool decreasing = false;
        bool sentinel = true;

        while (sentinel)
        {
            curTime += Time.deltaTime;

            //Scale up and then down
            //Rotate up and then down.
            if (!decreasing)
            {
                scoreText.fontSize = Mathf.LerpUnclamped(startFontSize, MidFontSize, LDUtil.EaseOutBack(curTime / totalTime));
                scoreText.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(startRotation, endRotation, LDUtil.EaseOutBack(curTime / totalTime)));
                scoreText.color = Color.Lerp(neonCyan, neonLime, curTime / totalTime);
            }
            else
            {
                scoreText.fontSize = Mathf.LerpUnclamped(MidFontSize, startFontSize, LDUtil.EaseOutBack(curTime / totalTime));
                scoreText.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(endRotation, startRotation, LDUtil.EaseOutBack(curTime / totalTime)));
                scoreText.color = Color.Lerp(neonLime, neonCyan, curTime / totalTime);
            }

            

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
        //set back to start rotation.
        scoreText.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, startRotation);
        //set back to default color.
        scoreText.color = neonCyan;
    }
}
