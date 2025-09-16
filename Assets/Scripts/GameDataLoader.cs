using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GameDataLoader : MonoBehaviour
{
    public static GameDataLoader Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public IEnumerator LoadGameDataFromURL(string url, System.Action<GameData> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    GameData gameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);
                    onSuccess?.Invoke(gameData);
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"Failed to parse JSON: {e.Message}");
                }
            }
            else
            {
                onError?.Invoke($"Failed to load from URL: {request.error}");
            }
        }
    }
    
    public GameData LoadGameDataFromStreamingAssets(string fileName)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        
        try
        {
            string jsonContent;
            
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                // For platforms like WebGL, Android
                var request = UnityWebRequest.Get(filePath);
                request.SendWebRequest();
                
                while (!request.isDone)
                {
                    System.Threading.Thread.Sleep(10);
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    jsonContent = request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError($"Failed to load from StreamingAssets: {request.error}");
                    return null;
                }
            }
            else
            {
                // For standalone platforms
                if (System.IO.File.Exists(filePath))
                {
                    jsonContent = System.IO.File.ReadAllText(filePath);
                }
                else
                {
                    Debug.LogError($"File not found: {filePath}");
                    return null;
                }
            }
            
            return JsonUtility.FromJson<GameData>(jsonContent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game data: {e.Message}");
            return null;
        }
    }
}
