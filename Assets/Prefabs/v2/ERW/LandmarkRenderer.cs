using System;
using UnityEngine;


public class LandmarkRenderer : MonoBehaviour
{
        
    [Header(@"Landmark Icons")]
    public LandmarkIcon IconFrom;
    public LandmarkIcon IconTo;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        RenderIcons();    
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void RenderIcons(){
        Way w = SessionData.Instance.GetData<Way>("SelectedWay");
        IconFrom?.GetComponent<LandmarkIcon>().SetSelectedLandmark(Int32.Parse(w.StartType));
        IconTo?.GetComponent<LandmarkIcon>().SetSelectedLandmark(Int32.Parse(w.DestinationType));
    }

}
