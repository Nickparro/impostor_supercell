using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class PlayerLobby : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(
        "Jugador",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetPlayerNameServerRpc($"Jugador {NetworkManager.Singleton.LocalClientId}");
        }
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string name)
    {
        PlayerName.Value = name;
    }

    [ServerRpc]
    public void ToggleReadyServerRpc()
    {
        IsReady.Value = !IsReady.Value;
    }
}