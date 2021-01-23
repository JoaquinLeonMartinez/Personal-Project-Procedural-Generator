using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_DungeonManager : MonoBehaviour
{
    public TMP_InputField roomsToGenerateInput;
    public void OnClickGenerate()
    {
        if (!string.IsNullOrEmpty(roomsToGenerateInput.text))
        {
            DungeonManager.Instance.roomsToGenerate = int.Parse(roomsToGenerateInput.text.ToString());
        }

        DungeonManager.Instance.ResetDungeon();

        DungeonManager.Instance.GenerateDungeon();
    }

    public void OnClickClear()
    {
        DungeonManager.Instance.ResetDungeon();
    }

    public void OnClickStart()
    {

    }
}
