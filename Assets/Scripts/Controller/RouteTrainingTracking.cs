using UnityEngine;
using UnityEngine.Android;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class PathpointEvent : UnityEvent<Pathpoint>
{
}

public class RouteTrainingTracking : MonoBehaviour
{
    [Header(@"Tracking Options")]
    [SerializeField] private int DesiredAccuracy = 5;
    [SerializeField] private int UpdateDistanceInMeters = 1;
    [SerializeField] private double GpsUpdateInterval = 3; // Update interval in seconds

    [Header(@"Simulation Options")]
    public bool RunSimulation = false;
    public bool SaveSimulatedWalk = false;
    public bool SimulatedWalkWithCurrentTimestamps = false;
    [SerializeField] private int WalkSimulationId = -8;
    [SerializeField] private int SimulationUpdateInterval = 3; // Update interval in seconds


    private Boolean running = false;    
    private List<PathpointLog> walkLog;
    private Route CurrentRoute;

    public PathpointEvent OnLocationUpdated;
    public UnityEvent OnSimulationEnded;

    public void StartTracking(Route route)
    {        
        CurrentRoute = route;

        // already running?
        if (running) return;

        if (!RunSimulation)
        {
            StartCoroutine(StartLocationService());
        }
        else
        {
            StartCoroutine(SimulateWalk());
        }

    }

    public void StartSimulationTracking(int routeWalkId, int updateInternval)
    {
        // stop any running tracking
        StopAllCoroutines();
        running = false;        
        RunSimulation = true;

        // parameters
        WalkSimulationId = routeWalkId;
        SimulationUpdateInterval = updateInternval;

        StartTracking(null);
    }


    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled location");
            yield break;
        }
        Input.location.Start(DesiredAccuracy, UpdateDistanceInMeters);
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        Debug.Log($"Lat: {Input.location.lastData.latitude} Long: {Input.location.lastData.longitude}" +
                  $"Acc: {Input.location.lastData.horizontalAccuracy}");

        this.running = true;       
    }


    int simIndex = 0;
    private bool PauseSimulation = false;
    private IEnumerator SimulateWalk()
    {
        simIndex = 0;
        PauseSimulation = false;

        walkLog = PathpointLog.GetAll(l => l.RouteWalkId == WalkSimulationId)
                          .OrderBy(l => l.Timestamp)  // Order the log by timestamp
                          .ToList();
        while (simIndex < walkLog.Count)
        {

            var wl = walkLog[simIndex];
            var pathpoint = new Pathpoint
            {
                Latitude = wl.Latitude,
                Longitude = wl.Longitude,
                Timestamp = wl.Timestamp,
                Accuracy = wl.Accuracy,
                Altitude = wl.Altitude
            };

            if (SimulatedWalkWithCurrentTimestamps)
            {
                pathpoint.Timestamp = DateUtils.UTCMilliseconds();
            }

            if (!PauseSimulation)
            {
                OnLocationUpdated.Invoke(pathpoint);
                simIndex = simIndex + 1;
            }
            

            yield return new WaitForSeconds(SimulationUpdateInterval);
        }

        OnSimulationEnded?.Invoke();
    }

    private double gpsLastUpdate = 0;
    /// <summary>
    /// Updates locations every x seconds
    /// </summary>
    private void Update()
    {
        // Check if tracking is on
        if (!this.running) return;

        // Check if tracking mode is running
        if (UnityEngine.Input.location.status != LocationServiceStatus.Running) return;

        // Check if it's time for a location update based on the update interval
        if (Time.realtimeSinceStartup - gpsLastUpdate < GpsUpdateInterval) return;

        gpsLastUpdate = Time.realtimeSinceStartup;

        var pathpoint = GetCurrentPathpoint();
        if (OnLocationUpdated != null)
        {
            OnLocationUpdated.Invoke(pathpoint);
        }
    }


    /// <summary>
    /// neuen wegpunkt anlegen
    /// </summary>
    private Pathpoint GetCurrentPathpoint()
    {
        Pathpoint punkt = new Pathpoint
        {
            RouteId = CurrentRoute.Id,
            Longitude = UnityEngine.Input.location.lastData.longitude,
            Latitude = UnityEngine.Input.location.lastData.latitude,
            Altitude = UnityEngine.Input.location.lastData.altitude,
            Accuracy = UnityEngine.Input.location.lastData.horizontalAccuracy,
            POIType = Pathpoint.POIsType.Point,
            Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FromAPI = false,
            IsDirty = true,
            Description = ""
        };
        return punkt;
    }

    /// <summary>
    /// Starts the Tracking Service Again
    /// </summary>
    public void RestartTracking()
    {
        this.running = true;
    }

    /// <summary>
    /// Pause the simulation 
    /// </summary>
    public void PauseSimulationTracking(bool pause)
    {
        PauseSimulation = pause;
    }

    /// <summary>
    /// Stop service if there is no need to query location updates continuously
    /// </summary>
    public void Stop()
    {
        if (this.running == true) { 
            UnityEngine.Input.location.Stop();
            this.running = false;
        }
    }


}