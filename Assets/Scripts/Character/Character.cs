using System;
using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float walkSpeed = 5;
    public float runSpeed = 8;
    public float surfSpeed = 6;
    public float moveSpeed;

    public bool IsMoving { get; private set; }
    public float OffsetY { get; private set; } = 0.5f;

    CharacterAnimator animator;

    private void Awake()
    {
        moveSpeed = walkSpeed;

        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + OffsetY;

        transform.position = pos;
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null, bool checkCollisions = true)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        var ledge = CheckForLedge(targetPos);
        if (ledge != null)
        {
            if (ledge.TryToJump(this, moveVec))
                yield break;
        }

        if (checkCollisions && !IsPathClear(targetPos))
            yield break;

        animator.OnBike = false;

        if (animator.IsSurfing && Physics2D.OverlapCircle(targetPos, 0.3f, GameLayers.i.WaterLayer) == null)
        {
            animator.IsSurfing = false;
        }

        IsMoving = true;

        if (!animator.IsSurfing && !animator.OnBike)
        {
            if (Input.GetButton("Run"))
            {
                animator.IsRunning = true;
                moveSpeed = runSpeed;
            }
            else
            {
                animator.IsRunning = false;
                moveSpeed = walkSpeed;
            }
        }

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    public bool IsPathClear(Vector3 targetPos) 
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        var collisionLayers = GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer;
        if (!animator.IsSurfing)
            collisionLayers = collisionLayers | GameLayers.i.WaterLayer;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayers))
            return false;

        return true;
    }

    Ledge CheckForLedge(Vector3 targetPos)
    {
        var collider = Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.i.LedgeLayer);
        return collider?.GetComponent<Ledge>();
    }

    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = MathF.Floor(targetPos.x) - MathF.Floor(transform.position.x);
        var ydiff = MathF.Floor(targetPos.y) - MathF.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
            Debug.Log("Error in Look Towards: You can't ask the character to look diagonally.");
    }

    public CharacterAnimator Animator => animator;
}