using Enums;
using Mirror;
using UnityEngine;

namespace Managers
{
    public class MyNetworkManager : NetworkManager
    {
        /// <summary>
        /// Extended the default mirror spawning so I could get reference to the spawned objects
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // NOTE: This is the default auto-spawn code (but I also want reference to the spawned object)
            Transform startPos = GetStartPosition();
            GameObject playerGo = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            playerGo.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, playerGo);
            
            // ------------------- CUSTOM BELOW -------------------
            
            // Get player
            Player player = playerGo.GetComponent<Player>();
            
            // Cache the spawned players in the round manager
            RoundManager.Instance.AddPlayer(player);

            // Set different paddle sprite for host
            PlayerSlot slot = (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                ? PlayerSlot.Player1
                : PlayerSlot.Player2;
            player.RpcSetSprite(slot);
            
            // Spawn ball for player
            player.SpawnBall();
            
            // Start round if player is host
            if (player.isLocalPlayer)
                RoundManager.Instance.StartNewRound();
        }
        
        // TODO: Would usually perform some "OnDisconnect" clean up too
    }
}