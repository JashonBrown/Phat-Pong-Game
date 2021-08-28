using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

// [RequireComponent(typeof(Rigidbody2D))]
public class Ball : NetworkBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _ball1Sprite;
    [SerializeField] private Sprite _ball2Sprite;
    
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
    
    private void Awake() => Rb = GetComponent<Rigidbody2D>();

    /// <summary>
    /// Assign the player owner of this ball
    /// </summary>
    /// <param name="id"></param>
    [Server]
    public void ServerSetOwner(uint id)
    {
        OwnerId = id;
        var playerGo = NetworkIdentity.spawned[id].gameObject; // Dunno if this is the best way too access objects via ID
        OwnerObject = playerGo.GetComponent<Player>();
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        Rb.velocity = Vector2.zero;
        HasLaunched = false;
    }

    [ClientRpc]
    public void RpcLaunch()
    {
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

    // NOTE: Only 1 trigger exists so will assume we have hit the red-zone
    [Server]
    private void OnTriggerEnter2D(Collider2D other)
    {
        OwnerObject.OnBallRespawn();
        RpcRespawn();
    }

    [Server]
    private void Update()
    {
        if (!HasLaunched)
            RpcSetPosition(OwnerObject.BallSpawnLocation);
    }

    [ClientRpc]
    private void RpcSetPosition(Vector3 position) => transform.position = position;
}
