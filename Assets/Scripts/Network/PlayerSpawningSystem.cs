using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawningSystem : NetworkBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject[] playerPrefabs; 

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;

    private HashSet<int> assignedModels = new HashSet<int>();
    private NetworkList<int> usedModelIndices;

    private void Awake()
    {
        usedModelIndices = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnNetworkDespawn();
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
        usedModelIndices.Add(modelIndex);
        Transform spawnPoint = GetRandomSpawnPoint();
        GameObject playerInstance = Instantiate(playerPrefabs[modelIndex], spawnPoint.position, spawnPoint.rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
        InformClientAboutModelClientRpc(clientId, modelIndex);
    }

    private int GetRandomUnusedModelIndex()
    {
        if (usedModelIndices.Count >= playerPrefabs.Length)
        {
            Debug.LogWarning("Todos los modelos han sido usados, reiniciando la selección.");
            usedModelIndices.Clear();
        }

        List<int> availableIndices = new List<int>();

        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            if (!usedModelIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        int randomIndex = Random.Range(0, availableIndices.Count);
        return availableIndices[randomIndex];
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay puntos de spawn definidos. Usando la posición (0,0,0).");
            return null; 
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex];
    }

    [ClientRpc]
    private void InformClientAboutModelClientRpc(ulong clientId, int modelIndex)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"Te ha sido asignado el modelo de jugador: {modelIndex}");
        }
    }
}