using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NatSuite.Recorders.Clocks;
using UnityEngine;
using UnityEngine.UI;

public class RouteTrainingController : MonoBehaviour
{
    [Header("Main panels")]
    public GameObject LoadingPanel;
    public GameObject StartPanel;
    public GameObject TrainingPanel;
    public GameObject CompletePanel;    

    [Header("Wayfind Instruction UI")]
    public WayStartInstruction StartInstruction;
    public WayGoToModes GoToInstruction;
    public WayDecisionModes DecisionInstruction;
    public WaySafetyInstruction SafetyInstruction;
    public WayEndInstruction EndInstruction;
    public WayOfftrackInstruction OfftrackInstruction;

    public AudioInstruction AuralInstruction;

    [Header("Wayfind Overlay UI")]
    public GameObject PausePanel;
    public GameObject HelpPanel;

    [Header("UI Components")]
    public WayfindInstruction Wayfind;    
    public LocationUtilsConfig RouteValidationConfig;
    public RouteTrainingTracking RouteTracking;
    public TrainingProgressBar RouteProgressBar;

    public GPSDebug GPSDebugDisplay;
    public SocketsAPI RealTimeAPI;
    public TrackingStatus GPSTrackingStatus;

    [Header("Configuration")]
    public bool UploadChachedRouteWalksEnabled;
    public bool UploadPerformedRouteWalkEnabled;
    public bool UseImprovedDesign;    


    private RouteSharedData SharedData;

    private POIWatcher POIWatch;
    private LocationUtils RouteValidation;
    private Text2SpeechUtils SpeechUtils;
    private WalkEventManager WalkEventHandler;
    private RouteWalkLogger RouteWalkLog;

    private RouteWalk CurrentRouteWalk;
    private TrainingStatus CurrentStatus = TrainingStatus.WAIT_START;
    private bool DataInitialised = false;
    private RealtimeClock clock;
    private bool pauseTraining;
    private int lastMinPathlogId;


    public enum TrainingStatus
    {       
        WAIT_START,
        WAIT_CONFIRM,
        TRAINING,
        PAUSED,
        CANCELED,
        ARRIVED
    }


    // Start is called before the first frame update
    void Start()
    {
        RouteValidation = LocationUtils.Instance;
        RouteValidation.Initialise(RouteValidationConfig);

        POIWatch = POIWatcher.Instance;
        POIWatch.InitialiseWatcher(RouteValidation, RouteValidationConfig);

        SpeechUtils = Text2SpeechUtils.Instance;
        SpeechUtils.Initialise();

        RouteWalkLog = RouteWalkLogger.Instance;

        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnOffTrackEnd -= POIWatch_OnOffTrackEnd;
        POIWatch.OnDecisionStart -= POIWatch_OnDecisionStart;
        POIWatch.OnDecisionEnd -= POIWatch_OnDecisionEnd;
        POIWatch.OnSegmentStart -= POIWatch_OnSegmentStart;
        POIWatch.OnSegmentEnd -= POIWatch_OnSegmentEnd;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange -= POIWatch_OnWalkStatusChange;
        POIWatch.OnLog -= POIWatch_OnLog;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;
        POIWatch.OnPOITargetChange -= POIWatch_OnPOITargetChange;

        DecisionInstruction.OnNavInstructionUsed -= DecisionInstruction_OnNavInstructionUsed;
        SafetyInstruction.OnNavInstructionUsed -= SafetyInstruction_OnNavInstructionUsed;
        OfftrackInstruction.OnRecoveryInstructionUsed -= OfftrackInstruction_OnRecoveryInstructionUsed;
        GoToInstruction.OnInstructionHideChange -= GoToInstruction_OnInstructionHideChange;
        GoToInstruction.OnAdaptationEventChange -= GoToInstruction_OnAdaptationEventChange;
        DecisionInstruction.OnAdaptationEventChange -= GoToInstruction_OnAdaptationEventChange;

        POIWatch.OnEnteredPOI += POIWatch_OnEnteredPOI;        
        POIWatch.OnLeftPOI += POIWatch_OnLeftPOI;        
        POIWatch.OnOffTrack += POIWatch_OnOffTrack;
        POIWatch.OnOffTrackEnd += POIWatch_OnOffTrackEnd;
        POIWatch.OnDecisionStart += POIWatch_OnDecisionStart;
        POIWatch.OnDecisionEnd += POIWatch_OnDecisionEnd;
        POIWatch.OnSegmentStart += POIWatch_OnSegmentStart;
        POIWatch.OnSegmentEnd += POIWatch_OnSegmentEnd;
        POIWatch.OnAlongTrack += POIWatch_OnAlongTrack;        
        POIWatch.OnArrived += POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange += POIWatch_OnWalkStatusChange;        
        POIWatch.OnLog += POIWatch_OnLog;        
        POIWatch.OnInvalidPathpoint += POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged += POIWatch_OnUserLocationChanged;
        POIWatch.OnPOITargetChange += POIWatch_OnPOITargetChange;

        DecisionInstruction.OnNavInstructionUsed += DecisionInstruction_OnNavInstructionUsed;
        SafetyInstruction.OnNavInstructionUsed += SafetyInstruction_OnNavInstructionUsed;
        OfftrackInstruction.OnRecoveryInstructionUsed += OfftrackInstruction_OnRecoveryInstructionUsed;
        GoToInstruction.OnInstructionHideChange += GoToInstruction_OnInstructionHideChange;
        GoToInstruction.OnAdaptationEventChange += GoToInstruction_OnAdaptationEventChange;
        DecisionInstruction.OnAdaptationEventChange += GoToInstruction_OnAdaptationEventChange;

        SharedData = RouteSharedData.Instance;
        SharedData.OnDataDownloaded -= RouteSharedData_OnDataDownloaded;
        SharedData.OnDataDownloaded += RouteSharedData_OnDataDownloaded;


        GoToInstruction.OnTaskCompleted.RemoveListener(LoadPOIInstruction);
        DecisionInstruction.OnTaskCompleted.RemoveListener(LoadGoToInstruction);
        SafetyInstruction.OnTaskCompleted.RemoveListener(LoadGoToInstruction);

        GoToInstruction.OnTaskCompleted.AddListener(LoadPOIInstruction);
        DecisionInstruction.OnTaskCompleted.AddListener(LoadGoToInstruction);
        SafetyInstruction.OnTaskCompleted.AddListener(LoadGoToInstruction);
       

        WalkEventHandler = new ();
    }


