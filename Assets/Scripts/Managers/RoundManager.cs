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
                DontDestroyOnLoad (this);
            } 
            else
            {
                Debug.Log("Should not create 2 instances of the same game manager");
                Destroy (gameObject);
            }
        }
        #endregion
    
        [Header("Dependencies")]
        [SerializeField] private BrickManager _brickManager;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private WinManager _winManager;

        private List<Player> _players;

        /// <summary>
        /// Perform setup
        /// </summary>
        [Server]
        public override void OnStartServer()
        {
            _players = new List<Player>();
            _brickManager.OnAllBricksDestroyed += OnAllBricksDestroyed;
        }

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
            
            // TODO: Bricks are still sometimes broken when newly generated even though we reset the ball first?
            _brickManager.ResetBricks();
            _scoreManager.ResetScore();
            _winManager.HideLabel();
        }

        [Server]
        private void OnAllBricksDestroyed() => _winManager.ShowLabel();
    }
}
