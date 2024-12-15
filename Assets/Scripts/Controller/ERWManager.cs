using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;

public class ERWManager : MonoBehaviour
{

    [Header(@"Main Components")]
    public PhoneCam CameraService;
    public RouteLocationService LocationService;
    public PermissionManager PermissionService;
    public RouteEditItem RoutePreview;

    [Header(@"Main Panels")]
    public GameObject PermissionPanel;
    public GameObject RecordingPanel;

    [Header(@"Recording Panels")]
    public GameObject StepGoTo;
    public GameObject StepStartPic;
    public GameObject StepRecording;
    public GameObject StepArrivedConfirm;
    public GameObject StepEndPic;
    public GameObject StepArrived;

    [Header(@"Recording Dialogs")]
    public GameObject DialogContainer;
    public GameObject DialogPause;
    public GameObject DialogExit;
    public GameObject DialogError;

    [Header(@"Utilities")]
    public ERWAudioInstruction AudioInstruction;


    private Pathpoint StartPOI;
    private Pathpoint EndPOI;
    private LocalFolder.ERWFolder RouteFolder;

    private void Awake()
    {
        RouteFolder = new LocalFolder.ERWFolder(AppState.ERW.CurrentRoute);
    }

    void Start()
    {
        PermissionPanel.SetActive(true);
    }

    #region Main Route Recording Actions

    /// <summary>
    /// Start the recording wizard, initialising the camera and location services
    /// </summary>
    public void InitialiseRecordingWizard(){
        CameraService.StartCamera();
        LocationService.InitialiseTracking();

        ShowMainView(RecordingPanel);
        ShowRecordingStep(StepGoTo);

        AudioInstruction.PlayGoToStartingPoint();
    }

    /// <summary>
    /// Show instructions to take picture of the starting point
    /// </summary>
    public void ShowPicStartingPoint(){
        ShowRecordingStep(StepStartPic);
        AudioInstruction.PlayAtStartingPoint();
    }    

    /// <summary>
    /// Start the route recording, including the path tracking
    /// </summary>
    public void StartRouteRecording(){
        ShowRecordingStep(StepRecording);
        CameraService.StartRecording();
        LocationService.StartTracking();

        AudioInstruction.PlayRecordingStarts();
    }

  

    /// <summary>
    /// End the route recording, stopping the camera and location services
    /// </summary>
    public void EndRouteRecording(){
        ShowPicEndPoint();
        LocationService.Stop();
        CameraService.StopRecording();
    }        

    /// <summary>
    /// Show instructions to take picture of the end point
    /// </summary>
    public void ShowPicEndPoint(){
        ShowRecordingStep(StepEndPic);
        AudioInstruction.PlayAtDestination();
    }    

    /// <summary>
    /// Show the arrived summary, including the route preview
    /// </summary>
    public void ShowArrivedSummary(){
        ShowRecordingStep(StepArrived);
        RoutePreview.LoadWay(AppState.ERW.CurrentWay);
        AudioInstruction.PlayRecordingComplete();
    }

    /// <summary>
    /// Cancel the route recording, deleting all the data
    /// </summary>
    public void CancelRouteRecording(){
        CameraService.CancelRecording();

        // We delete the route and all its pathpoints
        Route.Delete(AppState.ERW.CurrentRoute.Id);
        Pathpoint.DeleteFromRoute(AppState.ERW.CurrentRoute.Id, null, null);

        // We delete the way if it's local
        var way = Way.Get(AppState.ERW.CurrentWay.Id);
        if (!way.FromAPI)
        {
            Way.Delete(way.Id);
        }        
    }

    /// <summary>
    /// Pause or resume the route recording
    /// </summary>
    /// <param name="pause"></param>
    public void PauseRouteRecording(bool pause){    
        CameraService.TogglePause();
        if (pause){
            ShowRecordingDialog(DialogPause);
        }
        else {
            ShowRecordingDialog(null);
        }
    }   

    /// <summary>
    /// Triggers a fatal error, stopping the camera and location services
    /// </summary>
    public void TriggerFatalError(){
        AppLogger.Instance.LogFromMethod(this.name, "TriggerFatalError", "User triggered a fatal error");
        LocationService.Stop();
        CameraService.StopRecording();
        ShowRecordingDialog(DialogError);
    }

    #endregion    


    /// <summary>
    /// Show the user exit dialog, asking the user to confirm the exit
    /// </summary>
    public void ShowExitDialog(){
        ShowRecordingDialog(DialogExit);
    }

    /// <summary>
    /// Close the current dialog
    /// </summary>
    public void CloseDialog(){
        ShowRecordingDialog(null);
    }   

    /// <summary>
    /// Take a picture of the current POI
    /// </summary>
    public void TakePOIPicture(){
        LocationService.MarkPOIPhoto();
    }     

