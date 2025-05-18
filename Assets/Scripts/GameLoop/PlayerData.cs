using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Collections;

public class PlayerData : NetworkBehaviour
{
    public static readonly Dictionary<ulong, PlayerData> AllPlayers = new Dictionary<ulong, PlayerData>();
    private static int nextId = 1;
    public static PlayerData LocalPlayer;

    public NetworkVariable<bool> IsImpostor = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsEliminated = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> HasAnswered = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> HasAccused = new NetworkVariable<bool>(false);
    public NetworkVariable<int> id = new NetworkVariable<int>();

    public int Strikes = 0;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;
            Debug.Log("Este es mi PlayerData.");
        }
        if (IsServer)
        {
            // Asignar ID único
            id.Value = nextId;
            nextId++;

            // Registrar en el diccionario
            AllPlayers[OwnerClientId] = this;

            Debug.Log($"Servidor: Agregado jugador con ID Cliente {OwnerClientId}, PlayerID {id.Value}");
            LogAllPlayers();
        }
        if (IsServer && GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }

        base.OnNetworkSpawn();
    }

    public static void LogAllPlayers()
    {
        Debug.Log($"Total de jugadores en AllPlayers: {AllPlayers.Count}");
        foreach (var player in AllPlayers)
        {
            Debug.Log($"ID Cliente: {player.Key}, ID Jugador: {player.Value.id.Value}");
        }
    }

    [ClientRpc]
    public void AssignRoleClientRpc(string roleContext)
    {
        if (!string.IsNullOrEmpty(roleContext))
            Debug.Log($"Tu rol: {roleContext}");
        else
            Debug.Log("Eres el impostor. No tienes contexto.");
    }

    [ClientRpc]
    public void ShowRolePanelClientRpc(string roleName, string roleDescription)
    {
        if (!IsOwner) return;
        UIManager.Instance.ShowRolePanel(roleName, roleDescription);
    }

    public void AddStrike()
    {
        Strikes++;
        if (Strikes >= 2)
            IsEliminated.Value = true;
    }

    [ServerRpc]
    public void AddStrikeServerRpc()
    {
        AddStrike();
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"OnNetworkDespawn para cliente ID: {OwnerClientId}, IsServer: {IsServer}");

        // Eliminar del diccionario (solo en el servidor)
        if (IsServer)
        {
            Debug.Log($"Servidor: Eliminando jugador con ID Cliente {OwnerClientId}");
            AllPlayers.Remove(OwnerClientId);
            LogAllPlayers();
        }

        // Si es nuestro jugador local, limpiamos la referencia
        if (IsOwner && GameManager.Instance != null && GameManager.Instance.localPlayer == this)
        {
            GameManager.Instance.localPlayer = null;
        }

        base.OnNetworkDespawn();
    }
}