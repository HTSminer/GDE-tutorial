using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Walking Animation Sprites")]
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [Space(30)]
    [Header("Running Animation Sprites")]
    [SerializeField] List<Sprite> runDownSprites;
    [SerializeField] List<Sprite> runUpSprites;
    [SerializeField] List<Sprite> runLeftSprites;
    [SerializeField] List<Sprite> runRightSprites;
    [Space(30)]
    [Header("Surfing Animation Sprites")]
    [SerializeField] List<Sprite> surfSprites;
    [Space(30)]
    [Header("Default Facing Direction of player on screen load")]
    [SerializeField] FacingDir defaultDirection = FacingDir.Down;

    // Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public bool IsRunning { get; set; }
    public bool IsJumping { get; set; }
    public bool IsSurfing { get; set; }
    public bool OnBike { get; set; }

    // States
    public SpriteAnimator walkDownAnim;
    public SpriteAnimator walkUpAnim;
    public SpriteAnimator walkLeftAnim;
    public SpriteAnimator walkRightAnim;
    public SpriteAnimator runDownAnim;
    public SpriteAnimator runUpAnim;
    public SpriteAnimator runLeftAnim;
    public SpriteAnimator runRightAnim;

    public SpriteAnimator currentAnim;

    // References
    public SpriteRenderer spriteRenderer;
    private bool wasPreviouslyMoving;
    private FacingDir currentDir;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        runDownAnim = new SpriteAnimator(runDownSprites, spriteRenderer);
        runUpAnim = new SpriteAnimator(runUpSprites, spriteRenderer);
        runLeftAnim = new SpriteAnimator(runLeftSprites, spriteRenderer);
        runRightAnim = new SpriteAnimator(runRightSprites, spriteRenderer);
        SetFacingDir(defaultDirection);

        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (!IsSurfing)
        {
            HandleMovementAnimation();

            if (currentAnim != prevAnim || IsMoving != wasPreviouslyMoving)
                currentAnim.Start();

            if (IsJumping)
                spriteRenderer.sprite = currentAnim.Frames[^1]; 
            else if (IsMoving)
                currentAnim.HandleUpdate();
            else
                HandleIdleAnimation();
        }
        else
        {
            HandleSurfingAnimation();
        }

        wasPreviouslyMoving = IsMoving;
    }

    private void HandleMovementAnimation()
    {
        if (!IsRunning)
        {
            if (MoveX == 1)
                SetAnimation(walkRightAnim, FacingDir.Right);
            else if (MoveX == -1)
                SetAnimation(walkLeftAnim, FacingDir.Left);
            else if (MoveY == 1)
                SetAnimation(walkUpAnim, FacingDir.Up);
            else if (MoveY == -1)
                SetAnimation(walkDownAnim, FacingDir.Down);
        }
        else
        {
            if (MoveX == 1)
                SetAnimation(runRightAnim, FacingDir.Right);
            else if (MoveX == -1)
                SetAnimation(runLeftAnim, FacingDir.Left);
            else if (MoveY == 1)
                SetAnimation(runUpAnim, FacingDir.Up);
            else if (MoveY == -1)
                SetAnimation(runDownAnim, FacingDir.Down);
        }
    }

    private void HandleIdleAnimation()
    {
        if (!IsMoving && !IsSurfing && !OnBike)
        {
            switch (currentDir)
            {
                case FacingDir.Right:
                    spriteRenderer.sprite = walkRightAnim.Frames[0];
                    break;
                case FacingDir.Left:
                    spriteRenderer.sprite = walkLeftAnim.Frames[0];
                    break;
                case FacingDir.Up:
                    spriteRenderer.sprite = walkUpAnim.Frames[0];
                    break;
                case FacingDir.Down:
                    spriteRenderer.sprite = walkDownAnim.Frames[0];
                    break;
            }
        }
    }

    private void HandleSurfingAnimation()
    {
        if (MoveX == 1)
            spriteRenderer.sprite = surfSprites[2];
        else if (MoveX == -1)
            spriteRenderer.sprite = surfSprites[1];
        else if (MoveY == 1)
            spriteRenderer.sprite = surfSprites[3];
        else if (MoveY == -1)
            spriteRenderer.sprite = surfSprites[0];
    }

    private void SetAnimation(SpriteAnimator anim, FacingDir dir)
    {
        currentAnim = anim;
        currentDir = SetFacingDir(dir);
    }

    public FacingDir SetFacingDir(FacingDir dir)
    {
        MoveX = 0;
        MoveY = 0;

        if (dir == FacingDir.Right)
            MoveX = 1;
        if (dir == FacingDir.Left)
            MoveX = -1;
        if (dir == FacingDir.Up)
            MoveY = 1;
        if (dir == FacingDir.Down)
            MoveY = -1;

        return dir;
    }

    public FacingDir DefaultDirection => defaultDirection;

    public List<Sprite> WalkDownSprites {
        get => walkDownSprites; set => walkDownSprites = value;
    }
    public List<Sprite> WalkUpSprites {
        get => walkUpSprites; set => walkUpSprites = value;
    }
    public List<Sprite> WalkLeftSprites { 
        get => walkLeftSprites; set => walkLeftSprites = value; 
    }
    public List<Sprite> WalkRightSprites { 
        get => walkRightSprites; set => walkRightSprites = value; 
    }
}

public enum FacingDir { Up, Down, Left, Right }
