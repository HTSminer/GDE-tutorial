using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum Choice {  Yes, No }

public class AboutToUseState : State<BattleSystem>
{
    // Inputs
    public Pokemon NewPokemon { get; set; }
    public BattleUnit UnitToSwitch { get; set; }

    private Choice playerChoice = Choice.No;

    //bool aboutToUseChoice;
    public static AboutToUseState i { get; private set; }

    private void Awake() => i = this;

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        StartCoroutine(StartState());
    }

    IEnumerator StartState()
    {
        yield return bs.DialogBox.TypeDialog($"{bs.Trainer.Name} is about to use {NewPokemon.Base.Name}. Do you want to change Pokemon?");
        bs.DialogBox.EnableChoiceBox(true);
    }

    private IEnumerator HandleInput()
    {
        if (!bs.DialogBox.IsChoiceBoxEnabled)
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
        bs.DialogBox.EnableChoiceBox(false);
        AudioManager.i.PlaySfx(AudioID.UISelect);

        if (choice == Choice.Yes)
            yield return SwitchAndContinueBattle();
        else
            yield return ContinueBattle();
    }

    public override void Execute() => HandleInput();

    IEnumerator SwitchAndContinueBattle()
    {
        yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;
        if (selectedPokemon != null)
            yield return bs.SwitchPokemon(bs.CurrentUnit, selectedPokemon);

        yield return ContinueBattle();
    }

    IEnumerator ContinueBattle()
    {
        yield return bs.SendNextTrainerPokemon();
        bs.StateMachine.Pop();
    }
}
