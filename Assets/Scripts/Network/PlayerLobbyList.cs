using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerLobbyList : NetworkBehaviour
{
    [SerializeField] private GameObject playerItemPrefab;
    [SerializeField] private Transform playerListContent;

    private Dictionary<ulong, GameObject> playerItems = new Dictionary<ulong, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var item in playerItems.Values)
            {
                Destroy(item);
            }
            playerItems.Clear();

            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                AddPlayer(clientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;
        }
    }

    private void AddPlayer(ulong clientId)
    {
        if (playerItems.ContainsKey(clientId))
        {
            Debug.Log($"El jugador con ID {clientId} ya está en la lista");
            return; 
        }
        GameObject playerItem = Instantiate(playerItemPrefab, playerListContent);
        playerItem.GetComponentInChildren<TMP_Text>().text = $"Jugador {clientId}";
        playerItems.Add(clientId, playerItem);
        UpdatePlayerListClientRpc(clientId, true);
    }

    private void RemovePlayer(ulong clientId)
    {
        if (playerItems.TryGetValue(clientId, out GameObject playerItem))
        {
            Destroy(playerItem);
            playerItems.Remove(clientId);

            // Actualizar la UI para todos
            UpdatePlayerListClientRpc(clientId, false);
        }
    }

    [ClientRpc]
    private void UpdatePlayerListClientRpc(ulong clientId, bool isConnecting)
    {
        if (IsServer) return; // El servidor ya lo manejó

        if (isConnecting)
        {
            GameObject playerItem = Instantiate(playerItemPrefab, playerListContent);
            playerItem.GetComponentInChildren<TMP_Text>().text = $"Jugador {clientId}";
            playerItems.Add(clientId, playerItem);
        }
        else
        {
            if (playerItems.TryGetValue(clientId, out GameObject playerItem))
            {
                Destroy(playerItem);
                playerItems.Remove(clientId);
            }
        }
    }
}