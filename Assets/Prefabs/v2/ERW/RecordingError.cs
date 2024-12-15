using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RecordingError : MonoBehaviour
{

    [Header(@"UI Components")]
    public TMPro.TMP_Text TitleText;
    public TMPro.TMP_Text SubtitleText;
    public TMPro.TMP_Text MessageText;

    private void Start()
    {

    }

    public void InformVideoRecordingError(string message)
    {
        gameObject.SetActive(true);
        TitleText.text = "Videoaufnahme-Fehler";
        SubtitleText.text = "Bitte melden Sie dieses Problem dem IT-Support-Team";
        MessageText.text = message;
    }

    public void InformGPSTrackingError(string message)
    {
        gameObject.SetActive(true);
        TitleText.text = "GPS-Tracking-Fehler";
        SubtitleText.text = "Bitte melden Sie dieses Problem dem IT-Support-Team";
        MessageText.text = message;
    }



}


