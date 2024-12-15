using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraShooter : MonoBehaviour
{
    public GameObject snapEffect; // Reference to the CameraSnapEffect GameObject
    public AudioClip snapSound; // Reference to the snap sound effect
    public float blinkDuration = 0.2f;

    private Image snapEffectImage;
    private AudioSource audioSource;

    private void Start()
    {
        snapEffectImage = snapEffect.GetComponentInChildren<Image>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = snapSound;
    }

    public void TakePhoto()
    {
        // Play the snap sound effect
        audioSource.Play();

        StartCoroutine(BlinkScreen());
    }

    private IEnumerator BlinkScreen()
    {
        snapEffect.SetActive(true);

        // Turn the screen black
        snapEffectImage.color = new Color(0f, 0f, 0f, 1f);

        // Wait for the specified duration (blinkDuration)
        yield return new WaitForSeconds(blinkDuration);

        // Turn the screen back to normal (transparent)
        snapEffectImage.color = new Color(0f, 0f, 0f, 0f);

        // Deactivate the black screen
        snapEffect.SetActive(false);
    }
}


