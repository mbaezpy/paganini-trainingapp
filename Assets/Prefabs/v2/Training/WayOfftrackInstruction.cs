using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using static LocationUtils;

public class WayOfftrackInstruction : MonoBehaviour
{    
    [SerializeField] private GameObject WrongTurnMessage;
    [SerializeField] private GameObject MissedTurnMessage;
    [SerializeField] private GameObject WrongDirectionMessage;
    [SerializeField] private GameObject DeviationMessage;


    public event EventHandler<EventArgs<RouteWalkEventLog.RecoveryInstructionType>> OnRecoveryInstructionUsed;


    private static Dictionary<NavigationIssue, string> messageMap = new Dictionary<NavigationIssue, string>
    {
        { NavigationIssue.Deviation, "Sie sind von der Route abgewichen." },
        { NavigationIssue.WrongDirection, "Sie gehen in die falsche Richtung." },
        { NavigationIssue.MissedTurn, "Sie haben die Abzweigung verpasst." },
        { NavigationIssue.WrongTurn, "Sie haben die falsche Abzweigung genommen." }
    };
        

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadOfftrack(NavigationIssue issue)
    {
        WrongTurnMessage.SetActive(issue == NavigationIssue.WrongTurn);
        MissedTurnMessage.SetActive(issue == NavigationIssue.MissedTurn);
        WrongDirectionMessage.SetActive(issue == NavigationIssue.WrongDirection);
        DeviationMessage.SetActive(issue == NavigationIssue.Deviation);
    }

    public void LogRecoveryCompass()
    {
        TriggerRecoveryUsed(RouteWalkEventLog.RecoveryInstructionType.Compass);
    }

    public void LogRecoveryPause()
    {
        TriggerRecoveryUsed(RouteWalkEventLog.RecoveryInstructionType.NoIssue);
    }

    public void LogRecoveryCall()
    {
        TriggerRecoveryUsed(RouteWalkEventLog.RecoveryInstructionType.CallHelp);
    }

    private void TriggerRecoveryUsed(RouteWalkEventLog.RecoveryInstructionType recoveryType)
    {
        OnRecoveryInstructionUsed.Invoke(this, new EventArgs<RouteWalkEventLog.RecoveryInstructionType>(recoveryType));
    }

}
