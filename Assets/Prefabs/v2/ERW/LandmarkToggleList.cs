using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.UI;
using static LandmarkIcon;
using UnityEngine.Events;

public class LandmarkToggleList : MonoBehaviour
{
    public GameObject TogglePrefab;
    public GameObject Content;
    public TMPro.TMP_Text Title;

    public Color SelectedColor;
    public Color UnselectedColor;

    public ToggleGroup LandmarkToggleGroup;

    public LandmarkType SelectedLandMarkType;

    public UnityEvent<LandmarkIcon.LandmarkType> OnToggleSelected;    

    private readonly List<LandmarkIcon.LandmarkType> displayOrder = new List<LandmarkIcon.LandmarkType>
    {
        LandmarkIcon.LandmarkType.Home,
        LandmarkIcon.LandmarkType.Work,
        LandmarkIcon.LandmarkType.Park,
        LandmarkIcon.LandmarkType.Shopping,
        LandmarkIcon.LandmarkType.Train,
        LandmarkIcon.LandmarkType.Bus,
        LandmarkIcon.LandmarkType.Coffee,
        LandmarkIcon.LandmarkType.Friend,
        LandmarkIcon.LandmarkType.Finance,
        LandmarkIcon.LandmarkType.Recreation,
        LandmarkIcon.LandmarkType.Faith,
        LandmarkIcon.LandmarkType.Family,
        LandmarkIcon.LandmarkType.Sports,
        LandmarkIcon.LandmarkType.Health,
        LandmarkIcon.LandmarkType.Library
    };

    // Start is called before the first frame update
    void Start()
    {

        foreach(LandmarkIcon.LandmarkType icon in displayOrder)
        {
            if (icon != LandmarkIcon.LandmarkType.Placeholder) {
                GameObject obj = Instantiate(TogglePrefab, Content.transform);
                LandmarkIconToggle toggle = obj.GetComponent<LandmarkIconToggle>();

                toggle.LandmarkType = icon;
                toggle.ActiveColor = SelectedColor;
                toggle.InactiveColor = UnselectedColor;
                toggle.Group = LandmarkToggleGroup;
                

                Toggle t = toggle.GetComponent<Toggle>();
                t.onValueChanged.AddListener(delegate
                {
                    if (t.isOn) {
                        SelectedLandMarkType = GetSelectedLandmarkType();
                        OnToggleSelected?.Invoke(SelectedLandMarkType);
                    }
                    else if (!LandmarkToggleGroup.AnyTogglesOn())
                    {
                        SelectedLandMarkType = LandmarkIcon.LandmarkType.Placeholder;
                    }
                });

            }

        }

    }    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTitle(string title)
    {
        Title.text = title;
    }

    public void ShowListPicker(LandmarkType landmarkType = LandmarkType.Placeholder){
        gameObject.SetActive(true);
        SetSelectedLandmark(landmarkType);        
    }


    public LandmarkType GetSelectedLandmarkType()
    {

        foreach (Toggle toggle in LandmarkToggleGroup.ActiveToggles())
        {
            Debug.Log(toggle.GetComponent<LandmarkIconToggle>().LandmarkType);

            return (toggle.GetComponent<LandmarkIconToggle>().LandmarkType);
        }

        return LandmarkType.Placeholder;
    }


    public void SetSelectedLandmark(LandmarkType landmarkType)
    {
        Debug.Log("Setting selected landmark: " + landmarkType);
        SelectedLandMarkType = landmarkType;
        
        if (landmarkType == LandmarkType.Placeholder)
        {
            LandmarkToggleGroup.SetAllTogglesOff(sendCallback: true);
            return;
        }

        foreach (Transform child in Content.transform)
        {
            LandmarkIconToggle toggle = child.GetComponent<LandmarkIconToggle>();
            if (toggle.LandmarkType == landmarkType)
            {
                toggle.GetComponent<Toggle>().isOn = true;
            }
        }

        
    }

}
