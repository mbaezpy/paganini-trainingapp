﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Data save example.
 */

public class SaveQuaternionData : MonoBehaviour {

    Toggle toggleEnc0;
    InputField inputX;
    InputField inputY;
    InputField inputZ;
    InputField inputW;
    Text labelOut;

    // Use this for initialization
    void Start()
    {
        // Connect UI elements:
        inputX = transform.Find("InputX").GetComponent<InputField>();
        inputY = transform.Find("InputY").GetComponent<InputField>();
        inputZ = transform.Find("InputZ").GetComponent<InputField>();
        inputW = transform.Find("InputW").GetComponent<InputField>();
        toggleEnc0 = transform.Find("ToggleEnc (0)").GetComponent<Toggle>();
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
    }

    public void SaveData()
    {
        float x = (inputX.text != "") ? float.Parse(inputX.text) : 0f;
        float y = (inputY.text != "") ? float.Parse(inputY.text) : 0f;
        float z = (inputZ.text != "") ? float.Parse(inputZ.text) : 0f;
        float w = (inputW.text != "") ? float.Parse(inputW.text) : 0f;
        bool enc = toggleEnc0.isOn;
        // Saving the file
        Quaternion data = new Quaternion(x, y, z, w);
        FileManagement.SaveFile("qData", data, enc);
    }

    public void ReadData()
    {
        bool enc = toggleEnc0.isOn;
        // Reading the file
        Quaternion data = FileManagement.ReadFile<Quaternion>("qData", enc);
        labelOut.text = FileManagement.CustomToString(data);
    }
}