    /// <summary>
    /// Take a picture of the start landmark, or replace the existing one
    /// </summary>
    /// <returns>Saved Pathpoint and full path of the picture taken</returns>
    public (Pathpoint poi, string fullPath) TakeLandmarkPictureStart(UnityAction<bool, Pathpoint, string> onPictureReady){
        
        AppLogger.Instance.LogFromMethod(this.name, "TakeLandmarkPictureStart", "User took a landmark picture");
        try
        {
            var result = TakeLandmarkPicture(Pathpoint.POIsType.WayStart, StartPOI, onPictureReady);
            StartPOI = result.poi;
            
            AppLogger.Instance.LogFromMethod(this.name, "TakeLandmarkPictureStart", $"Pathpoint inserted [{result.poi.Latitude} {result.poi.Longitude} {result.poi.Accuracy}] ts {result.poi.Timestamp}");
            return result;
        }
        catch (Exception e)
        {
            CameraService.FatalError("TakeLandmarkPictureStart", "Error taking geo-located photo for the POI.", e, className: this.name);
        }

        return (null, null);
    }

    /// <summary>
    /// Take a picture of the end landmark, or replace the existing one
    /// </summary>
    /// <returns>Saved Pathpoint and full path of the picture taken</returns>
    /// <param name="onPictureReady"></param>
    /// <returns></returns>
    public (Pathpoint poi, string fullPath) TakeLandmarkPictureEnd(UnityAction<bool, Pathpoint, string> onPictureReady){
        AppLogger.Instance.LogFromMethod(this.name, "TakeLandmarkPictureEnd", "User took a landmark picture");
        try
        {
            var result =  TakeLandmarkPicture(Pathpoint.POIsType.WayDestination, EndPOI, onPictureReady);
            EndPOI = result.poi;
            
            AppLogger.Instance.LogFromMethod(this.name, "TakeLandmarkPictureEnd", $"Pathpoint inserted [{result.poi.Latitude} {result.poi.Longitude} {result.poi.Accuracy}] ts {result.poi.Timestamp}");
            
            return result;
        }
        catch (Exception e)
        {
            CameraService.FatalError("TakeLandmarkPictureEnd", "Error taking geo-located photo for the POI.", e, className: this.name);
        }

        return (null, null);
    }


    /// <summary>
    /// Take a picture of the current POI
    /// </summary>
    /// <param name="poiType"></param>
    /// <param name="previousPOI"></param>
    /// <param name="onPictureReady"></param>
    /// <returns></returns>
    private (Pathpoint poi, string fullPath) TakeLandmarkPicture(Pathpoint.POIsType poiType, Pathpoint previousPOI, UnityAction<bool, Pathpoint, string> onPictureReady){
        Pathpoint poi = LocationService.GetCurrentPathpoint();
        poi.POIType = poiType;
        
        // Set the photo filename
        DateTime currentTime = DateTimeOffset.FromUnixTimeMilliseconds(poi.Timestamp).LocalDateTime;
        poi.PhotoFilename = "recording_" + currentTime.ToString("yyyy_MM_dd_H_mm_ss_FF") + ".jpg";
        poi.TimeInVideo = CameraService.GetCurrentPlaybackTimeSeconds();

        // If there is a previous POI, we replace it and delete the previous
        if (previousPOI != null)
        {
            poi.Id = previousPOI.Id;
            RouteFolder.DeletePicture(previousPOI.PhotoFilename);
        }
        poi.InsertDirty();
    
        //Get the full path of the picture we will take       
        string fullPath = RouteFolder.GetPicturePath(poi.PhotoFilename);    

        // Add a one time listener to inform of th epicture taken
        UnityAction<bool> listener = null;
        listener = (success) =>
        {
            onPictureReady?.Invoke(success, poi, fullPath);            
            CameraService.OnPictureTakenReady.RemoveListener(listener); // Remove the listener using the defined variable
        };
        CameraService.OnPictureTakenReady.AddListener(listener);   

        CameraService.TakePicture(poi.PhotoFilename, forceBeforeRecordingStart: true); 

        return (poi, fullPath);
    }


    /// <summary>
    /// Show the given recording step
    /// </summary>
    /// <param name="step"></param>
    public void ShowRecordingStep(GameObject step){
        StepGoTo.SetActive(false);
        StepStartPic.SetActive(false);
        StepRecording.SetActive(false);
        StepArrivedConfirm.SetActive(false);
        StepEndPic.SetActive(false);
        StepArrived.SetActive(false);

        step.SetActive(true);
    }    

    /// <summary>
    /// Show the given dialog
    /// </summary>
    /// <param name="dialog">Dialog view to show, null if none</param>
    public void ShowRecordingDialog(GameObject dialog){

        DialogPause.SetActive(dialog == DialogPause);
        DialogExit.SetActive(dialog == DialogExit);
        DialogError.SetActive(dialog == DialogError);

        DialogContainer.SetActive(dialog != null);
    }

    /// <summary>
    /// Show the given main view
    /// </summary>
    /// <param name="view"></param>
    private void ShowMainView(GameObject view){
        RecordingPanel.SetActive(view == RecordingPanel);
        PermissionPanel.SetActive(view == PermissionPanel);
    }


}
