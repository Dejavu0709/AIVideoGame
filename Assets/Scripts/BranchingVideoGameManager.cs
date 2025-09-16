using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
public class BranchingVideoGameManager : MonoBehaviour
{
    [Header("Components")]
    public VideoPlayerController videoController;
    public GameUIController uiController;
    
    [Header("Game Configuration")]
    public TextAsset gameDataJson;
    public string gameDataUrl; // Alternative: load from URL
    
    [Header("Settings")]
    public float delayBeforeShowingChoices = 1f;
    
    private GameData gameData;
    private GameNode currentNode;
    private Dictionary<string, GameNode> nodeLookup;
    private bool isGameActive = false;
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // Load game data
        if (!LoadGameData())
        {
            Debug.LogError("Failed to load game data!");
            return;
        }
    
        
        // Setup video controller events
        if (videoController != null)
        {
            videoController.OnVideoFinished.AddListener(OnVideoFinished);
            videoController.OnVideoStarted.AddListener(OnVideoStarted);
        }
    
    }
    
   // Replace the existing LoadGameData method with this implementation
bool LoadGameData()
{
    if (gameDataJson != null)
    {
        try
        {
            gameData = JsonUtility.FromJson<GameData>(gameDataJson.text);
            Debug.Log($"Loaded game data: {gameData.meta.title}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse game data JSON: {e.Message}");
            return false;
        }
    }
    else if (!string.IsNullOrEmpty(gameDataUrl))
    {
        StartCoroutine(LoadGameDataFromUrl());
        return true; // Return true as we've started the loading process
    }
    
    Debug.LogError("No game data source provided!");
    return false;
}

// Add this new coroutine method to your BranchingVideoGameManager class
private IEnumerator LoadGameDataFromUrl()
{
    Debug.Log($"Loading game data from URL: {gameDataUrl}");
    
    using (UnityWebRequest request = UnityWebRequest.Get(gameDataUrl))
    {
        // Send the request and wait for it to complete
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                // Parse the JSON data
                gameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);
                Debug.Log($"Successfully loaded game data from URL: {gameData.meta.title}");
                        // Create node lookup dictionary for fast access
        CreateNodeLookup();
                // Start the game after data is loaded
                StartGame();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse game data JSON from URL: {e.Message}");
                ShowErrorMessage("Failed to parse game data");
            }
        }
        else
        {
            Debug.LogError($"Failed to load game data from URL: {request.error}");
            ShowErrorMessage($"Failed to load game data: {request.error}");
        }
    }
}

