using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class WayList : MonoBehaviour
{

    public GameObject scrollView;

    public GameObject content;

    public GameObject wayItemPrefab;

    public GameObject blankState;

    public WayEvent onWaySelected;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void ShowBlankState()
    {
        blankState.SetActive(true);
        scrollView.SetActive(false);
    }

    public void ShowWayList()
    {
        blankState.SetActive(false);
        scrollView.SetActive(true);
    }

    public void AddWayItem(Way w, Route r)
    {
        var neu = Instantiate(wayItemPrefab, content.transform);

        WayItem item = neu.GetComponent<WayItem>();
        item.FillWayItem(w, r);
        item.OnSelected = onWaySelected;
        
    }

    public void AddWayDestination(Way w, Route r)
    {
        var neu = Instantiate(wayItemPrefab, content.transform);

        WayItem item = neu.GetComponent<WayItem>();
        item.FillWayDestination(w, r);
        item.OnSelected = onWaySelected;        
    }

    public void ClearList()
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
    }

}