using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPrefab : MonoBehaviour
{
    public Button ButtonObject;
    public GameObject ButtonLabel;
    public GameObject BusyStatus;

    [Header(@"Pressed Effect")]
    public bool PressedEffect = true;
    public bool BusyOnClicked = false;
    public GameObject PressedObject;
    public GameObject NormalObject;

    Vector3 originalPosition;

    void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        RegisterButtonPressedEvent();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void RenderBusyState(bool isBusy) {

        if (isBusy){
            OnButtonPressed(force: true);
        }
        else{
            OnButtonReleased(force: true);
        }

        BusyStatus.SetActive(isBusy);
        ButtonLabel.SetActive(!isBusy);
        ButtonObject.interactable = !isBusy;        
        
    }

    private void RegisterButtonPressedEvent()
    {
        if (ButtonObject == null) return;

        EventTrigger trigger = ButtonObject.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = ButtonObject.gameObject.AddComponent<EventTrigger>();
        }

        // Create entry for pointer down event
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) => OnButtonPressed());
        trigger.triggers.Add(entry);

        // Create entry for pointer up event
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((eventData) => OnButtonReleased());
        trigger.triggers.Add(entry);

        // OnClick
        ButtonObject.onClick.AddListener(OnButtonClicked);
    }

    public void OnButtonPressed(bool force = false)
    {
        if (ButtonObject.interactable == false && !force) return;
        if (PressedEffect)
        {
            if (PressedObject != null && NormalObject != null)
            {
                originalPosition = ButtonLabel.transform.position;
                ButtonLabel.transform.position = PressedObject.transform.position;
                RenderPressed(true);
            }
        }
    }    

    public void OnButtonReleased(bool force = false)
    {
        if (ButtonObject.interactable == false && !force) return;

        if (PressedEffect)
        {
            if (PressedObject != null && NormalObject != null)
            {
                ButtonLabel.transform.position = originalPosition;
                RenderPressed(false);
            }
        }
    }

    public void OnButtonClicked()
    {
        if (BusyOnClicked){
            RenderBusyState(true);
        }
    }

    private void RenderPressed(bool pressed){
        PressedObject.SetActive(pressed);
        NormalObject.SetActive(!pressed);
    }

    void OnDestroy()
    {
        if (ButtonObject == null) return;

        EventTrigger trigger = ButtonObject.gameObject.GetComponent<EventTrigger>();
        if (trigger != null)
        {
            trigger.triggers.Clear();
        }

        ButtonObject?.onClick.RemoveListener(OnButtonClicked);
    }

}
