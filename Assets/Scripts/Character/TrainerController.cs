using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string trainerName;
    [SerializeField] int battleUnitCount = 1;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    [SerializeField] AudioClip trainerAppearsClip;

    // State
    bool battleLost = false;

    Character character;

    private void Awake() => character = GetComponent<Character>();

    private void Start() => SetFovRotation(character.Animator.DefaultDirection);

    private void Update() => character.HandleUpdate();

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            AudioManager.i.PlayMusic(trainerAppearsClip);

            yield return DialogManager.i.ShowDialog(dialog);
            GameController.i.StartTrainerBattle(this, battleUnitCount);
        }
        else
        {
            yield return DialogManager.i.ShowDialog(dialogAfterBattle);
        }
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        GameController.i.StateMachine.Push(CutsceneState.i);

        AudioManager.i.PlayMusic(trainerAppearsClip);

        // Show Exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        // Walk towards the player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        // Show dialog
        yield return DialogManager.i.ShowDialog(dialog);

        GameController.i.StateMachine.Pop();

        GameController.i.StartTrainerBattle(this, battleUnitCount);
    }

    public void SetFovRotation(FacingDir dir)
    {
        float angle = 0;
        if (dir == FacingDir.Right)
            angle = 90;
        else if (dir == FacingDir.Up)
            angle = 180;
        else if (dir == FacingDir.Left)
            angle = 270;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState() => battleLost;

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost)
            fov.gameObject.SetActive(false);
    }

    public string Name => trainerName;
    public Sprite Sprite => sprite;
    public int BattleUnitCount => battleUnitCount;
}
