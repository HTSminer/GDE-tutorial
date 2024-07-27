using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TMP_Text messageText;

    Pokemon _pokemon;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        SetMessage("");

        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        if (_pokemon.IsEgg)
        {
            hpBar.gameObject.SetActive(false);
            return;
        }

        hpBar.gameObject.SetActive(true);

        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
