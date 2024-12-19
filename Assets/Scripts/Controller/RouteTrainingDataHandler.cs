using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RouteTrainingDataHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Wege in
    /// </summary>

    //TODO: get from parent
    public LoginManager LoginHandler;
    public TMPro.TMP_Text GreetingText;

    [Header(@"List Configuration")]
    public GameObject WayListView;
    private WayList WayListHandler;


    private List<Way> ways;
    private Route selectedRoute;



    // Start is called before the first frame update
    void Start()
    {
        WayListHandler = WayListView.GetComponent<WayList>();
  
        if (AppState.CurrentUser != null)
        {
            GreetingText.text = GreetingText.text.Replace("{0}", AppState.CurrentUser.AppName); 
        }            
    }

    public void LoadRouteTrainings()
    {
        GreetingText.text = GreetingText.text.Replace("{0}", AppState.CurrentUser.AppName);
        PaganiniRestAPI.Way.GetAll(GetWaySucceed, GetWegeFailed);
    }

    public void SetSelectedWay(Way w, Route r)
    {
        // SessionData.Instance.SaveData("SelectedRoute", w.Routes[0]);
        // selectedRoute = w.Routes[0];
        SessionData.Instance.SaveData("SelectedRoute", r);
        selectedRoute = r;        
    }


    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void LoadWaysFromLocalStorage()
    {

        ways = new List<Way> { };
        var list = Way.GetWayListByUser(AppState.CurrentUser.Id);
        foreach (var way in list)
        {
            way.Routes = Route.GetAll(r => r.WayId == way.Id && r.Status == Route.RouteStatus.Training);

            if (way.Routes.Count > 0)
            {
                ways.Add(way);
            }
        }

        DisplayWays();
    }



    /// <summary>
    /// Updates the local routes with the data received from the API.
    /// </summary>
    /// <param name="list">An array of WayAPIResult objects containing the ways and associated routes information from the API.</param>
    /// <remarks>
    /// This function first deletes any non-dirty local copies of ways and routes. Then, it iterates through the list of WayAPIResult objects and
    /// checks if a local copy of each way already exists. If not, it inserts a new way with the information from the API. Next, for each way, the function
    /// iterates through its associated routes (if any) and checks if a local copy of each route already exists. If not, it inserts a new route with the
    /// information from the API.
    /// </remarks>
    private void UpdateLocalRoutes(WayAPIResult[] list)
    {
        // Delete the current local copy of ways

        Way.DeleteNonDirtyCopies();
        Route.DeleteNonDirtyCopies();

        // Create a local copy of the API results
        foreach (WayAPIResult wres in list)
        {
            // Get the local copy first (not to overwrite local copy)
            if (!Way.CheckIfExists(w => w.Id == wres.way_id))
            {
                // Insert new way
                Way w = new Way(wres);
                w.UserId = AppState.CurrentUser.Id;
                w.Insert();
            }

            if (wres.routes != null)
            {
                foreach (RouteAPIResult rres in wres.routes)
                {
                    // check for local copy
                    if (!Route.CheckIfExists(r => r.Id == rres.erw_id))
                    {
                        // Insert associated route
                        Route r = new Route(rres);
                        r.Insert();
                    }
                }
            }
        }
    }





    /// <summary>
    /// Request for getting Ways from the API was successful
    /// </summary>
    /// <param name="list">List of WayAPIList objects</param>
    private void GetWaySucceed(WayAPIList list)
    {
        UpdateLocalRoutes(list.ways);
        LoadWaysFromLocalStorage();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetWegeFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);


        Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("Error Loading Ways", errorMessage);


        // TODO: Implement scene switching with "messages" displayed on the login (e.g., pin no valid)

        if (errorMessage == "Unauthorised")
        {
            LoginHandler.Logout();
            LoginHandler.RecheckLogin();
        }

        // Load it from the database if there are problems (e.g., connection problems)
        LoadWaysFromLocalStorage();

    }

    /// <summary>
    /// Fills out the Way List component
    /// </summary>
    private void DisplayWays()
    {        

        if (this.ways == null || this.ways.Capacity == 0)
        {
            WayListHandler.ShowBlankState();
        }
        else
        {
            WayListHandler.ClearList();

            foreach (Way w in ways)
            {
                WayListHandler.AddWayDestination(w, w.Routes[0]);
            }

            WayListHandler.ShowWayList();
        }
    }


}

