using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;


[System.Serializable]
public class WayEvent : UnityEvent<Way,Route>
{
}


public class WayItem : MonoBehaviour
{
        
    [Header(@"Way start")]
    public TMPro.TMP_Text startName;
    public LandmarkIcon startLandmark;

    [Header(@"Way destination")]
    public TMPro.TMP_Text destinationName;
    public LandmarkIcon destinationLandmark;
    public RawImage DestinationPhoto;

    [Header(@"Route")]
    public TMPro.TMP_Text routeName;
    public TMPro.TMP_Text recordingDate;
    public TMPro.TMP_Text recordingFailed;

    [Header(@"Route export attributes")]
    public TMPro.TMP_Text exportedStatus;
    public GameObject localStatusFlag;        

    [Header(@"Other")]
    public Button selectionButton;
    public GameObject LoadingPanel;
    public GameObject CardPanel;


    public WayEvent OnSelected;

    private Way way;
    private Route route;

    void Awake()
    {
        RenderLoading(true);
    }


    // Start is called before the first frame update
    void Start()
    {
        
        var buttonPrefab = gameObject.GetComponent<ButtonPrefab>();
        if (buttonPrefab != null)
        {
            buttonPrefab.OnProperClick.AddListener(WaySelected);
        }
        else 
        {
            selectionButton.onClick.AddListener(WaySelected);        
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RenderLoading(bool doRender)
    {
        if (LoadingPanel!=null) LoadingPanel?.SetActive(doRender);
        if (CardPanel != null) CardPanel?.SetActive(!doRender);
    }

    public void FillWayItem(Way w, Route route)
    {
        if (routeName!=null) routeName.text = ToCapitalFirst(route.Name);
        if (startName!= null) startName.text = ToCapitalFirst(w.Start);;
        if (destinationName != null) destinationName.text = ToCapitalFirst(w.Destination);

        startLandmark?.SetSelectedLandmark(Int32.Parse(w.StartType)); //selectedLandmarkType = (LandmarkIcon.LandmarkType) Int32.Parse(w.StartType);
        destinationLandmark?.SetSelectedLandmark(Int32.Parse(w.DestinationType)); //(LandmarkIcon.LandmarkType)Int32.Parse(w.DestinationType);

        if (recordingDate != null) {
            // flags based on wether the route is local or not
            
            exportedStatus.gameObject.SetActive(route.FromAPI);
            localStatusFlag.SetActive(!route.FromAPI);

            if (!route.FromAPI) {                
                bool failed = true;
                if (route.StartTimestamp != null && route.StartTimestamp > 0)
                {
                    failed = false;
                    recordingDate.text =  DateUtils.ConvertMillisecondsToLocalString(route.StartTimestamp, dateFormat: "HH:mm 'Uhr' dd.MM.yyyy ");
                }

                recordingDate.gameObject.SetActive(!failed);
                recordingFailed.gameObject.SetActive(failed);                

            }
                
        }


        this.way = w;
        this.route = route;

        RenderLoading(false);
    }

    public void FillWayDestination(Way w, Route route)
    {
        var fmtName = ToCapitalFirst(w.Destination);
        //destinationName.text = destinationName.text.Replace("{0}", fmtName);
        destinationName.text =  fmtName;
        destinationLandmark?.SetSelectedLandmark(Int32.Parse(w.DestinationType)); //(LandmarkIcon.LandmarkType)Int32.Parse(w.DestinationType);

        if (DestinationPhoto!=null)
            RenderPicture(DestinationPhoto, route.PhotoDestination);

        this.way = w;
        this.route = route;

        RenderLoading(false);
    }

    private string ToCapitalFirst(string text){
        if (text == null || text.Trim() == "") return text;
        return char.ToUpper(text[0]) + text.Substring(1);
    }

    private void WaySelected()
    {
        Debug.Log("Way selected, id: " + way.Id);
        if (OnSelected != null)
        {
            OnSelected.Invoke(way, route);
        }
    }


    private void RenderPicture(RawImage img, byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        if (img.texture != null)
        {
            DestroyTexture(img.texture);
        }

        img.texture = texture;

        AspectRatioFitter ratioFitter = img.gameObject.GetComponent<AspectRatioFitter>();
        ratioFitter.aspectRatio = (float)texture.width / texture.height;
    }

    private void DestroyTexture(Texture texture)
    {
        if (texture != null)
        {
            // Use DestroyImmediate to properly destroy the texture at runtime
            DestroyImmediate(texture, true);
        }
    }

}
