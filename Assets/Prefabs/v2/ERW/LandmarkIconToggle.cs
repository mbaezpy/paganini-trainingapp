using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class LandmarkIconToggle : MonoBehaviour
{
    private Toggle toggle;
    private ToggleGroup group;

    public LandmarkIcon OnIcon;
    public Color InactiveColor = Color.white;

    private LandmarkIcon.LandmarkType landmarkType;

    public LandmarkIcon.LandmarkType LandmarkType
    {
        get { return landmarkType; }
        set {
            OnIcon.GetComponent<LandmarkIcon>().SelectedLandmarkType = value;
            landmarkType = value;
        }
    }

    public Color ActiveColor
    {
        get { return OnIcon.BackgroundColor; }
        set { OnIcon.BackgroundColor = value; }
    }
   
    public ToggleGroup Group
    {
        get { return group; }
        set
        {
            group = value;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
        
        if (group)
        {
            toggle.group = group;
        }        

        toggle.onValueChanged.AddListener(onValueChanged);
        toggle.isOn = false;
        onValueChanged(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(onValueChanged);
    }

    void onValueChanged(bool on)
    {
        //Debug.Log(OnIcon.SelectedLandmarkType + " Toggle value changed: " + on);
        if (on)
        {
            OnIcon.ResetColor();
        }
        else
        {            
            OnIcon.ApplyColor(InactiveColor);
        }
    }

}
