﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaganiniRestAPI
{
    //Base URL for the Rest APi
    public const string serverURL = "https://192.168.178.24:3000/";

    //getUserProfile  param: apitoken: ""
    public const string getUserProfile = serverURL + "userManagement/userProfile";
    //getUserBegehungen  param: apitoken: "", wegid: ""
    public static string getUserBegehungen(int wegid) { return serverURL + "weg/"+wegid.ToString()+"/begehung/"; }
    //getUserWege  param: apitoken: ""
    public const string getUserWege = serverURL + "weg/";

}
