using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;
using System;
public class ServicesAsync : MonoBehaviour
{
    public HttpHandlerSync httpHandler;
    string baseApiUri = "https://hackaton.qwestar.ai/api/";
    private void Start()
    {
        httpHandler = new HttpHandlerSync();
    }
    public void CreateGame()
    {
        string apiUrl = baseApiUri + "game/create";
        Debug.Log("Creating Game");

        StartCoroutine(
                  httpHandler.Post(apiUrl, "", (response) =>
                  {
                      GameCreationResponse data = JsonUtility.FromJson<GameCreationResponse>(response);
                      Debug.Log("Partida creada exitosamente");
                      Debug.Log("Mensaje: " + data.message);
                      Debug.Log("Game ID: " + data.game_id);
                  },
                  (error) =>
                  {
                      Debug.LogError("No se pudo crear la partida: " + error);
                  }));
    }
    public void GetPlayerRole(string gameId, string playerId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/role/" + playerId;
        Debug.Log("Getting Player Role");

        StartCoroutine(
                  httpHandler.Get(apiUrl, (response) =>
                  {
                      PlayerRoleResponse data = JsonUtility.FromJson<PlayerRoleResponse>(response);
                      Debug.Log("Rol del jugador obtenido exitosamente");
                      Debug.Log("Rol: " + data.role.name);
                      Debug.Log("Escenario: " + data.scenario.title);
                  },
                  (error) =>
                  {
                      Debug.LogError("No se pudo obtener el rol del jugador: " + error);
                  }));
    }
    public void GeneratePlayerQuestion(string gameId, string playerId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/question/" + playerId;
        Debug.Log("Generating player question");
        StartCoroutine(
            httpHandler.Get(apiUrl, (response) =>
            {
                PlayerQuestionResponse data = JsonUtility.FromJson<PlayerQuestionResponse>(response);
                Debug.Log("Pregunta generada: " + data.question);

            }, (error) =>
            {
                Debug.LogError("No se pudo generar la pregunta: " + error);
            }));
    }
    public void AnswerSherlockQuestion(string gameId, string playerId, string answer)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/question";

        SherlockAnswerRequest requestBody = new SherlockAnswerRequest
        {
            player_id = playerId,
            answer = answer
        };

        string json = JsonUtility.ToJson(requestBody);

        StartCoroutine(
            httpHandler.Post(apiUrl, json, (response) =>
            {
                Debug.Log("Respuesta enviada correctamente: " + response);

            }, (error) =>
            {
                Debug.LogError("Error al enviar respuesta: " + error);
            }));
    }
    public void GetSherlockAnswerSummary(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/summary";
        Debug.Log("Getting Sherlock answer summary");

        StartCoroutine(
            httpHandler.Get(apiUrl, (response) =>
            {
                SherlockAnswerSummary data = JsonUtility.FromJson<SherlockAnswerSummary>(response);
                Debug.Log("Resumen de respuesta: " + data.summary);

            }, (error) =>
            {
                Debug.LogError("No se pudo obtener el resumen de la respuesta: " + error);
            }));
    }

    public void AccusePlayer(string gameId, string accuserId, string accusedId, string reason)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/accuse";

        AccusationRequest requestBody = new AccusationRequest
        {
            accuser = accuserId,
            accused = accusedId,
            reason = reason
        };

        string json = JsonUtility.ToJson(requestBody);

        StartCoroutine(
            httpHandler.Post(apiUrl, json, (response) =>
            {
                Debug.Log("Acusación enviada exitosamente.");
                Debug.Log("Respuesta del servidor: " + response);

            }, (error) =>
            {
                Debug.LogError("Error al acusar: " + error);
            }));
    }

    public void GetAccusations(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/accusations";

        StartCoroutine(
            httpHandler.Get(apiUrl, (response) =>
            {
                var data = JsonConvert.DeserializeObject<AccusationsResponse>(response);

                foreach (var entry in data.accusations)
                {
                    Debug.Log($"Jugador ID: {entry.Key}");

                    foreach (var razon in entry.Value)
                    {
                        Debug.Log($" - Razón: {razon}");
                    }
                }

            }, (error) =>
            {
                Debug.LogError("Error al obtener acusaciones: " + error);
            }));
    }
    public void PostPlayerDefense(string gameId, string playerId, string defense)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/defend";

        DefenseRequest requestBody = new DefenseRequest
        {
            player_id = playerId,
            defense = defense
        };

        string json = JsonUtility.ToJson(requestBody);

        StartCoroutine(
            httpHandler.Post(apiUrl, json, (response) =>
            {
                Debug.Log("Defensa enviada exitosamente.");
                Debug.Log("Respuesta del servidor: " + response);

            }, (error) =>
            {
                Debug.LogError("Error al enviar defensa: " + error);
            }));
    }

    public void GetStrikes(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/strikes";

        StartCoroutine(
            httpHandler.Get(apiUrl, (response) =>
        {
            StrikesResponses data = JsonConvert.DeserializeObject<StrikesResponses>(response);

            foreach (var strike in data.strikes)
            {
                Debug.Log($"Jugador: {strike.player_id}, Ronda: {strike.round}, Motivo: {strike.reason}");
            }

        }, (error) =>
        {
            Debug.LogError("Error al obtener strikes: " + error);
        }));
    }

    public void PostStrikeToPlayer(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/strikes";

        StartCoroutine(
            httpHandler.Post(apiUrl, "", (response) =>
        {
            Debug.Log("Strike aplicado correctamente.");
            Debug.Log("Respuesta del servidor: " + response);

        }, (error) =>
        {
            Debug.LogError("Error al aplicar strike: " + error);
        }));
    }
    public void GetGeneralGameState(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/status";

        StartCoroutine(
            httpHandler.Get(apiUrl, (response) =>
            {
                GameStateResponse estado = JsonConvert.DeserializeObject<GameStateResponse>(response);

                Debug.Log($"Fase actual: {estado.phase}");
                Debug.Log($"Ronda: {estado.round}");

                foreach (var jugador in estado.players)
                {
                    Debug.Log($"Jugador: {jugador.name} | ¿Es culpable?: {jugador.is_guilty}");
                }

                foreach (var strike in estado.strikes)
                {
                    Debug.Log($"Strike → Jugador: {strike.player_id}, Ronda: {strike.round}, Motivo: {strike.reason}");
                }

            }, (error) =>
            {
                Debug.LogError("Error al obtener estado general: " + error);
            }));
    }
    public void PostGoToNextPhase(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/next-phase";

        StartCoroutine(
            httpHandler.Post(apiUrl, "", (response) =>
            {
                Debug.Log("Se pasó a la siguiente fase.");
                Debug.Log("Respuesta del servidor: " + response);

            }, (error) =>
            {
                Debug.LogError("Error al cambiar de fase: " + error);
            }));
    }
    public void VerifyIfGameHasFinished(string gameId) 
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/next-phase";

        StartCoroutine(
            httpHandler.Get(apiUrl, (response) =>
            {
                GameResultResponse data = JsonConvert.DeserializeObject<GameResultResponse>(response);

                Debug.Log("Mensaje del sistema: " + data.winner.message);

                if (!string.IsNullOrEmpty(data.winner.player_id))
                {
                    Debug.Log("Jugador ganador: " + data.winner.player_id);
                }

                if (!string.IsNullOrEmpty(data.winner.winner))
                {
                    Debug.Log("Ganador: " + data.winner.winner);
                }

            }, (error) =>
            {
                Debug.LogError("Error al verificar el estado del juego: " + error);
            }));
    }
}