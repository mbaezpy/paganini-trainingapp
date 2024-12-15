using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SyncPairViz : MonoBehaviour
{
    [Header("Data")]
    public TMPro.TMP_Text PhoneName;
    public TMPro.TMP_Text TabletName;

    [Header("Pairing viz")]
    public GameObject PairingCue;
    public Image TabletIconBackground;
    public Image TabletNameBackground;
    public PulsatingEffect PulsatingIcon;
    public Color ConnectedBackgroundColor;
    
    private Color originalNameBackgroundColor;
    private Color originalIconBackgroundColor;
    


    void Start()
    {
        PhoneName.text = AppState.CurrentUser.Mnemonic_token;
        originalNameBackgroundColor = TabletNameBackground.color;
        originalIconBackgroundColor = TabletIconBackground.color;        
    }

    public void RenderConnecting(){
        PairingCue.SetActive(true);
        TabletName.gameObject.SetActive(false);
    }

    public void RenderPairingTo(string pairedToTablet)
    {
        PairingCue.SetActive(false);
        TabletName.gameObject.SetActive(true);
        TabletName.text = pairedToTablet;
        TabletNameBackground.color = ConnectedBackgroundColor;
        TabletIconBackground.color = originalIconBackgroundColor;
        
        PulsatingIcon?.StopPulsating();
    }

    public void RenderPairingAccepted(){
        TabletNameBackground.color = originalNameBackgroundColor;
        TabletIconBackground.color = ConnectedBackgroundColor;
    }
    
}

