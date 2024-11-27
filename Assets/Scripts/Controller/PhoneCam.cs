using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using System;
using UnityEngine.Events;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.IO;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif


[System.Serializable]
public class RecordingEvent : UnityEvent<string>
{
}
public class PhoneCam : MonoBehaviour
{
    [Header(@"preview")]
    public RawImage rawImage;
    public AspectRatioFitter aspectRatioFitter;

    [Header(@"Recording")]
    public int videoWidth = 640;
    public int videoHeight = 480;
    public int fps = 12;
    public int videoBitRate = 1_000_000;
    public int BestVideoWidth = 1280;
    public int BestVideoHeight = 720;
    public bool RecordPortrait = false;
    
    public bool recordMicrophone;

    [Header(@"UI Configuration")]
    public GameObject IconFrom;
    public GameObject IconTo;
    public RecordingStatus RecordingInfo;

    [Header(@"Events")]
    public RecordingEvent OnRecordingError;

    //private MP4Recorder recorder;
    private HEVCRecorder recorder;    
    private AudioInput audioInput;
    private AudioSource microphoneSource;
    private WebCamTexture webCamTexture;
    private Color32[] pixelBuffer;
    private RealtimeClock clock;

    private string RouteName;
    private Route CurrentRoute;

    private void Start()
    {
        CurrentRoute = Route.Get(AppState.SelectedBegehung);

    }

    public void StartCamera()
    {
        AppLogger.Instance.LogFromMethod(this.name, "StartCamera", "Checking permissions");

#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

#endif

        if (AppState.currentBegehung == null || AppState.currentBegehung.Trim().Length == 0)
        {
            SceneManager.LoadScene(SceneSwitcher.UserLoginScene);
        }
        else
        {
            StartCoroutine(InitialiseCamera());
        }        
    }

    private IEnumerator InitialiseCamera()
    {
        AppLogger.Instance.LogFromMethod(this.name, "InitialiseCamera", "Initialising camera.");

        Way w = SessionData.Instance.GetData<Way>("SelectedWay");
        IconFrom.GetComponent<LandmarkIcon>().SetSelectedLandmark(Int32.Parse(w.StartType));
        IconTo.GetComponent<LandmarkIcon>().SetSelectedLandmark(Int32.Parse(w.DestinationType));


        RouteName = AppState.currentBegehung;


        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            FatalError("Initialiasing Camera", "No Phone camera detected", null);
            yield break;
        }
        else if (devices.Length == 1 && devices[0].isFrontFacing)
        {
            FatalError("Initialiasing Camera", "No usable camera detected. Phone only features a front-facing camera.", null);
            yield break;
        }

