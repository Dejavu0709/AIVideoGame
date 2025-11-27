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
    public string gameAssetFolder;
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
    // Stats system
    private Dictionary<string, int> currentStats = new Dictionary<string, int>();
    // Track per-operation stat effects to avoid double-counting when an operation outcome changes
    private Dictionary<string, Dictionary<string, int>> operationStatEffects = new Dictionary<string, Dictionary<string, int>>();
    // QTE control
    private Coroutine qteDelayRoutine;
    private bool qteShownForCurrentNode = false;
    private bool qtePendingForCurrentNode = false;
    private float qtePendingDelaySeconds = 0f;
    // Grouped QTE control
    private List<Coroutine> qteGroupRoutines = new List<Coroutine>();
    private int qteGroupRemaining = 0;
    private bool qteGroupActive = false;
    private bool hadQteGroupForCurrentNode = false; // track whether current node uses grouped QTE
    private bool qteGroupCompleted = false; // all scheduled QTEs finished, but settle at video end
    private bool isVideoPlaying = false;

    // Progress: visited nodes
    private const string PlayerPrefsVisitedKey = "BVGM_VisitedNodes";
    private const string PlayerPrefsStatsKey = "BVGM_Stats";
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
        // Initialize stats if data is already present (sync path)
        InitializeStatsFromGameData();
    }
    
   // Replace the existing LoadGameData method with this implementation
