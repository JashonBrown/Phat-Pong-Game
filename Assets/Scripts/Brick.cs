using System;
using Enums;
using Interfaces;
using Mirror;
using UnityEngine;

public class Brick : NetworkBehaviour, IHealth
{
    [Header("Sprites")]
    [SerializeField] private Sprite _sprite1;
    [SerializeField] private Sprite _sprite2;
    [SerializeField] private Sprite _sprite3;
    [SerializeField] private Sprite _sprite4;
    [SerializeField] private Sprite _sprite5;

    /// <summary>
    /// The layer this brick resides in.
    /// NOTE: Sync var used instead of RPC in order to update late joining clients
    /// </summary>
    [SyncVar(hook=nameof(OnUpdateLayer))]
    public BrickLayer Layer;
 
    /// <summary>
    /// This bricks starting health
    /// </summary>
    public int StartingHealth { get; private set; }
    
    /// <summary>
    /// This bricks current health
    /// </summary>
    public int CurrentHealth { get; private set; }
    
    /// <summary>
    /// Event invoked when this brick is destroyed
    /// </summary>
    public Action<Brick> OnBrickDestroyed;

    /// <summary>
    /// The sprite renderer for this brick
    /// </summary>
    private SpriteRenderer Renderer { get; set; }

    #region [Client Functions]
    [Client]
    private void Awake() => Renderer = GetComponent<SpriteRenderer>();
    
    /// <summary>
    /// Retrieve the appropriate sprite based on the provided layer
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    [Client]
    private Sprite GetSpriteForLayer(BrickLayer layer)
    {
        return layer switch
        {
            BrickLayer.First => _sprite1,
            BrickLayer.Second => _sprite2,
            BrickLayer.Third => _sprite3,
            BrickLayer.Forth => _sprite4,
            BrickLayer.Fifth => _sprite5,
            _ => throw new Exception("Invalid brick layer type")
        };
    }

    /// <summary>
    /// Sets the correct sprite based on the layer
    /// </summary>
    /// <param name="oldLayer"></param>
    /// <param name="newLayer"></param>
    [Client]
    private void OnUpdateLayer(BrickLayer oldLayer, BrickLayer newLayer) =>
        Renderer.sprite = GetSpriteForLayer(newLayer);
    #endregion

    #region [Server Functions]
    /// <summary>
    /// Performs setup of this brick
    /// </summary>
    /// <param name="startingHealth"></param>
    [Server]
    public void SetHealth(int startingHealth)
    {
        StartingHealth = startingHealth;
        CurrentHealth = startingHealth;
    }
    
    /// <summary>
    /// Decrement health and perform death checks
    /// </summary>
    /// <param name="damage"></param>
    [Server]
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        
        if (CurrentHealth <= 0)
            BreakBrick();
    }

    /// <summary>
    /// Destroys brick on clients and notifies death listeners
    /// </summary>
    [Server]
    private void BreakBrick()
    {
        OnBrickDestroyed?.Invoke(this);
        // TODO: Register scoreboard to increase total
        RpcDestroy();
    }
    #endregion

    #region [Client RPCs]
    /// <summary>
    /// Destroys this brick on clients
    /// </summary>
    [ClientRpc]
    public void RpcDestroy() => NetworkServer.Destroy(gameObject);
    #endregion
}
