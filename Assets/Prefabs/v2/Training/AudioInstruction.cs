using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DirectionIcon;
using static LocationUtils;

public class AudioInstruction : MonoBehaviour
{
    [Header(@"General sounds")]
    [SerializeField] AudioClip TapSound;
    [SerializeField] AudioClip PopupSound;

    [Header(@"Training sounds")]
    [SerializeField] AudioClip GotoSound;
    [SerializeField] AudioClip GotoConfirmationSound;
    [SerializeField] AudioClip OfftrackSound;
    [SerializeField] AudioClip OntrackSound;
    [SerializeField] AudioClip ArrivedSound;

    [Header(@"UI elements")]
    [SerializeField] Toggle AudioStatus;


    private AudioSource audioSource;
    private Queue<(AudioClip,bool)> soundQueue = new Queue<(AudioClip, bool)>();
    private bool freeUpSoundAsset;

    private AudioClip lastAudioClip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Check if audio source finished playing and toggle is still on
        if (!audioSource.isPlaying && AudioStatus.isOn)
        {
            // Turn off the toggle button
            AudioStatus.SetIsOnWithoutNotify(false);
            Debug.Log("TurningOff, bc isPlaying: " + audioSource.isPlaying);

        }

        if (!audioSource.isPlaying && soundQueue.Count > 0)
        {
            PlayNextSound();
        }


