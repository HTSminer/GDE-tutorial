using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreen;

    int selectedPage = 0;

    // Input
    public int SelectedPokemon { get; set; }

    public static SummaryState i { get; private set; }

    private void Awake() => i = this;

    List<Pokemon> playerParty;
    private void Start() => playerParty = PlayerController.i.GetComponent<PokemonParty>().Pokemons;

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        summaryScreen.state = SummaryStates.Info;

        summaryScreen.gameObject.SetActive(true);
        summaryScreen.SetBasicDetails(playerParty[SelectedPokemon]);
        summaryScreen.ShowPage(selectedPage);
    }

    public override void Execute()
    {
        if (!summaryScreen.InMoveSelection)
        {
            // Page Selection
            int prevPage = selectedPage;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                selectedPage = Mathf.Abs((selectedPage - 1) % 2);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                selectedPage = (selectedPage + 1) % 2;

            if (selectedPage != prevPage)
                summaryScreen.ShowPage(selectedPage);

            // Pokemon Selection
            int prevSelection = SelectedPokemon;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SelectedPokemon += 1;

                if (SelectedPokemon >= playerParty.Count)
                    SelectedPokemon = 0;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SelectedPokemon -= 1;

                if (SelectedPokemon <= 0)
                    SelectedPokemon = playerParty.Count - 1;
            }

            if (SelectedPokemon != prevSelection)
            {
                summaryScreen.SetBasicDetails(playerParty[SelectedPokemon]);
                summaryScreen.ShowPage(selectedPage);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedPage == 1 && !summaryScreen.InMoveSelection)
            {
                summaryScreen.InMoveSelection = true;
                selectedPage = 2;
                summaryScreen.ShowPage(selectedPage);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (summaryScreen.InMoveSelection)
            {
                summaryScreen.InMoveSelection = false;
                selectedPage = 1;
                summaryScreen.ShowPage(selectedPage);
            }
            else
            {
                gc.StateMachine.Pop();
                return;
            }
        }

        summaryScreen.HandleUpdate();
    }

    public override void Exit() => summaryScreen.gameObject.SetActive(false);
}
