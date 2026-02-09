using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;


public class AimTargetController : MonoBehaviour
{
    [HideInInspector]
    public Camera cam;
    [HideInInspector]
    public RectTransform canvasRect;


    public RectTransform aimBoundsRect;

    //the box within enemies will have
    //target lock.
    public RectTransform targetBox;

    public Vector2 input;

    public float mouseSensitivity = 0.01f;
    public float controllerSensitivity = 1f;

    public List<IDamageable> damageables = new();

    PlayerController player;

    public bool useControllerSensitivity = false;

    int mask;

    //This is a hard coded cap for the allowed attack distance,
    //but for different weapons I should use their individual 
    //distance as well.
    public float maxDist = 100f;

    private float GetCorrespondingLookSensitivity()
    {
        if (useControllerSensitivity)
        {
            return controllerSensitivity;
        }
        else 
        {
            return mouseSensitivity;
        }
    }

    private void Awake()
    {
        //Get everything except player and ignore raycast.
        mask = ~LayerMask.GetMask("Player", "IgnoreRaycast");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Apply sensitivity
        Vector2 clampedInput = Vector2.ClampMagnitude(input * GetCorrespondingLookSensitivity(), 1f);

        //map from -1,1 to 0,1
        float tx = (clampedInput.x + 1f) * 0.5f;
        float ty = (clampedInput.y + 1f) * 0.5f;

        // Get usable area inside aimBounds
        Vector2 boundsSize = aimBoundsRect.rect.size;
        Vector2 targetSize = targetBox.rect.size;

        //for now don't use target size so the targetbox
        //can reach the corners of the screen.
        float halfW = (boundsSize.x /*- targetSize.x*/) * 0.5f;
        float halfH = (boundsSize.y /*- targetSize.y*/) * 0.5f;

        // Convert normalized input into anchoredPosition
        Vector2 pos = new Vector2(
            Mathf.Lerp(-halfW, halfW, tx),
            Mathf.Lerp(-halfH, halfH, ty)
        );

        targetBox.anchoredPosition = pos;

        //targetBox.position = new Vector3(Mathf.Lerp(targetBox.rect.xMin, targetBox.rect.xMax, (input.x + 1f) / 2), Mathf.Lerp(targetBox.rect.yMin, targetBox.rect.yMax, (input.y + 1f) / 2), 0f);
    }

    //get the best target using 
    //our heuristic.
    public IDamageable GetBestTarget()
    {
        //Handle selecting targets
        HandleSelectingTargets();

        if (damageables.Count == 0)
        {
            return null;
        }

        float lastDist = Vector3.Distance((damageables[0] as Component).transform.position, cam.transform.position);

        //for now we'll find the closest target.
        IDamageable closest = damageables[0];

        foreach (IDamageable target in damageables)
        {
            float dist = Vector3.Distance((target as Component).transform.position, cam.transform.position);
            
            if (dist < lastDist)
            {
                closest = target;
                lastDist = dist;
            }
        }

        return closest;
    }

    public void HandleSelectingTargets()
    {
        //clear the damageables.
        damageables.Clear();

        

        foreach (IDamageable d in GameManager.Instance.damageables)
        {


            //add the damageable if it's in the target box.
            if (EnemyInsideTargetBox((d as Component).transform))
            {
                damageables.Add(d);
            }
        }
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

    public bool EnemyInsideTargetBox(Transform enemy)
    {
        //I tried checking the isVisible value to see if
        //that would help reduce the need for doing raycasts,
        //but it didn't work for making sure they weren't behind
        //an object.
        /* Renderer enemyRenderer = null;

         //if the enemy isn't visible, they can't be shot.
         if (enemy.TryGetComponent<Renderer>(out enemyRenderer))
         {
             if (enemyRenderer.isVisible == false)
             {
                 return false;
             }
         }
         else
         {
             Debug.Log("Enemy Not Found");
         }*/

        //Check that the enemy isn't blocked by an object.
        Vector3 startPos = CanvasToWorldPos(aimBoundsRect);


        //if the enemy is too far, skip that enemy 
        //so we don't do unnecessary raycast calculations
        //that are costly.
        if (Vector3.Distance(enemy.position, startPos) > maxDist)
        {
            return false;
        }
        

        //map enemy world pos to canvas local pos
        Vector2 enemyCanvasPos = WorldToCanvasPos(enemy.position);

        //Get targetBox rect in canvas local space
        Rect rect = GetCanvasRect(targetBox);

        //Check if enemy point inside the rect
        if (rect.Contains(enemyCanvasPos))
        {
            //Check that there are no objects blocking
            //the view to our target using a raycast.
            //This can become quite costly so use it as
            //our final check to reduce calls to it.
            Vector3 dir = (enemy.position - CanvasToWorldPos(aimBoundsRect)).normalized;

            if (Physics.Raycast(CanvasToWorldPos(aimBoundsRect), dir, out var hitInfo, maxDist, mask))
            {
                if (hitInfo.transform == enemy.transform)
                {
                    //We successfully found a path to the enemy that isn't blocked by objects.
                    return true;
                }
                else
                {
                    //Say the enemy is blocked by an object so we can't shoot it.
                    return false;
                }
            }
            else
            {
                //We didn't hit anything.
                //this may be due to the distance being too short,
                //so we'll return false here.
                return false;
            }

        }
        else
        {
            return false;
        }

        
    }

    //Convert a UI RectTransform into a rect in canvas-local coordinates
    private Rect GetCanvasRect(RectTransform rectTransform)
    {
        Vector2 size = rectTransform.rect.size;

        //Convert rectTransform's center into canvas local space
        Vector2 center;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(cam, rectTransform.TransformPoint(rectTransform.rect.center)),
            cam,
            out center
        );

        Vector2 min = center - size * 0.5f;
        return new Rect(min, size);
    }
}
