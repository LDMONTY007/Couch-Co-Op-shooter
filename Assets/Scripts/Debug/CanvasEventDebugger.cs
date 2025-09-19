using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasEventDebugger : MonoBehaviour
{
    void Start()
    {
        // Find all selectable UI elements within the Canvas
        Selectable[] selectables = GetComponentsInChildren<Selectable>(true);

        foreach (Selectable selectable in selectables)
        {
            // Add event triggers to each selectable element
            EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = selectable.gameObject.AddComponent<EventTrigger>();
            }

            // PointerEnter event
            EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
            entryPointerEnter.eventID = EventTriggerType.PointerEnter;
            entryPointerEnter.callback.AddListener((eventData) => { LogEvent(selectable.name, "PointerEnter", eventData); });
            trigger.triggers.Add(entryPointerEnter);

            // PointerExit event
            EventTrigger.Entry entryPointerExit = new EventTrigger.Entry();
            entryPointerExit.eventID = EventTriggerType.PointerExit;
            entryPointerExit.callback.AddListener((eventData) => { LogEvent(selectable.name, "PointerExit", eventData); });
            trigger.triggers.Add(entryPointerExit);

            // PointerDown event
            EventTrigger.Entry entryPointerDown = new EventTrigger.Entry();
            entryPointerDown.eventID = EventTriggerType.PointerDown;
            entryPointerDown.callback.AddListener((eventData) => { LogEvent(selectable.name, "PointerDown", eventData); });
            trigger.triggers.Add(entryPointerDown);

            // PointerUp event
            EventTrigger.Entry entryPointerUp = new EventTrigger.Entry();
            entryPointerUp.eventID = EventTriggerType.PointerUp;
            entryPointerUp.callback.AddListener((eventData) => { LogEvent(selectable.name, "PointerUp", eventData); });
            trigger.triggers.Add(entryPointerUp);

            // Click event (for buttons)
            if (selectable is Button button)
            {
                button.onClick.AddListener(() => { LogEvent(button.name, "Click", null); });
            }
            // Value Changed event (for toggles and sliders)
            else if (selectable is Toggle toggle)
            {
                toggle.onValueChanged.AddListener((value) => { LogEvent(toggle.name, "ValueChanged", value); });
            }
            else if (selectable is Slider slider)
            {
                slider.onValueChanged.AddListener((value) => { LogEvent(slider.name, "ValueChanged", value); });
            }
        }
    }

    private void LogEvent(string elementName, string eventType, object eventData)
    {
        if (eventData != null)
        {
            Debug.Log($"UI Event: {eventType} on '{elementName}' - Data: {eventData}");
        }
        else
        {
            Debug.Log($"UI Event: {eventType} on '{elementName}'");
        }
    }
}