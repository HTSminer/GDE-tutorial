using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ShopBuyingState : State<GameController>
{
    [SerializeField] Vector2 shopCameraOffset;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public static ShopBuyingState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    // Input
    public List<ItemBase> AvailableItems { get; set; }

    bool browseItems = false;

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        browseItems = false;
        StartCoroutine(StartBuyingState());
    }

    public override void Execute()
    {
        if (browseItems)
            shopUI.HandleUpdate();
    }

    IEnumerator StartBuyingState()
    {
        yield return GameController.i.MoveCamera(shopCameraOffset);
        walletUI.Show();
        shopUI.Show(AvailableItems, (item) => StartCoroutine(BuyItem(item)),
            () => StartCoroutine(OnBackFromBuying()));

        browseItems = true;
    }


    IEnumerator BuyItem(ItemBase item)
    {
        browseItems = false;

        yield return DialogManager.i.ShowDialogText($"How many would you like to buy?",
            waitForInput: false, autoClose: false);

        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Cost,
            (selectedCount) => countToBuy = selectedCount);

        DialogManager.i.CloseDialog();

        float totalPrice = item.Cost * countToBuy;

        if (Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"That will be {totalPrice}.",
                waitForInput: false,
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if (selectedChoice == 0)
            {
                // Yes
                inventory.AddItem(item, countToBuy);
                Wallet.i.TakeMoney(totalPrice);
                yield return DialogManager.i.ShowDialogText($"Thank you for shopping with us!");
            }
        }
        else
        {
            yield return DialogManager.i.ShowDialogText($"You don't have enough money for that.");
        }

        browseItems = true;
    }

    IEnumerator OnBackFromBuying()
    {
        yield return GameController.i.MoveCamera(-shopCameraOffset);
        shopUI.Close();
        walletUI.Close();
        gc.StateMachine.Pop();
    }
}
