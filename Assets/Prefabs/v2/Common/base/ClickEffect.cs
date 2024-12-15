using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VectorGraphics;

public class ClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header(@"Color effect")]
    [Tooltip("The Image GameObject to apply the effect.")]
    public Image imageObject; 
    [Tooltip("The Text GameObject to apply the effect.")]  
    public TMPro.TMP_Text textObject; 
    [Tooltip("Color to change to when pressed.")] 
    public Color pressedColor = Color.black; 

    [Header(@"Push scale effect")]
    [Tooltip("The GameObject that will be scaled down when pressed")]
    public GameObject Wrapper; 
    [Tooltip("Background effect that will be shown when the button is pressed")]
    public SVGImage ClickBackground; 

    [Tooltip("Scale reduction when pressed (0.9 means 90% of the original size)")]
    public float scaleReduction = 0.95f;     

    [Header(@"Vibration Effect")]    
    [Tooltip("Vibrate the phone when the button is pressed")]
    public bool VibrateOnPressed = true;     

    private Vector3 originalScale;   // Original scale of the GameObject.
    private Color? originalImageColor; // Original color of the Image.
    private Color? originalTextColor;  // Original color of the Text.

    private Button button;
    

    private void Start()
    {
        // Store the original scale and colors.
        originalScale = Wrapper.transform.localScale;
        originalImageColor = imageObject?.color;
        originalTextColor = textObject?.color;

        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        // Apply the click effect when the GameObject is pressed.
        Wrapper.transform.localScale = originalScale * scaleReduction;
        if (imageObject!= null) imageObject.color = pressedColor;
        if (textObject != null) textObject.color = pressedColor;

        if (ClickBackground!=null) ClickBackground.enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        // Reset to the original scale and colors when the click is released.
        Wrapper.transform.localScale = originalScale;
        if (imageObject != null) imageObject.color = (Color)originalImageColor;
        if (textObject != null)  textObject.color = (Color)originalTextColor;

        if (ClickBackground != null) ClickBackground.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (VibrateOnPressed) HapticUtils.VibrateForClick();
    }
}
