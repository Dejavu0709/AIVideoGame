using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using NexgenDragon;
using Newtonsoft.Json;

public class BranchingVideoGameManager : MonoSingleton<BranchingVideoGameManager>
{
    [Header("Components")]
    public VideoPlayerController videoController;
    public VideoManager videoManager;
    public GameUIController uiController;
    
    [Header("Game Configuration")]
    public TextAsset gameDataJson;

    public string cdnBase = "https://636c-cloud1-7gwlsz5m226cfca9-1369289063.tcb.qcloud.la/AIVideoGame";
    public string gameDataUrl; // Remote URL
    public string gameDataLocalUrl; // Local path or StreamingAssets-relative path
    
    [Header("Settings")]
    public float delayBeforeShowingChoices = 1f;

    [Header("Game Data")]
    private static GameData gameData;
    private GameNode currentNode;
    private Dictionary<string, GameNode> nodeLookup;
    private bool isGameActive = false;
    // QTE control
    private Coroutine qteDelayRoutine;
    private bool qteShownForCurrentNode = false;
    private bool qtePendingForCurrentNode = false;
    private float qtePendingDelaySeconds = 0f;

    // Progress: visited nodes
    private const string PlayerPrefsVisitedKey = "BVGM_VisitedNodes";
    private HashSet<string> visitedNodes = new HashSet<string>();

    public static GameData GameData { get => gameData; set => gameData = value; }

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
        // Load progress after data is ready
        LoadProgress();
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
    else if (!string.IsNullOrEmpty(gameDataLocalUrl))
    {
        StartCoroutine(LoadGameDataFromInput(gameDataLocalUrl, true));
        return true; // started async loading
    }
    else if (!string.IsNullOrEmpty(gameDataUrl))
    {
        StartCoroutine(LoadGameDataFromInput(gameDataUrl, false));
        return true; // Return true as we've started the loading process
    }
    
    Debug.LogError("No game data source provided!");
    return false;
}

