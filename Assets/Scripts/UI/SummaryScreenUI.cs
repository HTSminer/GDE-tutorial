using PKMNUtils.GenericSelectionUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SummaryStates { Info, Memo, Skills, Moves, Ribbons }

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("Basic Details")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] Image image;

    [Header("Pages")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] List<Sprite> bg;
    [SerializeField] GameObject infoPage;
    [SerializeField] GameObject pokemonInfo;
    [SerializeField] GameObject movesPage;
    [SerializeField] GameObject moveDataPage;

    [Header("Pokemon Stats")]
    [SerializeField] TMP_Text dexText;
    [SerializeField] TMP_Text speciesText;
    [SerializeField] Image typeIcon1;
    [SerializeField] Image typeIcon2;
    [SerializeField] TMP_Text otText;
    [SerializeField] TMP_Text idText;
    [SerializeField] TMP_Text expPointsText;
    [SerializeField] TMP_Text nextLevelExpText;
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text itemText;
    [SerializeField] Transform expBar;

    [Header("Current Pokemon Moves")]
    [SerializeField] List<Image> moveTypeSprites;
    [SerializeField] List<TMP_Text> moveNames;
    [SerializeField] List<TMP_Text> movePPs;
    [SerializeField] TMP_Text moveDescText;
    [SerializeField] TMP_Text movePowerText;
    [SerializeField] TMP_Text moveAccText;
    [SerializeField] GameObject moveEffectsUI;

    [Header("Pokemon Type Icons")]
    [SerializeField] List<Sprite> typeSprites;

    List<TextSlot> moveSlots;
    private void Start()
    {
        moveSlots = moveNames.Select(m => m.GetComponent<TextSlot>()).ToList();
        moveEffectsUI.SetActive(false);
        moveDescText.text = "";
    }

    private bool inMoveSelection;
    public bool InMoveSelection 
    { 
        get => inMoveSelection;
        set
        {
            inMoveSelection = value;

            if (inMoveSelection)
            {
                moveEffectsUI.SetActive(true);
                SetItems(moveSlots.Take(pokemon.Moves.Count).ToList());
            }
            else
            {
                moveEffectsUI.SetActive(true);
                moveDescText.text = "";
                ClearItems();
            }
        }
    }

    public SummaryStates state;

    Pokemon pokemon;
    public void SetBasicDetails(Pokemon pokemon)
    {
        this.pokemon = pokemon;

        nameText.text = pokemon.Base.name;
        levelText.text = "" + pokemon.Level;
        image.sprite = pokemon.Base.FrontSprite;
        dexText.text = pokemon.Base.Id.ToString();
        speciesText.text = pokemon.Base.name;

        SetType(typeIcon1, pokemon.Base.Type1);
        SetType(typeIcon2, pokemon.Base.Type2);

        //SetItem();
    }

    public void ShowPage(int pageNum)
    {
        if (pageNum == 0)
        {
            // Show Info Page
            headerText.text = "Info";
            this.GetComponent<Image>().sprite = bg[0];

            pokemonInfo.SetActive(true);
            infoPage.SetActive(true);
            movesPage.SetActive(false);

            SetInfo();
        }
        else if (pageNum == 1)
        {
            // Show Moves Page
            headerText.text = "Moves";
            this.GetComponent<Image>().sprite = bg[3];

            pokemonInfo.SetActive(true);
            infoPage.SetActive(false);
            movesPage.SetActive(true);

            SetMoves();
        } 
        else if  (pageNum == 2)
        {
            headerText.text = "Move Info";
            this.GetComponent<Image>().sprite = bg[5];

            pokemonInfo.SetActive(false);
            infoPage.SetActive(false);
            movesPage.SetActive(true);
        }
    }

    public void SetInfo()
    {
        
        otText.text = PlayerController.i.Name;
        idText.text = PlayerController.i.ID.ToString();

        expPointsText.text = "" + pokemon.Exp;
        nextLevelExpText.text = "" + (pokemon.Base.GetExpForLevel(pokemon.Level + 1) - pokemon.Exp);
        expBar.localScale = new Vector2(pokemon.GetNormalizedExp(), 1);
    }

    public void SetType(Image typeIcon, PokemonType type)
    {
        if (type != PokemonType.None)
        {
            typeIcon.enabled = true;
            typeIcon.sprite = SetTypeImg(type);
        }
        else
            typeIcon.enabled = false;
    }

    Sprite SetTypeImg(PokemonType type)
    {
        Sprite s;
        s = type switch
        {
            PokemonType.None => null,
            PokemonType.Normal => typeSprites[0],
            PokemonType.Fire => typeSprites[10],
            PokemonType.Water => typeSprites[11],
            PokemonType.Grass => typeSprites[12],
            PokemonType.Electric => typeSprites[13],
            PokemonType.Ice => typeSprites[15],
            PokemonType.Fighting => typeSprites[1],
            PokemonType.Poison => typeSprites[3],
            PokemonType.Ground => typeSprites[4],
            PokemonType.Flying => typeSprites[2],
            PokemonType.Psychic => typeSprites[14],
            PokemonType.Bug => typeSprites[6],
            PokemonType.Rock => typeSprites[5],
            PokemonType.Ghost => typeSprites[16],
            PokemonType.Dark => typeSprites[17],
            PokemonType.Steel => typeSprites[8],
            PokemonType.Fairy => typeSprites[18],
            _ => null
        };

        return s;
    }

    public void SetMoves()
    {
        for (int i = 0; i < moveNames.Count; i++)
        {
            if (i < pokemon.Moves.Count)
            {
                var move = pokemon.Moves[i];

                SetType(moveTypeSprites[i], move.Base.Type);
                moveNames[i].text = move.Base.name;
                movePPs[i].text = $"{move.PP}/{move.Base.PP}";
            }
            else
            {
                SetType(moveTypeSprites[i], PokemonType.None);
                moveNames[i].text = "-";
                movePPs[i].text = "-";
            }
        }
    }

    //public void setitem()
    //{
    //    if (pokemon.holdingitem)
    //    {
    //        itemtext.text = pokemon.helditem.name;

    //        itemicon.enabled = true;
    //        itemicon.sprite = pokemon.helditem.icon;
    //    }
    //    else
    //    {
    //        itemtext.text = "none";
    //        itemicon.enabled = false;
    //    }
    //}

    public override void HandleUpdate()
    {
        if (InMoveSelection)
            base.HandleUpdate();
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var move = pokemon.Moves[selectedItem];

        moveDescText.text = move.Base.Description;
        movePowerText.text = "" + move.Base.Power;
        moveAccText.text = "" + move.Base.Accuracy;
    }
}
