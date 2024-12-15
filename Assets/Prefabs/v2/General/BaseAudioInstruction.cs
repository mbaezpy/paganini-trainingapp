using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseAudioInstruction : MonoBehaviour
{
    [Header("General Sounds")]
    [SerializeField] protected AudioClip TapSound;
    [SerializeField] protected AudioClip PopupSound;

    [Header("UI Elements")]
    [SerializeField] protected Toggle AudioStatus;

    protected AudioSource audioSource;
    private Queue<(AudioClip clip, bool freeUp)> soundQueue = new Queue<(AudioClip, bool)>();
    private bool freeUpSoundAsset;

    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    protected virtual void Update()
    {
        if (!audioSource.isPlaying && AudioStatus && AudioStatus.isOn)
        {
            AudioStatus.SetIsOnWithoutNotify(false);
            Debug.Log("Audio finished, turning off toggle.");
        }

        if (!audioSource.isPlaying && soundQueue.Count > 0)
        {
            PlayNextSound();
        }
    }

    public void ReplayPause(bool doPlay)
    {
        if (audioSource.clip == null) return;

        if (doPlay && freeUpSoundAsset)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    public void CancelCurrentPlayback()
    {
        audioSource.Pause();
        if (AudioStatus)
        {
            AudioStatus.isOn = false;
        }
    }

    protected void PlaySound(AudioClip clip)
    {
        AddToQueue(clip, false);
    }

    protected void PlaySound(string filename, string basePath)
    {
        AudioClip clip = Resources.Load<AudioClip>(basePath + filename);
        if (clip != null)
        {
            AddToQueue(clip, true);
        }
        else
        {
            Debug.LogError($"Audio clip not found: {basePath}{filename}");
        }
    }

    private void AddToQueue(AudioClip clip, bool freeUp)
    {
        if (clip != null)
        {
            soundQueue.Enqueue((clip, freeUp));
        }
    }

    private void PlayNextSound()
    {
        if (soundQueue.Count == 0) return;

        (AudioClip clip, bool freeUp) = soundQueue.Dequeue();
        if (clip != null)
        {
            if (freeUpSoundAsset)
            {
                audioSource.clip?.UnloadAudioData();
            }

            freeUpSoundAsset = freeUp;
            audioSource.clip = clip;
            audioSource.Play();

            if (AudioStatus && freeUp)
            {
                AudioStatus.SetIsOnWithoutNotify(true);
                Debug.Log("Playing next sound.");
            }
        }
    }

    protected IEnumerator WaitForSoundCompletion()
    {
        yield return new WaitForSeconds(0.3f); // Small delay before checking
        yield return new WaitUntil(() => !audioSource.isPlaying);

        if (AudioStatus)
        {
            AudioStatus.isOn = false;
        }

        Debug.Log("Finished playing sound.");
    }
}
