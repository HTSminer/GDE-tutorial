using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;

    public static BattleState i { get; private set; }
    private void Awake() => i = this;

    // Inputs
    public BattleTrigger trigger { get; set; }
    public TrainerController trainer { get; set; }

    // References
    private GameController _gameController;

    public override void Enter(GameController owner)
    {
        _gameController = owner;

        battleSystem.gameObject.SetActive(true);
        _gameController.WorldCamera.gameObject.SetActive(false);

        var playerParty = _gameController.PlayerController.GetComponent<PokemonParty>();

        if (trainer == null)
        {
            var wildPokemon = _gameController.CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(trigger);
            var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
            StartCoroutine(battleSystem.StartBattle(playerParty, wildPokemonCopy, trigger));
        }
        else
        {
            var trainerParty = trainer.GetComponent<PokemonParty>();
            StartCoroutine(battleSystem.StartTrainerBattle(playerParty, trainerParty, unitCount: trainer.BattleUnitCount));
        }

        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute() => battleSystem.HandleUpdate();

    public override void Exit()
    {
        battleSystem.gameObject.SetActive(false);
        _gameController.WorldCamera.gameObject.SetActive(true);

        battleSystem.OnBattleOver -= EndBattle;
    }

    private void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        _gameController.StateMachine.Pop();
    }

    public BattleSystem BattleSystem => battleSystem;
}
