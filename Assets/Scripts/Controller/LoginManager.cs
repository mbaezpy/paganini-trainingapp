using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using static PaganiniRestAPI;

public class LoginManager : MonoBehaviour
{

    public UnityEvent OnAlreadyLoggedIn;
    public UnityEvent OnNotLoggedIn;

    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    void Start()
    {
        RecheckLogin();
    }

    // Checks if currentUser is set in Appstate and handles it acordingly
    // IF not set, try to pull from local Database
    // IF not found go to UserLogin Scene
    public void RecheckLogin()
    {
        if (AppState.CurrentUser == null)
        {
            CheckAuthToken();
        }
        else
        {
            LoginStatusNextAction();
        }

    }

    /// <summary>
    /// Logs out the user.
    /// </summary>
    public void Logout()
    {
        //DBConnector.Instance.TruncateTable<User>();

        AppState.CurrentUser = null;
        AppState.CurrentSocialWorker = null;
        User.DeleteNonDirtyCopies();
        AuthToken.DeleteAll();
    }

    /// <summary>
    /// Checks if there is an AuthToken, and try to continue with the user sign in.
    /// </summary>
    private void CheckAuthToken()
    {
        var list = AuthToken.GetAll();

        AuthToken authToken = null;
        foreach (var token in list)
        {
            authToken = token;
            break;
        }

        if (authToken != null)
        {
            AppState.APIToken = authToken.ApiToken;
            ContinueUserSignIn();
        }
        else
        {
            LoginStatusNextAction();
        }
    }

    /// <summary>
    /// Continue the user sign in, raising the events accordingly.
    /// </summary>
    private void LoginStatusNextAction()
    {
        if (AppState.CurrentUser == null)
        {
            if (SceneManager.GetActiveScene().name != SceneSwitcher.UserLoginScene)
            {
                SceneManager.LoadScene(SceneSwitcher.UserLoginScene);
            }

            OnNotLoggedIn.Invoke();
        }
        else
        {
            OnAlreadyLoggedIn.Invoke();
        }

    }

    /// <summary>
    /// Sends the credentials to the API.
    /// </summary>
    public void ContinueUserSignIn()
    {
        PaganiniRestAPI.User.GetProfile(GetUserProfileSucceed, GetUserProfileFailed);
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    private void GetUserProfileSucceed(IUserAPI userApi)
    {        
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

        LoginStatusNextAction();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage"></param>
    private void GetUserProfileFailed(string errorMessage)
    {
        Debug.Log("Error getting Profile: " + errorMessage);

        AppState.CurrentUser = null;        
        AppState.APIToken = null;
        LoginStatusNextAction();
    }

}
