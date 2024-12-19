using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

using UnityEngine.UI;

public class PermissionManager : MonoBehaviour
{
    [Header(@"Main Panels")]
    public GameObject PermissionPanel;
    public GameObject CameraSupportedPanel;
    public GameObject CameraNotSupportedPanel;

    [Header(@"Permissions Buttons")]
    public ButtonPrefab PermissionButtonCamera;
    public ButtonPrefab PermissionButtonMicrophone;
    public ButtonPrefab PermissionButtonGPS;

    public Button CloseButton;

    public bool RequireAllPermissions = true;


    public UnityEvent OnPanelClose;
    public UnityEvent OnPendingPermissions;

    private int NumAvailablePermissions = 0;
    private int NumGrantedPermissions = 0;
    private bool SetupCompleted = false;


    // Start is called before the first frame update
    void Start()
    {

#if PLATFORM_ANDROID

        NumGrantedPermissions = 0;
        SetupCompleted = false;
        SetupPermission(PermissionButtonMicrophone, Permission.Microphone);
        SetupPermission(PermissionButtonCamera, Permission.Camera);
        SetupPermission(PermissionButtonGPS, Permission.FineLocation);
        SetupCompleted = true;

        if (!AllGranted())
        {
            CameraSupportedPanel.SetActive(true);
            PermissionPanel.SetActive(true);
            OnPendingPermissions.Invoke();  

            CloseButton.onClick.AddListener(OnPanelCloseHandler);
            CloseButton.interactable = !RequireAllPermissions;
        }
        else
        {
            OnPanelCloseHandler();
            Debug.Log("Start: All permissions granted");
        }
#endif

    }

    // Update is called once per frame
    void Update()
    {
        if (!SetupCompleted) return;

        if (AllGranted())
        {
            CloseButton.interactable = true;
        }
        else 
        {
            CloseButton.interactable = false;
#if PLATFORM_ANDROID
        NumGrantedPermissions = 0;
        CheckIfGranted(PermissionButtonMicrophone, Permission.Microphone);
        CheckIfGranted(PermissionButtonCamera, Permission.Camera);
        CheckIfGranted(PermissionButtonGPS, Permission.FineLocation);        
#endif
        }
    }

    public bool CheckIfPermissionGranted(string permission)
    {
        return Permission.HasUserAuthorizedPermission(permission);
    }

    public void ClosePanel()
    {
        OnPanelCloseHandler();
    }

    private void OnPanelCloseHandler()
    {
        this.gameObject.SetActive(false);
        OnPanelClose.Invoke();
    }

    private bool SetupPermission(ButtonPrefab button, string permission)
    {
        // no setup if we are not using this permission
        if (button == null)
        {
            return false;
        }

        NumAvailablePermissions++;        

        var granted = CheckIfGranted(button, permission);

        if (!granted)
        {
            button.ButtonObject.onClick.AddListener(delegate
            {
                Debug.Log("Clicking permission: " + permission);
                button.RenderBusyState(true);
                Permission.RequestUserPermission(permission);   
                button.RenderBusyState(false);             
            });
        }

        return granted;
    }

    private bool CheckIfGranted(ButtonPrefab button, string permission){
        if (button == null)
        {
            return false;
        }

        //Debug.Log("Checking permission: " + permission + " granted: " + Permission.HasUserAuthorizedPermission(permission));

        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            button.ButtonObject.interactable = true;
            return false; 
        }

        button.ButtonObject.interactable = false;
        NumGrantedPermissions++;

        return true;
    }

    private bool AllGranted(){
        Debug.Log("NumAvailablePermissions: " + NumAvailablePermissions + " NumGrantedPermissions: " + NumGrantedPermissions);
        return NumAvailablePermissions == NumGrantedPermissions;
    }

}
