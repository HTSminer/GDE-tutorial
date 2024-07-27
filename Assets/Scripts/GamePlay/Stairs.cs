using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    private List<Transform> stairsTiles = new List<Transform>();

    private void Init()
    {
        stairsTiles.Clear();

        GameObject bottomLayer = GameObject.Find("BottomLayer1");
        if (bottomLayer != null)
        {
            foreach (Transform child in bottomLayer.transform)
            {
                stairsTiles.Add(child);
            }
        }
        else
        {
            Debug.LogError("BottomLayer1 Gameobject not found.");
        }
    }

    private void Start()
    {
        Init();
    }

    PlayerController playerController;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerController = collider.GetComponent<PlayerController>();
            playerController.Character.moveSpeed = 3f;
            if (playerController != null)
            {
                // Determine the direction the player is moving
                Vector2 playerMovementDirection = playerController.GetMovementDirection();

                // Handle the player's movement direction
                HandlePlayerMovementDirection(playerMovementDirection);
            }
        }
    }

    private void OnTriggerExit2D()
    {
        playerController.Character.moveSpeed = 5f;
    }

    private void HandlePlayerMovementDirection(Vector2 direction)
    {
        Vector2 playerPos = playerController.transform.position;
        Vector2 stairsPos = transform.position;

        // Determine the direction of the stairs
        Vector2 stairsDir = (stairsPos - playerPos).normalized;
        
        // Iterate over each tile of the stairs
        foreach (Transform tile in stairsTiles)
        {
            float tileXMin = tile.position.x - 0.5f; // Assuming each tile has a width of 1 unit
            float tileXMax = tile.position.x + 0.5f; // Assuming each tile has a width of 1 unit

            // Check if the player is on the same tile as the stairs tile
            if (playerPos.x >= tileXMin && playerPos.x <= tileXMax)
            {
                // Move the player based on the direction of the stairs
                playerPos.y += stairsDir.y;
                break; // Exit the loop once we find the tile the player is on
            }
        }

        // Update player's position.
        playerController.transform.position = playerPos;

        // Determine movement direction base on player's position to stairs.
        if (playerPos.y < stairsPos.y && direction == Vector2.up) Debug.Log("Player is going up stairs");
        else if (playerPos.y > stairsPos.y && direction == Vector2.down) Debug.Log("Player is going down stairs");
        else if (playerPos.x < stairsPos.x && direction == Vector2.right) Debug.Log("Player is going right");
        else if (playerPos.x > stairsPos.x && direction == Vector2.left) Debug.Log("Player is going left");
    }
}
