using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum Choice {  Yes, No }

public class AboutToUseState : State<BattleSystem>
{
    public static AboutToUseState i { get; private set; }
    private void Awake() => i = this;

    private Choice playerChoice = Choice.No;

    // Inputs
    public Pokemon NewPokemon { get; set; }

    // References
    private BattleSystem _battleSystem;
    private ActionSelectionState actionState;
    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        actionState = _battleSystem.GetComponent<ActionSelectionState>();
        StartCoroutine(StartState());
    }

    public override void Execute() => StartCoroutine(HandleInput());

    private IEnumerator StartState()
    {
        yield return _battleSystem.DialogBox.TypeDialog($"{_battleSystem.Trainer.Name} is about to use {NewPokemon.Base.Name}. Do you want to change Pokemon?");
        _battleSystem.DialogBox.EnableChoiceBox(true);
    }

    private IEnumerator HandleInput()
    {
        if (!_battleSystem.DialogBox.IsChoiceBoxEnabled)
            yield break;

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerChoice = (playerChoice == Choice.Yes) ? Choice.No : Choice.Yes;
            AudioManager.i.PlaySfx(AudioID.UISelect);
        }

        if (Input.GetKeyDown(KeyCode.Space))
            yield return HandleChoice(playerChoice);
        else if (Input.GetKeyDown(KeyCode.Escape))
            yield return HandleChoice(Choice.No);
    }

    private IEnumerator HandleChoice(Choice choice)
    {
        _battleSystem.DialogBox.EnableChoiceBox(false);
        AudioManager.i.PlaySfx(AudioID.UISelect);

        if (choice == Choice.Yes)
            yield return SwitchAndContinueBattle();
        else
            yield return ContinueBattle();
    }

    private IEnumerator SwitchAndContinueBattle()
    {
        yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;
        if (selectedPokemon != null)
            yield return _battleSystem.SwitchPokemon(_battleSystem.CurrentUnit, selectedPokemon);

        yield return ContinueBattle();
    }

    private IEnumerator ContinueBattle()
    {
        yield return _battleSystem.SendNextTrainerPokemon();
        _battleSystem.StateMachine.Pop();
    }
}