// Add this helper method to show error messages to the user
private void ShowErrorMessage(string message)
{
    // Display error message to the user
    // You can implement this based on your UI system
    Debug.LogError(message);
    // Example: if (uiController != null) uiController.ShowError(message);
}
    void CreateNodeLookup()
    {
        nodeLookup = new Dictionary<string, GameNode>();
        
        if (gameData?.nodes != null)
        {
            foreach (GameNode node in gameData.nodes)
            {
                nodeLookup[node.id] = node;
            }
        }
    }
    
    public void StartGame()
    {
        if (gameData?.meta?.startNodeId != null)
        {
            isGameActive = true;
            PlayNode(gameData.meta.startNodeId);
        }
        else
        {
            Debug.LogError("No start node ID specified in game data!");
        }
    }
    
    public void RestartGame()
    {
        StopAllCoroutines();
        
        if (videoController != null)
            videoController.StopVideo();
            
        if (uiController != null)
            uiController.HideAllUI();
        
        StartGame();
    }
    
    public void PlayNode(string nodeId)
    {
        if (!isGameActive)
            return;
            
        if (!nodeLookup.ContainsKey(nodeId))
        {
            Debug.LogError($"Node with ID '{nodeId}' not found!");
            return;
        }
        
        currentNode = nodeLookup[nodeId];
        Debug.Log($"Playing node: {nodeId} - {currentNode.question}");
        
        // Hide UI while video plays
        if (uiController != null)
            uiController.HideAllUI();
         uiController.functionPanel.SetActive(true);
        // Play the video
        if (videoController != null && !string.IsNullOrEmpty(currentNode.video))
        {
            string videoUrl = GetVideoUrl(currentNode.video);
            videoController.PlayVideo(videoUrl);
        }
        else
        {
            // If no video, go straight to choices/QTE
            OnVideoFinished();
        }
    }
    
    string GetVideoUrl(string videoFileName)
    {
        if (string.IsNullOrEmpty(gameData?.meta?.cdnBase))
        {
            // Local file path
            return System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
        }
        else
        {
            // CDN URL
            return $"{gameData.meta.cdnBase}/{videoFileName}";
        }
    }

    void OnVideoStarted()
    {
        Debug.Log("Video started playing");

        if (uiController != null)
        {
            uiController.functionPanel.SetActive(true);
            uiController.ShowAllCanvasGroup();
        }
    }
    
    void OnVideoFinished()
    {
        Debug.Log("Video finished, showing interaction: " + currentNode.question);
        
        if (currentNode == null)
            return;
        
        StartCoroutine(ShowInteractionAfterDelay());
    }
    
    IEnumerator ShowInteractionAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShowingChoices);
        Debug.Log("Showing interaction: " + currentNode.qte + currentNode.choices);
        if (currentNode.choices != null && currentNode.choices.Count > 0)
        {
            // Show choices
            ShowChoices();
        }
        else if (currentNode.qte != null)
        {
            // Show QTE
            ShowQTE();
        }
        else
        {
            // No interaction, game might be over
            Debug.Log("No choices or QTE available. Game might be finished.");
            OnGameFinished();
        }
    }
    
    void ShowChoices()
    {
        Debug.Log("Showing choices:" + currentNode.question + currentNode.choices.Count);
        if (uiController != null && currentNode.choices != null)
        {
            uiController.ShowChoices(currentNode.question, currentNode.choices, OnChoiceSelected);
        }
    }
    
    void ShowQTE()
    {
        if (uiController != null && currentNode.qte != null)
        {
            uiController.ShowQTE(currentNode.qte, OnQTECompleted);
        }
    }
    
    void OnChoiceSelected(string nextNodeId)
    {
        Debug.Log($"Choice selected: {nextNodeId}");

        if (!string.IsNullOrEmpty(nextNodeId))
        {
            PlayNode(nextNodeId);
            uiController.HideAllCanvasGroup();
        }
        else
        {
            Debug.LogWarning("Next node ID is empty!");
            OnGameFinished();
        }
    }
    
    void OnQTECompleted(bool success)
    {
        Debug.Log($"QTE completed: {(success ? "Success" : "Failed")}");
        
        if (currentNode?.qte != null)
        {
            string nextNodeId = success ? currentNode.qte.successNext : currentNode.qte.failNext;
            
            if (!string.IsNullOrEmpty(nextNodeId))
            {
                PlayNode(nextNodeId);
            }
            else
            {
                Debug.LogWarning("QTE next node ID is empty!");
                OnGameFinished();
            }
        }
    }
    
    void OnGameFinished()
    {
        Debug.Log("Game finished!");
        isGameActive = false;
        
        if (uiController != null)
            uiController.HideAllUI();
        
        // You can add game over UI or restart options here
        StartCoroutine(ShowRestartOption());
    }
    
    IEnumerator ShowRestartOption()
    {
        yield return new WaitForSeconds(2f);
        
        // Create a simple restart choice
        List<Choice> restartChoices = new List<Choice>
        {
            new Choice { label = "重新开始游�?", next = gameData.meta.startNodeId }
        };
        
        if (uiController != null)
        {
            uiController.ShowChoices("游戏结束", restartChoices, OnChoiceSelected);
        }
    }
    
    // Public methods for external control
    public void PauseGame()
    {
        if (videoController != null)
            videoController.PauseVideo();
    }
    
    public void ResumeGame()
    {
        if (videoController != null)
            videoController.ResumeVideo();
    }
    
    public void StopGame()
    {
        isGameActive = false;
        
        if (videoController != null)
            videoController.StopVideo();
            
        if (uiController != null)
            uiController.HideAllUI();
    }
    
    public GameNode GetCurrentNode()
    {
        return currentNode;
    }
    
    public bool IsGameActive()
    {
        return isGameActive;
    }
    
    // Debug methods
    [ContextMenu("Restart Game")]
    void DebugRestartGame()
    {
        RestartGame();
    }
    
    [ContextMenu("Show Current Node Info")]
    void DebugShowCurrentNodeInfo()
    {
        if (currentNode != null)
        {
            Debug.Log($"Current Node: {currentNode.id}\nVideo: {currentNode.video}\nQuestion: {currentNode.question}");
        }
        else
        {
            Debug.Log("No current node");
        }
    }
}