    void OnDestroy()
    {
        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnOffTrackEnd -= POIWatch_OnOffTrackEnd;
        POIWatch.OnDecisionStart -= POIWatch_OnDecisionStart;
        POIWatch.OnDecisionEnd -= POIWatch_OnDecisionEnd;
        POIWatch.OnSegmentStart -= POIWatch_OnSegmentStart;
        POIWatch.OnSegmentEnd -= POIWatch_OnSegmentEnd;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange -= POIWatch_OnWalkStatusChange;
        POIWatch.OnLog -= POIWatch_OnLog;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;
        POIWatch.OnPOITargetChange -= POIWatch_OnPOITargetChange;

        DecisionInstruction.OnNavInstructionUsed -= DecisionInstruction_OnNavInstructionUsed;
        SafetyInstruction.OnNavInstructionUsed -= SafetyInstruction_OnNavInstructionUsed;
        OfftrackInstruction.OnRecoveryInstructionUsed -= OfftrackInstruction_OnRecoveryInstructionUsed;
        GoToInstruction.OnInstructionHideChange -= GoToInstruction_OnInstructionHideChange;
        GoToInstruction.OnAdaptationEventChange -= GoToInstruction_OnAdaptationEventChange;
        DecisionInstruction.OnAdaptationEventChange -= GoToInstruction_OnAdaptationEventChange;

        SharedData.OnDataDownloaded -= RouteSharedData_OnDataDownloaded;

        GoToInstruction.OnTaskCompleted.RemoveListener(LoadPOIInstruction);
        DecisionInstruction.OnTaskCompleted.RemoveListener(LoadGoToInstruction);
        SafetyInstruction.OnTaskCompleted.RemoveListener(LoadGoToInstruction);

    }

    // we track instructions used by the user during the navigation, and log them
    private void DecisionInstruction_OnNavInstructionUsed(object sender, EventArgs<RouteWalkEventLog.NavInstructionType> e)
    {
        WalkEventHandler.AddUIEvent_NavInstructionUsed(e.Data);
    }

    private void SafetyInstruction_OnNavInstructionUsed(object sender, EventArgs<RouteWalkEventLog.NavInstructionType> e)
    {
        WalkEventHandler.AddUIEvent_NavInstructionUsed(e.Data);
    }

    private void OfftrackInstruction_OnRecoveryInstructionUsed(object sender, EventArgs<RouteWalkEventLog.RecoveryInstructionType> e)
    {
        WalkEventHandler.AddUIEvent_RecoveryInstructionUsed(e.Data);
    }