        AppLogger.Instance.LogFromMethod(this.name, "InitialiseCamera", $"We detected ({devices.Length}) cameras. Screen {Screen.orientation}");

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                var res = GetSupportedResolution(devices[i]);
                webCamTexture = new WebCamTexture(devices[i].name, res.width, res.height, fps);
                AppLogger.Instance.LogFromMethod(this.name, "InitialiseCamera", $"Selected camera {devices[i].name} - Width ({res.width}) Height:{res.height}.");
            }
        }

        if (webCamTexture == null)
        {         
            // No camera
            RecordingInfo.UpdateRecStatus(RecordingStatus.RunningStatus.Error, 0, fps);

            FatalError("Initialiasing Camera","Not able to initialise a camera.", null);

            yield break;
        }

        webCamTexture.Play();
        yield return new WaitUntil(() => webCamTexture.width > 16 && webCamTexture.height > 16); // workaround werid bug (according to natcorder)
        rawImage.texture = webCamTexture;
        aspectRatioFitter.aspectRatio = (float)webCamTexture.requestedWidth / webCamTexture.requestedHeight;

        AppLogger.Instance.LogFromMethod(this.name, "InitialiseCamera", $"Initialised camera with resolution - Width ({webCamTexture.width}) Height:{webCamTexture.height} AspectRatio: {aspectRatioFitter.aspectRatio}.");

        // Start microphone
        microphoneSource = gameObject.AddComponent<AudioSource>();
        microphoneSource.mute =
        microphoneSource.loop = true;
        microphoneSource.bypassEffects =
        microphoneSource.bypassListenerEffects = false;

        string deviceName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        AppLogger.Instance.LogFromMethod(this.name, "InitialiseCamera", $"Microphone ({deviceName}) detected.");

        try
        {
            microphoneSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
        }
        catch(Exception e)
        {
            FatalError($"Initialiasing Camera", "Problems Initialising microphone clip.", e);
        }
 
        yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
        microphoneSource.Play();
    }

    private Resolution GetSupportedResolution(WebCamDevice device)
    {
        // Get all available webcam devices.
        WebCamDevice[] devices = WebCamTexture.devices;


        Resolution defaultRes = new Resolution { height = videoWidth, width = videoHeight };
        int videoWidthToMath = BestVideoWidth;
        int videoHeightToMath = BestVideoHeight;
        if (RecordPortrait)
        {
            defaultRes = new Resolution { height = videoHeight, width = videoWidth };
            videoWidthToMath =  BestVideoHeight;
            videoHeightToMath = BestVideoWidth;
        }

        // Iterate through each device to check supported resolutions.

        if (device.availableResolutions != null)
        {
            // Get the supported resolutions for the current device.
            foreach (Resolution resolution in device.availableResolutions)
            {
                Debug.Log("Supported Resolution: " + resolution.width + "x" + resolution.height);
                if (resolution.width == videoWidthToMath && resolution.height == videoHeightToMath)
                {                    
                    return RecordPortrait? new Resolution { height = resolution.width, width =  resolution.height } : resolution;
                }
                    
                    //defaultRes = resolution;
            }
        }
        

        return defaultRes;
    }



    private void OnDestroy()
    {
        if (microphoneSource != null)
        {
            // Stop microphone
            microphoneSource.Stop();
            Microphone.End(null);
        }

        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(rawImage.texture);
            webCamTexture = null;
        }
    }

    // Update is called once per frame
    float frameCheckInterval = 3.0f;
    float timeSinceLastFrameUpdate = 0.0f;
    int frameCount = 0;
    void Update()
    {

        if (AppState.recording && !AppState.pausedRec)
        {
            if (webCamTexture.didUpdateThisFrame) 
            {
                webCamTexture.GetPixels32(pixelBuffer);
                recorder.CommitFrame(pixelBuffer, clock.timestamp);
                frameCount++;
            }

            timeSinceLastFrameUpdate += Time.deltaTime;

            if (timeSinceLastFrameUpdate >= frameCheckInterval)
            {
                float actualFPS = frameCount / frameCheckInterval;

                RecordingInfo.UpdateRecStatus(webCamTexture.isPlaying?
                    RecordingStatus.RunningStatus.Active : RecordingStatus.RunningStatus.Error, actualFPS, fps);

                if (!webCamTexture.isPlaying || actualFPS < 1)
                {
                    FatalError("Update", $"Error with video frame rate. Recording appears to be stuck at {actualFPS}", null);
                }

                //if (actualFPS < fps - fpsMargin || actualFPS > fps + fpsMargin)
                //{
                //    Debug.LogWarning("WebCamTexture frame update rate is different than expected. Actual FPS: " + actualFPS + ", Expected FPS: " + fps + " +/- "+ fpsMargin);                    
                //}
                frameCount = 0;
                timeSinceLastFrameUpdate = 0;
            }
        } 
    }

    public void StartRecording()
    {
        if (!AppState.recording)
        {
            AppLogger.Instance.LogFromMethod(this.name, "StartRecording", "Starting recording");
            try
            {            
                if (Directory.Exists(Path.Combine(Application.persistentDataPath, RouteName)))
                    Directory.Delete(Path.Combine(Application.persistentDataPath, RouteName),true);

                // Start recording
                var sampleRate = recordMicrophone ? AudioSettings.outputSampleRate : 0;
                var channelCount = recordMicrophone ? (int)AudioSettings.speakerMode : 0;
                clock = new RealtimeClock();
                //recorder = new MP4Recorder(videoWidth, videoHeight, fps, sampleRate, channelCount, audioBitRate: 96_000);
                //recorder = new HEVCRecorder(videoWidth, videoHeight, fps, sampleRate, channelCount, audioBitRate: 96_000, videoBitRate: 500_000);
                recorder = new HEVCRecorder(webCamTexture.width, webCamTexture.height, fps, sampleRate, channelCount, videoBitRate: videoBitRate); 

                // Create recording inputs
                pixelBuffer = webCamTexture.GetPixels32();
                rotatedPixelBuffer = webCamTexture.GetPixels32();
                audioInput = recordMicrophone ? new AudioInput(recorder, clock, microphoneSource, true) : null;
                // Unmute microphone
                microphoneSource.mute = audioInput == null;
                AppState.recording = true;

                CurrentRoute.SocialWorkerId = AppState.CurrentSocialWorker.Id;
                CurrentRoute.StartTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                CurrentRoute.LocalVideoResolution = $"{webCamTexture.requestedWidth}x{webCamTexture.requestedHeight}:{webCamTexture.width}x{webCamTexture.height}";
                CurrentRoute.InsertDirty();

                AppLogger.Instance.LogFromMethod(this.name, "StartRecording", $"Recording started for Route ({CurrentRoute.Id}, {CurrentRoute.Name})");
            }
            catch (InvalidOperationException hevcEx)
            {
                // Handle specific exception when HEVCRecorder initialization fails.
                FatalError("StartRecording","HEVCRecorder initialization failed.", hevcEx);
            }
            catch (Exception e)
            {
                // Handle other exceptions generically.
                FatalError("StartRecording","Error Initializing recording.", e);
            }
        }
    }

    public async void StopRecording()
    {
        if (AppState.recording)
        {
            try
            {
                AppLogger.Instance.LogFromMethod(this.name, "StopRecording", "Stopping and saving recording");

                AppState.recording = false;
                // Mute microphone
                microphoneSource.mute = true;
                // Stop recording
                audioInput?.Dispose();
                var path = await recorder.FinishWriting();

                // Playback recording
                Debug.LogWarning($"Saved recording to: {path}");
                string[] split = path.Split('/');
                string filename = "/" + split[split.Length - 1]; //".mp4";
                if (!Directory.Exists(Path.Combine(Application.persistentDataPath, RouteName)))
                {
                    Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, RouteName));
                }

                string VidDir = Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, RouteName, "Videos")).FullName;
                File.Move(path, VidDir + filename);

                CurrentRoute.EndTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                CurrentRoute.InsertDirty();

                AppLogger.Instance.LogFromMethod(this.name, "StopRecording", $"Recording successfully saved to {VidDir + filename}");
            }
            catch (Exception e)
            {
                FatalError("StopRecording", "Error saving the recording.", e);
            }

        }

        //Debug.Log(Application.persistentDataPath);
    }


    public async void CancelRecording()
    {
        try
        {
            AppLogger.Instance.LogFromMethod(this.name, "CancelRecording", $"Cancelling recording.");
            AppState.recording = false;

            string ImgDir = Path.Combine(Application.persistentDataPath, RouteName, "Fotos");
            Directory.Delete(ImgDir, true);

            var VidPath = await recorder.FinishWriting();
            File.Delete(VidPath);
            AppLogger.Instance.LogFromMethod(this.name, "CancelRecording", $"Cancelling done.");
        }
        catch (Exception e)
        {
            FatalError("CancelRecording", "Error Deleting recorded files.", e);
        }

    }

    private Color32[] rotatedPixelBuffer;
    public async void TakePicture(string filename)
    {
        if (AppState.recording)
        {
            try
            {
                AppLogger.Instance.LogFromMethod(this.name, "TakePicture", "Capturing picture.");

                string ImgDir = Path.Combine(Application.persistentDataPath, RouteName, "Fotos");
                if (!Directory.Exists(ImgDir))
                {
                    Directory.CreateDirectory(ImgDir);
                }

                JPGRecorder rec = new JPGRecorder(webCamTexture.requestedWidth, webCamTexture.requestedHeight);
                RotatePixelBuffer(pixelBuffer, rotatedPixelBuffer);
                rec.CommitFrame(rotatedPixelBuffer);
                //rec.CommitFrame(pixelBuffer);
                var path = await rec.FinishWriting();
                Debug.LogWarning($"Saved recording to: {path}");
                string[] split = path.Split('/');
                //string filename = split[split.Length - 1]+".jpg";
                string[] fotos = Directory.GetFiles(path);
                foreach (string foto in fotos)
                {
                    File.Move(foto, Path.Combine(ImgDir, filename));
                }
                Debug.LogWarning(path);
                Directory.Delete(path, true);

                AppLogger.Instance.LogFromMethod(this.name, "TakePicture", $"Picture captured and saved to: {path}.");
            }
            catch (Exception e)
            {
                FatalError("TakePicture","Error taking picture.", e);
            }
        }
    }

    public double GetCurrentPlaybackTimeSeconds()
    {
        return (double)clock.timestamp / 1_000_000_000.0;
    }

    public void TogglePause()
    {
        try
        {
            AppLogger.Instance.LogFromMethod(this.name, "TogglePause", $"Pausing the recording from {AppState.pausedRec} to {!AppState.pausedRec}.");

            clock.paused = !clock.paused;
            AppState.pausedRec = !AppState.pausedRec;
            if (clock.paused)
            {
                audioInput?.Dispose();
                microphoneSource.mute = true;
            }
            else
            {
                audioInput = recordMicrophone ? new AudioInput(recorder, clock, microphoneSource, true) : null;
                microphoneSource.mute = audioInput == null;
            }

            RecordingInfo.PauseUpdate(AppState.pausedRec);

            AppLogger.Instance.LogFromMethod(this.name, "TogglePause", "Pausing toggle successful.");

        }
        catch (Exception e)
        {
            FatalError("TogglePause", "Error executing the video pause / resume request.", e);
        }

    }


    private void FatalError(string method, string appMessage, Exception exception)
    {
        var trace = exception != null? exception.StackTrace : null;

        AppLogger.Instance.ErrorFromMethod(this.name, method, appMessage + " StackTrace: " + trace);

        Debug.Log(appMessage);
        if (trace!= null)
            Debug.Log(trace);

        StopRecording();

        OnRecordingError?.Invoke(appMessage);
    }

    //private void RotatePixelBuffer(Color32[] input, Color32[] output)
    //{
    //    int width = webCamTexture.width;
    //    int height = webCamTexture.height;

    //    for (int y = 0; y < height; y++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            output[x * height + (height - y - 1)] = input[y * width + x];
    //        }
    //    }
    //}

    private void RotatePixelBuffer(Color32[] input, Color32[] output)
    {
        int width = webCamTexture.width;
        int height = webCamTexture.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int newX = y;
                int newY = width - x - 1;

                output[newY * height + newX] = input[y * width + x];
            }
        }
    }



}
