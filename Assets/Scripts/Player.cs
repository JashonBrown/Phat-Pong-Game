using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _player1Sprite;
    [SerializeField] private Sprite _player2Sprite;
    
    [Header("Misc")]
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform _ballSpawnLocation;

    private Ball Ball { get; set; }
    private bool HasBall { get; set; }
    
    [Client]
    private void Update()
    {
        // Skip if not the local player
        if (!isLocalPlayer) return;

        HandleInput();
    }

    /// <summary>
    /// Getter for the balls spawn location relative to the player
    /// </summary>
    public Vector3 BallSpawnLocation => _ballSpawnLocation.position;

    /// <summary>
    /// Handles checking all the controller inputs from the player
    /// 
    /// No point over-engineering and making an input controller since this only handles 4 inputs
    ///
    /// NOTE: Only called on local player
    /// </summary>
    [Client]
    private void HandleInput()
    {
        // Handle Space key
        if (Input.GetKeyDown(KeyCode.Space))
            OnSpaceKeyPressed();
        
        // Handle R Key
        if (Input.GetKeyDown(KeyCode.R))
            OnRKeyPressed();
    }

    /// <summary>
    /// Event called the frame that the 'Space' key is pressed
    ///
    /// Will launch the ball provided the player is currently in possession of it
    /// 
    /// NOTE: Only called on local player
    /// </summary>
    [Client]
    private void OnSpaceKeyPressed() => CmdLaunchBall();
    
    /// <summary>
    /// Event called the frame that the 'R' key is pressed
    ///
    /// Will restart a match
    /// 
    /// NOTE: Only called on local player
    /// </summary>
    [Client]
    private void OnRKeyPressed()
    {
        // Only the host can restart the game
        if (!isServer) return;
        
        // TODO: Send restart match command to server
    }

    [Command]
    private void CmdLaunchBall()
    {
        // Skip if no ball
        if (!HasBall) return;
        
        Ball.RpcLaunch();
        HasBall = false;
    }

    [Server]
    public void OnBallRespawn() => HasBall = true;

    [Command]
    private void CmdSpawnBall()
    {
        GameObject ballGo = Instantiate(_ballPrefab, _ballSpawnLocation.position, Quaternion.identity);
        NetworkServer.Spawn(ballGo);
        Ball = ballGo.GetComponent<Ball>();
        Ball.ServerSetOwner(netId);
        Ball.RpcRespawn();
        HasBall = true;
    }

    public override void OnStartLocalPlayer() => CmdSpawnBall();
}
