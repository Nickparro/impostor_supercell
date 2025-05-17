using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class ServicesAsync : MonoBehaviour
{
    string baseApiUri = "https://hackaton.qwestar.ai/api/";

    public async Task<string> CreateGame()
    {
        string apiUrl = baseApiUri + "game/create";
        Debug.Log("Creating Game");

        using UnityWebRequest request = UnityWebRequest.Post(apiUrl, "", "application/json");
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudo crear la partida: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var data = JsonUtility.FromJson<GameCreationResponse>(responseText);

        Debug.Log("Partida creada exitosamente");
        Debug.Log("Mensaje: " + data.message);
        Debug.Log("Game ID: " + data.game_id);

        return data.game_id;
    }

    public async Task<PlayerRoleResponse> GetPlayerRole(string gameId, string playerId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/role/" + playerId;
        Debug.Log("Getting Player Role " + apiUrl);

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudo obtener el rol del jugador: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var data = JsonUtility.FromJson<PlayerRoleResponse>(responseText);

        Debug.Log("Rol del jugador obtenido exitosamente");
        Debug.Log("Rol: " + data.role.name);
        Debug.Log("Escenario: " + data.scenario.title);

        return data;
    }

    public async Task<PlayerQuestionResponse> GeneratePlayerQuestion(string gameId, string playerId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/question/" + playerId;
        Debug.Log("Generating player question");

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudo generar la pregunta: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var data = JsonUtility.FromJson<PlayerQuestionResponse>(responseText);

        Debug.Log("Pregunta generada: " + data.question);

        return data;
    }

    public async Task<string> AnswerSherlockQuestion(string gameId, string playerId, string answer)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/question";

        SherlockAnswerRequest requestBody = new SherlockAnswerRequest
        {
            player_id = playerId,
            answer = answer
        };

        string json = JsonUtility.ToJson(requestBody);

        using UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, "");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al enviar respuesta: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        Debug.Log("Respuesta enviada correctamente: " + responseText);

        return responseText;
    }

    public async Task<SherlockAnswerSummary> GetSherlockAnswerSummary(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/summary";
        Debug.Log("Getting Sherlock answer summary");

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudo obtener el resumen de la respuesta: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var data = JsonUtility.FromJson<SherlockAnswerSummary>(responseText);

        Debug.Log("Resumen de respuesta: " + data.summary);

        return data;
    }

    public async Task<string> AccusePlayer(string gameId, string accuserId, string accusedId, string reason)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/accuse";

        AccusationRequest requestBody = new AccusationRequest
        {
            accuser = accuserId,
            accused = accusedId,
            reason = reason
        };

        string json = JsonUtility.ToJson(requestBody);

        using UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, "");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al acusar: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        Debug.Log("Acusación enviada exitosamente.");
        Debug.Log("Respuesta del servidor: " + responseText);

        return responseText;
    }

    public async Task<AccusationsResponse> GetAccusations(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/accusations";

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener acusaciones: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var data = JsonConvert.DeserializeObject<AccusationsResponse>(responseText);

        foreach (var entry in data.accusations)
        {
            Debug.Log($"Jugador ID: {entry.Key}");

            foreach (var razon in entry.Value)
            {
                Debug.Log($" - Razón: {razon}");
            }
        }

        return data;
    }

    public async Task<string> PostPlayerDefense(string gameId, string playerId, string defense)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/defend";

        DefenseRequest requestBody = new DefenseRequest
        {
            player_id = playerId,
            defense = defense
        };

        string json = JsonUtility.ToJson(requestBody);

        using UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, "");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al enviar defensa: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        Debug.Log("Defensa enviada exitosamente.");
        Debug.Log("Respuesta del servidor: " + responseText);

        return responseText;
    }

    public async Task<StrikesResponses> GetStrikes(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/strikes";

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener strikes: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var data = JsonConvert.DeserializeObject<StrikesResponses>(responseText);

        foreach (var strike in data.strikes)
        {
            Debug.Log($"Jugador: {strike.player_id}, Ronda: {strike.round}, Motivo: {strike.reason}");
        }

        return data;
    }

    public async Task<string> PostStrikeToPlayer(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/strikes";

        using UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, "");
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al aplicar strike: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        Debug.Log("Strike aplicado correctamente.");
        Debug.Log("Respuesta del servidor: " + responseText);

        return responseText;
    }

    public async Task<GameStateResponse> GetGeneralGameState(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/status";

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener estado general: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var estado = JsonConvert.DeserializeObject<GameStateResponse>(responseText);

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

        return estado;
    }

    public async Task<string> PostGoToNextPhase(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/next-phase";

        using UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, "");
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cambiar de fase: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        Debug.Log("Se pasó a la siguiente fase.");
        Debug.Log("Respuesta del servidor: " + responseText);

        return responseText;
    }

    public async Task<GameResultResponse> VerifyIfGameHasFinished(string gameId)
    {
        string apiUrl = baseApiUri + "game/" + gameId + "/next-phase";

        using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al verificar el estado del juego: " + request.error);
            return null;
        }

        var responseText = request.downloadHandler.text;
        var data = JsonConvert.DeserializeObject<GameResultResponse>(responseText);

        Debug.Log("Mensaje del sistema: " + data.winner.message);

        if (!string.IsNullOrEmpty(data.winner.player_id))
        {
            Debug.Log("Jugador ganador: " + data.winner.player_id);
        }

        if (!string.IsNullOrEmpty(data.winner.winner))
        {
            Debug.Log("Ganador: " + data.winner.winner);
        }

        return data;
    }
}