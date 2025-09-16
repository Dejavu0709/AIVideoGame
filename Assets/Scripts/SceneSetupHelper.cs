using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RenderHeads.Media.AVProVideo;

public class SceneSetupHelper : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    
    [Header("Prefab References")]
    public Button choiceButtonPrefab;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("Setting up branching video game scene...");
        
        // Create main game manager
        GameObject gameManager = CreateGameManager();
        
        // Create video player setup
        GameObject videoPlayer = CreateVideoPlayer();
        
        // Create UI setup
        GameObject uiCanvas = CreateUICanvas();
        
        // Connect components
        ConnectComponents(gameManager, videoPlayer, uiCanvas);
        
        Debug.Log("Scene setup completed!");
    }
    
    GameObject CreateGameManager()
    {
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj == null)
        {
            gameManagerObj = new GameObject("GameManager");
        }
        
        if (gameManagerObj.GetComponent<BranchingVideoGameManager>() == null)
        {
            gameManagerObj.AddComponent<BranchingVideoGameManager>();
        }
        
        if (gameManagerObj.GetComponent<GameDataLoader>() == null)
        {
            gameManagerObj.AddComponent<GameDataLoader>();
        }
        
        return gameManagerObj;
    }
    
    GameObject CreateVideoPlayer()
    {
        GameObject videoPlayerObj = GameObject.Find("VideoPlayer");
        if (videoPlayerObj == null)
        {
            videoPlayerObj = new GameObject("VideoPlayer");
        }
        
        // Add MediaPlayer component
        MediaPlayer mediaPlayer = videoPlayerObj.GetComponent<MediaPlayer>();
        if (mediaPlayer == null)
        {
            mediaPlayer = videoPlayerObj.AddComponent<MediaPlayer>();
        }
        
        // Add VideoPlayerController
        VideoPlayerController controller = videoPlayerObj.GetComponent<VideoPlayerController>();
        if (controller == null)
        {
            controller = videoPlayerObj.AddComponent<VideoPlayerController>();
            controller.mediaPlayer = mediaPlayer;
        }
        
        // Create video display
        GameObject videoDisplay = CreateVideoDisplay(videoPlayerObj);
        
        return videoPlayerObj;
    }
    
    GameObject CreateVideoDisplay(GameObject parent)
    {
        GameObject displayObj = GameObject.Find("VideoDisplay");
        if (displayObj == null)
        {
            displayObj = new GameObject("VideoDisplay");
            displayObj.transform.SetParent(parent.transform);
        }
        
        // Add RawImage for video display
        RawImage rawImage = displayObj.GetComponent<RawImage>();
        if (rawImage == null)
        {
            rawImage = displayObj.AddComponent<RawImage>();
        }
        
        // Add DisplayUGUI component
        DisplayUGUI displayUGUI = displayObj.GetComponent<DisplayUGUI>();
        if (displayUGUI == null)
        {
            displayUGUI = displayObj.AddComponent<DisplayUGUI>();
           // displayUGUI._targetTexture = rawImage;
        }
        
        // Set RectTransform to fill screen
        RectTransform rectTransform = displayObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = displayObj.AddComponent<RectTransform>();
        }
        
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        return displayObj;
    }
    
    GameObject CreateUICanvas()
    {
        GameObject canvasObj = GameObject.Find("UICanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("UICanvas");
        }
        
        // Add Canvas component
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // Above video
        }
        
        // Add CanvasScaler
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        // Add GraphicRaycaster
        if (canvasObj.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create UI panels
        CreateChoicePanel(canvasObj);
        CreateQTEPanel(canvasObj);
        
        // Add GameUIController
        GameUIController uiController = canvasObj.GetComponent<GameUIController>();
        if (uiController == null)
        {
            uiController = canvasObj.AddComponent<GameUIController>();
        }
        
        return canvasObj;
    }
    
    void CreateChoicePanel(GameObject canvas)
    {
        GameObject choicePanelObj = canvas.transform.Find("ChoicePanel")?.gameObject;
        if (choicePanelObj == null)
        {
            choicePanelObj = new GameObject("ChoicePanel");
            choicePanelObj.transform.SetParent(canvas.transform);
        }
        
        // Add RectTransform and position at bottom
        RectTransform choiceRect = choicePanelObj.GetComponent<RectTransform>();
        if (choiceRect == null)
        {
            choiceRect = choicePanelObj.AddComponent<RectTransform>();
        }
        
        choiceRect.anchorMin = new Vector2(0, 0);
        choiceRect.anchorMax = new Vector2(1, 0.4f);
        choiceRect.offsetMin = Vector2.zero;
        choiceRect.offsetMax = Vector2.zero;
        
        // Add background
        Image bgImage = choicePanelObj.GetComponent<Image>();
        if (bgImage == null)
        {
            bgImage = choicePanelObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);
        }
        
        // Create question text
        CreateQuestionText(choicePanelObj);
        
        // Create choice button container
        CreateChoiceButtonContainer(choicePanelObj);
    }
    
    void CreateQuestionText(GameObject parent)
    {
        GameObject questionObj = parent.transform.Find("QuestionText")?.gameObject;
        if (questionObj == null)
        {
            questionObj = new GameObject("QuestionText");
            questionObj.transform.SetParent(parent.transform);
        }
        
        TextMeshProUGUI questionText = questionObj.GetComponent<TextMeshProUGUI>();
        if (questionText == null)
        {
            questionText = questionObj.AddComponent<TextMeshProUGUI>();
            questionText.text = "Question will appear here";
            questionText.fontSize = 36;
            questionText.color = Color.white;
            questionText.alignment = TextAlignmentOptions.Center;
        }
        
        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0.1f, 0.6f);
        questionRect.anchorMax = new Vector2(0.9f, 0.9f);
        questionRect.offsetMin = Vector2.zero;
        questionRect.offsetMax = Vector2.zero;
    }
    
    void CreateChoiceButtonContainer(GameObject parent)
    {
        GameObject containerObj = parent.transform.Find("ChoiceButtonContainer")?.gameObject;
        if (containerObj == null)
        {
            containerObj = new GameObject("ChoiceButtonContainer");
            containerObj.transform.SetParent(parent.transform);
        }
        
        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        if (containerRect == null)
        {
            containerRect = containerObj.AddComponent<RectTransform>();
        }
        
        containerRect.anchorMin = new Vector2(0.1f, 0.1f);
        containerRect.anchorMax = new Vector2(0.9f, 0.5f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        // Add VerticalLayoutGroup
        VerticalLayoutGroup layoutGroup = containerObj.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = containerObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
        }
        
        // Create button prefab if not assigned
        if (choiceButtonPrefab == null)
        {
            CreateChoiceButtonPrefab(containerObj);
        }
    }
    
    void CreateChoiceButtonPrefab(GameObject container)
    {
        GameObject buttonObj = new GameObject("ChoiceButtonPrefab");
        buttonObj.transform.SetParent(container.transform);
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        
        // Add Image for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);
        
        // Add RectTransform
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(400, 60);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Choice Text";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Set as prefab reference
        choiceButtonPrefab = button;
        
        // Disable the prefab object
        buttonObj.SetActive(false);
    }
    
    void CreateQTEPanel(GameObject canvas)
    {
        GameObject qtePanelObj = canvas.transform.Find("QTEPanel")?.gameObject;
        if (qtePanelObj == null)
        {
            qtePanelObj = new GameObject("QTEPanel");
            qtePanelObj.transform.SetParent(canvas.transform);
        }
        
        RectTransform qteRect = qtePanelObj.GetComponent<RectTransform>();
        if (qteRect == null)
        {
            qteRect = qtePanelObj.AddComponent<RectTransform>();
        }
        
        qteRect.anchorMin = new Vector2(0.2f, 0.3f);
        qteRect.anchorMax = new Vector2(0.8f, 0.7f);
        qteRect.offsetMin = Vector2.zero;
        qteRect.offsetMax = Vector2.zero;
        
        // Add background
        Image qteBg = qtePanelObj.GetComponent<Image>();
        if (qteBg == null)
        {
            qteBg = qtePanelObj.AddComponent<Image>();
            qteBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        }
        
        // Create QTE elements
        CreateQTEElements(qtePanelObj);
    }
    
    void CreateQTEElements(GameObject parent)
    {
        // QTE Instruction Text
        GameObject instructionObj = new GameObject("QTEInstructionText");
        instructionObj.transform.SetParent(parent.transform);
        
        TextMeshProUGUI instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
        instructionText.text = "QTE Instructions";
        instructionText.fontSize = 32;
        instructionText.color = Color.white;
        instructionText.alignment = TextAlignmentOptions.Center;
        
        RectTransform instructionRect = instructionObj.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.1f, 0.7f);
        instructionRect.anchorMax = new Vector2(0.9f, 0.9f);
        instructionRect.offsetMin = Vector2.zero;
        instructionRect.offsetMax = Vector2.zero;
        
        // QTE Progress Bar
        GameObject progressObj = new GameObject("QTEProgressBar");
        progressObj.transform.SetParent(parent.transform);
        
        Slider progressBar = progressObj.AddComponent<Slider>();
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        
        RectTransform progressRect = progressObj.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.1f, 0.4f);
        progressRect.anchorMax = new Vector2(0.9f, 0.6f);
        progressRect.offsetMin = Vector2.zero;
        progressRect.offsetMax = Vector2.zero;
        
        // QTE Key Text
        GameObject keyTextObj = new GameObject("QTEKeyText");
        keyTextObj.transform.SetParent(parent.transform);
        
        TextMeshProUGUI keyText = keyTextObj.AddComponent<TextMeshProUGUI>();
        keyText.text = "KEY";
        keyText.fontSize = 48;
        keyText.color = Color.yellow;
        keyText.alignment = TextAlignmentOptions.Center;
        
        RectTransform keyTextRect = keyTextObj.GetComponent<RectTransform>();
        keyTextRect.anchorMin = new Vector2(0.1f, 0.1f);
        keyTextRect.anchorMax = new Vector2(0.9f, 0.3f);
        keyTextRect.offsetMin = Vector2.zero;
        keyTextRect.offsetMax = Vector2.zero;
    }
    
    void ConnectComponents(GameObject gameManager, GameObject videoPlayer, GameObject uiCanvas)
    {
        BranchingVideoGameManager gameManagerScript = gameManager.GetComponent<BranchingVideoGameManager>();
        VideoPlayerController videoController = videoPlayer.GetComponent<VideoPlayerController>();
        GameUIController uiController = uiCanvas.GetComponent<GameUIController>();
        
        // Connect video controller
        if (gameManagerScript != null && videoController != null)
        {
            gameManagerScript.videoController = videoController;
        }
        
        // Connect UI controller
        if (gameManagerScript != null && uiController != null)
        {
            gameManagerScript.uiController = uiController;
        }
        
        // Connect UI references
        if (uiController != null)
        {
            uiController.choicePanel = uiCanvas.transform.Find("ChoicePanel")?.gameObject;
            uiController.questionText = uiController.choicePanel?.transform.Find("QuestionText")?.GetComponent<TextMeshProUGUI>();
            uiController.choiceButtonContainer = uiController.choicePanel?.transform.Find("ChoiceButtonContainer");
            uiController.choiceButtonPrefab = choiceButtonPrefab;
            
            uiController.qtePanel = uiCanvas.transform.Find("QTEPanel")?.gameObject;
            uiController.qteInstructionText = uiController.qtePanel?.transform.Find("QTEInstructionText")?.GetComponent<TextMeshProUGUI>();
            uiController.qteProgressBar = uiController.qtePanel?.transform.Find("QTEProgressBar")?.GetComponent<Slider>();
            uiController.qteKeyText = uiController.qtePanel?.transform.Find("QTEKeyText")?.GetComponent<TextMeshProUGUI>();
        }
        
        // Connect MediaPlayer to DisplayUGUI
        MediaPlayer mediaPlayer = videoPlayer.GetComponent<MediaPlayer>();
        DisplayUGUI displayUGUI = videoPlayer.GetComponentInChildren<DisplayUGUI>();
        
        if (mediaPlayer != null && displayUGUI != null && videoController != null)
        {
            videoController.mediaPlayer = mediaPlayer;
            videoController.displayUGUI = displayUGUI;
            //displayUGUI._mediaPlayer = mediaPlayer;
        }
    }
}