    private void GoToInstruction_OnInstructionHideChange(object sender, EventArgs<(bool IsHidden, bool WasAwakenByUser)> e)
    {
        InstructionSleepStatusArgs args = POIWatch.GetBaseInstructionSleepStats();
        args.IsSleeping = e.Data.IsHidden;
        args.WasAwakenByUser = e.Data.WasAwakenByUser;
        WalkEventHandler.ProcessInstructionSleepStatusChange(args);
    }

    private void GoToInstruction_OnAdaptationEventChange(object sender, EventArgs<AdaptationTaskArgs> e)
    {
        AdaptationTaskArgs args = POIWatch.GetBaseAdaptationTaskStats(e.Data);
        WalkEventHandler.ProcessAdaptationTaskStatusChange(args);
    }

    private void POIWatch_OnPOITargetChange(object sender, EventArgs<Pathpoint> e)
    {
        ReLoadProgressBar();
        Debug.Log("Target changed: " + e.Data.Id);
    }

    private void POIWatch_OnUserLocationChanged(object sender, LocationChangedArgs e)
    {
        if (!RouteTracking.RunSimulation || RouteTracking.SaveSimulatedWalk) {
            CurrentRouteWalk.WalkCompletionPercentage = (float)e.SegmentInfo.ClosestSegmentIndex / (float)SharedData.PathpointList.Count;
            CurrentRouteWalk.EndDateTime = DateTime.Now;
            CurrentRouteWalk.InsertDirty();
        }
        
    }

    private void POIWatch_OnDecisionEnd(object sender, DecisionArgs e)
    {
        WalkEventHandler.ProcessDecisionStatusChange(e);

        // Are we currently muting?
        if (POIWatch.IsCurrentPOIMuted())
        {
            AdaptationTaskArgs args = new AdaptationTaskArgs
            {
                TargetPOI = e.TargetPOI,
                AdaptationSupportMode = PathpointPIM.SupportMode.Mute,
                AdaptationIntroShown = false,
                IsTaskStart = false,
                AdaptationTaskCorrect = e.IsCorrectDecision,
                AdaptationTaskCompleted = true
            };

            WalkEventHandler.ProcessAdaptationTaskBackground(args, isPOI: true);
            Debug.Log("POIWatch_OnDecisionEnd Mute");
        }
    }

    private void POIWatch_OnDecisionStart(object sender, DecisionArgs e)
    {
        WalkEventHandler.ProcessDecisionStatusChange(e);

        // Are we currently muting?
        if (POIWatch.IsCurrentPOIMuted())
        {
            AdaptationTaskArgs args = new AdaptationTaskArgs
            {
                TargetPOI = e.TargetPOI,
                AdaptationSupportMode = PathpointPIM.SupportMode.Mute,
                AdaptationIntroShown = false,
                //AdaptPIM = e.TargetPOI.CurrentInstructionMode,
                IsTaskStart = true
            };

            WalkEventHandler.ProcessAdaptationTaskBackground(args, isPOI: true, forceLogReset: true);
            Debug.Log("POIWatch_OnDecisionStart Mute");
        }
    }

    private void POIWatch_OnSegmentEnd(object sender, SegmentCompletedArgs e)
    {
        WalkEventHandler.ProcessSegmentCompleteStatusChange(e);

        // Are we currently muting?
        if (POIWatch.IsPreviousPOIMuted()) 
        {
            AdaptationTaskArgs args = new AdaptationTaskArgs
            {
                SegPOIStartId = e.SegPOIStartId,
                SegExpectedPOIEndId = e.SegExpectedPOIEndId,
                SegReachedPOIEndId = e.SegReachedPOIEndId,
                AdaptationSupportMode = PathpointPIM.SupportMode.Mute,
                AdaptationIntroShown = false,
                IsTaskStart = false,
                AdaptationTaskCorrect = true,
                AdaptationTaskCompleted = true
            };

            WalkEventHandler.ProcessAdaptationTaskBackground(args, isPOI: false);
            Debug.Log("POIWatch_OnSegmentEnd Mute");
        }
    }

    private void POIWatch_OnSegmentStart(object sender, SegmentCompletedArgs e)
    {
        WalkEventHandler.ProcessSegmentCompleteStatusChange(e);

        // Are we currently muting?
        if (POIWatch.IsCurrentPOIMuted())
        {
            AdaptationTaskArgs args = new AdaptationTaskArgs
            {
                SegPOIStartId = e.SegPOIStartId,
                SegExpectedPOIEndId = e.SegExpectedPOIEndId,
                AdaptationSupportMode = PathpointPIM.SupportMode.Mute,
                AdaptationIntroShown = false,
                //AdaptationPIMId = POIWatch.GetCurrentTargetPathpoint().PathpointPIMId,
                IsTaskStart = true
            };

            WalkEventHandler.ProcessAdaptationTaskBackground(args, isPOI: false, forceLogReset: true);
            Debug.Log("POIWatch_OnSegmentStart Mute");
        }
    }

