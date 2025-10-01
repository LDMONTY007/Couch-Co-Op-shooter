using System.Collections;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    public RectTransform target;
    RectTransform rect;

    public Vector3 worldStartPos = Vector3.zero;
    public Camera cam;
    public RectTransform canvasRect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rect = GetComponent<RectTransform>();

        //Start the coroutine.
        StartCoroutine(MoveToTarget());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator MoveToTarget()
    {
        float totalTime = 1f;
        float curTime = 0f;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = target.anchoredPosition;

        // Pick a random curve height so each popup looks different
        float curveHeight = 100f;

        while (curTime < totalTime)
        {
            //Recalculate the start position on the canvas.
            //so that when looking around the score is trying to move
            //from where it spawned initially.
            startPos = CalculateCanvasPos();

            curTime += Time.deltaTime;
            float t = Mathf.Clamp01(curTime / totalTime);

            // Base linear interpolation
            Vector2 basePos = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0,1, t));

            // Add an upward arc using a sine wave
            float arc = Mathf.Sin(t * Mathf.PI) * curveHeight;
            basePos.y += arc;

            rect.anchoredPosition = basePos;

            yield return null;
        }

        rect.anchoredPosition = endPos;
    }

    public Vector2 CalculateCanvasPos()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(worldStartPos);
        Vector2 canvasPos = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out canvasPos))
        {
            return canvasPos;
        }
        return Vector2.zero;
    }
}
