using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialog : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;

    [SerializeField] TMP_Text dialogText;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<TMP_Text> moveTexts;

    [SerializeField] TMP_Text yesText;
    [SerializeField] TMP_Text noText;

    Color highlightedColor;

    public static BattleDialog i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
    }

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public bool IsChoiceBoxEnabled => choiceBox.activeSelf;

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else
                moveTexts[i].text = "-";
        }
    }
}