    private void POIWatch_OnInvalidPathpoint(object sender, PathpointInvalidArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnInvalidPathpoint Accuracy: {e.Accuracy} Speed: {e.Speed}");
        Debug.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnInvalidPathpoint Accuracy: {e.Accuracy} Speed: {e.Speed}");

        SendPathpointTrace(e.Point, POIWatcher.POIState.Invalid);
    }

    private void POIWatch_OnLog(object sender, EventArgs<string> e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnLog: {e.Data}");
    }

    private void POIWatch_OnWalkStatusChange(object sender, WalkingStatusArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnWalkStatusChange IsWalking: {e.IsWalking} Pace: {e.Pace}");
        Debug.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnWalkStatusChange IsWalking: {e.IsWalking} Pace: {e.Pace}");

        WalkEventHandler.ProcessWalkStatusChange(e);
    }

    private void POIWatch_OnArrived(object sender, POIArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnArrived distance {e.Distance}");

        CurrentStatus = TrainingStatus.ARRIVED;

        SaveRouteWalk(RouteWalk.RouteWalkCompletion.Completed, true);

        LoadArrived();

        HapticUtils.VibrateForNudge();

        // We save the local route walk data
        //UploadLocalRouteWalk();
    }

    private void POIWatch_OnAlongTrack(object sender, ValidationArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnAlongTrack {e.SegmentInfo}");
        //Wayfind.LoadOnTrack();
        //LoadInstruction(POIWatch.CurrentPOIIndex);

        if (POIWatch.GetPreviousState() == POIWatcher.POIState.OffTrack) {
            // We are on track now, so we can show the 'Next POI' instruction again
            AuralInstruction.PlayEffectOnTrack();
            HapticUtils.VibrateForNotification();

            LoadGoToInstruction();

            // We update the progress bar to reflect in what segment we ended up 
            ReLoadProgressBar();
            
        }
        else if (POIWatch.GetPreviousState() == POIWatcher.POIState.LeftPOI)
        {
            LoadPOIConfirmation();

            //TODO: Move sounds to instructions
            //AuralInstruction.PlayEffectLeavePOI();
            //HapticUtils.VibrateForNudge();
        }

        Debug.Log($"POIWatch_OnAlongTrack (prev {POIWatch.GetPreviousState()})");        
    }

    private void POIWatch_OnOffTrack(object sender, ValidationArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnOffTrack {e.SegmentInfo} issue: {e.Issue}");
        LoadOffTrack(e.Issue);

        WalkEventHandler.ProcessOfftrackStatusChange(e);

        Debug.Log($"POIWatch_OnOffTrack {e.Issue}");

        HapticUtils.VibrateForAlert();

        SpeechUtils.Speak("You are off-track!");

        WalkEventHandler.AddUIEvent_OfftrackShown();
    }

    private void POIWatch_OnOffTrackEnd(object sender, ValidationArgs e)
    {        
        WalkEventHandler.ProcessOfftrackStatusChange(e);
    }

    private void POIWatch_OnLeftPOI(object sender, POIArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnLeftPOI distance {e.Distance}");
        // Let's watch out for the next POI along the route

        //TODO: Modify next POI to search for that without mute.
        POIWatch.NextPOI();
        // Let's load the wayfinding instructions for the next
        //LoadGoToInstruction(POIWatch.CurrentPOIIndex);

        //CHANGED: We wait until the OnAlongTrack to confirm
        //LoadPOIConfirmation();

        Debug.Log("POIWatch_OnLeftPOI");
        

        //ReLoadProgressBar();
    }

    private void POIWatch_OnEnteredPOI(object sender, POIArgs e)
    {
        //Confirm that we are finally here
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnEnteredPOI distance {e.Distance}");

        // Are we getting back from an Off-track situation?
        if (POIWatch.GetPreviousState() == POIWatcher.POIState.OffTrack)
        {
            // We are on track now, so we can show the 'Next POI' instruction again
            AuralInstruction.PlayEffectOnTrack();
            HapticUtils.VibrateForNotification();

            // We update the progress bar to reflect in what segment we ended up 
            ReLoadProgressBar();

            LoadPOIInstruction();

        }
        else if (POIWatch.CurrentPOIIndex != 0)
        {
            LoadGoToConfirmation();
        }
        else
        {
            LoadPOIInstruction();
        }

        
        Debug.Log("POIWatch_OnEnteredPOI");

        SpeechUtils.Speak("Careful, look at the instructions!");
    }


