using System;
using System.Collections.Generic;
using Newtonsoft.Json;
public interface ISocialWorkerAPI
{
    public int socialw_id { get; set; }
    public string socialw_username { get; set; }
    public string socialw_firstname { get; set; }
    public string socialw_sirname { get; set; }
    public string socialw_photo { get; set; }
    public WorkshopAPI socialw_workshop { get; set; }
}


public class SocialWorkerAPIBase : BaseAPI
{
    public string socialw_username { get; set; }
    public string socialw_firstname { get; set; }
    public string socialw_sirname { get; set; }
    //public string socialw_photo { get; set; }
}


[System.Serializable]
public class SocialWorkerAPI : SocialWorkerAPIBase, ISocialWorkerAPI
{
    [JsonIgnore]
    public int socialw_id { get; set; }

    [JsonIgnore]
    public WorkshopAPI socialw_workshop { get; set; }

    [JsonProperty]
    public string socialw_photo { get; set; }
}

[System.Serializable]
public class SocialWorkerAPIUpdate : SocialWorkerAPIBase, ISocialWorkerAPI
{
    [JsonIgnore]
    public int socialw_id { get; set; }

    [JsonIgnore]
    public WorkshopAPI socialw_workshop { get; set; }

    [JsonIgnore]
    public Dictionary<string, byte[]> files;    

    [JsonIgnore]
    public string socialw_photo { get; set; }
}

[System.Serializable]
public class SocialWorkerAPIResult : SocialWorkerAPIBase, ISocialWorkerAPI
{
    [JsonProperty]
    public int socialw_id { get; set; }

    [JsonProperty]
    public WorkshopAPI socialw_workshop { get; set; }

    [JsonProperty]
    public string socialw_photo { get; set; }    
}

[System.Serializable]
public class WorkshopAPI
{
    public int works_id;
    public string works_name;
    public string works_street;
    public string works_city;
    public string works_zip;
    public int? works_type;
}
