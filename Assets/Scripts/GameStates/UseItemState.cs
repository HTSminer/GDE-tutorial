using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UseItemState : State<GameController>
{
    [SerializeField] BattleSystem bs;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    // Output
    public bool ItemUsed { get; private set; }

    public static UseItemState i { get; private set; }
    Inventory inventory;

    private void Awake()
    {
        i = this;
        inventory = Inventory.GetInventory();
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        ItemUsed = false;

        StartCoroutine(UseItem());
    }

    IEnumerator UseItem()
    {
        var usedItem = inventoryUI.SelectedItem;
        var pokemon = partyScreen.SelectedMember;

        if (usedItem is TmItem)
        {
            yield return HandleTmItems();
        }
        else
        {
            // Handle Evolution Items
            if (usedItem is EvolutionItem)
            {
                var evolution = pokemon.CheckForEvolution(usedItem);
                if (evolution != null)
                {
                    yield return EvolutionState.i.Evolve(pokemon, evolution);
                }
                else
                {
                    yield return DialogManager.i.ShowDialogText($"It won't have any effect.");
                    gc.StateMachine.Pop();
                    yield break;
                }
            }

            usedItem = inventory.UseItem(usedItem, partyScreen.SelectedMember);

            if (usedItem != null)
            {
                ItemUsed = true;

                if (usedItem is RecoveryItem)
                    yield return DialogManager.i.ShowDialogText($"{PlayerController.i.Name} used {usedItem.Name}.");
            }
            else
            {
                if (inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                    yield return DialogManager.i.ShowDialogText($"It won't have any effect.");
            }
        }

        gc.StateMachine.Pop();
    }

    IEnumerator HandleTmItems()
    {
        var tmItem = inventoryUI.SelectedItem as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name} Already knows {tmItem.Move.Name}.");
            yield break;
        }

        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name} can't learn {tmItem.Move.Name}.");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumberOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name} learned {tmItem.Move.Name}.");
        }
        else
        {
            // Option to forget a move.
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name} wants to learn {tmItem.Move.Name}");
            yield return DialogManager.i.ShowDialogText($" But it cannot learn more than {PokemonBase.MaxNumberOfMoves} moves.");

            yield return DialogManager.i.ShowDialogText($"Would you like to learn {tmItem.Move.Name}?", true, false);

            MoveToForgetState.i.NewMove = tmItem.Move;
            MoveToForgetState.i.CurrentMoves = pokemon.Moves.Select(m => m.Base).ToList();
            yield return gc.StateMachine.PushAndWait(MoveToForgetState.i);

            var moveIndex = MoveToForgetState.i.Selection;
            if (moveIndex == PokemonBase.MaxNumberOfMoves || moveIndex == -1)
            {
                // Don't learn new move
                yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name} did not learn {tmItem.Move.Name}.");
            }
            else
            {
                // Forget old move and learn the new move
                var selectedMove = pokemon.Moves[moveIndex].Base;
                yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name} forgot {selectedMove.Name} and learned {tmItem.Move.Name}.");
                
                pokemon.Moves[moveIndex] = new Move(tmItem.Move);
            }
        }
    }

}
