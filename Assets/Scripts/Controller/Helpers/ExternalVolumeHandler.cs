using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;
using SFB = SimpleFileBrowser;
using SimpleFileBrowser;

public class ExternalVolumeHandler : MonoBehaviour
{
    // Change this to the desired directory path on the external volume.

    private string DestinationFolderPath;
    private string DestinationFolderName;

    public SyncProcessHandler SyncProcess;

    [Header("UI Configuration")]

    [SerializeField] private GameObject VolumeLookupPanel;
    [SerializeField] private GameObject VolumeLSelectPanel;
    [SerializeField] private GameObject FinishSuccessPanel;
    [SerializeField] private GameObject ExportErrorPanel;

    [Header("Volume Data UI")]
    [SerializeField] private TMPro.TMP_Text VolumeNameText;
    [SerializeField] private TMPro.TMP_Text ErrorMessageText;

    void Start()
    {
        // Start a coroutine to detect volume insertion.
        
    }

    /// <summary>
    /// Initialises the external volume handler.
    /// </summary>
    public void Initialise()
    {
        DestinationFolderName = null;
        DestinationFolderPath = null;

        StartCoroutine(CheckPathCoroutine());
        StartCoroutine(CheckVolumeInserted());

        try {
            SFB.FileBrowser.DirectNativeSAF();
        }
        catch(Exception e) {
            Debug.Log(e.StackTrace);
        }
    }

    IEnumerator CheckPathCoroutine()
    {

        Debug.Log("CheckPathCoroutine(): " + (SFB.FileBrowser.GetCurrentPath() == null));
        yield return new WaitForSeconds(1.0f);

        if (SFB.FileBrowser.GetCurrentPath() != null) {
            DestinationFolderName = SFB.FileBrowser.GetCurrentPathName();
            DestinationFolderPath = SFB.FileBrowser.GetCurrentPath();


            Debug.Log($"DestinationFolderName: {DestinationFolderName} | DestinationFolderPath: {DestinationFolderPath} ");

            VolumeNameText.text = DestinationFolderName;

            // VolumeLookupPanel.SetActive(false);
            // VolumeLSelectPanel.SetActive(true);      
            ShowView(VolumeLSelectPanel);      
        }
        else
        {
            yield return null;
        }

    }

    /// <summary>
    /// Checks if the volume is inserted.
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckVolumeInserted()
    {
        bool volumeDisconnected = false;
        // Wait for a short delay before checking if the volume is inserted.

        Debug.Log("CheckVolumeInserted(): " + (DestinationFolderPath == null));

        yield return new WaitForSeconds(1.0f);

        if (DestinationFolderPath != null)
        {
            volumeDisconnected = !SFB.FileBrowserHelpers.DirectoryExists(DestinationFolderPath);
        }

        if (volumeDisconnected) {

            ErrorMessageText.text = "The destination folder is no longer present. Is the USB stick still connected? ";

            // VolumeLSelectPanel.SetActive(false);
            // ExportErrorPanel.SetActive(true);
            ShowView(ExportErrorPanel);

            yield return null;
        }
    }

    /// <summary>
    /// Starts the export process.
    /// </summary>
    public void StartFilesExport()
    {       

        if (!SFB.FileBrowserHelpers.DirectoryExists(DestinationFolderPath))
        {
            ErrorMessageText.text = "The destination folder is no longer present. Is the USB stick still connected? ";

            // VolumeLSelectPanel.SetActive(false);
            // ExportErrorPanel.SetActive(true);
            ShowView(ExportErrorPanel);

            return;
        }

        try
        {
            string exportFolder = AppState.CurrentUser.Mnemonic_token + "-export-" + DateTime.Now.ToLongDateString();
            string path = SyncProcess.SyncroniseByFiles(exportFolder);

            DestinationFolderPath = SFB.FileBrowserHelpers.CreateFolderInDirectory(DestinationFolderPath, exportFolder);
            SFB.FileBrowserHelpers.MoveDirectory(path, DestinationFolderPath);

            // FinishSuccessPanel.SetActive(true);
            // VolumeLSelectPanel.SetActive(false);
            ShowView(FinishSuccessPanel);
        }
        catch (Exception e)
        {
            ErrorMessageText.text = "Error copying files. ";
            Debug.Log(e.StackTrace);

            // VolumeLSelectPanel.SetActive(false);
            // ExportErrorPanel.SetActive(true);
            ShowView(ExportErrorPanel);
        }
    }

    public void CancelExport()
    {
        // Stop coroutines if they are running
        StopCoroutine(CheckPathCoroutine());
        StopCoroutine(CheckVolumeInserted());

    }


    public void ShowView(GameObject view){
        VolumeLSelectPanel.SetActive(VolumeLSelectPanel == view);
        VolumeLookupPanel.SetActive(VolumeLookupPanel == view);
        FinishSuccessPanel.SetActive(FinishSuccessPanel == view);
        ExportErrorPanel.SetActive(ExportErrorPanel == view);
    }

}
