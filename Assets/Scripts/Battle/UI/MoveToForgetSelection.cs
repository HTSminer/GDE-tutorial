using PKMNUtils.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveToForgetSelection : SelectionUI<TextSlot>
{
    [SerializeField] List<TMP_Text> moveText;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i=0; i<currentMoves.Count; ++i)
        {
            moveText[i].text = currentMoves[i].Name;
        }

        moveText[currentMoves.Count].text = newMove.Name;

        SetItems(moveText.Select(m => m.GetComponent<TextSlot>()).ToList());
    }
}
