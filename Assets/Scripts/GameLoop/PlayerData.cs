using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public static Dictionary<ulong, PlayerData> AllPlayers = new();

    public NetworkVariable<bool> IsImpostor = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsEliminated = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> HasAnswered = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> HasAccused = new NetworkVariable<bool>(false);

    public int Strikes = 0;

    [ClientRpc]
    public void AssignRoleClientRpc(string roleContext)
    {
        if (!string.IsNullOrEmpty(roleContext))
        {
            Debug.Log($"Tu rol: {roleContext}");
        }
        else
        {
            Debug.Log("Eres el impostor. No tienes contexto.");
        }
    }

    public void AddStrike()
    {
        Strikes++;
        if (Strikes >= 2)
            IsEliminated.Value = true;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AllPlayers[OwnerClientId] = this;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            AllPlayers.Remove(OwnerClientId);
        }
    }
}

