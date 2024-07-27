using UnityEngine;

public class BerryTreeAnimator : MonoBehaviour
{
    public SpriteAnimator plantAnim;
    public SpriteAnimator sproutAnim;
    public SpriteAnimator grownAnim;
    public SpriteAnimator flowerAnim;
    public SpriteAnimator berryAnim;

    public SpriteAnimator currentAnim;

    SpriteRenderer spriteRenderer;
    BerryTree berry;

    private void Start()
    {
        berry = GetComponentInParent<BerryTree>();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        plantAnim = new SpriteAnimator(berry.item.PlantSprites, spriteRenderer);
        sproutAnim = new SpriteAnimator(berry.item.SproutSprites, spriteRenderer, 0.5f);
        grownAnim = new SpriteAnimator(berry.item.GrownSprites, spriteRenderer, 0.5f);
        flowerAnim = new SpriteAnimator(berry.item.FlowerSprites, spriteRenderer, 0.5f);
        berryAnim = new SpriteAnimator(berry.item.BerrySprites, spriteRenderer, 0.5f);

        currentAnim = plantAnim;
    }

    private void Update()
    {
        var state = berry.state;

        if (state == BerryState.None)
            spriteRenderer.enabled = false;
        else
        {
            spriteRenderer.enabled = true;

            if (state == BerryState.Planted)
                currentAnim = plantAnim;
            else if (state == BerryState.Sprouted)
                currentAnim = sproutAnim;
            else if (state == BerryState.Growing)
                currentAnim = grownAnim;
            else if (state == BerryState.Flowering)
                currentAnim = flowerAnim;
            else if (state == BerryState.Fruit)
                currentAnim = berryAnim;

            currentAnim.HandleUpdate();
        }
    }
}