    private void SendPathpointTrace(Pathpoint pathpoint, POIWatcher.POIState eventType = POIWatcher.POIState.None)
    {
        PathpointTraceMessage message = new PathpointTraceMessage();
        message.seq = POIWatch.GetLocationIndex();
        message.pathpoint = pathpoint.ToAPI();
        if (RouteTracking.RunSimulation) RealTimeAPI.SendPathpointTrace(message, eventType.ToString());
    }

    private void RouteSharedData_OnDataDownloaded(object sender, EventArgs e)
    {
        ShowMainPanel(StartPanel);
        LoadTrainingComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTraining()
    {
        // Download definition
        SharedData.DownloadRouteDefinition();

        // We save any pending route walk
        UploadCachedRouteWalk();

        // We initialite the clock
        clock = new RealtimeClock();

        CurrentStatus = TrainingStatus.WAIT_START;

        PausePanel.SetActive(false);
        HelpPanel.SetActive(false);
    }

    public void PauseTraining(bool doPause)
    {
        CurrentStatus = doPause? TrainingStatus.PAUSED : TrainingStatus.TRAINING;
        PausePanel.SetActive(doPause);
        GoToInstruction.CancelHideSupport();

        GPSTrackingStatus.UpdateGPSStatus(TrackingStatus.RunningStatus.Inactive, 0);

        // Log pause
        var pauseStats = POIWatch.GetBasePauseStats();
        pauseStats.IsPaused = doPause;
        WalkEventHandler.ProcessPauseStatusChange(pauseStats);
    }

    public void CancelTraining()
    {
        // Did we start the route?
        if (CurrentRouteWalk != null)
        {
            InterruptLogging();

            CurrentStatus = TrainingStatus.CANCELED;
            SaveRouteWalk(RouteWalk.RouteWalkCompletion.Cancelled, true);
        }
        
        //TODO: Load the canceled view
        ShowMainPanel(CompletePanel);
        EndInstruction.LoadCancelled();
    }

    public void PromptAskHelp(bool doAsk)
    {
        HelpPanel.SetActive(doAsk);
        GoToInstruction.CancelHideSupport();
    }

    public void LoadTrainingComponents()
    {
        SharedData.LoadRouteFromDatabase();

        LoadRoadSafety();

        LoadStartingPoint();

        LoadLocationTracking();

        POIWatch.LoadTargetPOIs(SharedData.POIList);

        // delete later
        if (RouteTracking.RunSimulation) {
            ShowMainPanel(StartPanel);
            StartInstruction.EnableStartTraining();
        }

        DataInitialised = true;

    }

    public void ConfirmUserReady()
    {
        ShowMainPanel(TrainingPanel);
        if (!RouteTracking.RunSimulation || RouteTracking.SaveSimulatedWalk)
        {
            // save route walk
            var lastWalk = RouteWalk.GetWithMinId(x => x.Id);

            CurrentRouteWalk = new RouteWalk();
            CurrentRouteWalk.Id = (lastWalk != null ? lastWalk.Id : 0) - 1;
            CurrentRouteWalk.StartDateTime = DateTime.Now;
            CurrentRouteWalk.RouteId = SharedData.CurrentRoute.Id;
            CurrentRouteWalk.InsertDirty();
        }
        else
        {
            WalkEventHandler.DisableLogging = true;
        }
        
        Debug.Log("Walk Event Logging Disable? : " + WalkEventHandler.DisableLogging);

        // last used local log id
        var lastLog = PathpointLog.GetWithMinId(x => x.Id);
        lastMinPathlogId = (lastLog != null ? lastLog.Id : 0);

        CurrentStatus = TrainingStatus.TRAINING;

        // We setup the first segment in the progress bar
        var poiStartIndex = 0;
        var poiEndIndex = FindPathointIndexById(SharedData.POIList[1].Id);
        RouteProgressBar.Initialise(poiStartIndex, poiEndIndex);

        // We put as first goal the second POI, as we are already in the first one (start POI)
        POIWatch.SetStartingPoint(1, POIWatcher.POIState.AtStart);

        AuralInstruction.CancelCurrentPlayback();
        LoadGoToInstruction();

    }

    public void OnLocationChanged(Pathpoint pathpoint)
    {

        GPSTrackingStatus.UpdateGPSStatus(TrackingStatus.RunningStatus.Active, (float)pathpoint.Accuracy);

        if (CurrentStatus == TrainingStatus.TRAINING)
        {
            // Let's initialise the pathpoint log, and inform the
            // walk event manager about it
            //TODO: This is apparently costly, let's just get the last log
            // when the user cºonfirms, and then just keep track of the last id
            // using it incrementally
            PathpointLog CurrentLog = new PathpointLog(pathpoint);
            //var lastLog = PathpointLog.GetWithMinId(x => x.Id);
            //CurrentLog.Id = (lastLog != null ? lastLog.Id : 0) - 1;
            CurrentLog.Id = lastMinPathlogId - 1;
            lastMinPathlogId = CurrentLog.Id;

            WalkEventHandler.SetCurrentPathpointLog(CurrentLog);
            WalkEventHandler.SetCurrentRouteWalk(CurrentRouteWalk); // move out to avoid repeaded calls

            // We process the current location with POI Watch, which identify
            // navigation events. Some statistics are returned in the log
            PathpointLog log = POIWatch.UpdateUserLocation(pathpoint);
            if (log != null && (!RouteTracking.RunSimulation || RouteTracking.SaveSimulatedWalk))
            {
                log.Id = CurrentLog.Id;
                log.RouteWalkId = CurrentRouteWalk.Id;
                // Store raw
                log.Latitude = CurrentLog.Latitude;
                log.Longitude = CurrentLog.Longitude;
                log.Timestamp = CurrentLog.Timestamp;
                log.Accuracy = CurrentLog.Accuracy;
                log.Altitude = CurrentLog.Altitude;
                log.TotalWalkingTime = clock.timestamp;
                // End Store raw
                log.InsertDirty();
            }

            if (log != null)
            {
                var point = new Pathpoint
                {
                    Latitude = log.Latitude,
                    Longitude = log.Longitude,
                    Timestamp = log.Timestamp,
                    Accuracy = log.Accuracy,
                    Altitude = log.Altitude
                };
                //var point = new Pathpoint
                //{
                //    Latitude = pathpoint.Latitude,
                //    Longitude = pathpoint.Longitude,
                //    Timestamp = pathpoint.Timestamp,
                //    Accuracy = pathpoint.Accuracy,
                //    Altitude = pathpoint.Altitude
                //};

                SendPathpointTrace(point, POIWatch.GetCurrentState());
            }

            //DebugMetrics(pathpoint);

        }
        else if (CurrentStatus == TrainingStatus.WAIT_START ||
                 CurrentStatus == TrainingStatus.WAIT_CONFIRM)
        {
            if (DataInitialised && POIWatch.IsUserAtStartingPoint(pathpoint))
            {
                // if we are already waiting for confirmation, we don't need
                // to re-render
                if (CurrentStatus != TrainingStatus.WAIT_CONFIRM)
                {
                    ShowMainPanel(StartPanel);
                    StartInstruction.EnableStartTraining();
                    AuralInstruction.PlayEffectEnterPOI();
                    AuralInstruction.PlayStartArrived();
                    CurrentStatus = TrainingStatus.WAIT_CONFIRM;
                }                
            }
            // else : what happens if we leave the starting point again?
        }



    }

    public void DebugMetrics(Pathpoint pathpoint)
    {
        if (CurrentStatus == TrainingStatus.TRAINING && GPSDebugDisplay != null)
        {
            GPSDebugDisplay.DisplayGPS(pathpoint);
            GPSDebugDisplay.Ping();

            var distance = LocationUtils.HaversineDistance(pathpoint, SharedData.POIList[POIWatch.CurrentPOIIndex]);
            var segmentInfo = RouteValidation.CalculateMinDistanceAndBearing(pathpoint);

            GPSDebugDisplay.DisplayMetrics(distance, segmentInfo);
        }
    }

    // uui management functions

    public void ShowStartPanel()
    {
        ShowMainPanel(StartPanel);
    }

    public void ShowTrainingPanel()
    {
        ShowMainPanel(TrainingPanel);
    }

    public void ShowCompletePanel()
    {
        ShowMainPanel(CompletePanel);
    }

    // Managing the diffeent types of instructions

    private void LoadGoToInstruction()
    {
        // The GoTo message has a timed delayed (timed button), so we make sure
        // we haven't arrived by the time we tell the user to go to the next POI
        // This could happen with close by POIs, or low GPS accuracy
        if (POIWatch.GetCurrentState() == POIWatcher.POIState.Arrived)
        {
            return;
        }

        var item = SharedData.PreparePOIData(POIWatch.CurrentPOIIndex);
        ShowInstructionPanel(GoToInstruction.gameObject);
        GoToInstruction.LoadInstruction(SharedData.CurrentWay, item);

        ShowProgressBar(true);
        ReLoadProgressBar();

        //AuralInstruction.PlayGotoInstruction(item);
    }

    private void LoadGoToConfirmation()
    {
        // Ok, was the challenge, if any, accepted?
        if (POIWatch.IsCurrentPOIMuted())
        {
            // we cancel the muting,
            GoToInstruction.CancelIfChalengeNotAccepted();
        }

        // no we check again, and confirm only the actual goto completion
        if (!POIWatch.IsCurrentPOIMuted()) { 

            // If the POIs are too close to each other, you might leave and enter directly the next
            // POI. For consistency, let's load the GOTo in this case, so as to get confirmation
            if (POIWatch.GetPreviousState() == POIWatcher.POIState.LeftPOI)
            {
                var item = SharedData.PreparePOIData(POIWatch.CurrentPOIIndex);
                GoToInstruction.LoadInstruction(SharedData.CurrentWay, item); // here we select the upcoming not muted one
                ShowInstructionPanel(GoToInstruction.gameObject);

                ReLoadProgressBar();
            }
            GoToInstruction.CancelHideSupport();

            AuralInstruction.PlayEffectEnterPOI();
            GoToInstruction.LoadInstructionConfirmation();       // Make sure we load the right POI
        }
    }

    private void LoadPOIInstruction()
    {
        if (!POIWatch.IsCurrentPOIMuted())
        {
            // If we are in the first landmark, the OnLeftPOI was not triggered (there was no previous POI)
            // so let's load the instruction photos as well, not only the directions
            var item = SharedData.PreparePOIData(POIWatch.CurrentPOIIndex);
            Way way = SharedData.CurrentWay;

            ShowProgressBar(false);

            if (item.POIType == Pathpoint.POIsType.Landmark)
            {
                ShowInstructionPanel(DecisionInstruction.gameObject);
                //// test
                //item.CurrentInstructionMode = new InstructionMode
                //{
                //    IsNewToUser = true,
                //    Mode = InstructionMode.SupportMode.ChallengeDecision
                //};

                DecisionInstruction.LoadInstruction(way, item);
                //AuralInstruction.PlayNavInstruction(item);
            }
            else if (item.POIType == Pathpoint.POIsType.Reassurance)
            {
                ShowInstructionPanel(SafetyInstruction.gameObject);
                SafetyInstruction.LoadInstruction(item);
                AuralInstruction.PlaySafetyPointInstruction(item);
            }
            else if (item.POIType == Pathpoint.POIsType.WayStart)
            {
                ShowInstructionPanel(DecisionInstruction.gameObject);
                DecisionInstruction.LoadInstruction(way, item);
                AuralInstruction.PlayStartInstruction(item);
            }

            //Wayfind.LoadInstructionDirection(POIWatch.CurrentPOIIndex == 0);

            HapticUtils.VibrateForNudge();
        }
    }

    private void LoadPOIConfirmation()
    {
        if (!POIWatch.WasPreviousMuted)
        {
            if (DecisionInstruction.gameObject.activeSelf)
            {
                DecisionInstruction.LoadInstructionConfirmation();
            }
            else
            {
                SafetyInstruction.LoadInstructionConfirmation();
            }

            AuralInstruction.PlayEffectLeavePOI();
            HapticUtils.VibrateForNudge();
        
        }

    }

    private void LoadStartingPoint()
    {
        var item = SharedData.PreparePOIData(0);
        StartInstruction.LoadStart(SharedData.CurrentWay, item);

        AuralInstruction.PlayStartInstruction(item);
    }

    private void LoadArrived()
    {        
        ShowMainPanel(CompletePanel);
        var item = SharedData.PreparePOIData(POIWatch.CurrentPOIIndex);

        GoToInstruction.InformSegmentCompletedSilently();        

        EndInstruction.LoadCompleted(SharedData.CurrentWay, item);
        AuralInstruction.PlayArrivedInstruction(item);
        AuralInstruction.PlayEffectArrived();

    }

    private void LoadOffTrack(LocationUtils.NavigationIssue issue)
    {
        GoToInstruction.CancelHideSupport();

        ShowInstructionPanel(OfftrackInstruction.gameObject);
        // off track audio instruction should have the reason
        AuralInstruction.CancelCurrentPlayback();
        AuralInstruction.PlayEffectOffTrack();
        AuralInstruction.PlayEffectOffTrack();
        AuralInstruction.PlayOfftrackInstruction(issue);        

        OfftrackInstruction.LoadOfftrack(issue);        
    }

    private void ReLoadProgressBar()
    {
        // progres bar
        var targetPOI = POIWatch.GetUpcomingNonMutedTarget();
        ReloadProgressBar(SharedData.POIList[POIWatch.CurrentPOIIndex - 1], targetPOI);
    }

    private void ShowProgressBar(bool show)
    {
        RouteProgressBar.gameObject.SetActive(show);
    }

    private void ReloadProgressBar(Pathpoint p1, Pathpoint p2)
    {
        var poiStartIndex = FindPathointIndexById(p1.Id);
        var poiEndIndex = FindPathointIndexById(p2.Id);

        RouteProgressBar.Initialise(poiStartIndex, poiEndIndex);

        Debug.Log($"ReloadProgressBar: {poiStartIndex} --- {poiEndIndex}");
    }


    // additional utilities

    private void SaveRouteWalk(RouteWalk.RouteWalkCompletion completion, bool upload )
    {
        if (!RouteTracking.RunSimulation || RouteTracking.SaveSimulatedWalk)
        {
            CurrentRouteWalk.WalkCompletion = completion;
            CurrentRouteWalk.EndDateTime = DateTime.Now;
            CurrentRouteWalk.InsertDirty();

            if (upload)
            {
                // We save the local route walk data
                UploadLocalRouteWalk();
            }
        }
        
    }


    private void LoadRoadSafety()
    {
        RouteValidation.LoadRoutePathpoints(SharedData.PathpointList);
    }

    private void LoadLocationTracking()
    {
        RouteTracking.StartTracking(SharedData.CurrentRoute);     
    }

    private void UploadLocalRouteWalk()
    {
        if (UploadPerformedRouteWalkEnabled)
        {
            RouteWalkLog.UploadPerformedRouteWalk(SharedData.CurrentRoute, CurrentRouteWalk);
        }

    }

    private void UploadCachedRouteWalk()
    {
        if (UploadChachedRouteWalksEnabled)
        {
            RouteWalkLog.UploadRouteWalksForRoute(SharedData.CurrentRoute);
        }
    }

    private int FindPathointIndexById(int pid)
    {
        return SharedData.PathpointList.FindIndex(p => p.Id == pid);
    }

    private void ShowInstructionPanel(GameObject obj)
    {        
        StartInstruction.gameObject.SetActive(StartInstruction.gameObject == obj);
        GoToInstruction.gameObject.SetActive(GoToInstruction.gameObject == obj);
        DecisionInstruction.gameObject.SetActive(DecisionInstruction.gameObject == obj);
        SafetyInstruction.gameObject.SetActive(SafetyInstruction.gameObject == obj);
        OfftrackInstruction.gameObject.SetActive(OfftrackInstruction.gameObject == obj);

        if (GoToInstruction.gameObject != obj)
        {
            GoToInstruction.CleanUpView();
        }
        if (DecisionInstruction.gameObject != obj)
        {
            DecisionInstruction.CleanUpView();
        }
        if (StartInstruction.gameObject != obj)
        {
            StartInstruction.CleanUpView();
        }
        if (SafetyInstruction.gameObject != obj)
        {
            SafetyInstruction.CleanUpView();
        }

    }

    private void ShowMainPanel(GameObject obj)
    {
        LoadingPanel.SetActive(LoadingPanel == obj);
        StartPanel.SetActive(StartPanel == obj);
        TrainingPanel.SetActive(TrainingPanel == obj);
        CompletePanel.SetActive(CompletePanel == obj);
    }

    private void InterruptLogging()
    {
        (ValidationArgs OfftrackStats, DecisionArgs DecisionStats, SegmentCompletedArgs SegmentStats) = POIWatch.GetNavigationStats();
        WalkEventHandler.InterruptDecision(DecisionStats);
        WalkEventHandler.InterruptOfftrack(OfftrackStats);
        WalkEventHandler.InterruptSegmentComplete(SegmentStats);
        WalkEventHandler.InterruptWalkStatus();
    }

    // Terminate correctly the route.
    private void OnApplicationQuit()
    {

        if (RouteTracking.RunSimulation && !RouteTracking.SaveSimulatedWalk) return;

        if (CurrentRouteWalk.WalkCompletion != RouteWalk.RouteWalkCompletion.Completed)
        {
            SaveRouteWalk(RouteWalk.RouteWalkCompletion.Cancelled, false);

            // We close all open events (if any) as interrupted
            InterruptLogging();
        }

    }
}


