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
    }
}
