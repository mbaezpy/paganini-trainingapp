using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static SimpleFileBrowser.FileBrowser;

public class GameButton : MonoBehaviour
{
    public Button MainButton;
    public GameObject StatusNormal;
    public GameObject StatusPressed;

    public GameObject IconNormal;
    public GameObject IconPressed;
    public GameObject IconPressedError;
    public GameObject IconPressedSuccess;
    public Image IconPressedMask;    

    public Color SuccessOutlineColor;
    public Color ErrorOutlineColor;

    private Color OriginaOutlinelColor;

    private bool isPressed = false;
    private bool isConfirmed = false;

    private void Start()
    {
        // Set the initial state
        SetButtonState(isPressed);

        // Register a listener for the button click event
        MainButton.onClick.AddListener(ToggleButtonState);
        var outlineComponent = IconPressed.GetComponent<Outline>();
        if (outlineComponent != null)
        {
            OriginaOutlinelColor = outlineComponent.effectColor;
        }
    }

    public void ResetButton()
    {
        isConfirmed = false;
        MainButton.interactable = true;
        SetButtonState(false);
        ChangeOutlineColor(IconPressed, OriginaOutlinelColor);
        IconPressedSuccess.SetActive(true);
        IconPressedError.SetActive(false);
    }

    // Function to toggle between normal and pressed states
    public void ToggleButtonState()
    {
        // we don't switch when in confirmed state
        if (isConfirmed) return;

        isPressed = !isPressed; // Toggle the state
        SetButtonState(isPressed); // Apply the new state

        HapticUtils.VibrateForClick();
    }

    // Function to set the button state based on the given parameter
    public void SetButtonState(bool pressed)
    {
        StatusNormal.SetActive(!pressed); // Show or hide the normal state
        StatusPressed.SetActive(pressed); // Show or hide the pressed state
    }

    public void RenderConfirmationState(bool? success)
    {
        
        isConfirmed = true;
        MainButton.interactable = false;

        // neutral
        if (success == null)
        {
            return;
        }

        // in case of error, we still show the arrow for a second, in red
        //IconPressedSuccess.SetActive(true);
        //IconPressedError.SetActive(false);

        // Ensure that the button is in the pressed state        
        SetButtonState(success != null);

        // Set the outline color of the pressed icon based on success

        if ((bool)success)
        {
            ChangeOutlineColor(StatusPressed, SuccessOutlineColor);
        }
        else
        {
            ChangeOutlineColor(StatusPressed, ErrorOutlineColor);
        }

        //we wait for two seconds, and then to the folowing:
        StartCoroutine(ToggleIconsAfterDelay((bool)success));

    }


    private IEnumerator ToggleIconsAfterDelay(bool success)
    {
        yield return new WaitForSeconds(2f);

        IconPressedError.SetActive(!success);
        IconPressedSuccess.SetActive(success);
    }

    private void ChangeOutlineColor(GameObject obj, Color color)
    {
        // Set the outline color to error color
        var outlineComponent = obj.GetComponent<Outline>();
        if (outlineComponent != null)
        {
            outlineComponent.effectColor = color;
            IconPressedMask.color = color;
        }
    }


    // Clean up the listener when the object is destroyed
    private void OnDestroy()
    {
        MainButton.onClick.RemoveAllListeners();
    }
}

