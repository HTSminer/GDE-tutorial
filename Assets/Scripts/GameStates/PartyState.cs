using PKMNUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;

    public Pokemon SelectedPokemon { get; private set; }

    bool isMovingPokemon;
    int selectedIndexForMoving = 0;

    public static PartyState i { get; private set; }
    public static event Action<Pokemon> OnPokemonSelected;

    private void Awake() => i = this;

    PokemonParty playerParty;
    private void Start() => playerParty = PlayerController.i.GetComponent<PokemonParty>();

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        SelectedPokemon = null;
        partyScreen.gameObject.SetActive(true);
        partyScreen.OnSelected += OnPokemonSelectedInternal;
        partyScreen.OnBack += OnBack;
    }

    public override void Execute() => partyScreen.HandleUpdate();

    public override void Exit()
    {
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnPokemonSelectedInternal;
        partyScreen.OnBack -= OnBack;
    }

    void OnPokemonSelectedInternal(int selection)
    {
        SelectedPokemon = partyScreen.SelectedMember;
        StartCoroutine(PokemonSelectedAction(selection));
    }

    IEnumerator PokemonSelectedAction(int selectedPokemonIndex)
    {
        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == InventoryState.i)
        {
            // Use Item
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            DynamicMenuState.i.MenuItems = new List<string>() { "Switch", "Summary", "Cancel" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if (DynamicMenuState.i.SelectedItem == 0)
            {
                // Switch Pokemon
                if (SelectedPokemon.HP <= 0)
                {
                    partyScreen.SetMessageText("You can't send out a fainted pokemon.");
                    yield break;
                }
                if (battleState.BattleSystem.PlayerUnits.Any(p => p.Pokemon == SelectedPokemon))
                {
                    partyScreen.SetMessageText("You can't switch with an active pokemon.");
                    yield break;
                }

                var action = new BattleAction()
                {
                    Type = ActionType.SwitchPokemon,
                    User = BattleSystem.i.CurrentUnit,
                    SelectedPokemon = SelectedPokemon
                };

                ActionSelectionState.i.AddBattleAction(action);

                gc.StateMachine.Pop();
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                // Summary
                SummaryState.i.SelectedPokemon = selectedPokemonIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i);
            }
            else
            {
                yield break;
            }
        }
        else
        {
            string select = !NPCController.i.BreederActive ? "Summary" : "Select";
            string move = !NPCController.i.BreederActive ? "Move" : "Summary";

            if (isMovingPokemon)
            {
                if (selectedIndexForMoving == selectedPokemonIndex)
                {
                    partyScreen.SetMessageText("You can't switch with the same pokemon.");
                    yield break;
                }

                isMovingPokemon = false;

                var tmpPokemon = playerParty.Pokemons[selectedIndexForMoving];
                playerParty.Pokemons[selectedIndexForMoving] = playerParty.Pokemons[selectedPokemonIndex];
                playerParty.Pokemons[selectedPokemonIndex] = tmpPokemon;
                playerParty.PartyUpdated();

                yield break;
            }

            DynamicMenuState.i.MenuItems = new List<string>() { select, move, "Cancel" };

            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);

            if (DynamicMenuState.i.SelectedItem == 0)
                if (select == "Summary")
                {
                    SummaryState.i.SelectedPokemon = selectedPokemonIndex;
                    yield return gc.StateMachine.PushAndWait(SummaryState.i);
                }
                else
                {
                    OnPokemonSelected?.Invoke(SelectedPokemon);
                }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                if (move == "Summary")
                {
                    // Summary
                    SummaryState.i.SelectedPokemon = selectedPokemonIndex;
                    yield return gc.StateMachine.PushAndWait(SummaryState.i);
                }
                else
                {
                    // Move Pokemon
                    isMovingPokemon = true;
                    selectedIndexForMoving = selectedPokemonIndex;
                    partyScreen.SetMessageText("Choose a Pokemon to switch position with.");
                }
            }
            else
            {
                yield break;
            }
        }
    }

    IEnumerator GoToUseItemState()
    {
        yield return gc.StateMachine.PushAndWait(UseItemState.i);
        gc.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedPokemon = null;

        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnits.Any(p => p.Pokemon.HP <= 0))
            {
                partyScreen.SetMessageText("You have to choose a Pokemon to continue.");
                return;
            }
        }

        gc.StateMachine.Pop();
    }
}
