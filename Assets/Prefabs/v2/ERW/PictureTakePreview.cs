using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class PictureTakePreview : MonoBehaviour
{
        
    [Header(@"Main views")]
    public GameObject PictureTakePanel;
    public GameObject PicturePreviewPanel;

    [Header(@"Components")]
    public ERWManager Manager;
    //public RawImage PreviewImage;
    public Image PreviewImage;

    [Header(@"Configuration")]    
    public WayLandmarkType LandmarkType = WayLandmarkType.Start;

    [Header(@"Events")]    
    public UnityEvent OnPictureTaken;

    // private
    private (Pathpoint poi, string fullPath) CurrentPOIData;

    public enum WayLandmarkType
    {
        Start = 1,
        Destination = 2
    }

    void Awake()
    {

    }

    // Start is called before the first frame update
    void OnEnable()
    {
        ShowCameraView();
    }

    public void TakeLandmarkPicture(){

        if (LandmarkType == WayLandmarkType.Start){
            Manager.TakeLandmarkPictureStart(OnLandmarkPictureReady);

            Debug.Log("START - TakeLandmarkPicture - CurrentPOIData.fullPath: " + CurrentPOIData.fullPath);
        }
        else {
            Manager.TakeLandmarkPictureEnd(OnLandmarkPictureReady);

            Debug.Log("END - TakeLandmarkPicture - CurrentPOIData.fullPath: " + CurrentPOIData.fullPath);
        }    
    }

    private void OnLandmarkPictureReady(bool success, Pathpoint poi, string fullPath){
        if (success){
            CurrentPOIData = (poi, fullPath);
            OnPictureTaken?.Invoke();     
            return; 
        }
        
        Debug.Log("OnLandmarkPictureReady: Error taking picture");
    }

    public void ShowCameraView(){
        ShowView(PictureTakePanel);
    }

    public void ShowPreview(){
        ShowView(PicturePreviewPanel);

        // Load Preview Image
        Texture2D pic = PictureUtils.LoadImage(CurrentPOIData.fullPath);
        Debug.Log("ShowPreview - CurrentPOIData.fullPath: " + CurrentPOIData.fullPath + " - pic: " + pic);
        PictureUtils.RenderPicture(PreviewImage, pic, destroyPrevious: true);                          
        
    }

    private void ShowView(GameObject view)
    {
        PictureTakePanel.SetActive(view == PictureTakePanel);
        PicturePreviewPanel.SetActive(view == PicturePreviewPanel);
    }

    void Destroy(){
        if (PreviewImage.sprite != null){
            DestroyImmediate(PreviewImage.sprite, true);
        }
    }    

}
