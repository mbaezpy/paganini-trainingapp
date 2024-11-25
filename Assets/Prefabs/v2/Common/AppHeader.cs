using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppHeader : MonoBehaviour
{
    [Header(@"Header UI Configuration")]
    //public TMPro.TMP_Text AppName;
    public UserPicEdit UserPhoto;
    public UserPicEdit SWPhoto;
    public Image PairBackground;

    // Start is called before the first frame update
    void Start()
    {
        LoadFromAppSession();
    }

    public void LoadFromAppSession()
    {
        if (AppState.CurrentUser != null)
        {
            //AppName.text = AppState.CurrentUser.AppName;
            UserPhoto.RenderProfilePic(AppState.CurrentUser.ProfilePic);
        }

        if (AppState.CurrentSocialWorker != null)
        {
            SWPhoto.gameObject.SetActive(true);
            SWPhoto.RenderProfilePic(AppState.CurrentSocialWorker.ProfilePic);

            // Set Pair Background
            HidePairBackground(false);
        }
        else {
            SWPhoto.gameObject.SetActive(false);
            SWPhoto.gameObject.transform.parent.gameObject.SetActive(false);
            HidePairBackground(true);
        }
    }

    private void HidePairBackground(bool hide)
    {
        var color = PairBackground.color;
        color.a = hide ? 0 : 1;
        PairBackground.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
