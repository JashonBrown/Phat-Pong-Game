using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Managers
{
    public class RoundManager : NetworkBehaviour
    {
        #region [Singleton]
        public static RoundManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                _players = new List<Player>();
                DontDestroyOnLoad (this);
            } 
            else
            {
                Debug.Log("Should not create 2 instances of the same game manager");
                Destroy (gameObject);
            }
        }
        #endregion
    
        [Header("Dependency Injects")]
        [SerializeField] private BrickManager _brickManager;
        [SerializeField] private ScoreManager _scoreManager;

        private List<Player> _players;

        /// <summary>
        /// Add a player to be cached
        /// </summary>
        /// <param name="player"></param>
        [Server]
        public void AddPlayer(Player player) => _players.Add(player);
        
        /// <summary>
        /// Clean up and reset all components of a round (excluding player position)
        /// </summary>
        [Server]
        public void StartNewRound()
        {
            // Reset balls for every player
            foreach (var player in _players)
                player.ResetBall();
            
            // NOTE: Reset balls first otherwise they occasionally break newly reset bricks
            _brickManager.ResetBricks();
            _scoreManager.ResetScore();
        }
    }
}
