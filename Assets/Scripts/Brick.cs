using System;
using Mirror;
using UnityEngine;

public class Brick : NetworkBehaviour, IHealth
{
    public enum BrickLayer { Layer1, Layer2, Layer3, Layer4, Layer5 }
    
    [Header("Sprites")]
    [SerializeField] private Sprite _sprite1;
    [SerializeField] private Sprite _sprite2;
    [SerializeField] private Sprite _sprite3;
    [SerializeField] private Sprite _sprite4;
    [SerializeField] private Sprite _sprite5;
 
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
    public event Action OnDeath;

    /// <summary>
    /// The layer this brick is in
    /// </summary>
    private BrickLayer Layer { get; set; }
    
    /// <summary>
    /// The sprite renderer for this brick
    /// </summary>
    private SpriteRenderer Renderer { get; set; }
    
    /// <summary>
    /// Performs setup of this brick
    /// </summary>
    /// <param name="startingHealth"></param>
    /// <param name="layer"></param>
    [Server]
    public void Init(int startingHealth, BrickLayer layer)
    {
        StartingHealth = startingHealth;
        CurrentHealth = startingHealth;
        Layer = layer;
        Renderer = GetComponent<SpriteRenderer>();
        Renderer.sprite = GetSpriteForLayer(layer);
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
        OnDeath?.Invoke();
        // TODO: Register scoreboard to increase total
        RpcDestroy();
    }
    
    /// <summary>
    /// Destroys this brick on clients
    /// </summary>
    [ClientRpc]
    private void RpcDestroy() => NetworkServer.Destroy(gameObject);

    /// <summary>
    /// Retrieve the appropriate sprite based on the provided layer
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    [Server]
    private Sprite GetSpriteForLayer(BrickLayer layer)
    {
        return layer switch
        {
            BrickLayer.Layer1 => _sprite1,
            BrickLayer.Layer2 => _sprite2,
            BrickLayer.Layer3 => _sprite3,
            BrickLayer.Layer4 => _sprite4,
            BrickLayer.Layer5 => _sprite5,
            _ => throw new Exception("Invalid brick layer type")
        };
    }
}