bool LoadGameData()
{
    if (gameDataJson != null)
    {
        try
        {
            // Use Newtonsoft to support Dictionary fields, unlike JsonUtility
            // Ensure UTF-8 encoding for proper handling of Chinese characters
            gameData = JsonConvert.DeserializeObject<GameData>(gameDataJson.text);
            Debug.Log($"Loaded game data: {gameData.meta.title}");
            // Build node lookup immediately for TextAsset path to keep story tree correct
            CreateNodeLookup();
            // Initialize stats from loaded data
            InitializeStatsFromGameData();
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
            // Relative to StreamingAssets (platform-aware)
            finalUrl = BuildStreamingUrl($"{gameAssetFolder}/{input}");
        }
        else
        {
            // For remote without scheme, assume https (rare). Users should provide full URL.
            Debug.LogWarning($"Input '{input}' has no scheme; attempting to treat as StreamingAssets relative.");
            finalUrl = BuildStreamingUrl(input);
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
                // Get text with UTF-8 encoding to properly handle Chinese characters
                string jsonText = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                Debug.Log($"get game data from URL: {jsonText}");
                    // Parse the JSON data
                gameData = JsonConvert.DeserializeObject<GameData>(jsonText);
                 //   gameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);
                Debug.Log($"Successfully loaded game data from URL: {gameData.meta.title}");
                        // Create node lookup dictionary for fast access
                CreateNodeLookup();
                // Initialize stats after async load
                InitializeStatsFromGameData();
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

  // Build a platform-aware URL to access a file inside StreamingAssets.
  // - Android: Application.streamingAssetsPath is a jar URL already; return combined as-is.
  // - WebGL: Application.streamingAssetsPath is a URL path served by the build; return combined without file scheme.
  // - iOS/Desktop/Editor: requires file:/// prefix.
  private string BuildStreamingUrl(string relativePath)
  {
      string basePath = Application.streamingAssetsPath;
      string combined = string.IsNullOrEmpty(relativePath)
          ? basePath
          : Path.Combine(basePath, relativePath).Replace("\\", "/");

      bool hasScheme = combined.Contains("://");

#if UNITY_ANDROID && !UNITY_EDITOR
      // e.g. jar:file:///...!/assets/...
      return combined;
#elif UNITY_WEBGL && !UNITY_EDITOR
      // Served by web server; should not use file scheme
      return combined;
#else
      // iOS, Windows, macOS, Editor
      return hasScheme ? combined : ("file:///" + combined);
#endif
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
        Debug.Log("PlayNode:" + nodeId);
        if (!isGameActive)
        {
            Debug.Log("isGameActive false playnode fail");
            return;
        }
            
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
        // reset group QTE state
        StopAndClearQteGroupRoutines();
        qteGroupRemaining = 0;
        qteGroupActive = false;
        hadQteGroupForCurrentNode = false;
        qteGroupCompleted = false;
        _curScore = 0;
        Debug.Log($"Playing node: {nodeId} - {currentNode.question}");
        
        // Hide UI while video plays
        if (uiController != null)
            uiController.HideAllUI();
        // uiController.functionPanel.SetActive(true);
        // Play the video
        if (videoController != null && !string.IsNullOrEmpty(currentNode.video))
        {
            string videoUrl = GetVideoUrl(currentNode.video);
            if (uiController != null)
                uiController.ShowBlackMask();
            videoController.PlayVideo(videoUrl);
            // Defer QTE countdown until the video is confirmed started (OnVideoStarted)
            // Single QTE path (fallback)
            if (currentNode.qte != null && (currentNode.qteGroup == null || currentNode.qteGroup.Count == 0))
            {
                qtePendingForCurrentNode = true;
                qtePendingDelaySeconds = Mathf.Max(0f, currentNode.qte.startDelayFromStartSeconds);
            }
            // Grouped QTEs during video playback only
            //Debug.Log("currentNode.qteGroup.Count :" + currentNode.qteGroup.Count );
            if (currentNode.qteGroup != null && currentNode.qteGroup.Count > 0)
            {
                qteGroupActive = true;
                hadQteGroupForCurrentNode = true;
                qteGroupCompleted = false;
                qteGroupRemaining = currentNode.qteGroup.Count;
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
        //return "https://api.hisplayer.com/media/master.m3u8?contentKey=s7PwvPwJ";
        //return "https://636c-cloud1-7gwlsz5m226cfca9-1369289063.tcb.qcloud.la/Videos/0.m3u8";
        //  return BuildStreamingUrl($"Videos/0.m3u8");
        //return BuildStreamingUrl($"Videos/{videoFileName}");
        if(videoController.IsLocalVideo)
        {
            // Use platform-aware StreamingAssets URL
            return BuildStreamingUrl($"{gameAssetFolder}/Videos/{videoFileName}");
            //return BuildStreamingUrl($"Videos/{videoFileName.Split('.')[0]}_hls/master.m3u8");
        }
        else
        {
            
            return $"https://video-1318091918.cos.ap-beijing.myqcloud.com/{videoFileName.Split('.')[0]}.mp4";
            // return $"https://636c-cloud1-7gwlsz5m226cfca9-1369289063.tcb.qcloud.la/Videos/{videoFileName.Split('.')[0]}_hls/master.m3u8";
            //gameData.meta.cdnBase = "https://636c-cloud1-7gwlsz5m226cfca9-1369289063.tcb.qcloud.la/AIVideoGame/Videos/";
            string cdnWeb = $"{cdnBase}/Videos/";
            if (!string.IsNullOrEmpty(cdnWeb))
                return cdnWeb.EndsWith("/") ? (cdnWeb + videoFileName) : ($"{cdnWeb}/{videoFileName}");
            return null;
        }
    }
    public string GetThumbnailUrl(string thumbnailFileName)
    {
        //Debug.Log($"GetThumbnailUrl: {thumbnailFileName}");
        if(videoController.IsLocalVideo)  
        {
            Debug.Log("xxxx" + BuildStreamingUrl($"{gameAssetFolder}/Thumbnails/{thumbnailFileName}"));
            // Use platform-aware StreamingAssets URL
            return BuildStreamingUrl($"{gameAssetFolder}/Thumbnails/{thumbnailFileName}");
        }
        else
        {
            //gameData.meta.cdnBase = "https://636c-cloud1-7gwlsz5m226cfca9-1369289063.tcb.qcloud.la/AIVideoGame/Videos/";
            string cdnWeb = $"{cdnBase}/Thumbnails/";
            if (!string.IsNullOrEmpty(cdnWeb))
                return cdnWeb.EndsWith("/") ? (cdnWeb + thumbnailFileName) : ($"{cdnWeb}/{thumbnailFileName}");
            return null;
        }
    }
    void OnVideoStarted()
    {
        Debug.Log("Video started playing");
        isVideoPlaying = true;
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

        // Start grouped QTEs scheduling (each with its own delay) only while video is playing
        if (isGameActive && currentNode != null && qteGroupActive && currentNode.qteGroup != null && currentNode.qteGroup.Count > 0)
        {
            StopAndClearQteGroupRoutines();
            foreach (var q in currentNode.qteGroup)
            {
                // guard against nulls
                if (q == null) { qteGroupRemaining = Mathf.Max(0, qteGroupRemaining - 1); continue; }
                var co = StartCoroutine(ShowGroupedQTEAtDelay(q.startDelayFromStartSeconds, q));
                qteGroupRoutines.Add(co);
            }
        }
    }
    
    void OnVideoFinished()
    {
        Debug.Log("Video finished, showing interaction: " + currentNode.question);
        
        if (currentNode == null)
            return;
        isVideoPlaying = false;
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
        // For grouped QTEs: always settle at video end (regardless of whether last QTE finished earlier)
        if (hadQteGroupForCurrentNode)
        {
            qteGroupActive = false; // prevent any further group QTE from starting
            StopAndClearQteGroupRoutines();
            DecideNextNodeByQTE();
            hadQteGroupForCurrentNode = false;
            return;
        }
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
        else if (currentNode.qte != null && !qteShownForCurrentNode && (currentNode.qteGroup == null || currentNode.qteGroup.Count == 0))
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
        float target = Mathf.Max(0f, delay);
        // Wait until real playback time reaches target (pause/seek safe)
        while (true)
        {
            if (!isGameActive || currentNode == null || currentNode.qte == null || qteShownForCurrentNode)
                yield break;
            if (!isVideoPlaying)
            {
                yield return null; // wait resume
                continue;
            }
            float t = 0f;
            #if ADV_PLAYER
            if (videoController != null && videoController.videoManager != null)
            {
                t = (float)videoController.videoManager.videoPlayer.time;
            }
            #else
            if (videoController != null && videoController.hisPlayerController != null)
            {
                t = videoController.hisPlayerController.GetCurrentTimeSeconds();
            }
            #endif
            if (t >= target)
                break;
            yield return null;
        }
        // trigger
        qteShownForCurrentNode = true;
        ShowQTE();
    }

    // Trigger a specific QTE from the grouped list after a delay while the video is playing
    private IEnumerator ShowGroupedQTEAtDelay(float delay, QTEData qte)
    {
        float target = Mathf.Max(0f, delay);
        while (true)
        {
            // Ensure still valid state, same node, and video still playing (no QTE after end frame)
            if (!isGameActive || currentNode == null || qte == null || !qteGroupActive)
                yield break;
            if (!isVideoPlaying)
            {
                yield return null;
                continue;
            }
            float t = 0f;
            #if ADV_PLAYER
            if (videoController != null && videoController.videoManager != null)
            {
                t = (float)videoController.videoManager.videoPlayer.time;
            }
            #else
            if (videoController != null && videoController.hisPlayerController != null)
            {
                t = videoController.hisPlayerController.GetCurrentTimeSeconds();
            }
            #endif
            if (t >= target)
                break;
            yield return null;
        }
        if (uiController != null)
        {
            uiController.ShowQTE(qte, OnQTECompleted);
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
        // Apply choice stat effects before navigating
        if (currentNode != null && currentNode.choices != null)
        {
            var choice = currentNode.choices.FirstOrDefault(c => c != null && c.next == nextNodeId);
            if (choice != null && choice.statEffects != null && choice.statEffects.Count > 0)
            {
                string opKey = BuildChoiceOperationKey(currentNode, choice);
                ApplyStatEffects(choice.statEffects, source: $"Choice[{choice.label}]", operationKey: opKey);
            }
        }
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
        if (currentNode?.qte != null)
        {
            Debug.Log($"currentNode.qte.startDelayFromStartSeconds: {currentNode.qte.startDelayFromStartSeconds}");
        }
        if (qteGroupActive || hadQteGroupForCurrentNode)
        {
            qteGroupRemaining = Mathf.Max(0, qteGroupRemaining - 1);
            Debug.Log($"QTE group progress: remaining={qteGroupRemaining}, totalScore={_curScore}");
            if (qteGroupRemaining == 0)
            {
                qteGroupActive = false;
                qteGroupCompleted = true; // mark done; settle at video end
                StopAndClearQteGroupRoutines();
            }
            return;
        }
        else
        {
            DecideNextNodeByQTE();
        }


        
    }

    private void DecideNextNodeByQTE()
    {
        Debug.Log("DecideNextNodeByQTE");
        if (currentNode == null) return;

        string nextNodeId = null;
        Dictionary<int, string> map = null;
        // Priority:
        // 1) If it is a grouped QTE node and node-level qteNextNodeMap exists, use it
        // 2) Else, if single QTE exists, use its NextNodeMap
        // 3) Else, for legacy grouped QTE data, fallback to first group's NextNodeMap
        if (currentNode.qteGroup != null && currentNode.qteGroup.Count > 0 && currentNode.qteNextNodeMap != null && currentNode.qteNextNodeMap.Count > 0)
        {
            map = currentNode.qteNextNodeMap;
        }
        else if (currentNode.qte != null)
        {
            map = currentNode.qte.NextNodeMap;
        }
        else if (currentNode.qteGroup != null && currentNode.qteGroup.Count > 0)
        {
            var first = currentNode.qteGroup[0];
            if (first != null) map = first.NextNodeMap;
        }

        if (map != null && map.Count > 0)
        {
            // Prefer exact match
           
            // Apply QTE-based stat effects based on score, if defined
            var effects = ResolveQteScoreEffects(_curScore);
            if (effects != null && effects.Count > 0)
            {
                string opKey = BuildQteOperationKey(currentNode);
                ApplyStatEffects(effects, source: "QTE", operationKey: opKey);
            }
            if (!map.TryGetValue(_curScore, out nextNodeId) || string.IsNullOrEmpty(nextNodeId))
            {
                /*
                // Fallback: pick the highest key <= total score
                int bestKey = int.MinValue;
                foreach (var kv in map)
                {
                    if (kv.Key <= _curScore && kv.Key > bestKey)
                    {
                        bestKey = kv.Key;
                        nextNodeId = kv.Value;
                    }
                }
                // If still null, pick the smallest key as default
                if (string.IsNullOrEmpty(nextNodeId))
                {
                    int minKey = int.MaxValue;
                    foreach (var kv in map)
                    {
                        if (kv.Key < minKey)
                        {
                            minKey = kv.Key;
                            nextNodeId = kv.Value;
                        }
                    }
                }
                */
            }
            if (!string.IsNullOrEmpty(nextNodeId))
            {
                PlayNode(nextNodeId);
                uiController.HideAllCanvasGroup();
            }
            else
            {
                // No mapping for this score: default continue (do nothing)
            }
        }
    }

    private void StopAndClearQteGroupRoutines()
    {
        if (qteGroupRoutines != null)
        {
            foreach (var co in qteGroupRoutines)
            {
                if (co != null) StopCoroutine(co);
            }
            qteGroupRoutines.Clear();
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
    
    public int GetStatValue(string statName)
    {
        if (string.IsNullOrEmpty(statName) || currentStats == null)
            return 0;
        int value;
        if (currentStats.TryGetValue(statName, out value))
            return value;
        return 0;
    }

    // ===== Stats helpers =====
    private void InitializeStatsFromGameData()
    {
        // Prefer loading from saved stats; fall back to game data defaults
        string savedJson = PlayerPrefs.GetString(PlayerPrefsStatsKey, string.Empty);
        if (!string.IsNullOrEmpty(savedJson))
        {
            try
            {
                var saved = JsonConvert.DeserializeObject<Dictionary<string, int>>(savedJson);
                currentStats = saved ?? new Dictionary<string, int>();
                LogStats("Loaded stats from save");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse saved stats JSON: {e.Message}");
                if (gameData != null && gameData.stats != null)
                {
                    currentStats = new Dictionary<string, int>(gameData.stats);
                }
                else
                {
                    if (currentStats == null) currentStats = new Dictionary<string, int>();
                }
                LogStats("Initialized stats after failed load");
            }
        }
        else
        {
            if (gameData != null && gameData.stats != null)
            {
                currentStats = new Dictionary<string, int>(gameData.stats);
            }
            else
            {
                if (currentStats == null) currentStats = new Dictionary<string, int>();
            }
            LogStats("Initialized stats (no saved data)");
        }

        // Reset per-operation effects when (re)initializing stats
        if (operationStatEffects == null)
        {
            operationStatEffects = new Dictionary<string, Dictionary<string, int>>();
        }
        else
        {
            operationStatEffects.Clear();
        }
    }

    private void ApplyStatEffects(Dictionary<string, int> effects, string source, string operationKey = null)
    {
        if (effects == null || effects.Count == 0) return;
        if (currentStats == null) currentStats = new Dictionary<string, int>();
        if (operationStatEffects == null)
        {
            operationStatEffects = new Dictionary<string, Dictionary<string, int>>();
        }

        Dictionary<string, int> deltas = new Dictionary<string, int>();

        if (string.IsNullOrEmpty(operationKey))
        {
            // Fallback behavior for callers that do not distinguish per-operation effects
            foreach (var kv in effects)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                int baseVal = 0;
                currentStats.TryGetValue(kv.Key, out baseVal);
                long result = (long)baseVal + kv.Value; // avoid overflow
                int clamped = (int)Mathf.Clamp(result, int.MinValue, int.MaxValue);
                currentStats[kv.Key] = clamped;
                int delta = clamped - baseVal;
                if (delta != 0)
                {
                    deltas[kv.Key] = delta;
                }
            }
        }
        else
        {
            // Use per-operation storage: adjust currentStats by (newEffect - oldEffect)
            Dictionary<string, int> previousEffects;
            if (!operationStatEffects.TryGetValue(operationKey, out previousEffects) || previousEffects == null)
            {
                previousEffects = new Dictionary<string, int>();
            }

            // Build the union of all affected stat keys
            HashSet<string> allKeys = new HashSet<string>(previousEffects.Keys);
            foreach (var kv in effects)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                allKeys.Add(kv.Key);
            }

            foreach (var statKey in allKeys)
            {
                if (string.IsNullOrEmpty(statKey)) continue;
                int oldVal = 0;
                previousEffects.TryGetValue(statKey, out oldVal);
                int newVal = 0;
                effects.TryGetValue(statKey, out newVal);

                int baseVal = 0;
                currentStats.TryGetValue(statKey, out baseVal);

                long result = (long)baseVal + (long)newVal - (long)oldVal; // avoid overflow
                int clamped = (int)Mathf.Clamp(result, int.MinValue, int.MaxValue);
                currentStats[statKey] = clamped;

                int delta = clamped - baseVal;
                if (delta != 0)
                {
                    deltas[statKey] = delta;
                }
            }

            // Store a copy of the latest effects for this operation
            operationStatEffects[operationKey] = new Dictionary<string, int>(effects);
        }

        // Show toast for stats that should display changes
        if (uiController != null && deltas.Count > 0)
        {
            foreach (var kv in deltas)
            {
                if (ShouldShowStatChangeToast(kv.Key))
                {
                    uiController.ShowStatChangeToast(kv.Key, kv.Value);
                }
            }
        }

        // Persist updated stats
        SaveStats();
        LogStats($"Applied effects from {source}");
    }

    private bool ShouldShowStatChangeToast(string statName)
    {
        if (string.IsNullOrEmpty(statName)) return false;
        if (gameData == null || gameData.statShowToast == null) return true; // default: show
        bool show;
        if (gameData.statShowToast.TryGetValue(statName, out show))
        {
            return show;
        }
        return true;
    }

    private void SaveStats()
    {
        if (currentStats == null) return;
        try
        {
            string json = JsonConvert.SerializeObject(currentStats);
            PlayerPrefs.SetString(PlayerPrefsStatsKey, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save stats: {e.Message}");
        }
    }

    private string BuildChoiceOperationKey(GameNode node, Choice choice)
    {
        if (node == null || choice == null) return null;
        string nodeId = string.IsNullOrEmpty(node.id) ? "<nullNode>" : node.id;
        string label = string.IsNullOrEmpty(choice.label) ? "<noLabel>" : choice.label;
        string next = string.IsNullOrEmpty(choice.next) ? "<noNext>" : choice.next;
        return $"choice:{nodeId}:{label}:{next}";
    }

    private string BuildQteOperationKey(GameNode node)
    {
        if (node == null) return null;
        string nodeId = string.IsNullOrEmpty(node.id) ? "<nullNode>" : node.id;
        return $"qte:{nodeId}";
    }

    private Dictionary<string, int> ResolveQteScoreEffects(int score)
    {
        // Prefer single QTE's effects
        Dictionary<int, Dictionary<string, int>> map = null;
        if (currentNode != null)
        {
            if (currentNode.qte != null && currentNode.qte.scoreEffects != null && currentNode.qte.scoreEffects.Count > 0)
            {
                map = currentNode.qte.scoreEffects;
            }
            else if (currentNode.qteGroup != null)
            {
                foreach (var q in currentNode.qteGroup)
                {
                    if (q != null && q.scoreEffects != null && q.scoreEffects.Count > 0)
                    {
                        map = q.scoreEffects; break;
                    }
                }
            }
        }
        if (map == null || map.Count == 0) return null;
        // Exact match first
        if (map.TryGetValue(score, out var eff) && eff != null && eff.Count > 0) return eff;
        // Best <= threshold
        int bestKey = int.MinValue; Dictionary<string, int> best = null;
        foreach (var kv in map)
        {
            if (kv.Key <= score && kv.Key > bestKey && kv.Value != null)
            {
                bestKey = kv.Key; best = kv.Value;
            }
        }
        if (best != null) return best;
        // Fallback: smallest key
        int minKey = int.MaxValue; Dictionary<string, int> minEff = null;
        foreach (var kv in map)
        {
            if (kv.Key < minKey && kv.Value != null)
            {
                minKey = kv.Key; minEff = kv.Value;
            }
        }
        return minEff;
    }

    private void LogStats(string prefix)
    {
        if (currentStats == null) { Debug.Log($"{prefix}: <null>"); return; }
        string s = string.Join(", ", currentStats.Select(kv => kv.Key + "=" + kv.Value));
        Debug.Log($"{prefix}: {s}");
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