// Unified loader for both local and remote inputs
private IEnumerator LoadGameDataFromInput(string inputPath, bool preferLocal)
{
    // Compute final URL for inputPath
    string input = inputPath?.Trim();
    if (string.IsNullOrEmpty(input))
    {
        Debug.LogError("gameDataUrl is empty");
        yield break;
    }

    string finalUrl = input;
    bool hasScheme = input.Contains("://");

    // If no scheme and not rooted
    if (!hasScheme && !Path.IsPathRooted(input))
    {
        if (preferLocal)
        {
            // Relative to StreamingAssets
            string localPath = Path.Combine(Application.streamingAssetsPath, input).Replace("\\", "/");
            bool localHasScheme = localPath.Contains("://");
            finalUrl = localHasScheme ? localPath : ("file:///" + localPath);
        }
        else
        {
            // For remote without scheme, assume https (rare). Users should provide full URL.
            Debug.LogWarning($"Input '{input}' has no scheme; attempting to treat as StreamingAssets relative.");
            string localPath = Path.Combine(Application.streamingAssetsPath, input).Replace("\\", "/");
            finalUrl = "file:///" + localPath;
        }
    }
    // If absolute path to existing file, convert to file URL
    else if (!hasScheme && Path.IsPathRooted(input) && File.Exists(input))
    {
        string normalized = input.Replace("\\", "/");
        finalUrl = normalized.StartsWith("file://") ? normalized : ("file:///" + normalized);
    }
    // else: keep as provided (http/https or platform-specific scheme)

    Debug.Log($"Loading game data from URL: {finalUrl}");

    using (UnityWebRequest request = UnityWebRequest.Get(finalUrl))
    {
        Debug.Log($"1Loading game data from URL: {finalUrl}");
        // Send the request and wait for it to complete
        yield return request.SendWebRequest();
        Debug.Log($"2Loading game data from URL: {finalUrl}");
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                Debug.Log($"get game data from URL: {request.downloadHandler.text}");
                    // Parse the JSON data
                gameData = JsonConvert.DeserializeObject<GameData>(request.downloadHandler.text);
                 //   gameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);
                Debug.Log($"Successfully loaded game data from URL: {gameData.meta.title}");
                        // Create node lookup dictionary for fast access
                CreateNodeLookup();
                // Start the game after data is loaded
                //StartGame();
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
        Debug.Log("PlayNode");
        if (!isGameActive)
            return;
            
        if (!nodeLookup.ContainsKey(nodeId))
        {
            Debug.LogError($"Node with ID '{nodeId}' not found!");
            return;
        }
        
        currentNode = nodeLookup[nodeId];
        // Mark visited and persist
        MarkVisited(currentNode.id);
        // reset QTE state for the new node
        if (qteDelayRoutine != null)
        {
            StopCoroutine(qteDelayRoutine);
            qteDelayRoutine = null;
        }
        qteShownForCurrentNode = false;
        qtePendingForCurrentNode = false;
        qtePendingDelaySeconds = 0f;
        Debug.Log($"Playing node: {nodeId} - {currentNode.question}");
        
        // Hide UI while video plays
        if (uiController != null)
            uiController.HideAllUI();
        // uiController.functionPanel.SetActive(true);
        // Play the video
        if (videoController != null && !string.IsNullOrEmpty(currentNode.video))
        {
            string videoUrl = GetVideoUrl(currentNode.video);
            videoController.PlayVideo(videoUrl);
            // Defer QTE countdown until the video is confirmed started (OnVideoStarted)
            if (currentNode.qte != null)
            {
                qtePendingForCurrentNode = true;
                qtePendingDelaySeconds = Mathf.Max(0f, currentNode.qte.startDelayFromStartSeconds);
            }
        }
        else
        {
            // If no video, go straight to choices/QTE
            OnVideoFinished();
        }
    }
    
    public string GetVideoUrl(string videoFileName)
    {
        Debug.Log($"GetVideoUrl: {videoFileName}");
        if(videoManager.advancedVideoManager.isLocalVideo)
        {
            return $"{Application.streamingAssetsPath}/Videos/{videoFileName}";
        }
        else
        {
            //gameData.meta.cdnBase = "https://636c-cloud1-7gwlsz5m226cfca9-1369289063.tcb.qcloud.la/AIVideoGame/Videos/";
            string cdnWeb = $"{cdnBase}/Videos/";
            if (!string.IsNullOrEmpty(cdnWeb))
                return cdnWeb.EndsWith("/") ? (cdnWeb + videoFileName) : ($"{cdnWeb}/{videoFileName}");
            return null;
        }
    }
    public string GetThumbnailUrl(string thumbnailFileName)
    {
        Debug.Log($"GetThumbnailUrl: {thumbnailFileName}");
        if(videoManager.advancedVideoManager.isLocalVideo)
        {
            return $"{Application.streamingAssetsPath}/Thumbnails/{thumbnailFileName}";
        }
        else
        {
            //gameData.meta.cdnBase = "https://636c-cloud1-7gwlsz5m226cfca9-1369289063.tcb.qcloud.la/AIVideoGame/Videos/";
            string cdnWeb = $"{cdnBase}/Thumbnails";
            if (!string.IsNullOrEmpty(cdnWeb))
                return cdnWeb.EndsWith("/") ? (cdnWeb + thumbnailFileName) : ($"{cdnWeb}/{thumbnailFileName}");
            return null;
        }
    }
    void OnVideoStarted()
    {
        Debug.Log("Video started playing");
        if (uiController != null)
        {
            //uiController.functionPanel.SetActive(true);
            uiController.ShowAllCanvasGroup();
        }

        // Start QTE countdown only after video successfully starts
        if (isGameActive && currentNode != null && qtePendingForCurrentNode && !qteShownForCurrentNode)
        {
            // Stop any stray coroutine
            if (qteDelayRoutine != null)
            {
                StopCoroutine(qteDelayRoutine);
                qteDelayRoutine = null;
            }

            if (qtePendingDelaySeconds > 0f)
            {
                Debug.Log($"Starting QTE countdown in {qtePendingDelaySeconds} seconds");
                qteDelayRoutine = StartCoroutine(ShowQTEAtDelay(qtePendingDelaySeconds));
            }
            //else
            {
                // Show immediately on video start when delay is 0
                //qteShownForCurrentNode = true;
                //ShowQTE();
            }
        }
    }
    
    void OnVideoFinished()
    {
        Debug.Log("Video finished, showing interaction: " + currentNode.question);
        
        if (currentNode == null)
            return;
        // If this node is explicitly marked as an end/death node, show death UI
        if (currentNode.isEnd)
        {
            if (uiController != null)
            {
                uiController.ShowDeath(currentNode);
            }
            return;
        }
        else if(!string.IsNullOrEmpty(currentNode.next))
        {
            PlayNode(currentNode.next);
            uiController.HideAllCanvasGroup();
            return;
        }
        // if(currentNode.qte != null && currentNode.qte.startDelayFromStartSeconds > 0)//播放过程中已经完成qte结果
        // {
        //     DecideNextNodeByQTE();
        //     return;
        // }
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
        else if (currentNode.qte != null && !qteShownForCurrentNode)
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

    // Trigger QTE after a delay while video is playing
    private IEnumerator ShowQTEAtDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        // Ensure still valid state and same node
        if (!isGameActive || currentNode == null || currentNode.qte == null || qteShownForCurrentNode)
            yield break;
        // Pause video at QTE start
        // (User preference) Don't pause video automatically here
        qteShownForCurrentNode = true;
        ShowQTE();
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
        Debug.Log("Showing QTE:" + currentNode.question + currentNode.qte);
        if (uiController != null && currentNode.qte != null)
        {
            Debug.Log("Start Showing QTE:" + currentNode.question + currentNode.qte);
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
            Debug.LogWarning("Next node ID is empty! Treat as death.");
            // Show death view and pause gameplay
            if (uiController != null)
            {
                uiController.ShowDeath(currentNode);
            }
        }
    }
    private int _curScore = 0;
    public void OnQTECompleted(int score)
    {
        Debug.Log($"QTE completed: {score}");
        _curScore += score;
        Debug.Log($"currentNode.qte.startDelayFromStartSeconds: {currentNode.qte.startDelayFromStartSeconds}");
        // if(currentNode.qte.startDelayFromStartSeconds > 0)
        // {
        //     return;
        // }
        // else
        {
            DecideNextNodeByQTE();
        }


     
    }

    private void DecideNextNodeByQTE()
    {
        Debug.Log("DecideNextNodeByQTE");
        if (currentNode?.qte != null)
        {
            string nextNodeId = null;
            var map = currentNode.qte.NextNodeMap;
            if (map != null && map.Count > 0)
            {
                map.TryGetValue(_curScore, out nextNodeId);
                if (!string.IsNullOrEmpty(nextNodeId))
                {
                    PlayNode(nextNodeId);
                    uiController.HideAllCanvasGroup();
                }
                else //默认成功继续播放视频
                {

                }
            }
            
            // if (!string.IsNullOrEmpty(nextNodeId))
            // {
            //     PlayNode(nextNodeId);
            // }
            // else
            // {
            //     Debug.LogWarning("QTE next node ID is empty! Treat as death.");
            //     if (uiController != null)
            //     {
            //         uiController.ShowDeath(currentNode);
            //     }
            // }
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
        Debug.Log("pause");
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
    
    // Progress persistence
    private void LoadProgress()
    {
        visitedNodes.Clear();
        var saved = PlayerPrefs.GetString(PlayerPrefsVisitedKey, string.Empty);
        if (!string.IsNullOrEmpty(saved))
        {
            var parts = saved.Split(',');
            foreach (var id in parts)
            {
                var t = id.Trim();
                if (!string.IsNullOrEmpty(t)) visitedNodes.Add(t);
            }
        }
    }

    private void SaveProgress()
    {
        var list = visitedNodes.ToList();
        PlayerPrefs.SetString(PlayerPrefsVisitedKey, string.Join(",", list));
        PlayerPrefs.Save();
    }

    private void MarkVisited(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;
        if (visitedNodes.Add(nodeId))
        {
            SaveProgress();
        }
    }

    public IReadOnlyCollection<string> GetVisitedNodes()
    {
        return visitedNodes;
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
