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

    public static PartyState i { get; private set; }
    private void Awake() => i = this;

    private bool isMovingPokemon;
    private int selectedIndexForMoving = 0;

    // Outputs
    public Pokemon SelectedPokemon { get; private set; }

    // References
    private GameController _gameController;
    private PokemonParty _playerParty;

    // Events
    public static event Action<Pokemon> OnPokemonSelected;

    private void Start() => _playerParty = PlayerController.i.GetComponent<PokemonParty>();

    public override void Enter(GameController owner)
    {
        _gameController = owner;

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

    private void OnPokemonSelectedInternal(int selection)
    {
        SelectedPokemon = partyScreen.SelectedMember;
        StartCoroutine(PokemonSelectedAction(selection));
    }

    private IEnumerator PokemonSelectedAction(int selectedPokemonIndex)
    {
        var prevState = _gameController.StateMachine.GetPrevState();
        if (prevState == InventoryState.i)
        {
            // Use Item
            yield return GoToUseItemState();
        }
        else if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            DynamicMenuState.i.MenuItems = new List<string>() { "Switch", "Summary", "Cancel" };
            yield return _gameController.StateMachine.PushAndWait(DynamicMenuState.i);
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

                yield return ActionSelectionState.i.AddBattleAction(action);

                _gameController.StateMachine.Pop();
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                // Summary
                SummaryState.i.SelectedPokemon = selectedPokemonIndex;
                yield return _gameController.StateMachine.PushAndWait(SummaryState.i);
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

                var tmpPokemon = _playerParty.Pokemons[selectedIndexForMoving];
                _playerParty.Pokemons[selectedIndexForMoving] = _playerParty.Pokemons[selectedPokemonIndex];
                _playerParty.Pokemons[selectedPokemonIndex] = tmpPokemon;
                _playerParty.PartyUpdated();

                yield break;
            }

            DynamicMenuState.i.MenuItems = new List<string>() { select, move, "Cancel" };

            yield return _gameController.StateMachine.PushAndWait(DynamicMenuState.i);

            if (DynamicMenuState.i.SelectedItem == 0)
                if (select == "Summary")
                {
                    SummaryState.i.SelectedPokemon = selectedPokemonIndex;
                    yield return _gameController.StateMachine.PushAndWait(SummaryState.i);
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
                    yield return _gameController.StateMachine.PushAndWait(SummaryState.i);
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

    private IEnumerator GoToUseItemState()
    {
        yield return _gameController.StateMachine.PushAndWait(UseItemState.i);
        _gameController.StateMachine.Pop();
    }

    private void OnBack()
    {
        SelectedPokemon = null;

        var prevState = _gameController.StateMachine.GetPrevState();
        if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnits.Any(p => p.Pokemon.HP <= 0))
            {
                partyScreen.SetMessageText("You have to choose a Pokemon to continue.");
                return;
            }
        }

        _gameController.StateMachine.Pop();
    }
}
