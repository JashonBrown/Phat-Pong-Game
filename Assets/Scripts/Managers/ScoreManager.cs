using Mirror;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class ScoreManager : NetworkBehaviour
    {
        [SerializeField] private TMP_Text _label;

        [SyncVar(hook = nameof(OnUpdateScore))] 
        private int _score;

        [Server]
        public void ResetScore() => _score = 0;

        [Server]
        public void OnBrickDestroyed() => _score += 100;

        [Client]
        private void OnUpdateScore(int oldVal, int newVal) => _label.text = "Score: " + _score;
    }
}