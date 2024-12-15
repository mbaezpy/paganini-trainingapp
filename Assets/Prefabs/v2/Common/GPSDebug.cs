using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LocationUtils;

public class GPSDebug : MonoBehaviour
{

    public TMPro.TMP_Text LonText;
    public TMPro.TMP_Text LatText;
    public TMPro.TMP_Text AccText;
    public TMPro.TMP_Text TsText;
    public TMPro.TMP_Text LogText;

    public TMPro.TMP_Text POIDistance;
    public TMPro.TMP_Text SegDistance;
    public TMPro.TMP_Text SegBearing;
    public TMPro.TMP_Text UserHeading;
    public TMPro.TMP_Text SegmentHeading;

    public TMPro.TMP_Text PingText;

    private int pingNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        pingNum = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayGPS(Pathpoint pathpoint)
    {
        LonText.text = pathpoint.Longitude.ToString();
        LatText.text = pathpoint.Latitude.ToString();
        AccText.text = pathpoint.Accuracy.ToString();
        TsText.text = pathpoint.Timestamp.ToString();
    }

    //GPSDebugDisplay.DisplayMetrics(distance, minDis, minBearing, userHeading, segmentHeading);
    public void DisplayMetrics(double poiDistance, SegmentDistanceAndBearing segmentInfo)
    {
        POIDistance.text = poiDistance.ToString();

        POIDistance.text = poiDistance.ToString();
        SegDistance.text = segmentInfo.MinDistanceToSegment.ToString();
        SegBearing.text = segmentInfo.MinBearingDifference.ToString();

        UserHeading.text = segmentInfo.UserHeading.ToString();
        SegmentHeading.text = segmentInfo.SegmentHeading.ToString();
    }

    public void Log(string message)
    {
        LogText.text = message;
    }

    public void Ping()
    {
        pingNum++;
        PingText.text = pingNum.ToString();
    }

}
