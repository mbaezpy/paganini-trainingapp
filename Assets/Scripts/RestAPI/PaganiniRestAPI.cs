using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ServerCommunication;
using UnityEngine.Events;
using System;
using static PaganiniRestAPI;
using System.Text;

public class PaganiniRestAPI
{
    ////Base URL for the Rest APi
    //public const string serverURL = "https://infinteg-main.fh-bielefeld.de/paganini/api/usr/";
    ////public const string serverURL = "http://localhost:3000/usr/";
    ////getAuthToken  param: apitoken: ""
    //public const string getAuthToken = serverURL + "me/authentification";
    ////getUserProfile  param: apitoken: ""
    //public const string getUserProfile = serverURL + "me/profile";
    ////getUserBegehungen  param: apitoken: "", wegid: ""
    //public static string GetUserBegehungen(int wegid) { return serverURL + "way/"+wegid.ToString()+"/erw/"; }
    ////getUserWege  param: apitoken: ""
    //public const string getUserWege = serverURL + "me/ways/";

    public class Path
    {
        //Base URL for the Rest APi
        public const string BaseUrl = "https://infinteg-main.fh-bielefeld.de/paganini/api/usr/";
        public const string BaseSWUrl = "https://infinteg-main.fh-bielefeld.de/paganini/api/sw/";
        //public const string BaseUrl = "http://192.168.178.22:3000/usr/";

        public const string SWAuthenticate = BaseSWUrl + "me/authentification";
        public const string SwProfile = BaseSWUrl + "me/profile";

        public const string Authenticate = BaseUrl + "me/authentification";

        public const string UsrProfile = BaseUrl + "me/profile";

        public const string UsrWaysList = BaseUrl + "me/ways/";
        public const string UsrWays = BaseUrl + "me/ways/{0}";

        public const string UsrRoutesList = UsrWays + "/routes";
        public const string UsrRoutes = BaseUrl + "me/routes/{0}";

        public const string UsrRoutePhotoList = UsrRoutes + "/photos";
        public const string UsrPOIList = UsrRoutes + "/pois";

        public const string UsrPhotoDataList = UsrRoutePhotoList + "/data";

        public const string UsrPathpointList = UsrRoutes + "/pathpoints";

        public const string UsrRouteWalkList = UsrRoutes + "/routewalks";
        public const string UsrRouteWalks = BaseUrl + "/me/routewalks/{0}";
        public const string UsrRouteWalkEventList = UsrRouteWalks + "/events";
        public const string UsrRouteWalkPathLogList = UsrRouteWalks + "/pathlog";



        // Function to build a query string from a dictionary
        public static string BuildQueryString(Dictionary<string, string> query)
        {
            if (query == null || query.Count == 0)
            {
                return string.Empty; // No query parameters, return an empty string
            }

            var queryString = new StringBuilder("?");
            foreach (var kvp in query)
            {
                // Encode and append each key-value pair to the query string
                queryString.Append(Uri.EscapeDataString(kvp.Key));
                queryString.Append("=");
                queryString.Append(Uri.EscapeDataString(kvp.Value));
                queryString.Append("&");
            }

            return queryString.ToString().TrimEnd('&');
        }

    }

    public class SocialWorker
    {

        public static void Authenticate(string username, string password, UnityAction<AuthTokenAPI> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            RESTAPI.Instance.Get<AuthTokenAPI>(Path.SWAuthenticate, successCallback, errorCallback, headers);
        }

        public static void GetProfile(UnityAction<SocialWorkerAPIResult> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.SWAPIToken }
            };

            string url = Path.SwProfile;

            RESTAPI.Instance.Get<SocialWorkerAPIResult>(url, successCallback, errorCallback, headers);
        }

    }

    public class User
    {

        public static void Authenticate(int pin, UnityAction<AuthTokenAPI> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "pin", pin.ToString() }
            };

            RESTAPI.Instance.Get<AuthTokenAPI>(Path.Authenticate, successCallback, errorCallback, headers);
        }

        public static void GetProfile(UnityAction<UserAPIResult> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = Path.UsrProfile;

            RESTAPI.Instance.Get<UserAPIResult>(url, successCallback, errorCallback, headers);
        }

    }

    public class Way
    {

        public static void GetAll(UnityAction<WayAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = Path.UsrWaysList;

            RESTAPI.Instance.Get<WayAPIList>(url, successCallback, errorCallback, headers);
        }

    }

    public class Route
    {

        public static void GetAll(Int32 wayId, UnityAction<RouteAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = string.Format(Path.UsrRoutesList, wayId);

            RESTAPI.Instance.Get<RouteAPIList>(url, successCallback, errorCallback, headers);
        }

    }



    public class Pathpoint
    {
        public static void GetAll(Int32 routeId, UnityAction<PathpointAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = string.Format(Path.UsrPathpointList, routeId);

            RESTAPI.Instance.Get<PathpointAPIList>(url, successCallback, errorCallback, headers);
        }

    }



    public class PathpointPOI
    {
        public static void GetAll(Int32 routeId, UnityAction<PathpointPOIAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = string.Format(Path.UsrPOIList, routeId);
            RESTAPI.Instance.Get<PathpointPOIAPIList>(url, successCallback, errorCallback, headers);
        }

    }

    public class PhotoData
    {
        public static void GetAll(Int32 routeId, Dictionary<string, string> query, UnityAction<PhotoDataAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            var queryString = Path.BuildQueryString(query);

            string url = string.Format(Path.UsrPhotoDataList, routeId) + queryString;
            Debug.Log(url);

            RESTAPI.Instance.Get<PhotoDataAPIList>(url, successCallback, errorCallback, headers);
        }

    }

    public class RouteWalk
    {

        public static void Create(Int32 routeId, RouteWalkAPI routeWalkAPI, UnityAction<RouteWalkAPIResult> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };


            string url = string.Format(Path.UsrRouteWalkList, routeId);
            RESTAPI.Instance.Post<RouteWalkAPIResult>(url, routeWalkAPI, successCallback, errorCallback, headers);
        }

    }

    public class RouteWalkEvent
    {

        public static void BatchCreate(Int32 routeWalkId, RouteWalkEventAPIBatch batch, UnityAction<RouteWalkEventAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };


            string url = string.Format(Path.UsrRouteWalkEventList, routeWalkId);
            RESTAPI.Instance.Post<RouteWalkEventAPIList>(url, batch, successCallback, errorCallback, headers);
        }

    }

    public class RouteWalkPathLog
    {

        public static void BatchCreate(Int32 routeWalkId, RouteWalkPathAPIBatch batch, UnityAction<RouteWalkPathAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };


            string url = string.Format(Path.UsrRouteWalkPathLogList, routeWalkId);
            RESTAPI.Instance.Post<RouteWalkPathAPIList>(url, batch, successCallback, errorCallback, headers);
        }

    }

}
