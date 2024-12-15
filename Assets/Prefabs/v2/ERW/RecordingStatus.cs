using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RecordingStatus : MonoBehaviour
{
    public GameObject RecIcon;
    public GameObject GPSPanelColor;    
    public GameObject RecPanelColor;
    public TMPro.TMP_Text GPSInfoLabel;
    public TMPro.TMP_Text RecInfoLabel;

    private Color GoodStatus = Color.green;
    private Color MediumStatus = Color.yellow;
    private Color PoorStatus = Color.red;
    private Color InactiveStatus = Color.gray;


    private float fpsMargin = 4;


    // Pulsating animation
    private bool isRecording = false;
    public float pulseSpeed = 1.0f;   // Speed of the pulse
    public float pulsePause = 0.2f;  // Pause between pulses
    public float betweenPulsePause = 1f;  // Pause between pulses
    public float minScale = 0.95f;    // Minimum scale
    public float maxScale = 1.0f;    // Maximum scale

    Color pastGPSColor;
    Color pastRecColor;
    string pastGPSLabel;
    string pastRecLabel;

    public enum RunningStatus
    {
        Active,
        Inactive,
        Error
    }


    private void Start()
    {
        GPSInfoLabel.text = "-";
        RecInfoLabel.text = "-";

        StartCoroutine(PulsatingAnimation());
    }


    void Update()
    {
     
    }

    IEnumerator PulsatingAnimation()
    {
        while (true)
        {

            // Scale up
            while (RecIcon.transform.localScale.x < maxScale)
            {
                if (isRecording) RecIcon.transform.localScale += Vector3.one * pulseSpeed * Time.deltaTime;
                yield return null;
            }

            // Pause
            yield return new WaitForSeconds(pulsePause);

            // Scale down
            while (RecIcon.transform.localScale.x > minScale)
            {
                if (isRecording) RecIcon.transform.localScale -= Vector3.one * pulseSpeed * Time.deltaTime;
                yield return null;
            }

            // Pause
            yield return new WaitForSeconds(betweenPulsePause);
        }
    }


    public void PauseUpdate(bool toglePause)
    {
        if (toglePause)
        {            
            ApplyColorToBackground(GPSPanelColor, InactiveStatus);
            GPSInfoLabel.text = "-";

            ApplyColorToBackground(RecPanelColor, InactiveStatus);
            RecInfoLabel.text = "-";
        } else
        {
            ApplyColorToBackground(GPSPanelColor, pastRecColor);
            GPSInfoLabel.text = pastGPSLabel;

            ApplyColorToBackground(RecPanelColor, pastRecColor);
            RecInfoLabel.text = pastRecLabel;
        }
        
    }


    public void UpdateGPSStatus(RunningStatus status, float accuracy)
    {
        Color color = InactiveStatus;
        string label = Math.Round(accuracy) + " m";

        if (status == RunningStatus.Inactive)
        {
            color= InactiveStatus;
            label = "-";
        }
        else if (status == RunningStatus.Error)
        {
            color = PoorStatus;
            label = "-";
        }
        else if (accuracy < 8)
        {
            color = GoodStatus;
        }
        else if (accuracy < 15)
        {
            color = MediumStatus;
        }
        else
        {
            color = PoorStatus;
        }

        ApplyColorToBackground(GPSPanelColor, color);
        GPSInfoLabel.text = label;

        pastGPSLabel = label;
        pastGPSColor = color;
    }

    public void UpdateRecStatus(RunningStatus status, float actualFPS, float expectedFPS)
    {
        Color color = InactiveStatus;
        string label = Math.Round(actualFPS) + " fps";

        if (status == RunningStatus.Inactive)
        {
            color = InactiveStatus;
            RecIcon.transform.localScale = Vector3.one;
            label = "-";
        }
        if (status == RunningStatus.Error)
        {
            // Show something else
            color = InactiveStatus;
            RecIcon.transform.localScale = Vector3.one;
            label = "-";
        }
        else if (actualFPS < expectedFPS - fpsMargin)
        {
            color = MediumStatus;
        }
        else if (actualFPS > expectedFPS + fpsMargin)
        {
            color = MediumStatus;
        }
        else
        {
            color = GoodStatus;
        }

        ApplyColorToBackground(RecPanelColor, color);
        RecInfoLabel.text = label;
        isRecording = status == RunningStatus.Active;

        pastRecLabel = label;
        pastRecColor = color;
    }

    public void UpdateGPSError()
    {

    }

    private void ApplyColorToBackground(GameObject panel, Color color)
    {
        Image img = panel.GetComponent<Image>();
        if (img != null) {
            img.color = color;
        }        
    }
}


