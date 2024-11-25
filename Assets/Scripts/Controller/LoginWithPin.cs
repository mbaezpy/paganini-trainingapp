using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoginWithPin : MonoBehaviour
{
    public TMP_InputField textinput;
    public ButtonPrefab LoginButton;
    public GameObject ErrorMessage;
    public UnityEvent OnLoginSucceed;
    public UnityEvent OnLoginFail;

    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    void Start()
    {
        ErrorMessage.SetActive(false);
    }

    public void SendPinToAPI()
    {
        LoginButton.RenderBusyState(true);
        ErrorMessage.SetActive(false);

        int pin = System.Int32.Parse(textinput.text);
        PaganiniRestAPI.User.Authenticate(pin, GetAuthSucceed, GetAuthFailed);
    }


    public void ContinueUserSignIn(AuthTokenAPI token)
    {
        PaganiniRestAPI.User.GetProfile(GetUserProfileSucceed, GetUserProfileFailed);
    }


    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param String authtoken</param>
    private void GetAuthSucceed(AuthTokenAPI token)
    {
        // Clear out old auth tokens
        AuthToken.DeleteAll();

        // Let's save the new token
        var authToken = new AuthToken(token);
        authToken.Insert();
        AppState.APIToken = authToken.ApiToken;

        ContinueUserSignIn(token);
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetAuthFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
        ErrorMessage.SetActive(true);
        LoginButton.RenderBusyState(false);        

        OnLoginFail.Invoke();
    }

    private void GetUserProfileSucceed(IUserAPI userApi)
    {
        ErrorMessage.SetActive(false);

        var list = User.GetAll( u => u.Id == userApi.user_id);

        User user = new User(userApi);

        // Keep the local copy if there is one
        if (list.Capacity > 0)
        {
            user.AppName = list[0].AppName;  
            user.ProfilePic = list[0].ProfilePic;  
            user.IsDirty = true;        
        }
        user.Insert();

        AppState.CurrentUser = user;

        OnLoginSucceed.Invoke();
    }

    private void GetUserProfileFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);              
        //Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("AuthFailed", errorMessage);

        OnLoginFail.Invoke();
    }
}
