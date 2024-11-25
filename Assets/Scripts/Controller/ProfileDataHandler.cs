using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ProfileDataHandler : MonoBehaviour
{

    [Header(@"Profile form Configuration")]
    public TMPro.TMP_InputField AppName;     
    public UserPicEdit UserPhoto;

    [Header("Events")]
    public UnityEvent OnLocalProfileFound;
    public UnityEvent OnLocalProfileNotFound;

    void Awake()
    {
        UserPhoto.OnProfilePicChanged.AddListener(PhotoChangedHandler);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// Display the user data
    /// </summary>
    public void DisplayUserData(){
        AppName.text = AppState.CurrentUser.AppName;
        UserPhoto.RenderProfilePic(AppState.CurrentUser.ProfilePic);
    }

    /// <summary>
    /// Update the local profile
    /// </summary>
    public void UpdateLocalProfile()
    {
        User user = AppState.CurrentUser;
        user.AppName = AppName.text;

        user.InsertDirty();
    }

    /// <summary>
    ///  Trigger the local profile verification
    /// </summary>
    public void TriggerLocalProfileVerification()
    {
        // Let's check if there is a name
        if (AppState.CurrentUser.AppName == null || AppState.CurrentUser.AppName.Trim().Length == 0)
        {
            OnLocalProfileNotFound.Invoke();
        }
        else
        {            
            OnLocalProfileFound.Invoke();
        }
    }

    /// <summary>
    /// Handle the photo selected event
    /// </summary>
    /// <param name="picTexture">The selected photo texture</param>
    private void PhotoChangedHandler(byte[] picBytes){

        var user = AppState.CurrentUser;
        if (picBytes != null) {            
            user.ProfilePic = picBytes;
            user.InsertDirty();            
        }

    }

    void OnEnable(){
        if (AppState.CurrentUser != null){
            DisplayUserData();
        }        
    }

    private void OnDestroy(){
        UserPhoto.OnProfilePicChanged.RemoveListener(PhotoChangedHandler);
    }    

}
