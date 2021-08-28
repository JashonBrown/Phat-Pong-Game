using Mirror;
using UnityEngine;

public class PlayerLocomotion : NetworkBehaviour
{
    private enum Direction { Right, Left }
    
    /// <summary>
    /// Speed the paddle travels at
    /// </summary>
    [SerializeField] private float _speed = 5f;

    /// <summary>
    /// The size of the paddle (used for boundary checking)
    /// </summary>
    [SerializeField] private float _paddleSize = 2f;

    private void Update()
    {
        // Skip if not the local player
        if (!isLocalPlayer) return;

        HandleInput();
    }

    /// <summary>
    /// Handles checking all the controller inputs from the player
    /// 
    /// NOTE: Only called on local player
    /// </summary>
    private void HandleInput()
    {
        // Handle A key
        if (Input.GetKey(KeyCode.A))
            OnAKeyHeld();

        // Handle D key
        if (Input.GetKey(KeyCode.D))
            OnDKeyHeld();
    }

    /// <summary>
    /// Event called every frame the 'A' key is held
    ///
    /// This will move the players paddle left provided there is space to do so
    /// 
    /// NOTE: Only called on local player
    /// </summary>
    private void OnAKeyHeld() => MovePaddle(Direction.Left);

    /// <summary>
    /// Event called every frame the 'D' key is held
    ///
    /// This will move the players paddle right provided there is space to do so
    /// 
    /// NOTE: Only called on local player
    /// </summary>
    private void OnDKeyHeld() => MovePaddle(Direction.Right);

    /// <summary>
    /// Attempt to move the paddle in the specified direction
    /// </summary>
    /// <param name="direction"></param>
    private void MovePaddle(Direction direction)
    {
        // Get frame independent movement increment
        var movementIncrement = Time.deltaTime * _speed;
        
        // Invert movement increment if moving left
        if (direction == Direction.Left)
            movementIncrement = -movementIncrement;

        var cachedTransform = transform; // Cache as it's more performant
        var newPosition = cachedTransform.position + new Vector3(movementIncrement, 0 ,0);
        
        // Check if the move is valid
        if (!IsValidMove(newPosition)) return;
        
        cachedTransform.position = newPosition;
    }

    /// <summary>
    /// Check if the provided position vector is a valid move for this paddle
    /// </summary>
    /// <param name="newPosition"></param>
    /// <returns></returns>
    private bool IsValidMove(Vector3 newPosition)
    {
        var paddleOffset = _paddleSize / 2; // Offset since the paddles pivot is centered
        return newPosition.x < 5-paddleOffset && newPosition.x > -5+paddleOffset;
    }
}
