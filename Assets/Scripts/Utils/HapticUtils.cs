using UnityEngine;

/// <summary>
/// Provides vibration functions for different scenarios.
/// </summary>
public static class HapticUtils
{
    // Duration for short vibrations
    private const long ShortVibrationDuration = 30;

    // Duration for alert vibrations
    private const long AlertVibrationDuration = 500;
    private const long AlertVibrationPauseDuration = 100;
    private static readonly long[] AlertVibrationPattern = { 0, AlertVibrationDuration, AlertVibrationPauseDuration, AlertVibrationDuration };

    // Duration for nudge vibrations
    private const long NudgeVibrationDuration = 30;
    private const long NudgeVibrationPauseDuration = 100;
    private static readonly long[] NudgeVibrationPattern = { 0, NudgeVibrationDuration, NudgeVibrationPauseDuration, NudgeVibrationDuration };


    public static void VibrateForClick(){
        Vibration.Instance.CreateOneShot(ShortVibrationDuration);
    }

    /// <summary>
    /// Triggers a short vibration for a notification.
    /// </summary>
    public static void VibrateForNotification()
    {
        Vibration.Instance.CreateOneShot(ShortVibrationDuration);
    }    

    /// <summary>
    /// Triggers an alert vibration pattern to call for attention.
    /// </summary>
    public static void VibrateForAlert()
    {
        Vibration.Instance.CreateWaveform(AlertVibrationPattern, -1);
    }

    /// <summary>
    /// Triggers a nudge vibration pattern.
    /// </summary>
    public static void VibrateForNudge()
    {
        Vibration.Instance.CreateWaveform(NudgeVibrationPattern, -1);
    }
}
