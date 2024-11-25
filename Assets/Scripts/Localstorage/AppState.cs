using UnityEngine;

public static class AppState
{

    public static User CurrentUser=null;

    public static string APIToken = null;


    //Erstbegehung
    public static int SelectedWeg = -1;
    public static int SelectedBegehung = -1;
    public static bool recording = false;
    public static bool pausedRec=false;
    public static bool wrapBegehung = false;
    public static int poiType = -1;
    public static string picturePath = "";
    public static float connectionCheckIntervall = 3.0f;

    public static bool isMute = false;

    public static bool isIncognito = false;


    // Current State

    public static string currentScene = "";
    public static string lastScene = "";
    public static string currentBegehung = "";

    public static int screenSleepTimeout = Screen.sleepTimeout;


    // Social Worker info
    public static SocialWorker CurrentSocialWorker = null;
    public static string SWAPIToken = null;

    public static class Training
    {
        public enum DesignMode
        {
            Baseline,
            Improved,
            Adaptive
        }
        // Temporal Configurations
        public static DesignMode ActiveDesignMode = DesignMode.Adaptive;

        public static int DecisionPointConfirmationDelay = 5;
        public static int SafetyPointConfirmationDelay = 3;
        public static int GotoConfirmationDelay = 3;

        public static int GotoHideInstructionTutorialSegments = 2;
    }
    

}