using System;
using Enums;
using Managers;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Player : NetworkBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _player1PaddleSprite;
    [SerializeField] private Sprite _player2PaddleSprite;
    
    [Header("Misc")]
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform _ballSpawnLocation;

    private SpriteRenderer Renderer { get; set; }
    private Ball Ball { get; set; }
    private bool HasBall { get; set; }

    /// <summary>
    /// Getter for the balls spawn location relative to the player
    /// </summary>
    public Vector3 BallSpawnLocation => _ballSpawnLocation.position;

    public override void OnStartLocalPlayer() => CmdSpawnBall();
    
    #region [Client Functions]

    [Client]
    private void Awake() => Renderer = GetComponent<SpriteRenderer>();

    [Client]
    private void Update()
    {
        // Skip if not the local player
        if (!isLocalPlayer) return;

        HandleInput();
    }
    
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

        CmdNewGame();
    }
    #endregion

    #region [Client RPCs]
    /// <summary>
    /// Sets this players paddle sprite
    /// </summary>
    [ClientRpc]
    public void RpcSetSprite(PlayerSlot slot) => Renderer.sprite = (slot == PlayerSlot.Player1) 
        ? _player1PaddleSprite : _player2PaddleSprite;
    #endregion
    
    #region [Commands]
    /// <summary>
    /// Reset ball then passthrough to round manager
    /// </summary>
    [Command]
    private void CmdNewGame() => RoundManager.Instance.StartNewRound();
    
    /// <summary>
    /// Sets up the ball on the server and spawns it on clients
    /// </summary>
    [Command]
    private void CmdSpawnBall()
    {
        // Spawn ball
        GameObject ballGo = Instantiate(_ballPrefab, _ballSpawnLocation.position, Quaternion.identity);
        NetworkServer.Spawn(ballGo);
        Ball = ballGo.GetComponent<Ball>();
        
        // Set properties
        Ball.SetOwner(netId);
        Ball.RpcSetSprite(isLocalPlayer ? PlayerSlot.Player1 : PlayerSlot.Player2);
        ResetBall();
    }
    
    /// <summary>
    /// Launch the ball on the server (clients update through NetworkTransforms)
    /// </summary>
    [Command]
    private void CmdLaunchBall()
    {
        // Skip if no ball
        if (!HasBall) return;
        
        Ball.Launch();
        HasBall = false;
    }
    #endregion

    #region [Server Functions]
    /// <summary>
    /// Reset the ball on the server and clients
    /// </summary>
    [Server]
    public void ResetBall()
    {
        Ball.RpcRespawn();
        HasBall = true;
    }
    #endregion
}
