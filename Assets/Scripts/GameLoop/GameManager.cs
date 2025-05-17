using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using System.Threading.Tasks;
using System;

public enum GamePhase { Contextualization, Questions, Accusation, Strike, GameOver }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float phaseDelay = 2f;
    [SerializeField] private ServicesAsync services;

    public NetworkVariable<GamePhase> CurrentPhase = new NetworkVariable<GamePhase>(GamePhase.Contextualization, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<FixedString32Bytes> gameID = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private List<PlayerData> players = new();
    private PlayerData impostor;
    private int round = 1;

    private void Awake() => Instance = this;

    public async void StartGame()
    {
        if (IsHost)
        {
            Debug.Log("start gameeeeeeeee");
            string id = await services.CreateGame();
            if (!string.IsNullOrEmpty(id))
            {
                gameID.Value = id;
                StartCoroutine(GameLoop());
            }
            else
            {
                Debug.LogWarning("No se pudo obtener el Game ID.");
            }
        }
    }

    private IEnumerator GameLoop()
    {
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

    private async void AssignRoles()
    {
        players = PlayerData.AllPlayers.Values.ToList();
        foreach (var player in players)
        {
            PlayerRoleResponse rol = await services.GetPlayerRole(gameID.Value.ToString(), player.id.Value.ToString());
            player.IsImpostor.Value = rol.role.is_guilty;
            string roleInfo = player.IsImpostor.Value ? "" : rol.role.facade;
            player.AssignRoleClientRpc(roleInfo);
            player.ShowRolePanelClientRpc(roleInfo, rol.role.hidden);
        }
    }

    [ClientRpc]
    private void ShowIAContextClientRpc()
    {
        ShowIAContext();
    }


    private async void ShowIAContext()
    {
        PlayerRoleResponse rol = await services.GetPlayerRole(gameID.Value.ToString(), "1");
        UIManager.Instance.ShowIAPanel(rol.scenario.intro);
    }


    private IEnumerator ContextualizationPhase()
    {
        yield return new WaitForSeconds(5f);
        CurrentPhase.Value = GamePhase.Contextualization;
        ShowIAContextClientRpc();
        yield return new WaitForSeconds(13f);
        AssignRoles();
        yield return new WaitForSeconds(13f);
        yield return new WaitForSeconds(phaseDelay);
        CurrentPhase.Value = GamePhase.Questions;
    }

    private IEnumerator QuestionsPhase()
    {
        CurrentPhase.Value = GamePhase.Questions;
        foreach (var player in players)
        {
            if (!player.IsEliminated.Value)
            {
                TaskCompletionSource<PlayerQuestionResponse> tcs = new TaskCompletionSource<PlayerQuestionResponse>();
                FetchQuestionForPlayer(player, tcs);
                yield return new WaitUntil(() => tcs.Task.IsCompleted);
                PlayerQuestionResponse response = tcs.Task.Result;
                AskQuestionClientRpc(player.OwnerClientId, response.question);
                yield return new WaitUntil(() => player.HasAnswered.Value);
            }
        }
        yield return null;
        CurrentPhase.Value = GamePhase.Accusation;
    }

    private async void FetchQuestionForPlayer(PlayerData player, TaskCompletionSource<PlayerQuestionResponse> tcs)
    {
        try
        {
            PlayerQuestionResponse response = await services.GeneratePlayerQuestion(
                gameID.Value.ToString(),
                player.id.Value.ToString()
            );
            tcs.SetResult(response);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener pregunta: {ex.Message}");
            tcs.SetException(ex);
        }
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

        var guilty = DecideGuiltyPlayer(); // lógica simple o IA futura
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

    private PlayerData DecideGuiltyPlayer()
    {
        // Prototipo: simplemente devuelve el primer acusado con más votos
        return null;
    }

    [ClientRpc]
    private void AskQuestionClientRpc(ulong targetClientId, string question)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
             UIManager.Instance.ShowQuestion(question);
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
