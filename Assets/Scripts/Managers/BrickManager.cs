using System;
using System.Collections.Generic;
using Enums;
using Mirror;
using UnityEngine;

namespace Managers
{
    public class BrickManager : NetworkBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject _brickPrefab;
    
        [Header("Layout Properties")]
        [SerializeField] private Transform _startPosition;
        [SerializeField] private float _xOffset;
        [SerializeField] private float _yOffset;

        [Header("Dependencies")] 
        [SerializeField] private ScoreManager _scoreManager;
    
        /// <summary>
        /// All currently spawned bricks
        /// </summary>
        private List<Brick> _bricks;

        public Action OnAllBricksDestroyed;
        
        public void Awake() => _bricks = new List<Brick>();

        /// <summary>
        /// Destroys old bricks and creates new ones
        /// </summary>
        [Server]
        public void ResetBricks()
        {
            // Clear all bricks on clients
            foreach (var brick in _bricks)
                brick.RpcDestroy();

            // Clear cached list
            _bricks = new List<Brick>();

            // Re-populate bricks
            var cachedPosition = _startPosition.position;
            for (int layer = 1; layer <= 5; layer++)
            {
                for (int row = 1; row <= 10; row++)
                {
                    var brickPosition = new Vector3(cachedPosition.x + (row * _xOffset), cachedPosition.y + (layer * _yOffset), 0);
                    GameObject brickGo = Instantiate(_brickPrefab, brickPosition, Quaternion.identity);
                    NetworkServer.Spawn(brickGo);
                    Brick brick = brickGo.GetComponent<Brick>();
                    _bricks.Add(brick);
                
                    // Set properties
                    brick.SetHealth(10);
                    brick.Layer = ConvertIndexToLayer(layer);
                    brick.OnBrickDestroyed += OnBrickDestroyed;
                }
            }
        }

        /// <summary>
        /// Cleans up destroyed brick then passes through to score manager
        /// </summary>
        /// <param name="brick"></param>
        [Server]
        private void OnBrickDestroyed(Brick brick)
        {
            _scoreManager.OnBrickDestroyed();
            _bricks.Remove(brick);
            
            // Is game over?
            if (_bricks.Count == 0)
                OnAllBricksDestroyed?.Invoke();
        }

        /// <summary>
        /// Converts values 1-5 to a brick layer enum
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Server]
        private static BrickLayer ConvertIndexToLayer(int index)
        {
            return index switch
            {
                1 => BrickLayer.First,
                2 => BrickLayer.Second,
                3 => BrickLayer.Third,
                4 => BrickLayer.Forth,
                5 => BrickLayer.Fifth,
                _ => throw new Exception("Cannot convert provided index to brick layer")
            };
        }
    }
}