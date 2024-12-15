using UnityEngine;
using UnityEngine.Android;
using System.Collections;
using System;
using UnityEngine.Events;
using static PaganiniRestAPI;

[System.Serializable]
public class LocationEvent : UnityEvent<string>
{
}

public class RouteLocationService : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Geopositon Info in
    /// </summary>
    public RecordingStatus RecordingInfo;
    public PhoneCam PhoneCam;

    [Header(@"Events")]
    public LocationEvent OnLocationTrackingError;
    public UnityEvent OnLocationTrackingStarted;

    [Header(@"Location Quality params")]
    public LocationUtilsConfig config;

    public double locQosExpectedGPSAccuracy = 15;    
    public double locQosMaxAllowedAccuracy = 30;
    public double locQosMaxAllowedSpeed = 10;
    public int locQosSlidingWindowSize = 5;

    private Boolean running = false;
    private int maxLocationTrackingAttempts = 10;


    private LocationQualityControl locationQualityControl;


    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    private void Start()
    {
        AppLogger.Instance.LogFromMethod(this.name, "Start", "Start RouteLocationService");

        WalkingDetector walkingDetector = new WalkingDetector(config);
        locationQualityControl = new LocationQualityControl(walkingDetector, config);
    }

    /// <summary>
    /// Starts tracking the device's location.
    /// </summary>
    public void InitialiseTracking()
    {
        AppLogger.Instance.LogFromMethod(this.name, "StartTracking", "Checking permissions");

        Pathpoint.DeleteFromRoute(AppState.ERW.CurrentRoute.Id, null, null);

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

#elif UNITY_IOS
        PlayerSettings.iOS.locationUsageDescription = "Details to use location";
#endif
        StartCoroutine(StartLocationService());
    }

    /// <summary>
    /// Starts the location tracking service on the background, waiting for the location service to initialise.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartLocationService()
    {
        AppLogger.Instance.LogFromMethod(this.name, "StartLocationService", "Starting location tracking");

        if (!Input.location.isEnabledByUser)
        {
            FatalError("StartLocationService", "User has not enabled location", null);
            yield break;
        }
        Input.location.Start(5, 1);

        int nTrials = 0;
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            if (nTrials > maxLocationTrackingAttempts)
            {
                FatalError("StartLocationService", $"Failed to initialise locationt tracking after {nTrials}", null);
                yield break;
            }
            AppLogger.Instance.LogFromMethod(this.name, "StartLocationService", $"Trying to initialise Status: {Input.location.status}");
            nTrials++;
            yield return new WaitForSeconds(1);
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            FatalError("StartLocationService", "Unable to determine device location", null);
            yield break;
        }
        Debug.Log("Latitude : " + Input.location.lastData.latitude);
        Debug.Log("Longitude : " + Input.location.lastData.longitude);
        Debug.Log("Altitude : " + Input.location.lastData.altitude);
        AppLogger.Instance.LogFromMethod(this.name, "StartLocationService", $"Finalised location tracking with status {Input.location.status}");

        OnLocationTrackingStarted.Invoke();
    }

    
    private double last = 0;  // last timestamp
    /// <summary>
    /// Continuously updates the GPS location and logs it to the database while recording.
    /// </summary>
    private void Update()
    {
        //check if tracking on
        if (!this.running) return;
        //check if tracking mode is running        

        if (AppState.ERW.recording && !AppState.ERW.pausedRec && UnityEngine.Input.location.status != LocationServiceStatus.Running)
        {
            // Problems with GPS
            RecordingInfo.UpdateGPSStatus(RecordingStatus.RunningStatus.Error, 0);
            FatalError("Update","Location tracking is not running.", null);
            return;
        }
        if (AppState.ERW.recording && !AppState.ERW.pausedRec && !UnityEngine.Input.location.isEnabledByUser)
        {
            // Problems with GPS
            RecordingInfo.UpdateGPSStatus(RecordingStatus.RunningStatus.Error, 0);
            FatalError("Update", "User has not enabled location tracking.", null);
            return;
        }
        else if (UnityEngine.Input.location.status != LocationServiceStatus.Running)
        {
            return;
        }

        //check if last position newer than last check
        if (UnityEngine.Input.location.lastData.timestamp == last)
        {            
            //Debug.Log($"Update: EnabledByUser?: {UnityEngine.Input.location.isEnabledByUser} Last Update: {last}  Timestamp: {Input.location.lastData.timestamp}" );
            return;
        }
        last = UnityEngine.Input.location.lastData.timestamp;

        //punkt in db schreiben
        if (AppState.ERW.recording&&!AppState.ERW.pausedRec)
        {
            var pathpoint = GetCurrentPathpoint();
            pathpoint.InsertDirty();

            RecordingInfo.UpdateGPSStatus(RecordingStatus.RunningStatus.Active, (float)pathpoint.Accuracy);
        }
    }


    /// <summary>
    /// Creates a new path point using the current GPS location.
    /// </summary>
    /// <returns>A new path point object with the current GPS location information.</returns>
    public Pathpoint GetCurrentPathpoint()
    {
        Debug.Log("GetCurrentPathpoint: Getting new Pathpoint");
        Pathpoint punkt = new Pathpoint
        {
            RouteId = AppState.ERW.CurrentRoute.Id,            
            Longitude = UnityEngine.Input.location.lastData.longitude,
            Latitude = UnityEngine.Input.location.lastData.latitude,
            Altitude = UnityEngine.Input.location.lastData.altitude,
            Accuracy = UnityEngine.Input.location.lastData.horizontalAccuracy,
            POIType = Pathpoint.POIsType.Point,
            Timestamp = DateUtils.UTCMilliseconds(),
            TimeInVideo = PhoneCam.GetCurrentPlaybackTimeSeconds(),
            FromAPI = false,
            IsDirty = true,
            Description = ""
        };
        punkt.Id = -(punkt.RouteId * 10000 + System.DateTime.Now.Minute * 1000 + System.DateTime.Now.Millisecond);


        // We normalise the incoming location to correct for GPS innacuracies
        (Pathpoint userLocation, double valAccuracy, double valSpeed) = locationQualityControl.ProcessLocation(punkt);
        if (userLocation != null)
        {
            punkt.Longitude = userLocation.Latitude;
            punkt.Longitude = userLocation.Longitude;
            punkt.Altitude = userLocation.Altitude;
        }

        return punkt;
    }

    /// <summary>
    /// Marks a point of interest (POI) on the current path with the specified POI type.
    /// </summary>
    /// <param name="poiType">The type of the point of interest to mark.</param>
    public void MarkPOI(int poiType)
    {
        Pathpoint poi = GetCurrentPathpoint();
        poi.POIType = (Pathpoint.POIsType)poiType;

        poi.InsertDirty();
    }

    /// <summary>
    /// Saves a point of interest (POI) with a photo.
    /// </summary>
    public void MarkPOIPhoto()
    {
        AppLogger.Instance.LogFromMethod(this.name, "MarkPOIPhoto", "User took a picture");
        try
        {
            Pathpoint currentLocation = GetCurrentPathpoint();
            currentLocation.POIType = Pathpoint.POIsType.Reassurance;

            DateTime currentTime = DateTimeOffset.FromUnixTimeMilliseconds(currentLocation.Timestamp).LocalDateTime;
            currentLocation.PhotoFilename = "recording_" + currentTime.ToString("yyyy_MM_dd_H_mm_ss_FF") + ".jpg";

            currentLocation.TimeInVideo = PhoneCam.GetCurrentPlaybackTimeSeconds();
            currentLocation.InsertDirty();

            AppLogger.Instance.LogFromMethod(this.name, "MarkPOIPhoto", $"Pathpoint inserted [{currentLocation.Latitude} {currentLocation.Longitude} {currentLocation.Accuracy}] ts {currentLocation.Timestamp}");

            // this will take the picture and save it to the file system, async
            PhoneCam.TakePicture(currentLocation.PhotoFilename);
        }
        catch (Exception e)
        {
            FatalError("MarkPOIPhoto", "Error taking geo-located photo for the POI.", e);
        }

    }

    /// <summary>
    /// Restarts or starts the GPS tracking service for the current route.
    /// </summary>
    public void StartTracking()
    {
        AppLogger.Instance.LogFromMethod(this.name, "RestartTracking", "Restarting/Starting GPS tracking for current route");                
        this.running = true;      
    }

    /// <summary>
    /// Stops the location tracking service.
    /// </summary>
    public void Stop()
    {
        AppLogger.Instance.LogFromMethod(this.name, "Stop", "Stopping location tracking");
        try
        {
            if (UnityEngine.Input.location.status != LocationServiceStatus.Stopped)
            {
                UnityEngine.Input.location.Stop();                                
            }
            AppLogger.Instance.LogFromMethod(this.name, "Stop", "Stopping tracking done.");
            this.running = false;
        }
        catch (Exception e)
        {
            AppLogger.Instance.LogWarning("Error stopping location tracking: " + e.Message);
        }

    }


    private void FatalError(string method, string appMessage, Exception exception)
    {
        var trace = exception != null ? exception.StackTrace : null;
        AppLogger.Instance.ErrorFromMethod(this.name, method, appMessage + " StackTrace: " + trace);

        Debug.Log(appMessage);
        if (trace != null)
            Debug.Log(trace);

        Stop();

        OnLocationTrackingError?.Invoke(appMessage);
    }

    private void OnDestroy()
    {
        Stop();
    }

}