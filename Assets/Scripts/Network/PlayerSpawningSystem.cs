using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawningSystem : NetworkBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject[] playerPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;

    private NetworkList<int> usedModelIndices;
    private NetworkList<int> usedSpawnIndices;

    private void Awake()
    {
        usedModelIndices = new NetworkList<int>();
        usedSpawnIndices = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn PlayerSpawningSystem");

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnPlayerForClient(client);
            }
        }
    }


    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        int modelIndex = GetRandomUnusedModelIndex();
        int spawnIndex = GetRandomUnusedSpawnIndex();

        if (modelIndex == -1 || spawnIndex == -1)
        {
            Debug.LogWarning("No hay modelos o puntos de spawn disponibles.");
            return;
        }

        usedModelIndices.Add(modelIndex);
        usedSpawnIndices.Add(spawnIndex);

        Transform spawnPoint = spawnPoints[spawnIndex];
        GameObject playerInstance = Instantiate(playerPrefabs[modelIndex], spawnPoint.position, spawnPoint.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        // Enviar solo al cliente correspondiente
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong> { clientId }
            }
        };

        InformClientAboutModelClientRpc(modelIndex, rpcParams);
    }

    private int GetRandomUnusedModelIndex()
    {
        if (usedModelIndices.Count >= playerPrefabs.Length) return -1;

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            if (!usedModelIndices.Contains(i))
                availableIndices.Add(i);
        }

        return availableIndices[Random.Range(0, availableIndices.Count)];
    }

    private int GetRandomUnusedSpawnIndex()
    {
        if (usedSpawnIndices.Count >= spawnPoints.Length) return -1;

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedSpawnIndices.Contains(i))
                availableIndices.Add(i);
        }

        return availableIndices[Random.Range(0, availableIndices.Count)];
    }

    [ClientRpc]
    private void InformClientAboutModelClientRpc(int modelIndex, ClientRpcParams rpcParams = default)
    {
        Debug.Log($"Te ha sido asignado el modelo de jugador: {modelIndex}");
    }
}
