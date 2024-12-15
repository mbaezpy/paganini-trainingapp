using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NatSuite.Recorders.Clocks;
using UnityEngine;
using UnityEngine.UI;
using static PaganiniRestAPI;

public class Testing : MonoBehaviour
{


    [Header("UI Components")]
    public LocationUtilsConfig RouteValidationConfig;
    public RouteTrainingTracking RouteTracking;
    public RouteWalkSimulation WalkSimulation;

    //public GPSDebug GPSDebugDisplay;
    public SocketsAPI RealTimeAPI;
    public int SimulatedRoute = -1;

    private RouteSharedData SharedData;

    private POIWatcher POIWatch;
    private LocationUtils RouteValidation;


    public enum TrainingStatus
    {       
        WAIT_START,
        WAIT_CONFIRM,
        TRAINING,
        PAUSED,
        CANCELED,
        ARRIVED
    }

    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    // Start is called before the first frame update
    void Start()
    {
        RouteValidation = LocationUtils.Instance;
        RouteValidation.Initialise(RouteValidationConfig);

        POIWatch = POIWatcher.Instance;
        POIWatch.InitialiseWatcher(RouteValidation, RouteValidationConfig);


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

        SharedData = RouteSharedData.Instance;


        StartTraining();

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

    }

    private void POIWatch_OnUserLocationChanged(object sender, LocationChangedArgs e)
    {

        
    }

    private void POIWatch_OnDecisionEnd(object sender, DecisionArgs e)
    {

    }

    private void POIWatch_OnDecisionStart(object sender, DecisionArgs e)
    {

    }

    private void POIWatch_OnSegmentEnd(object sender, SegmentCompletedArgs e)
    {

    }

    private void POIWatch_OnSegmentStart(object sender, SegmentCompletedArgs e)
    {

    }

    private void POIWatch_OnInvalidPathpoint(object sender, PathpointInvalidArgs e)
    {        
        Debug.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnInvalidPathpoint Accuracy: {e.Accuracy} Speed: {e.Speed}");

        SendPathpointTrace(e.Point, POIWatcher.POIState.Invalid);
    }

    private void POIWatch_OnLog(object sender, EventArgs<string> e)
    {

    }

    private void POIWatch_OnWalkStatusChange(object sender, WalkingStatusArgs e)
    {        
        Debug.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnWalkStatusChange IsWalking: {e.IsWalking} Pace: {e.Pace}");

    }

    private void POIWatch_OnArrived(object sender, POIArgs e)
    {
        Debug.Log($"POIWatch_OnArrived (prev {POIWatch.GetPreviousState()})");
    }

    private void POIWatch_OnAlongTrack(object sender, ValidationArgs e)
    {
        
        //Wayfind.LoadOnTrack();
        //LoadInstruction(POIWatch.CurrentPOIIndex);


        Debug.Log($"POIWatch_OnAlongTrack (prev {POIWatch.GetPreviousState()})");        
    }

    private void POIWatch_OnOffTrack(object sender, ValidationArgs e)
    {

        Debug.Log($"POIWatch_OnOffTrack {e.Issue}");

    }

    private void POIWatch_OnOffTrackEnd(object sender, ValidationArgs e)
    {        

    }

    private void POIWatch_OnLeftPOI(object sender, POIArgs e)
    {
        // Let's watch out for the next POI along the route
        POIWatch.NextPOI();


        Debug.Log("POIWatch_OnLeftPOI");

    }

    private void POIWatch_OnEnteredPOI(object sender, POIArgs e)
    {
        
        Debug.Log("POIWatch_OnEnteredPOI");

    }


    private void SendPathpointTrace(Pathpoint pathpoint, POIWatcher.POIState eventType = POIWatcher.POIState.None)
    {
        PathpointTraceMessage message = new PathpointTraceMessage();
        message.seq = POIWatch.GetLocationIndex();
        message.pathpoint = pathpoint.ToAPI();
        if (RouteTracking.RunSimulation) RealTimeAPI.SendPathpointTrace(message, eventType.ToString());
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTraining()
    {
        // Download definition
        SharedData.CurrentRoute = Route.Get(SimulatedRoute);
        SharedData.CurrentWay = Way.Get(SharedData.CurrentRoute.WayId);
        AppState.CurrentUser = User.Get(SharedData.CurrentWay.UserId);

        SessionData.Instance.SaveData("SelectedRoute", SharedData.CurrentRoute);
      

        LoadTrainingComponents();

    }

    public void RestartTraining()
    {

        RouteValidation = LocationUtils.Instance;
        RouteValidation.Initialise(RouteValidationConfig);

        POIWatch = POIWatcher.Instance;
        POIWatch.InitialiseWatcher(RouteValidation, RouteValidationConfig);

        StartTraining();
    }


    public void LoadTrainingComponents()
    {
        SharedData.LoadRouteFromDatabase();

        LoadRoadSafety();

        LoadLocationTracking();

        POIWatch.LoadTargetPOIs(SharedData.POIList);

        ConfirmUserReady();

        WalkSimulation.gameObject.SetActive(true);
        
        RealTimeAPI.ConnectToServer();

    }


    public void ConfirmUserReady()
    {

        // We put as first goal the second POI, as we are already in the first one (start POI)
        POIWatch.SetStartingPoint(1, POIWatcher.POIState.AtStart);

    }

    public void RenderPath()
    {
        Debug.Log("RenderPath: "+ SharedData.POIList.Count);
        int i = 0;
        foreach (var point in SharedData.PathpointList)
        {

            PathpointTraceMessage message = new PathpointTraceMessage();
            message.seq = i;
            message.pathpoint = point.ToAPI();
            RealTimeAPI.SendRouteTrace(message);
            i++;
        }
            
    }

    public void OnLocationChanged(Pathpoint pathpoint)
    {


        // We process the current location with POI Watch, which identify
        // navigation events. Some statistics are returned in the log
        PathpointLog log = POIWatch.UpdateUserLocation(pathpoint);

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

            SendPathpointTrace(point, POIWatch.GetCurrentState());
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


    private int FindPathointIndexById(int pid)
    {
        return SharedData.PathpointList.FindIndex(p => p.Id == pid);
    }

}


