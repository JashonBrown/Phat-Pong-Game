using Enums;
using Interfaces;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Ball : NetworkBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _player1BallSprite;
    [SerializeField] private Sprite _player2BallSprite;
    
    /// <summary>
    /// The damage this ball deals when colliding with an object
    /// </summary>
    [Header("Misc")]
    [SerializeField] private int _damage = 10;

    /// <summary>
    /// The speed the ball travels at
    /// </summary>
    [SerializeField] private float _speed = 10;

    /// <summary>
    /// The rigidbody of this ball
    /// </summary>
    private Rigidbody2D Rb { get; set; }
    
    /// <summary>
    /// The sprite renderer of this ball
    /// </summary>
    private SpriteRenderer Renderer { get; set; }

    /// <summary>
    /// The ID of the player that owns this ball
    /// </summary>
    private uint OwnerId { get; set; }
    
    /// <summary>
    /// The player game object that is the owner of this ball
    /// </summary>
    private Player OwnerObject { get; set; }
    
    /// <summary>
    /// Has the ball been launched yet?
    /// </summary>
    private bool HasLaunched { get; set; }

    private void Awake()
    {
        // Cache objects
        Rb = GetComponent<Rigidbody2D>();
        Renderer = GetComponent<SpriteRenderer>();
    }

    #region [Server Functions]
    /// <summary>
    /// Set owner properties
    /// </summary>
    /// <param name="ownerId"></param>
    [Server]
    public void SetOwner(uint ownerId)
    {
        OwnerId = ownerId;
        var playerGo = NetworkIdentity.spawned[ownerId].gameObject; // Dunno if this is the best way to access objects via netID
        OwnerObject = playerGo.GetComponent<Player>();
    }
    
    [Server]
    public void Launch()
    {
        Rb.velocity = Vector2.zero; // Zero out velocity just in case
        
        Transform cachedTransform;
        (cachedTransform = transform).up = new Vector3(Random.Range(-1f, 1f), 1f, 0f); // Set up direction to something between -45 deg and +45 deg

        var cachedUp = cachedTransform.up;
        Rb.AddForce(new Vector3(cachedUp.x * _speed,cachedUp.y * _speed,0));
        HasLaunched = true;
    }

    [Server]
    private void OnCollisionEnter2D(Collision2D other)
    {
        // Check if collision with owners paddle
        var owner = other.gameObject.GetComponent<NetworkIdentity>();
        if (owner != null && owner.netId == OwnerId)
        {
            // TODO: Handle owner player collision with ball
        }
    
        // Try get damageable component
        IHealth damageable = other.gameObject.GetComponent<IHealth>();
        damageable?.TakeDamage(_damage);
    }

    /// <summary>
    /// Reset ball if it collides with the bottom red-zone
    /// </summary>
    /// <param name="other"></param>
    [Server]
    private void OnTriggerEnter2D(Collider2D other) => OwnerObject.ResetBall();

    [Server]
    private void Update()
    {
        if (HasLaunched) return;
        
        Rb.velocity = Vector2.zero; // If cross-player collision is on, when th balls collide it adds velocity thus we zero it out here
        RpcSetPosition(OwnerObject.BallSpawnLocation);
    }
    #endregion
    
    #region [Client RPCs]
    /// <summary>
    /// Set balls sprite depending on whether if the local player owns it or not
    /// </summary>
    [ClientRpc]
    public void RpcSetSprite(PlayerSlot slot) => Renderer.sprite = slot == PlayerSlot.Player1 
        ? _player1BallSprite : _player2BallSprite;

    /// <summary>
    /// Resets flags and physics for the ball on clients
    /// </summary>
    [ClientRpc]
    public void RpcRespawn()
    {
        Rb.velocity = Vector2.zero;
        HasLaunched = false;
    }

    /// <summary>
    /// Teleports this ball to the provided location on clients
    /// </summary>
    /// <param name="position"></param>
    [ClientRpc]
    private void RpcSetPosition(Vector3 position) => transform.position = position;
    #endregion
}
