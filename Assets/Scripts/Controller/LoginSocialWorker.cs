
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSocialWorker : MonoBehaviour
{
    public GameObject InputFieldUsername;
    public GameObject InputFieldPassword;
    public GameObject ErrorMessage;

    public ButtonPrefab LoginButton;
    public UnityEvent OnNotLoggedIn;
    public UnityEvent OnLoginSucceed;
    public UnityEvent OnLoginFail;

    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    // Start is called before the first frame update
    void Start()
    {

        // We do not check the credentials again
        if (AppState.SWAPIToken != null && AppState.CurrentSocialWorker != null)
        {
            OnLoginSucceed?.Invoke();
        } else
        {
            OnNotLoggedIn?.Invoke();
        }
    }

    /// <summary>
    /// Sends the credentials to the API.
    /// </summary>
    public void SendCredentialsToAPI()
    {
        ErrorMessage?.SetActive(false);
        LoginButton.RenderBusyState(true);

        PaganiniRestAPI.SocialWorker.Authenticate(InputFieldUsername.GetComponent<TMP_InputField>().text,
                                                  InputFieldPassword.GetComponent<TMP_InputField>().text,
                                                  GetAuthSucceed, GetAuthFailed);
    }

    public void LogoutSocialWorker()
    {
        AppState.SWAPIToken = null;
        AppState.CurrentSocialWorker = null;
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="token">AuthTokenAPI</param>
    private void GetAuthSucceed(AuthTokenAPI token)
    {
        AppState.SWAPIToken = token.apitoken;
        PaganiniRestAPI.SocialWorker.GetProfile(GetProfileSucceed, GetAuthFailed);  
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="profile">SocialWorkerAPIResult</param>
    private void GetProfileSucceed(SocialWorkerAPIResult profile)
    {
        AppState.CurrentSocialWorker = new SocialWorker(profile);
        OnLoginSucceed?.Invoke();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetAuthFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
        LoginButton.RenderBusyState(false);
        ErrorMessage.SetActive(true);

        OnLoginFail.Invoke();
    }

    private void VerifyExistingAuthFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
        OnNotLoggedIn?.Invoke();
    }


}
