using UnityEngine;

public static class AppState
{

    public static User CurrentUser=null;

    public static string APIToken = null;

    public static class ERW
    {
        public static Way CurrentWay;
        public static Route CurrentRoute;
        public static bool recording = false;
        public static bool pausedRec = false;

        public static void ResetValues()
        {
            CurrentWay = null;
            CurrentRoute = null;
            recording = false;
            pausedRec = false;
        }
    }
    //Erstbegehung
    // public static int SelectedWeg = -1;
    // public static int SelectedBegehung = -1;
    // public static bool recording = false;
    // public static bool pausedRec=false;




    // Current State

    public static string currentScene = "";
    public static string lastScene = "";
    //public static string currentBegehung = "";

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