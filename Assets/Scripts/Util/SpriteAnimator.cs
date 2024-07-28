using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimator
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float frameRate;

    int currentFrame;
    float timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate = .16f)
    {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.frameRate = frameRate;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = 0;
        spriteRenderer.sprite = frames[0];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if (timer > frameRate)
        {
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            timer -= frameRate;
        }
    }

    public IEnumerator AnimateOnce(List<Sprite> frames, Image anim)
    {
        float elapsedTime = 0f;
        int frameCount = frames.Count;
        int currentFrame = 0;

        while (currentFrame < frameCount)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= frameRate)
            {
                elapsedTime = 0f;
                anim.sprite = frames[currentFrame];
                currentFrame++;
            }

            yield return null;
        }

        if (frameCount > 0)
            anim.sprite = frames[frameCount - 1];
    }

    public List<Sprite> Frames
    {
        get { return frames; }
    }
}
