using UnityEngine;

public class EnableOnCollision : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Get the SpriteRenderer component on this game object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure the sprite renderer is initially disabled
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    // Detect collisions
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Enable the sprite renderer
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }
    }
}