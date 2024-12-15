using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;

public class SceneSwitcher : MonoBehaviour
{

    // Scenes
    public static string UserLoginScene = "UserLogin";
    public static string MyExploratoryRouteWalkScenes = "MyExploratoryRouteWalks";
    public static string SyncExploratoryRouteWalkScenes = "SyncExploratoryRouteWalks";
    public static string ExploratoryRouteWalkRecodingScene = "ExploratoryRouteWalkRecording";
    public static string UserSettingsScene = "UserSettings";
    public static string MyRouteTrainingScene = "MyRouteTraining";
    public static string RouteTrainingScene = "RouteTraining";
    public static string ARDirectionScene = "ARDirection";

    public static string startScene = "01Start";
    public static string allOkScene = "02AllOk";
    public static string wegeFührungScene = "03Wegeführung";
    public static string pausedScene = "04Paused";
    public static string goalReachedScene = "05ZielErreicht";
    public static string videoHelpScene = "06VideoHelp";
    public static string settingsScene = "07Settings";
    public static string profileScene = "08ShowProfile";
    public static string overviewScene = "09Overview";





    public void GotoExploratoryRouteWalkRecording()
    {

        //AppState.SelectedBegehung = way.Id;
        //AppState.SelectedWeg = way.Id;

        //DBConnector.Instance.GetConnection().Execute("DELETE FROM ExploratoryRouteWalk where Id=" + AppState.SelectedWeg);
        //DBConnector.Instance.GetConnection().Execute("DELETE FROM Pathpoint where Erw_id=" + AppState.SelectedWeg);


        //ExploratoryRouteWalk walk = new ExploratoryRouteWalk();
        //walk.Id = way.Id;
        //walk.Way_id = way.Id;
        //walk.Date = DateTime.Now;
        //walk.Name = way.Name;
        //// We set the current exploratory walk
        //AppState.currentBegehung = walk.Name;

        //DBConnector.Instance.GetConnection().InsertOrReplace(walk);


        SceneManager.LoadScene(ExploratoryRouteWalkRecodingScene);

        // Prevent screen from dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void CancelRecordingAndGoMyExploratoryRouteWalk()
    {
        Route.Delete(AppState.ERW.CurrentRoute.Id);
        Pathpoint.DeleteFromRoute(AppState.ERW.CurrentRoute.Id, null, null);

        // We delete the way if it's local
        var way = Way.Get(AppState.ERW.CurrentWay.Id);
        if (!way.FromAPI)
        {
            Way.Delete(way.Id);
        }

        GotoMyExploratoryRouteWalk();
    }


    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void GotoMyExploratoryRouteWalk()
    {
        SceneManager.LoadScene(MyExploratoryRouteWalkScenes);

        // Restore default screen dimming timeout
        Screen.sleepTimeout = AppState.screenSleepTimeout;
    }

    public void GotoSyncExploratoryRouteWalk()
    {
        SceneManager.LoadScene(SyncExploratoryRouteWalkScenes);

        // Prevent screen from dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void GotoUserLogin()
    {
        SceneManager.LoadScene(UserLoginScene);
    }

    public void GotoUserSettings()
    {
        Screen.sleepTimeout = AppState.screenSleepTimeout;
        SceneManager.LoadScene(UserSettingsScene);
    }

    public void GotoMyRouteTraining()
    {
        Screen.sleepTimeout = AppState.screenSleepTimeout;
        SceneManager.LoadScene(MyRouteTrainingScene);
    }

    public void GotoRouteTraining()
    {
        SceneManager.LoadScene(RouteTrainingScene);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void GotoARDirectionBackground()
    {
        SceneManager.LoadScene(ARDirectionScene, LoadSceneMode.Additive);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}