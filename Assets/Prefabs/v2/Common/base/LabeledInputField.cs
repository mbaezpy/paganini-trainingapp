using UnityEngine;


public class LabeledInputField : MonoBehaviour
{
    public TMPro.TMP_Text ErrorMessage;   // The Text GameObject to apply the effect.

    public void DisplayErrorMessage(bool show)
    {
        ErrorMessage.gameObject.SetActive(show);
    }

}
