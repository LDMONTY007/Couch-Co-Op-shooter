using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUIEntity
    : MonoBehaviour
{
    Color neonCyan { get { return LDUtil.GetHexColor("#5fffe4"); } }
    Color neonLime { get { return LDUtil.GetHexColor("#a6fd29"); } }
    Color neonMagenta { get { return LDUtil.GetHexColor("#ff1493"); } }

    public ScoreUIController scoreUIController;
    public TMP_Text scoreText;
    public int score = 0;

    public RectTransform target;
    RectTransform rect;

    public Vector3 worldStartPos = Vector3.zero;
    public Camera cam;
    public RectTransform canvasRect;
    public GameObject worldRefPoint;
    public GameObject worldTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Select a random color for this spawned score entity.
        int randColor = Random.Range(0, 3);

        switch (randColor)
        {
            case 0:
                scoreText.color = neonCyan;
                break;
            case 1:
                scoreText.color = neonLime;
                break;
            case 2:
                scoreText.color = neonMagenta;
                break;
        }

        //Set the score text value.
        scoreText.text = score.ToString();
        rect = GetComponent<RectTransform>();

        worldRefPoint = new GameObject("RefScoreWorld");
        worldRefPoint.transform.position = worldStartPos;
        worldTarget = new GameObject("WorldTarget");
        worldTarget.transform.position = CanvasToWorldPos(target);

/*        //Start the coroutine.
        StartCoroutine(MoveToTarget());*/


    }

    // Update is called once per frame
    void Update()
    {
        //get the world position of our UI target.
        worldTarget.transform.position = CanvasToWorldPos(target);
        //Do kinematic arrive
        worldRefPoint.transform.position += GetSteering(worldTarget.transform.position) * Time.deltaTime;
        //Set the position on the UI for this object based on the world object
        //so it moves relative to the world but is the same scale in canvas.
        rect.anchoredPosition = WorldToCanvasPos(worldRefPoint.transform.position);

        //If we've reached the target position
        //then let's destroy this object and update
        //the actual score UI controller to
        //do the adding animation.
        if (Vector3.Distance(worldRefPoint.transform.position, worldTarget.transform.position) <= 0.1f)
        {
            //Add the score.
            scoreUIController.AddScore(score);
            //Destroy the other gameobjects we created
            Destroy(worldTarget);
            Destroy(worldRefPoint);
            //Destroy this object.
            Destroy(gameObject);
        }
    }

    //max speed
    public float maxSpeed = 20f;

    public float rotationSpeed = 5f;

    //Satisfaction radius
    public float radius = 5f;

    //Time to target constant.
    public float timeToTarget = 0.25f;

    public Vector3 GetSteering(Vector3 targetPos)
    {
        //Create structure for output
        Vector3 steering = new Vector3();

        //get direction to the target
        steering = targetPos - worldRefPoint.transform.position;

        //Check if we're within radius of satisfaction
/*        if (steering.magnitude < radius)
        {
            //Return empty steering
            return new Vector3();
        }*/

        //We need to move our target,
        //we want to get there in timeToTarget seconds
        //so divide total velocity required to reach target
        //by the time.
        steering /= timeToTarget;

        //Clamp to max speed
        steering = Vector3.ClampMagnitude(steering, maxSpeed);

        
        //output the steering
        return steering;
    }

    public Vector2 WorldToCanvasPos(Vector3 worldPos)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        Vector2 canvasPos = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out canvasPos))
        {
            return canvasPos;
        }
        return Vector2.zero;
    }

    //It took so long to figure this one out:
    //https://stackoverflow.com/questions/29848565/unity3d-how-to-get-screen-or-world-position-of-ui-element
    public Vector3 CanvasToWorldPos(RectTransform uiObject)
    {
        Vector3 worldPos = uiObject.TransformPoint(uiObject.rect.center);
        return worldPos;
    }
}
