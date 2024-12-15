using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaysDataHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Wege in
    /// </summary>
    public LoginManager LoginHandler;

    [Header(@"Main views")]
    public GameObject ERWListView;
    public GameObject ERWCreationView;
    public GameObject ERWStartView;
    public GameObject ERWDeleteView;

    [Header(@"List Configuration")]
    public GameObject WayListView;
    private WayList WayListHandler;

    private Way selectedWay;
    private Route selectedRoute;

    [Header(@"Creation form Configuration")]
    public RouteEditItem RouteEditFields;

    [Header(@"Start UI Configuration")]
    public RouteEditItem RouteEditPreview;    

    public GameObject StartPanelOverwrite;
    public GameObject StartPanelNew;
    public GameObject StartPanelUnderTraining;
    public GameObject DeleteERWButton;

    private bool StatusOverwriteERW;


    // Start is called before the first frame update
    void Start()
    {
        WayListHandler = WayListView.GetComponent<WayList>();

        Debug.Log(Application.persistentDataPath);
        AppState.ERW.ResetValues();

        ShowView(ERWListView);
    }


    /// <summary>
    ///  Loads the ways from the API
    /// </summary>
    public void LoadWaysFromAPI()
    {
        Debug.Log("Loading Ways from API");
        PaganiniRestAPI.Way.GetAll(GetWaysSucceed, GetWaysFailed);
    }    

    public void LoadCachedWays()
    {
        ShowView(ERWListView);
        DisplayLocalERWList();
    }

    /// <summary>
    /// Cancels the creation of a new ERW
    /// </summary>
    public void CancelNewERW()
    {
        // If we are cancelling the overwriting, let's not delete the existing ERW.
        if (StatusOverwriteERW) return;

        // The ERW creation was cancelled, so we delete the Way if it was new
        var way = Way.Get(selectedWay.Id);
        if (way.FromAPI)
        {
            Way.Delete(selectedWay.Id);
        }

        AppState.ERW.ResetValues();
        selectedWay = null;
        selectedRoute = null;

        DisplayLocalERWList();
    }

    /// <summary>
    /// Create new way and load the start panel
    /// </summary>
    public void AddWayAndStart()
    {
        var w = RouteEditFields.ExportEditedWay();

        // safely create a local ID  
        var minWay = Way.GetWithMinId(w => w.Id);
        w.Id = minWay == null ? -1 : Math.Min(minWay.Id, 0) - 1;

        w.InsertDirty();

        SetUpStartPanel(w, null);        
    }

    /// <summary>
    /// Set up creation panel
    /// </summary>
    public void SetUpCreationPanel(){
        ShowView(ERWCreationView);
        RouteEditFields.ResetValues();
    }

    /// <summary>
    /// Show delete panel
    /// </summary>
    public void ShowDeletePanel(){
        ShowView(ERWDeleteView);
    }

    public void DeleteERW(){
        // Route.Delete(selectedRoute.Id);
        // Pathpoint.DeleteFromRoute(selectedRoute.Id, null, null);        

        // // Delete the way if it's local, and there are no other routes along this way
        // if (!selectedWay.FromAPI)        
        // {
        //     var routes = Route.GetAll(r => r.WayId == selectedWay.Id);
        //     if (routes.Count == 0)
        //     {
        //         Way.Delete(selectedWay.Id);
        //     }
        // }

        Route.DeleteDraftCascade(selectedRoute.Id);

        var folder = new LocalFolder.ERWFolder(selectedRoute);
        folder.DeleteFolder();

        selectedWay = null;
        selectedRoute = null;

        LoadCachedWays();
    }

    /// <summary>
    /// Set up start panel with given information
    /// </summary>
    /// <param name="w">Way object</param>
    public void SetUpStartPanel(Way w, Route r)
    {        
        SessionData.Instance.SaveData("SelectedWay", w);
        selectedWay = w;
        selectedRoute = r;        

        ShowView(ERWStartView);        
        
        RouteEditPreview.LoadWay(selectedWay);
        SetupERWCreationOptions();
    }

    /// <summary>
    /// Saves ways to SQLite
    /// </summary>
    /// <param name="list">List of WegAPI objects</param>
    void SaveWaysToLocalStorage(WayAPIList list)
    {
        // Delete local cache
        Way.DeleteNonDirtyCopies();
        Route.DeleteNonDirtyCopies();

        foreach (var w in list.ways)
        {
            var way = new Way(w);
            way.Insert();

            foreach(var r in w.routes) {
                var route = new Route(r);
                route.Insert();
            }
        }
    }


    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="ways">List of WegAPI objects</param>
    private void GetWaysSucceed(WayAPIList list)
    {
        Debug.Log("Ways loaded successfully");
        SaveWaysToLocalStorage(list);
        DisplayLocalERWList();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetWaysFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);

        //ToastManager.Instance.Toast.ShowError("Error Loading Ways", errorMessage);
       // Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("Error Loading Ways", errorMessage);


        // TODO: Implement scene switching with "messages" displayed on the login (e.g., pin no valid)

        //TODO: Replace for a more general error handling (see tablet app)
        if (errorMessage == "Unauthorised")
        {
            LoginHandler.Logout();
            LoginHandler.RecheckLogin();
        }

        // Load it from the database if there are problems (e.g., connection problems)
        DisplayLocalERWList();

    }

    /// <summary>
    /// Prints the Begehungen to the GUI
    /// </summary>
    private void DisplayLocalERWList()
    {

        List<Way> ways = Way.GetAll(w => w.FromAPI == false); 

        if (ways == null || ways.Count == 0)
        {
            WayListHandler.ShowBlankState();    
            return;    
        }

        WayListHandler.ClearList();

        int routeCount = 0;
        foreach (Way w in ways)
        {
            var routes = Route.GetAll(r => r.WayId == w.Id);
            if (routes.Count > 0)
            {
                var dirtyCopies = routes.FindAll(r => !r.FromAPI);
                // If we don't have dirty copies, we take one example, of the route
                // in the lowest process status. The user would be able to click on it
                // and add more routes to the way
                if (dirtyCopies.Count == 0)
                {
                    routes = routes.OrderByDescending(r => (int)r.Status).ToList();                    
                    routes = routes.Take(1).ToList();
                }

            }
            w.Routes = routes;

            foreach(var route in w.Routes) 
            {                
                routeCount++;
                WayListHandler.AddWayItem(w, route);                
            }                
        }
        WayListHandler.ShowWayList();        
    }

    /// <summary>
    /// Create a new ERW
    /// </summary>
    public void CreateNewOrReplaceERW(bool overwrite)
    {
        if (overwrite && selectedRoute != null)
        {
            Route.Delete(selectedRoute.Id);
            Pathpoint.DeleteFromRoute(selectedWay.Id, null, null);
        }


        // local id (negative range)
        var r = Route.GetWithMinId(r => r.WayId);
        int routeId = r == null ? -1 : Math.Min(r.Id, 0) - 1;

        Route route = new Route();
        route.Id = routeId;
        route.WayId = selectedWay.Id;
        route.Date = DateTime.Now.ToUniversalTime();
        route.Name = selectedWay.Name;

        route.InsertDirty();

        // We set the current exploratory walk
        AppState.ERW.CurrentRoute = route;
        AppState.ERW.CurrentWay = selectedWay;

    }

    /// <summary>
    /// Setup the ERW creation options based on the selected way, enabling
    /// users to either create a new route or overwrite an existing one.
    /// </summary>
    private void SetupERWCreationOptions()
    {
        var routeExists = selectedRoute != null && Route.CheckIfExists(r => r.Id == selectedRoute.Id);
       
       StartPanelNew.SetActive(!routeExists);
       DeleteERWButton.SetActive(routeExists && !selectedRoute.FromAPI); // delete only local routes
       
       if (routeExists) {
            StartPanelUnderTraining.SetActive(selectedRoute.FromAPI);  // already exported
            StartPanelOverwrite.SetActive(!selectedRoute.FromAPI); // not exported, exists locally
       }
       else {
              StartPanelUnderTraining.SetActive(false);
              StartPanelOverwrite.SetActive(false);
       }

        StatusOverwriteERW = routeExists;
    }

    /// <summary>
    /// Show a given view, hiding the others
    /// </summary>
    private void ShowView(GameObject view)
    {
        ERWListView.SetActive(view == ERWListView);
        ERWCreationView.SetActive(view == ERWCreationView);
        ERWStartView.SetActive(view == ERWStartView);
        ERWDeleteView.SetActive(view == ERWDeleteView);
    }


}
