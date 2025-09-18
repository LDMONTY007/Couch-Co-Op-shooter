using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualScreen : GraphicRaycaster
{
    public Camera screenCamera; // Reference to the camera responsible for rendering the virtual screen's rendertexture

    public GraphicRaycaster screenCaster; // Reference to the GraphicRaycaster of the canvas displayed on the virtual screen

    public RectTransform cursorTransform;

    // Called by Unity when a Raycaster should raycast because it extends BaseRaycaster.
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        //Debug.Log("HERE");
        //Debug.Log("EVENT POS: " + eventData.position.x + ", " + eventData.position.y);
        Ray ray = eventCamera.ScreenPointToRay(eventData.position); // Mouse

        List<RaycastResult> uiHits = new List<RaycastResult>();
        screenCaster.Raycast(eventData, uiHits);

        foreach (var result in uiHits)
        {
            //Debug.Log("Hit Main UI element: " + result.gameObject.name);

            RectTransform rt = result.gameObject.GetComponent<RectTransform>();
            if (rt != null)
            {
                //Debug.Log("HERE2");

                // Convert world hit point to local point relative to RectTransform
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt,
                    eventData.position,
                    //use the camera from the latest pointer event.
                    eventData.enterEventCamera,
                    out localPoint
                );

                // rect is centered around (0,0), so localPoint ranges from -rect.width/2 .. rect.width/2
                Rect rect = rt.rect;

                // Normalize to 0–1 range relative to THIS rect
                float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
                float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

                //offset by 0.5f
                u -= 0.5f;
                v -= 0.5f;

                // Convert to render texture space
                Vector3 virtualPos = new Vector3(
                    u * 923.7604f,//screenCamera.targetTexture.width,
                    v * 519.6152f,//screenCamera.targetTexture.height
                   screenCamera.nearClipPlane
                );

                cursorTransform.localPosition = virtualPos;

                //Debug.Log($"LocalPoint: {localPoint}, UV: ({u},{v}), VirtualPos: {virtualPos}");

                // Update eventData
                //eventData.position = virtualPos;

                List<RaycastResult> resultList = new();

                PointerEventData virtualEvent = new PointerEventData(EventSystem.current)
                {
                    position = virtualPos,
                    button = eventData.button,
                    clickCount = eventData.clickCount,
                    pointerId = eventData.pointerId,
                    scrollDelta = eventData.scrollDelta,
                };

                //this.GetComponent<GraphicRaycaster>().Raycast(virtualEvent, resultList);

                //Use the normal raycast for this player's canvas.
                // base.Raycast(virtualEvent, resultList);

                base.Raycast(virtualEvent, resultList);

                //make sure we don't count any other UI objects
                //other than our own.
                for (int i = resultList.Count - 1; i >= 0; i--)
                {
                    if (resultList[i].module != this) // or != screenCaster
                        resultList.RemoveAt(i);
                }

                foreach (var r in resultList)
                {
                    Debug.Log("Hit Player UI element: " + r.gameObject.name);
                }
            }
        }

        /*if (screenCaster.Raycast()
        {
            Debug.Log("HERE2");



            RectTransform rt = hit.collider.gameObject.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Convert world hit point to local point relative to RectTransform
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt,
                    eventData.position,
                    eventCamera,
                    out localPoint
                );

                // Convert localPoint to 0-1 UV
                Rect rect = rt.rect;
                float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
                float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

                // Scale to render texture size
                Vector3 virtualPos = new Vector3(
                    u * screenCamera.targetTexture.width,
                    v * screenCamera.targetTexture.height,
                    0
                );

                Debug.Log($"VirtualPos: {virtualPos}");
                eventData.position = virtualPos;

                screenCaster.Raycast(eventData, resultAppendList);
            }
        }*/
    }

    private void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            // Check if the mouse is over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Clicked on UI. Ignoring game world click.");
                return;
            }

            // Your regular game world click logic here
            Debug.Log("Clicked on the game world.");
        }
    }

}