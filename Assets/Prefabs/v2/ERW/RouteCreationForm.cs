using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class RouteCreationForm : MonoBehaviour
{
        
    [Header(@"Creation Steps")]
    public GameObject Step0; // Preparation
    public GameObject Step1; // Route name
    public GameObject Step2; // Start
    public GameObject Step3; // Destination

    [Header(@"Components")]
    public GameObject OverlayPanel;


    void Awake()
    {
        ShowStep(Step0);
    }

    void OnEnable()
    {
        ShowStep(Step0);
    }



    private void ClearOutForm(GameObject formObject){
        var form = formObject.GetComponent<FormValidator>();
        if (form != null){
            form.ClearOutFields();
        }

    }

    public void ShowStep(GameObject step){
        Step0.SetActive(step == Step0);
        Step1.SetActive(step == Step1);
        Step2.SetActive(step == Step2);
        Step3.SetActive(step == Step3);
        
        ClearOutForm(step);

        OverlayPanel.SetActive(step != Step0);

    }

  
}
