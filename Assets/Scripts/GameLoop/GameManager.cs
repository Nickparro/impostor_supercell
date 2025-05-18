using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using System.Threading.Tasks;
using System;
using TMPro;

public enum GamePhase { Contextualization, Questions, Strike, GameOver }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [HideInInspector]
    public PlayerData localPlayer;
    private Dictionary<ulong, PlayerData> allPlayers => PlayerData.AllPlayers;

    [SerializeField] private float phaseDelay = 2f;
    [SerializeField] public ServicesAsync services;

    // Number of questions each player should answer per round
    [SerializeField] private int questionsPerPlayer = 2;

    public NetworkVariable<GamePhase> CurrentPhase = new NetworkVariable<GamePhase>(GamePhase.Contextualization, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<FixedString32Bytes> gameID = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private List<PlayerData> players = new();
    private PlayerData impostor;
    private int round = 1;

    private NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> currentQuestionNumber = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            StartCoroutine(InitializeLocalPlayer());
        }
    }

    private IEnumerator InitializeLocalPlayer()
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        while (localPlayer == null)
        {
            if (PlayerData.AllPlayers.TryGetValue(localClientId, out PlayerData player))
            {
                localPlayer = player;
                Debug.Log($"GameManager: Jugador local inicializado con ID {player.id.Value} para cliente {localClientId}");
                break;
            }
            yield return new WaitForSeconds(0.1f);
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

                case GamePhase.Strike:
                    yield return StrikePhase();
                    break;
            }

             if (CheckWinConditionsAsync())
             {
                 CurrentPhase.Value = GamePhase.GameOver;
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
            PlayerData.LocalPlayer.playerNameText.text = rol.role.name;
            if (rol.role.is_guilty) impostor = player;
            string roleInfo = player.IsImpostor.Value ? "" : rol.role.facade;
            player.AssignRoleClientRpc(roleInfo);
            player.ShowRolePanelClientRpc(rol.role.name, roleInfo + " " + rol.role.hidden);
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

    [ClientRpc]
    private void ShowIAStrikeClientRpc(string ia)
    {
        UIManager.Instance.ShowIAPanel(ia);
    }

    public async void SendAnswerAsync(string answer)
    {
        await services.AnswerSherlockQuestion(gameID.Value.ToString(), PlayerData.LocalPlayer.id.ToString(), answer);

        if (IsHost)
        {
            PlayerData.LocalPlayer.HasAnswered.Value = true;
        }
        else if (PlayerData.LocalPlayer != null)
        {
            NotifyAnsweredServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyAnsweredServerRpc(ulong clientId)
    {
        var player = players.FirstOrDefault(p => p.OwnerClientId == clientId);
        if (player != null)
        {
            player.HasAnswered.Value = true;
        }
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
        Debug.Log("Starting Questions Phase");

        currentQuestionNumber.Value = 0;

        var activePlayers = players.Where(p => !p.IsEliminated.Value).ToList();
        for (int questionRound = 1; questionRound <= questionsPerPlayer; questionRound++)
        {
            currentQuestionNumber.Value = questionRound;
            Debug.Log($"Starting question round {currentQuestionNumber.Value} of {questionsPerPlayer}");
            foreach (var player in activePlayers)
            {
                player.HasAnswered.Value = false;
            }

            for (int i = 0; i < activePlayers.Count; i++)
            {
                var player = activePlayers[i];
                currentPlayerIndex.Value = i; 

                TaskCompletionSource<PlayerQuestionResponse> tcs = new TaskCompletionSource<PlayerQuestionResponse>();
                FetchQuestionForPlayer(player, tcs);
                yield return new WaitUntil(() => tcs.Task.IsCompleted);

                if (tcs.Task.IsCompletedSuccessfully)
                {
                    PlayerQuestionResponse response = tcs.Task.Result;
                    AskQuestionClientRpc(player.OwnerClientId, response.question);
                    yield return new WaitUntil(() => player.HasAnswered.Value);
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    Debug.LogError($"Failed to fetch question for player {player.id.Value}");
                    continue;
                }
            }
            yield return new WaitForSeconds(2f);
        }

        Debug.Log("Questions Phase Completed - Moving to Accusation Phase");
        currentPlayerIndex.Value = -1;
        currentQuestionNumber.Value = 0;
        yield return new WaitForSeconds(2f);

        GetSummaryForAllClientsAsync();
        CurrentPhase.Value = GamePhase.Strike;
    }

    async void GetSummaryForAllClientsAsync()
    {
        if (IsHost)
        {
            SherlockAnswerSummary summary = await services.GetSherlockAnswerSummary(gameID.Value.ToString());
            ShowSummaryClientRpc(summary.summary);
        }
    }

    [ClientRpc]
    private void ShowSummaryClientRpc(string summary)
    {
        UIManager.Instance.ShowIAPanel(summary);
    }
    private async void FetchQuestionForPlayer(PlayerData player, TaskCompletionSource<PlayerQuestionResponse> tcs)
    {
        try
        {
            Debug.Log($"Fetching question {currentQuestionNumber.Value} for player {player.id.Value}");
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

    private IEnumerator StrikePhase()
    {
        StopCoroutine(QuestionsPhase());
        CurrentPhase.Value = GamePhase.Strike;
        Debug.Log("Starting Strike Phase");
        GetStikeAsync();
        round++;
        Debug.Log("Strike Phase Completed - Moving back to Questions Phase");
        yield return new WaitForSeconds(3f);
        CurrentPhase.Value = GamePhase.Questions;
    }

    async void GetStikeAsync()
    {
        await services.PostGoToNextPhase(gameID.Value.ToString());
        Strike strike = await services.PostStrikeToPlayer(gameID.Value.ToString());
        if (strike != null)
        {
            foreach (var player in players)
            {
                if (player.id.Value.ToString() == strike.player_id)
                {
                    player.Strikes++;
                    UpdateStrikesClientRpc(player.OwnerClientId, player.Strikes);
                    Debug.Log($"Player {player.id.Value} received a strike. Total: {player.Strikes}");
                    ShowIAStrikeClientRpc(strike.reason);
                    break;
                }
            }
        }

    }
    private bool CheckWinConditionsAsync()
    {
        int eliminated = players.FindAll(p => p.IsEliminated.Value).Count;
        return eliminated >= 2;
    }


    [ClientRpc]
    private void AskQuestionClientRpc(ulong targetClientId, string question)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            Debug.Log($"Showing question {currentQuestionNumber.Value} to player {targetClientId}: {question}");
            UIManager.Instance.ShowQuestion(question);
        }
    }

    [ClientRpc]
    private void UpdateStrikesClientRpc(ulong targetClientId, int strikes)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            Debug.Log($"Recibiste un strike. Total: {strikes}");
            localPlayer.AddStrikeServerRpc();
        }
    }

    [ClientRpc]
    private void EndGameClientRpc(bool innocentsWon)
    {
        string msg = innocentsWon ? "¡Los inocentes ganan!" : "¡El impostor gana!";
        //UIManager.Instance.ShowEndScreen(msg);
    }

    public void ConfirmAnswerSubmitted()
    {
        if (PlayerData.LocalPlayer != null)
        {
            NotifyAnsweredServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
}