using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { Items, Medicine, Pokeballs, TMs, Berries, Mail, Battle, Key }

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> medSlots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;
    [SerializeField] List<ItemSlot> berrySlots;
    [SerializeField] List<ItemSlot> mailSlots;
    [SerializeField] List<ItemSlot> battleSlots;
    [SerializeField] List<ItemSlot> keySlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;


    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { slots, medSlots, pokeballSlots, tmSlots, berrySlots, mailSlots, battleSlots, keySlots };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS", "MEDICINE", "POKEBALLS", "TMS & HMS", "BERRIES", "MAIL", "BATTLE ITEMS", "KEY ITEMS"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int catIndex)
    {
        var currSlots = GetSlotsByCategory(catIndex);
        return currSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        return UseItem(item, selectedPokemon);
    }

    public ItemBase UseItem(ItemBase item, Pokemon selectedPokemon)
    {
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            if (!item.IsReusable)
                RemoveItem(item);

            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count=1)
    {
        var category = (int)GetCategoryFromItem(item);
        var currSlots = GetSlotsByCategory(category);

        var itemSlot = currSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    public int GetItemCount(ItemBase item)
    {
        var category = (int)GetCategoryFromItem(item);
        var currSlots = GetSlotsByCategory(category);

        var itemSlot = currSlots.FirstOrDefault(slot => slot.Item == item);

        if (itemSlot != null)
            return itemSlot.Count;
        else
            return 0;
    }

    public void RemoveItem(ItemBase item, int countToRemove=1)
    {
        var category = (int)GetCategoryFromItem(item);
        var currSlots = GetSlotsByCategory(category);

        var itemSlot = currSlots.First(slot => slot.Item == item);
        itemSlot.Count -= countToRemove;
        if (itemSlot.Count == 0)
            currSlots.Remove(itemSlot);

        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        var category = (int)GetCategoryFromItem(item);
        var currSlots = GetSlotsByCategory(category);

        return currSlots.Exists(slot => slot.Item == item);
    }

    ItemCategory GetCategoryFromItem(ItemBase item) 
    {
        if (item is RecoveryItem)
            return ItemCategory.Medicine;
        else if (item is EvolutionItem)
            return ItemCategory.Items;
        else if (item is PokeballItem)
            return ItemCategory.Pokeballs;
        else if (item is BerryItem)
            return ItemCategory.Berries;
        else if (item is BattleItem)
            return ItemCategory.Battle;
        else
            return ItemCategory.TMs;
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList(),
            berries = berrySlots.Select(i => i.GetSaveData()).ToList(),
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();
        berrySlots = saveData.berries.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>>() { slots, medSlots, pokeballSlots, tmSlots, berrySlots, mailSlots, battleSlots, keySlots };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }

    public int Count
    {
        get => count;
        set => count = value;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
    public List<ItemSaveData> berries;
}