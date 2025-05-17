using System.Text;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public interface IHttpResponseDeserializer
{
    public T Deserialice<T>(string strData);
}
public class HttpOptions
{
    public (string key, string value)[] headers = null;
    public bool requiredAuth = false;
    public string body = "";
}
public class HttpResponse<T>
{
    public string error;
    public string errorString;
    public long responseCode;
    public T value;
    public string data;

    public bool success = false;
}
public class HttpHandlerSync
{
    public IEnumerator Get(string url, System.Action<string> onSuccess, System.Action<string> onError = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            onError?.Invoke(request.error);
        }
    }
    public IEnumerator Post(string url, string jsonBody, System.Action<string> onSuccess, System.Action<string> onError = null)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            onError?.Invoke(request.error);
        }
    }
}

public static class JsonHelper
{
    public static T[] FromJsonArray<T>(string json)
    {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
