using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PropagateScrollEvents : MonoBehaviour
{
    public Button ButtonSource;
    private EventTrigger eventTrigger;

    private void Awake()
    {
        // Ensure the EventTrigger component exists
        eventTrigger = ButtonSource.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = ButtonSource.gameObject.AddComponent<EventTrigger>();
        }

        // Register relevant scroll events
        RegisterToPropagate(EventTriggerType.Scroll, ExecuteEvents.scrollHandler);
        RegisterToPropagate(EventTriggerType.BeginDrag, ExecuteEvents.beginDragHandler);
        RegisterToPropagate(EventTriggerType.EndDrag, ExecuteEvents.endDragHandler);
        RegisterToPropagate(EventTriggerType.Drag, ExecuteEvents.dragHandler);
    }

    private void RegisterToPropagate<T>(EventTriggerType eventType, ExecuteEvents.EventFunction<T> eventFunction) where T : IEventSystemHandler
    {
        // Create a new entry for the specified event type
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };

        entry.callback.AddListener((eventData) =>
        {
            if (eventData is PointerEventData pointerEventData)
            {
                //Debug.Log($"Propagating event of type: {eventType}");
                PassEventToParent(pointerEventData, eventFunction);
            }
            else
            {
                Debug.LogWarning("Event data is not PointerEventData; skipping propagation.");
            }
        });

        // Add the entry to the EventTrigger component
        eventTrigger.triggers.Add(entry);
    }
    

    private void PassEventToParent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunction) where T : IEventSystemHandler
    {
        Transform parent = transform.parent;

        while (parent != null)
        {
            if (ExecuteEvents.Execute(parent.gameObject, eventData, eventFunction))
            {
                //Debug.Log($"Event propagated to: {parent.gameObject.name}");
                break;
            }
            parent = parent.parent;
        }
    }
}
