using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string playerName;
    [SerializeField] Sprite sprite;

    private Vector2 input;

    public static PlayerController i { get; private set; }

    private Character character;
    private Follower follower;
    public PokemonParty Party { get; private set; }

    private Vector3 lastTilePos;
    public int TileCount { get; private set; }

    public event Action OnTileMoved;

    private void Awake()
    {
        i = this;
        character = GetComponent<Character>();
        follower = FindObjectOfType<Follower>().GetComponent<Follower>();
        Party = GetComponent<PokemonParty>();

        lastTilePos = transform.position; 
        TileCount = 0; 
    }

    public int id { get; private set; }
    public int secretId { get; private set; }

    private void Start()
    {
        id = SetPlayerID();
        secretId = SetPlayerID();
    }

    public void SetMovementDirection(Vector2 direction) => input = direction.normalized;

    public Vector2 GetMovementDirection() => input;

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            //remove diagonal movement
            if (input.x != 0) input.y = 0;

            //Input management 
            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));

                if (character.IsPathClear((Vector2)this.transform.position + input))
                    follower.Follow(this.transform.position);

                SetMovementDirection(input); 
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(Interact());
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer | GameLayers.i.WaterLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyInTrigger;

    private void OnMoveOver()
    {
        if (Vector3.Distance(transform.position, lastTilePos) >= 1.0f)
        {
            TileCount++;
            lastTilePos = transform.position;
            OnTileMoved?.Invoke();
        }

        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);
        IPlayerTriggerable triggerable = null;

        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;

                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger)
            currentlyInTrigger = null;
    }

    private int SetPlayerID() => (int)UnityEngine.Random.Range(1, 65535);

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        // Restore players position in world
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        // Restore the players party
        GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }

    public string Name => playerName;
    public int ID => id;
    public int SecretID => secretId;
    public Sprite Sprite => sprite;
    public Character Character => character;
    public Follower Follower { get => follower; private set => follower = value; }
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}
