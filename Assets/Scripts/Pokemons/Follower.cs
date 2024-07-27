using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 destPos;

    private bool isMoving;
    private float moveSpeed;

    private PlayerController player;
    private CharacterAnimator animator;

    SpriteRenderer spriteRenderer;
    List<Pokemon> pkmn;
    PokemonBase pkBase;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<PlayerController>();
        animator = GetComponent<CharacterAnimator>();

        pkmn = PokemonParty.i.Pokemons;

        if (pkmn.Count == 0 || pkmn[0] == null)
        {
            Debug.LogError("No Pokemon found or the first Pokemon in the party is null.");
        }
        else
        {
            pkBase = pkmn[0].Base;

            if (pkmn[0] != null)
                SetAnimations();

            SetPosition();
        }
    }

    public void SetAnimations()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        animator.WalkLeftSprites = pkBase.WalkLeftSprites;
        animator.WalkRightSprites = pkBase.WalkRightSprites;
        animator.WalkUpSprites = pkBase.WalkUpSprites;
        animator.WalkDownSprites = pkBase.WalkDownSprites;

        animator.spriteRenderer = spriteRenderer;

        animator.walkDownAnim = new SpriteAnimator(pkBase.WalkDownSprites, spriteRenderer);
        animator.walkUpAnim = new SpriteAnimator(pkBase.WalkUpSprites, spriteRenderer);
        animator.walkRightAnim = new SpriteAnimator(pkBase.WalkRightSprites, spriteRenderer);
        animator.walkLeftAnim = new SpriteAnimator(pkBase.WalkLeftSprites, spriteRenderer);

        animator.currentAnim = animator.walkDownAnim;
    }

    public void Follow(Vector3 movePosition)
    {
        if (isMoving)
            return;

        moveSpeed = player.Character.moveSpeed;
        Vector3 moveVector = movePosition - (transform.position - transform.position);
        StartCoroutine(Move(moveVector - transform.position));
    }

    public void SetPosition()
    {
        transform.position = player.transform.position;
        animator.IsMoving = false;
        isMoving = false;
    }

    public IEnumerator Move(Vector2 moveVec)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y + 0.7f;

        isMoving = true;
        animator.IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
        yield return StandStill();
    }

    private IEnumerator StandStill()
    {
        Vector3 myPosition = transform.position;
        yield return new WaitForFixedUpdate();

        if (myPosition == transform.position)
        {
            animator.IsMoving = false;
        }
    }
}