using PKMNUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    public static GameController i { get; private set; }

    private void Awake()
    {
        i = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        AbilityDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    // Outputs
    public StateMachine<GameController> StateMachine { get; private set; }
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    // References
    TrainerController trainer;

    private void Start()
    {
        StateMachine = new StateMachine<GameController>(this);
        StateMachine.ChangeState(FreeRoamState.i);

        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.i.OnShowDialog += () =>
        {
            StateMachine.Push(DialogState.i);
        };

        DialogManager.i.OnDialogFinished += () =>
        {
            StateMachine.Pop();
        };
    }

    private void Update() => StateMachine.Execute();

    public void PauseGame(bool pause)
    {
        if (pause)
            StateMachine.Push(PauseState.i);
        else
            StateMachine.Pop();
    }

    public void StartBattle(BattleTrigger trigger)
    {
        BattleState.i.trigger = trigger;
        StateMachine.Push(BattleState.i);
    }

    public void StartTrainerBattle(TrainerController trainer, int unitCount)
    {
        BattleState.i.trainer = trainer;
        StateMachine.Push(BattleState.i);
    }

    public void OnEnterTrainersView(TrainerController trainer) => StartCoroutine(trainer.TriggerTrainerBattle(playerController));

    private void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        partyScreen.SetPartyData();

        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<PokemonParty>();
        bool hasEvolutions = playerParty.CheckForEvolutions();

        if (hasEvolutions)
            StartCoroutine(playerParty.RunEvolutions());
        else
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);

    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut=false)
    {
        yield return Fader.i.FadeIn(0.5f);

        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        if (waitForFadeOut)
            yield return Fader.i.FadeOut(0.5f);
        else
            StartCoroutine(Fader.i.FadeOut(0.5f));
    }

    private void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = 24;

        GUILayout.Label("STATE STACK", style);
        foreach (var state in StateMachine.StateStack)
        {
            GUILayout.Label(state.GetType().ToString(), style);
        }
    }

    public PlayerController PlayerController { get => playerController; set => playerController = value; }
    public BattleSystem BattleSystem => battleSystem;
    public Camera WorldCamera => worldCamera;
    public PartyScreen PartyScreen => partyScreen;
}
