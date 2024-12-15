using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.VectorGraphics;



public class RouteEditItem : MonoBehaviour
{
    public bool ReadOnly = false;

    [Header(@"Way start")]
    public TMPro.TMP_Text startName;
    public LandmarkIcon startLandmark;
    public GameObject NoStartLandmark;

    [Header(@"Way destination")]
    public TMPro.TMP_Text destinationName;
    public LandmarkIcon destinationLandmark;
    public GameObject NoDestinationLandmark;

    [Header(@"Route")]
    public TMPro.TMP_Text routeName;

    [Header(@"Interaction Effects")]        
    public Color FocusColor;


    [Header(@"Utilities")]
    public LandmarkToggleList LandmarkPicker;   // bind clicking on the landmarks based on what is editable
    public Button EditStartButton;
    public Button EditDestinationButton;
    public string PickerTitleTemplate = "Wähle ein Symbol für den {0}";

    // private
    private EditableField _editableField;
    private Color _originalTextColor;
    private Color _originalLandmarkColor;

    private string _defaultStartName;
    private string _defaultDestName;
    private string _defaultRouteName;

    enum EditableField { Route, Start, Destination };

    // Start is called before the first frame update
    void Awake()
    {
        _originalTextColor = startName.color;
        _originalLandmarkColor = NoStartLandmark.transform.GetComponentInChildren<Image>().color;
        _defaultStartName = startName.text;
        _defaultDestName = destinationName.text;
        _defaultRouteName = routeName.text;

        EditStartButton?.onClick.AddListener(ShowPicker);
        EditDestinationButton?.onClick.AddListener(ShowPicker);
        if (LandmarkPicker != null)
        {
            LandmarkPicker.OnToggleSelected.AddListener(UpdateLandmarkIcon);
        }

        if (ReadOnly){
            EditStartButton.interactable = false;
            EditDestinationButton.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }   

    public Way ExportEditedWay(){

        var w = new Way
        {
            Name = routeName.text,
            Start = startName.text,
            StartType = ((int) startLandmark.SelectedLandmarkType).ToString(),
            Destination = destinationName.text,
            DestinationType = ((int)destinationLandmark.SelectedLandmarkType).ToString(),
            Description = startName.text + "->" + destinationName.text,
        };      

        return w;  
    } 

    public void ResetValues(){
        routeName.text = _defaultRouteName;
        startName.text = _defaultStartName;
        destinationName.text = _defaultDestName;

        UpdateLandmarkIcon(startLandmark, LandmarkIcon.LandmarkType.Placeholder, NoStartLandmark, focus: false);
        UpdateLandmarkIcon(destinationLandmark, LandmarkIcon.LandmarkType.Placeholder, NoDestinationLandmark, focus: false);
    }

    public void LoadWay(Way way){
        routeName.text = way.Name;
        startName.text = way.Start;
        destinationName.text = way.Destination;

        LandmarkIcon.LandmarkType startType = (LandmarkIcon.LandmarkType)int.Parse(way.StartType);
        LandmarkIcon.LandmarkType destType = (LandmarkIcon.LandmarkType)int.Parse(way.DestinationType);

        UpdateLandmarkIcon(startLandmark, startType, NoStartLandmark, focus: false);
        UpdateLandmarkIcon(destinationLandmark, destType, NoDestinationLandmark, focus: false);
    }

    #region Making fields editable
    public void MakeRouteEditable(){
        _editableField = EditableField.Route;
        SetFocus(_editableField);
    }

    public void MakeStartEditable(){
        _editableField = EditableField.Start;   
        SetFocus(_editableField);
    }

    public void MakeDestinationEditable(){
        _editableField = EditableField.Destination;
        SetFocus(_editableField);
    }

    #endregion

    #region Update fields
    /// <summary>
    /// Update the landmark name of the current editable field
    /// </summary>
    /// <param name="text"></param>
    public void UpdateLandmarkName(string text){
        if (ReadOnly){
            return;
        }

        if(_editableField == EditableField.Start){
            UpdateTextOrDefault(startName, text, _defaultStartName);
        }
        else
        {
            UpdateTextOrDefault(destinationName, text, _defaultDestName);
        }
    }

    /// <summary>
    /// Update the landmark icon of the current editable field
    /// </summary>
    /// <param name="landmarkType"></param>
    public void UpdateLandmarkIcon(LandmarkIcon.LandmarkType landmarkType){
        if (ReadOnly){
            return;
        }

        if(_editableField == EditableField.Start){
            UpdateLandmarkIcon(startLandmark, landmarkType, NoStartLandmark);
        }
        else
        {
            UpdateLandmarkIcon(destinationLandmark, landmarkType, NoDestinationLandmark);
        }        
    }

    /// <summary>
    /// Update the route name
    /// </summary>
    /// <param name="text"></param>
    public void UpdateRouteName(string text){
        if (ReadOnly){
            return;
        }
        UpdateTextOrDefault(routeName, text, _defaultRouteName);
    }

    #endregion


    private void UpdateLandmarkIcon(LandmarkIcon landmark, LandmarkIcon.LandmarkType landmarkType, GameObject NoData, bool focus = true){
        

        NoData.SetActive(landmarkType == LandmarkIcon.LandmarkType.Placeholder);
        landmark.gameObject.SetActive(landmarkType != LandmarkIcon.LandmarkType.Placeholder);

        landmark.SetSelectedLandmark((int)landmarkType);
        if (focus){
            landmark.ApplyColor(FocusColor);
        }
    }

    // Handling effects

    private void UpdateTextOrDefault(TMPro.TMP_Text field, string text, string defaultText){
        if (text.Trim() == ""){
            field.text = defaultText;
        }
        else
        {
            field.text = text;
        }
    }

    private void SetFocus(EditableField field){
        SetTextFocusColor(routeName, field == EditableField.Route);
        SetTextFocusColor(startName, field == EditableField.Start);
        SetTextFocusColor(destinationName, field == EditableField.Destination);  

        SetLandmarkFocusColor(startLandmark, NoStartLandmark, field == EditableField.Start);
        SetLandmarkFocusColor(destinationLandmark, NoDestinationLandmark, field == EditableField.Destination);  

        EditStartButton.interactable = field == EditableField.Start;    
        EditDestinationButton.interactable = field == EditableField.Destination;
    }

    private void SetTextFocusColor(TMPro.TMP_Text field, bool focus){
        if (focus) {
            field.color = FocusColor;
        }            
        else {
            field.color = _originalTextColor;
        }                
    }
    /// <summary>
    /// Show the landmark picker
    /// </summary>
    private void ShowPicker(){ 
        if (ReadOnly){
            return;
        }
        Debug.Log("Show picker");  
        var title = "";   
        LandmarkIcon.LandmarkType defaultIcon = LandmarkIcon.LandmarkType.Placeholder;
        if (_editableField == EditableField.Start){
            defaultIcon = startLandmark.SelectedLandmarkType;
            title = string.Format(PickerTitleTemplate, _defaultStartName);
        }
        else if (_editableField == EditableField.Destination){
            defaultIcon = destinationLandmark.SelectedLandmarkType;
            title = string.Format(PickerTitleTemplate, _defaultDestName);
        }

        Debug.Log("Default icon: " + defaultIcon + " Editable field: " + _editableField);  

        LandmarkPicker.SetTitle(title);
        LandmarkPicker.ShowListPicker(defaultIcon);
    }      

    /// <summary>
    /// Set the color of the landmark icon and the NoData placeholder
    /// </summary>
    /// <param name="landmark"></param>
    /// <param name="NoData"></param>
    /// <param name="focus"></param>
    private void SetLandmarkFocusColor(LandmarkIcon landmark, GameObject NoData, bool focus)
    {
        if (landmark.SelectedLandmarkType == LandmarkIcon.LandmarkType.Placeholder)
        {
            var image = NoData.transform.GetComponentInChildren<Image>();
            image.color = focus ? FocusColor : _originalLandmarkColor;
        }
        else
        {
            if (focus)
            {
                landmark.ApplyColor(FocusColor);
            }
            else
            {
                landmark.ResetColor();
            }
        }
    }


    void OnDestroy(){
        EditStartButton?.onClick.RemoveListener(ShowPicker);
        EditDestinationButton?.onClick.RemoveListener(ShowPicker);  
        if (LandmarkPicker != null)
        {
            LandmarkPicker.OnToggleSelected.RemoveListener(UpdateLandmarkIcon);      
        }
        
    }

}
