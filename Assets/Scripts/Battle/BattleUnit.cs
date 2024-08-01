using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    [SerializeField] Image megaAnim;
    [SerializeField] List<Sprite> megaFrames;

    public SpriteAnimator animator;
    private SpriteRenderer spriteRenderer;

    public BattleHud Hud => hud; 
    public bool IsPlayerUnit => isPlayerUnit; 

    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = new SpriteAnimator(megaFrames, spriteRenderer);

        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
            image.sprite = Pokemon.Base.BackSprite;
        else
            image.sprite = Pokemon.Base.FrontSprite;

        hud.gameObject.SetActive(true);
        hud.SetData(pokemon);

        transform.localScale = new Vector3(1, 1, 1);
        image.color = originalColor;

        if (!Pokemon.isMega)
            PlayEnterAnimation();

        pokemon.FirstTurn = true;

    }

    public void Clear() => hud.gameObject.SetActive(false);

    public void SetSelected(bool selected) => image.color = (selected) ? GlobalSettings.i.HighlightedColor : originalColor;

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, originalPos.y);

        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayIdleAnimation()
    {
        animator.HandleUpdate();
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    public void PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        sequence.WaitForCompletion();
    }

    public void PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        sequence.WaitForCompletion();
    }

    public IEnumerator PlayMegaAnimation()
    {
        megaAnim.gameObject.SetActive(true);

        var sequence = DOTween.Sequence();
        sequence.Append(megaAnim.DOFade(1, 0.5f));
        yield return animator.AnimateOnce(megaFrames, megaAnim);
        sequence.WaitForCompletion();
    }

    public IEnumerator MegaEvolve(Forms mega)
    {
        var gc = GameController.i;

        Pokemon.MegaEvolve(mega);
        
        yield return PlayMegaAnimation();
        yield return new WaitForSeconds(1);

        gc.BattleSystem.CurrentUnit.Setup(Pokemon);
        gc.PartyScreen.SetPartyData();

        megaAnim.gameObject.SetActive(false);
    }
}
