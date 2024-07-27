using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BerryState { None, Planted, Sprouted, Growing, Flowering, Fruit }

public class BerryTree : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string ID;

    [SerializeField] Sprite drySoil;
    [SerializeField] Sprite dampSoil;
    [SerializeField] Sprite wetSoil;

    public BerryItem item;
    public BerryState state;
    public SpriteRenderer soil;

    private readonly int moisture;
    private readonly int amount = 4;

    private float PlantTimer { get; set; }

    private const int MinutesToSprout = 15;
    private const int MinutesToGrow = 30;
    private const int MinutesToFlower = 45;
    private const int MinutesToFruit = 60;
    private const int sIM = 3;

    private static BerryTree _activeBerryTree;

    public static BerryTree ActiveBerryTree
    {
        get { return _activeBerryTree; }
        set { _activeBerryTree = value; }
    }

    private void Awake()
    {
        PlantTimer = 0;
    }

    private void Start()
    {
        StartCoroutine(UpdateBerryTree());
    }

    public IEnumerator Interact(Transform initiator)
    {
        ActiveBerryTree = this;

        var dialog = BerryStageText();
        if (dialog != "")
            yield return DialogManager.i.ShowDialogText(dialog);

        if (item == null)
        {
            int selectedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"Would you like to plant a berry?",
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                GameController.i.StateMachine.Push(InventoryState.i);
            }
        }
        else if (item != null && soil.sprite == drySoil)
        {
            int selectedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"The soil is dry. Would you like to water it?",
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
                soil.sprite = wetSoil;
        }
        else
        {
            if (state == BerryState.Fruit)
            {
                initiator.GetComponent<Inventory>().AddItem(item, amount);

                AudioManager.i.PlaySfx(AudioID.ItemObtained, pauseMusic: true);

                state = BerryState.None;
                PlantTimer = 0;
                soil.sprite = drySoil;
                item = null;
            }
        }
    }

    public void PlantBerry(BerryItem berry)
    {
        item = berry;
        PlantTimer = 1;
        state = BerryState.Planted;
    }

    private string BerryStageText()
    {
        string playerName = PlayerController.i.Name;

        return state switch
        {
            BerryState.Planted => $"A {item.Name} was planted here.",
            BerryState.Sprouted => $"The {item.Name} has sprouted",
            BerryState.Growing => $"The {item.Name} plant is growing bigger",
            BerryState.Flowering => $"The {item.Name} plant is in bloom!",
            BerryState.Fruit => $"{playerName} added {amount} {item.Name} to the berries pouch.",
            _ => $""
        };
    }

    private void UpdateBerryState()
    {
        if ((state == BerryState.None || state == BerryState.Fruit) && PlantTimer == 0)
            return;

        PlantTimer++;

        if (PlantTimer < MinutesToSprout * sIM)
            state = BerryState.Planted;
        else if (PlantTimer < MinutesToGrow * sIM)
            state = BerryState.Sprouted;
        else if (PlantTimer < MinutesToFlower * sIM)
            state = BerryState.Growing;
        else if (PlantTimer < MinutesToFruit * sIM)
            state = BerryState.Flowering;
        else
            state = BerryState.Fruit;
    }

    private IEnumerator UpdateBerryTree()
    {
        while (true)
        {
            UpdateBerryState();
            yield return new WaitForSeconds(1f);
        }
    }

    public object CaptureState()
    {
        var saveData = new BerryTreeSaveData()
        {
            treeId = ID,
            berryType = item.name,
            plantTimer = PlantTimer
        };

        return saveData;
    }

    public void RestoreState(object s)
    {
        if (s is BerryTreeSaveData saveData)
        {
            if (ID == saveData.treeId)
            {
                item = (BerryItem)ItemDB.GetObjectByName(saveData.berryType);
                PlantTimer = saveData.plantTimer;
            }
        }
    }
}

[Serializable]
public struct BerryTreeSaveData
{
    public string treeId;
    public string berryType;
    public float plantTimer;
}

