using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GamePhase { Contextualization, Questions, Accusation, Strike, GameOver }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float phaseDelay = 2f;

    public NetworkVariable<GamePhase> CurrentPhase = new NetworkVariable<GamePhase>(GamePhase.Contextualization, NetworkVariableReadPermission.Everyone);

    private List<PlayerData> players = new();
    private PlayerData impostor;
    private int round = 1;

    private void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        AssignRoles();

        while (CurrentPhase.Value != GamePhase.GameOver)
        {
            switch (CurrentPhase.Value)
            {
                case GamePhase.Contextualization:
                    yield return ContextualizationPhase();
                    break;

                case GamePhase.Questions:
                    yield return QuestionsPhase();
                    break;

                case GamePhase.Accusation:
                    yield return AccusationPhase();
                    break;

                case GamePhase.Strike:
                    yield return StrikePhase();
                    break;
            }

            if (CheckWinConditions())
            {
                CurrentPhase.Value = GamePhase.GameOver;
                EndGameClientRpc(impostor.Strikes >= 2);
                yield break;
            }

            yield return new WaitForSeconds(phaseDelay);
        }
    }

    private void AssignRoles()
    {
        players = PlayerData.AllPlayers.Values.ToList();
        impostor = players[Random.Range(0, players.Count)];
        impostor.IsImpostor.Value = true;

        foreach (var player in players)
        {
            string roleInfo = player == impostor ? "" : GenerateInnocentContext(player);
            player.AssignRoleClientRpc(roleInfo);
        }
    }

    private IEnumerator ContextualizationPhase()
    {
        CurrentPhase.Value = GamePhase.Contextualization;
        BroadcastScenarioClientRpc("Un crimen ocurrió en el castillo...");
        yield return new WaitForSeconds(phaseDelay);
        CurrentPhase.Value = GamePhase.Questions;
    }

    private IEnumerator QuestionsPhase()
    {
        CurrentPhase.Value = GamePhase.Questions;

        foreach (var player in players)
        {
            if (!player.IsEliminated.Value)
                AskQuestionClientRpc(player.OwnerClientId);
            yield return new WaitUntil(() => player.HasAnswered.Value);
        }

        yield return null;
        CurrentPhase.Value = GamePhase.Accusation;
    }

    private IEnumerator AccusationPhase()
    {
        CurrentPhase.Value = GamePhase.Accusation;

        foreach (var player in players)
        {
            if (!player.IsEliminated.Value)
                AskAccusationClientRpc(player.OwnerClientId);
            yield return new WaitUntil(() => player.HasAccused.Value);
        }

        yield return null;
        CurrentPhase.Value = GamePhase.Strike;
    }

    private IEnumerator StrikePhase()
    {
        CurrentPhase.Value = GamePhase.Strike;

        var guilty = DecideGuiltyPlayer();
        if (guilty != null)
        {
            guilty.AddStrike();
            UpdateStrikesClientRpc(guilty.OwnerClientId, guilty.Strikes);
        }

        round++;
        CurrentPhase.Value = GamePhase.Questions;
        yield return null;
    }

    private bool CheckWinConditions()
    {
        if (impostor.Strikes >= 2)
            return true;

        int eliminated = players.FindAll(p => p.IsEliminated.Value).Count;
        return eliminated >= players.Count / 2;
    }

    private string GenerateInnocentContext(PlayerData player)
    {
        return $"Tu personaje es el jardinero. Estabas podando rosas al momento del crimen.";
    }

    private PlayerData DecideGuiltyPlayer()
    {
        // Prototipo: simplemente devuelve el primer acusado con más votos
        return null;
    }

    [ClientRpc]
    private void BroadcastScenarioClientRpc(string text)
    {
        Debug.Log($"Escenario: {text}");
    }

    [ClientRpc]
    private void AskQuestionClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
           // UIManager.Instance.ShowQuestion("¿Dónde estabas cuando ocurrió el crimen?");
        }
    }

    [ClientRpc]
    private void AskAccusationClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
           // UIManager.Instance.ShowAccusationPanel();
        }
    }

    [ClientRpc]
    private void UpdateStrikesClientRpc(ulong targetClientId, int strikes)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            Debug.Log($"Recibiste un strike. Total: {strikes}");
        }
    }

    [ClientRpc]
    private void EndGameClientRpc(bool innocentsWon)
    {
        string msg = innocentsWon ? "¡Los inocentes ganan!" : "¡El impostor gana!";
        //UIManager.Instance.ShowEndScreen(msg);
    }
}