        //Debug.Log(audioSource.isPlaying);
    }

    public void ReplayPause(bool doPLay)
    {
        // no sounds have been played
        if (audioSource.clip == null) return;

        if (doPLay && freeUpSoundAsset)
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
        AudioStatus.isOn = false;
    }

    public void PlayGotoInstruction(Pathpoint point)
    {
        PlaySound("Goto");
    }

    public void PlayNavInstruction(Pathpoint point)
    {
        DirectionType directionType;
        if (Enum.TryParse(point.Instruction.ToString(), out directionType))
        {
            PlaySound(directionType.ToString());
        }        
    }
    

    public void PlayNavInstructionCorrect()
    {
        PlaySound("DecisionPoint-Correct");
    }

    public void PlayChallengeDirectionIntro()
    {
        PlaySound("ChallengeDirectionIntro");
    }

    public void PlayChallengeDirectionTask()
    {
        PlaySound("ChallengeDirectionTask");
    }

    public void PlayTriviaDirectionTask()
    {
        PlaySound("TriviaDirectionTask");
    }

    public void PlayTriviaDirectionIntro()
    {
        PlaySound("TriviaDirectionIntro");
    }

    public void PlayTriviaDirectionFeedbackOk()
    {
        PlaySound("TriviaDirection-FeedbackCorrect");
    }

    public void PlayTriviaDirectionFeedbackWrong()
    {
        PlaySound("TriviaDirection-FeedbackWrong");
    }

    public void PlayTriviaGoToTask()
    {
        PlaySound("TriviaGoToTask");
    }

    public void PlayTriviaGoToIntro()
    {
        PlaySound("TriviaGoToIntro");
    }

    public void PlayTriviaGoToFeedbackOk()
    {
        PlaySound("TriviaGoTo-FeedbackCorrect");
    }

    public void PlayChallengeGoToIntro()
    {
        PlaySound("ChallengeGoToIntro");
    }

    public void PlayChallengeGoToFeedbackOk()
    {
        PlaySound("ChallengeGoTo-FeedbackCorrect");
    }

    public void PlayChallengeGoToFeedbackWrong()
    {
        PlaySound("ChallengeGoTo-FeedbackWrong");
    }

    public void PlayTriviaGoToFeedbackWrong()
    {
        PlaySound("TriviaGoTo-FeedbackWrong");
    }

    public void PlayTriviaGoToTooLate()
    {
        PlaySound("TriviaGoTo-Late");
    }

    public void PlayDirectionTask()
    {
        PlaySound("TriviaDirectionTask");
    }

    public void PlayRefuseTask()
    {
        PlaySound("RefuseTask");
    }

    public void PlayStartInstruction(Pathpoint point)
    {
        PlaySound("Start");
    }

    public void PlayStartArrived()
    {
        PlaySound("Start-Correct");
    }

    public void PlayArrivedInstruction(Pathpoint point)
    {
        PlaySound("Arrived");
    }

    public void PlaySafetyPointInstruction(Pathpoint point)
    {
        PlaySound("SafetyPoint");
    }

    public void PlayBackOnTrackConfusion()
    {
        PlaySound("BackOnTrack-Confused");
    }

    public void PlayBackOnTrackDowngradeMode(PathpointPIM.SupportMode supportMode)
    {
        PlaySound("BackOnTrack-Downgrade-" + supportMode);
    }

    public void PlayOfftrackInstruction(NavigationIssue navigationIssue)
    {
        //{ NavigationIssue.Deviation, "Sie sind von der Route abgewichen." },
        //{ NavigationIssue.WrongDirection, "Sie gehen in die falsche Richtung." },
        //{ NavigationIssue.MissedTurn, "Sie haben die Abzweigung verpasst." },
        //{ NavigationIssue.WrongTurn, "Sie haben die falsche Abzweigung genommen." }

        PlaySound(navigationIssue.ToString());
    }

    public void PlayCompassGPSIssues()
    {
        PlaySound("Compass-GPSIssue");
    }

    public void PlayPayAttentionStart(bool verbose)
    {
        if (verbose)
        {
            PlaySound("Attention_Start-Long");
        }
        else
        {
            PlaySound("Attention_Start-Short");
        }
        
    }

    public void PlayPayAttention()
    {
        PlaySound("Attention");
    }

    // Effects
    public void PlayEffectEnterPOI()
    {
        PlaySound(GotoConfirmationSound);
    }

    public void PlayEffectLeavePOI()
    {
        PlaySound(GotoSound);
    }

    public void PlayEffectOffTrack()
    {
        PlaySound(OfftrackSound);
    }

    public void PlayEffectOnTrack()
    {
        PlaySound(OntrackSound);
    }

    public void PlayEffectArrived()
    {
        PlaySound(ArrivedSound);
    }

    public void PlayPopup()
    {
        PlaySound(PopupSound);
    }

    // private utils
    protected void PlaySound(AudioClip clip)
    {
        AddToQueue(clip, false);
    }

    protected void PlaySound(string filename)
    {

        // AudioClip clip = Resources.Load<AudioClip>("v2/Sounds/Instruction_" + filename);
        // if (clip != null)
        // {
        //     AddToQueue(clip, true);         
        // }
        // else
        // {
        //     Debug.LogError("Audio clip not found: " + filename);
        // }
        PlaySound(filename, "v2/Sounds/Training/Instruction_");
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
            Debug.LogError("Audio clip not found: " + filename);
        }
    }


    protected void AddToQueue(AudioClip clip, bool freeUp)
    {
        if (clip != null)
        {
            soundQueue.Enqueue(new ( clip, freeUp ));
        }
    }

    protected void PlayNextSound()
    {
        (AudioClip clip, bool freeUp) = soundQueue.Dequeue();
        if (clip != null)
        {
            if (freeUpSoundAsset)
            {
                audioSource.clip.UnloadAudioData();
            }
            freeUpSoundAsset = freeUp;
            audioSource.clip = clip;
            audioSource.Play();
            if (AudioStatus && freeUp)
            {
                AudioStatus.SetIsOnWithoutNotify(true);
                Debug.Log("Start playing");
                
            }
        }
    }

    protected IEnumerator WaitForSoundCompletion()
    {
        // let's wait a for a moment before checking.
        yield return new WaitForSeconds(0.3f);

        yield return new WaitUntil(() => !audioSource.isPlaying);

        // When the sound is done playing, turn off the toggle
        AudioStatus.isOn = false;
        Debug.Log("Finished playing");
    }


}


