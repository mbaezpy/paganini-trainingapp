using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DirectionIcon;
using static LocationUtils;

public class ERWAudioInstruction : BaseAudioInstruction
{
   
   public void PlayRecordingStarts(){
         PlaySound("RecordingStarts");
   }

   public void PlayAtStartingPoint(){
         PlaySound("AtStartingPoint");
    }

    public void PlayGoToStartingPoint(){
         PlaySound("GoToStartingPoint");
    }

    public void PlayAtDestination(){
         PlaySound("AtDestination");
    }

    public void PlayRecordingComplete(){
         PlaySound("RecordingComplete");
    }

    public void PlayPermissionRequired(){
         PlaySound("PermissionRequired");
    }

    protected void PlaySound(string filename)
    {
       PlaySound(filename, "v2/Sounds/ERW/Instruction_");
    }

}


