using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Breeder : MonoBehaviour
{
    [SerializeField] Pokemon pokemonEgg;

    // Inputs
    public Pokemon Pokemon1 { get; set; }
    public Pokemon Pokemon2 { get; set; }
    public Pokemon ChangedPokemon { get; private set; }

    // Outputs
    public PokemonEgg Egg { get; private set; }
    public bool HasEgg { get; private set; } = false;

    // Private Variables
    private int eggNumber = 0;
    private int cycleSteps = 2;
    private int tilesTraveled;
    private PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<PlayerController>();
        player.OnTileMoved += OnTileMoved;
        tilesTraveled = 0;
        HasEgg = false;
    }

    public IEnumerator Breed()
    {
        player.Character.Animator.IsMoving = false;

        // Handle if Nursery has an Egg ready for player to pick up.
        #region Handle Egg pick up
        if (HasEgg)
        {
            int eggChoice = 0;
            yield return DialogManager.i.ShowDialogText(
                    "We have an egg!" +
                    "Would you like to take the egg?",
                    choices: new List<string>() { "Yes", "No" },
                    onChoiceSelected: (choiceIndex) => eggChoice = choiceIndex
                    );

            if (eggChoice == 0)
            {
                player.Party.AddEgg(Egg);
                pokemonEgg.IsEgg = true;
                player.Party.AddPokemon(pokemonEgg);

                yield return DialogManager.i.ShowDialogText($"Take care of the egg!");
            }
            else if (eggChoice == 1)
            {
                yield return DialogManager.i.ShowDialogText($"Then I will take care of this egg myself!");
            }

            HasEgg = false;
            Egg = null;
        }
        #endregion
        else
        {
            // Handle Pokemon relations when there are 2 Pokemon in Nursery.
            #region Handle Relations
            int startChoice = 0;
            string relationText = "";
            string pkmnText = "";

            if (Pokemon1 != null && Pokemon2 != null)
            {
                pkmnText = $"Hi {player.Name}! {Pokemon1.Base.Name} and {Pokemon2.Base.Name} are doing fine.";

                bool matchingOT = Pokemon1.Trainer == Pokemon2.Trainer;
                bool matchingSpecies = Pokemon1.Base.Species == Pokemon2.Base.Species;

                if (Pokemon1 != null && Pokemon2 != null)
                {
                    if (Pokemon1.Gender != Pokemon2.Gender && HasMatchingEggGroup())
                    {
                        relationText = matchingSpecies
                            ? (matchingOT ? "They seem to get along all right." : "They really seem to like hanging out!")
                            : (matchingOT ? "They don't seem to like each other very much, though." : "They seem to get along all right.");

                        eggNumber = matchingSpecies
                            ? (matchingOT ? 50 : 70)
                            : (matchingOT ? 20 : 50);
                    }
                    else
                        relationText = "They don't seem to like playing together, though.";
                }
            }
            else if (Pokemon1 != null || Pokemon2 != null)
            {
                var pokemon = Pokemon1 != null ? Pokemon1.Base.Name : Pokemon2.Base.Name;
                pkmnText = $"Hi {player.Name}! {pokemon} is doing fine.";
            }
            else
            {
                pkmnText = $"Hi {player.Name}! Would you like to leave a Pokemon with the Nursery today?";
            }
            #endregion

            // Handle Leaving a Pokemon or Picking up Pokemon from the Nursery.
            #region Handle Pick-up / Drop-off
            yield return DialogManager.i.ShowDialogText(
                $"{pkmnText} {relationText}",
                choices: new List<string>() { "Leave Pokemon", "Take Pokemon", "Nothing" },
                onChoiceSelected: (choiceIndex) => startChoice = choiceIndex
                );

            if (startChoice == 0)
            {
                if (Pokemon1 != null && Pokemon2 != null)
                {
                    yield return DialogManager.i.ShowDialogText($"There is not enough space to leave another Pokemon.");
                    yield break;
                }

                NPCController.i.BreederActive = true;
                PartyState.OnPokemonSelected += HandlePokemonSelected;
                GameController.i.StateMachine.Push(PartyState.i);

                yield return new WaitUntil(() => GameController.i.StateMachine.CurrentState == FreeRoamState.i);

                if (Pokemon1 == ChangedPokemon || Pokemon2 == ChangedPokemon)
                {
                    yield return DialogManager.i.ShowDialogText($"We will take good care of {ChangedPokemon.Base.Name}.");
                    player.Party.RemovePokemon(ChangedPokemon == Pokemon1 ? Pokemon1 : Pokemon2);
                }
                else
                    Debug.LogError("Error in Leave Pokemon Choices");

                NPCController.i.BreederActive = false;
            }
            else if (startChoice == 1)
            {
                if (Pokemon1 == null && Pokemon2 == null)
                {
                    yield return DialogManager.i.ShowDialogText($"There are no Pokemon in the Day Care Center to return.");
                    yield break;
                }

                List<string> choices = new List<string>();
                if (Pokemon1 != null) choices.Add(Pokemon1.Base.Name);
                if (Pokemon2 != null) choices.Add(Pokemon2.Base.Name);
                if (Pokemon1 != null && Pokemon2 != null) choices.Add("Both");
                choices.Add("Cancel");

                int takeChoice = 0;

                yield return DialogManager.i.ShowDialogText(
                    $"Sure thing. Which Pokemon will you be taking back?",
                    choices: choices,
                    onChoiceSelected: (choiceIndex) => takeChoice = choiceIndex
                    );

                switch (takeChoice)
                {
                    case 0:
                        if (Pokemon1 != null)
                        {
                            yield return DialogManager.i.ShowDialogText($"{Pokemon1.Base.Name} has returned to your party.");
                            player.Party.AddPokemon(Pokemon1);
                            Pokemon1 = null;
                        }
                        else if (Pokemon2 != null)
                        {
                            yield return DialogManager.i.ShowDialogText($"{Pokemon2.Base.Name} has returned to your party.");
                            player.Party.AddPokemon(Pokemon2);
                            Pokemon2 = null;
                        }
                        break;
                    case 1:
                        if (Pokemon2 == null)
                            yield return DialogManager.i.ShowDialogText($"Okay! Whenever your ready to take a Pokemon back, just speak to me again.");
                        else
                        {
                            yield return DialogManager.i.ShowDialogText($"{Pokemon2.Base.Name} has returned to your party.");
                            player.Party.AddPokemon(Pokemon2);
                            Pokemon2 = null;
                        }
                        break;
                    case 2:
                        if (Pokemon1 != null && Pokemon2 != null)
                        {
                            yield return DialogManager.i.ShowDialogText($"{Pokemon1.Base.Name} and {Pokemon2.Base.Name} have both returned to your party.");
                            player.Party.AddPokemon(Pokemon1);
                            player.Party.AddPokemon(Pokemon2);
                            Pokemon1 = null;
                            Pokemon2 = null;
                        }
                        break;
                    case 3:
                        yield return DialogManager.i.ShowDialogText($"Okay! Whenever you're ready to take a Pokemon back, just speak to me again.");
                        break;
                }
            }
            else if (startChoice == 2)
                yield return DialogManager.i.ShowDialogText($"Okay! Come back if you change your mind.");
            #endregion
        }
    }
   
    private PokemonEgg CreateEgg()
    {
        Pokemon female = Pokemon1.Gender == PokemonGender.Female ? Pokemon1 : Pokemon2;
        Pokemon newPokemon = new Pokemon(female.Base, 1);
        Pokemon hatchingPokemon = BuildPokemon(female, newPokemon);

        int hatchSteps = female.Base.EggCycles * cycleSteps;

        PokemonEgg egg = new PokemonEgg(hatchingPokemon, hatchSteps);
        
        return egg;
    }

    private Pokemon BuildPokemon(Pokemon female, Pokemon newPokemon)
    {
        newPokemon.Init();

        float abilityRoll = UnityEngine.Random.value;
        newPokemon.Ability = abilityRoll <= 0.8f ? female.Ability : AbilityDB.Abilities[newPokemon.Base.GetRandomAbility()];

        newPokemon.Trainer = player.Name;

        // Set Hatching Pokemons Nature depending on Everstone or not
        #region Handle Everstone
        bool parent1Everstone = Pokemon1.HeldItem != null && Pokemon1.HeldItem.Name == "Everstone";
        bool parent2Everstone = Pokemon2.HeldItem != null && Pokemon2.HeldItem.Name == "Everstone";

        if (parent1Everstone && parent2Everstone)
            newPokemon.Nature = UnityEngine.Random.value < 0.5f ? Pokemon1.Nature : Pokemon2.Nature;
        else if (parent1Everstone)
            newPokemon.Nature = Pokemon1.Nature;
        else if (parent2Everstone)
            newPokemon.Nature = Pokemon1.Nature;
        #endregion

        // Build Hatching Pokemons IVs
        #region Build IVs
        List<int> inheritedIVs = new List<int>();

        bool parent1HasKnot = Pokemon1.HeldItem != null && Pokemon1.HeldItem.Name == "Destiny Knot";
        bool parent2HasKnot = Pokemon2.HeldItem != null && Pokemon2.HeldItem.Name == "Destiny Knot";

        int inheritedIVCount = parent1HasKnot || parent2HasKnot ? 5 : 3;

        Stat[] ivStats = { Stat.HitPoints, Stat.Attack, Stat.Defense, Stat.Special_attack, Stat.Special_defense, Stat.Speed };

        for (int i = 0; i < inheritedIVCount; i++)
        {
            bool fromParent1 = UnityEngine.Random.value < 0.5f;
            Pokemon parent = fromParent1 ? Pokemon1 : Pokemon2;
            
            int statIndex;
            int attempts = 0;

            do 
            { 
                statIndex = UnityEngine.Random.Range(0, ivStats.Length);
                attempts++;
                if (attempts > 100)
                {
                    Debug.LogError("Exceeded attempts to find a unique stat index.");
                    break;
                }
            } 
            while (inheritedIVs.Contains(statIndex));

            inheritedIVs.Add(statIndex);
            newPokemon.StatIndividualValues[ivStats[statIndex]] = parent.StatIndividualValues[ivStats[statIndex]];
        }

        foreach (Stat stat in ivStats)
        {
            if (!inheritedIVs.Contains(Array.IndexOf(ivStats, stat)))
                newPokemon.StatIndividualValues[stat] = UnityEngine.Random.Range(0, 32);
        }
        #endregion

        return newPokemon;
    }

    private bool HasMatchingEggGroup()
    {
        foreach (var eggGroup1 in Pokemon1.Base.EggGroup)
            if (Pokemon2.Base.EggGroup.Contains(eggGroup1))
                return true;

        return false;
    }

    private void HandlePokemonSelected(Pokemon pokemon)
    {
        ChangedPokemon = pokemon;

        if (Pokemon1 == null)
            Pokemon1 = pokemon;
        else if (Pokemon1 != null && Pokemon2 == null)
            Pokemon2 = pokemon;

        PartyState.OnPokemonSelected -= HandlePokemonSelected;

        while (GameController.i.StateMachine.CurrentState != FreeRoamState.i)
            GameController.i.StateMachine.Pop();
    }

    private void OnTileMoved()
    {
        tilesTraveled++;

        if (tilesTraveled >= 15)
        {
            tilesTraveled = 0;

            if (Pokemon1 != null && Pokemon2 != null && !HasEgg)
            {
                int r = UnityEngine.Random.Range(1, 101);
                if (r <= eggNumber)
                {
                    Egg = CreateEgg();

                    HasEgg = true;
                    Debug.LogError("An egg has been found!");
                }
            }
        }

        HatchEggs();
    }

    private void HatchEggs()
    {

        List<PokemonEgg> ownedEggs = player.Party.Eggs;

        if (ownedEggs != null && ownedEggs.Count > 0)
        {
            var eggsCopy = new List<PokemonEgg>(ownedEggs);

            foreach (PokemonEgg egg in eggsCopy)
            {
                egg.IncrementSteps(1);

                if (egg.IsHatched)
                {
                    player.Party.AddPokemon(egg.Hatch());
                    player.Party.RemoveEgg(egg, pokemonEgg);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnTileMoved -= OnTileMoved;
    }
}
