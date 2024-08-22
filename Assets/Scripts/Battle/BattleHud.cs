using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] TMP_Text genderText;
    [SerializeField] Text hpText;
    [SerializeField] Image megaIcon;
    [SerializeField] Image statusSprite;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Sprite psnSprite;
    [SerializeField] Sprite slpSprite;
    [SerializeField] Sprite brnSprite;
    [SerializeField] Sprite frzSprite;
    [SerializeField] Sprite parSprite;

    Pokemon _pokemon;
    Dictionary<ConditionID, Sprite> statusSprites;

    public Image MegaIcon => megaIcon;

    private void Update()
    {
        hpText.text = _pokemon.HP + " / " + _pokemon.MaxHp;
    }

    public void SetData(Pokemon pokemon)
    {
        if (_pokemon != null)
        {
            _pokemon.OnHPChanged -= UpdateHP;
            _pokemon.OnStatusChanged -= SetStatusImg;
            SetGenderText();
        }

        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        SetExp();
        
        statusSprites = new Dictionary<ConditionID, Sprite>()
        {
            { ConditionID.poison, psnSprite },
            { ConditionID.sleep, slpSprite },
            { ConditionID.burn, brnSprite },
            { ConditionID.paralysis, parSprite },
            { ConditionID.freeze, frzSprite }
        };
        
        SetStatusImg();
        _pokemon.OnStatusChanged += SetStatusImg;
        _pokemon.OnHPChanged += UpdateHP;
    }

    void SetStatusImg()
    {
        if (_pokemon.Status == null)
        {
            statusSprite.enabled = false;
        }
        else
        {
            statusSprite.enabled = true;
            statusSprite.sprite = statusSprites[_pokemon.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = _pokemon.GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public void SetGenderText()
    {
        switch (_pokemon.Gender)
        {
            case PokemonGender.Female:
                char f = '\u2640';
                genderText.text = f.ToString();
                genderText.color = Color.magenta;
                break;
            case PokemonGender.Male:
                char m = '\u2642';
                genderText.text = m.ToString();
                genderText.color = Color.blue;
                break;
            default:
                genderText.text = "";
                break;
        }

    }

    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = _pokemon.GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public void ClearData()
    {
        if (_pokemon != null)
        {
            _pokemon.OnHPChanged -= UpdateHP;
            _pokemon.OnStatusChanged -= SetStatusImg;
            _pokemon.PrevMoveUsed = null;
        }
    }
}
