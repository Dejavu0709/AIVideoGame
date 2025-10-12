using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using NexgenDragon;
using Michsky.UI.Dark;

public class GameUIController : MonoSingleton<GameUIController>
{
    [Header("UI Elements")]
    public GameObject choicePanel;
    public TextMeshProUGUI questionText;
    public Transform choiceButtonContainer;
    public Button choiceButtonPrefab;

    [Header("QTE UI")]
    public GameObject qtePanel;
    public TextMeshProUGUI qteInstructionText;
    public Slider qteProgressBar;
    public TextMeshProUGUI qteKeyText;

    [Header("Animation")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;

    //public GameObject functionPanel;

    public StoryTreeView TreeView;

    public Button playButton;
    private List<Button> currentChoiceButtons = new List<Button>();
    private System.Action<string> onChoiceSelected;
    private System.Action<int> onQTECompleted;
    private CanvasGroup choicePanelCanvasGroup;
    private CanvasGroup qtePanelCanvasGroup;

    public CanvasGroup mainCanvasGroup;

    public ModalWindowManager MainView;

    void Start()
    {
        // Get or add canvas groups for fading
        if (choicePanel != null)
        {
            choicePanelCanvasGroup = choicePanel.GetComponent<CanvasGroup>();
            if (choicePanelCanvasGroup == null)
                choicePanelCanvasGroup = choicePanel.AddComponent<CanvasGroup>();
        }

        if (qtePanel != null)
        {
            qtePanelCanvasGroup = qtePanel.GetComponent<CanvasGroup>();
            if (qtePanelCanvasGroup == null)
                qtePanelCanvasGroup = qtePanel.AddComponent<CanvasGroup>();
        }

        HideAllUI();
    }

    public void ShowChoices(string question, List<Choice> choices, System.Action<string> onChoice)
    {
        if (choicePanel == null || questionText == null || choiceButtonContainer == null)
        {
            Debug.LogError("UI elements not properly assigned!");
            return;
        }

        onChoiceSelected = onChoice;

        // Clear existing buttons
        ClearChoiceButtons();

        // Set question text
        questionText.text = question;

        // Create choice buttons
        foreach (Choice choice in choices)
        {
            CreateChoiceButton(choice);
        }

        //functionPanel.SetActive(false);

        // Show the choice panel with fade in
        StartCoroutine(FadeInPanel(choicePanelCanvasGroup, choicePanel));
    }

    public void HideChoices()
    {
        if (choicePanel != null)
        {
            StartCoroutine(FadeOutPanel(choicePanelCanvasGroup, choicePanel));
        }
    }

    public void ShowQTE(QTEData qteData, System.Action<int> onComplete)
    {
        if (qtePanel == null)
        {
            Debug.LogError("QTE Panel not assigned!");
            return;
        }

        onQTECompleted = onComplete;
        Debug.Log($"Showing QTE: {qteData.type}, Target clicks: {qteData.param1}, Duration: {qteData.duration}, Delay: {qteData.startDelayFromStartSeconds}");
        // Setup QTE UI based on type
        switch (qteData.type.ToLower())
        {
            case "button":
                StartCoroutine(ButtonQTE(qteData));
                break;
            case "timing":
                StartCoroutine(TimingQTE(qteData));
                break;
            case "clicks":
                StartCoroutine(ClicksQTE(qteData));
                break;
            default:
                Debug.LogWarning($"Unknown QTE type: {qteData.type}");
                onQTECompleted?.Invoke(0);
                break;
        }
    }

    public void HideQTE()
    {
        if (qtePanel != null)
        {
            StartCoroutine(FadeOutPanel(qtePanelCanvasGroup, qtePanel));
        }
    }

    public void HideAllUI()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        if (qtePanel != null) qtePanel.SetActive(false);
    }

    private void CreateChoiceButton(Choice choice)
    {
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("Choice button prefab not assigned!");
            return;
        }

        Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            buttonText.text = choice.label;
        }

        button.onClick.AddListener(() => OnChoiceButtonClicked(choice.next));
        currentChoiceButtons.Add(button);
    }

    private void ClearChoiceButtons()
    {
        foreach (Button button in currentChoiceButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        currentChoiceButtons.Clear();
    }

    private void OnChoiceButtonClicked(string nextNodeId)
    {
        HideChoices();
        onChoiceSelected?.Invoke(nextNodeId);
    }

    private IEnumerator ButtonQTE(QTEData qteData)
    {
        StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));

        if (qteInstructionText != null)
            qteInstructionText.text = "Press SPACE when the bar is in the green zone!";

        float timeRemaining = qteData.duration;
        bool success = false;

        while (timeRemaining > 0 && !success)
        {
            // Update progress bar
            if (qteProgressBar != null)
                qteProgressBar.value = 1f - (timeRemaining / qteData.duration);

            // Check for input in the "green zone" (last 20% of the duration)
            if (timeRemaining <= qteData.duration * 0.2f && Input.GetKeyDown(KeyCode.Space))
            {
                success = true;
                break;
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        HideQTE();
        onQTECompleted?.Invoke(1);
    }

    // New QTE: Count clicks within duration (after optional start delay)
    private IEnumerator ClicksQTE(QTEData qteData)
    {
        // Parse optional target clicks from param1 for UI hint only
        int targetClicks = 0;
        int.TryParse(qteData.param1, out targetClicks);
        float duration = Mathf.Max(0f, qteData.duration);
        float delay = Mathf.Max(0f, qteData.startDelayFromStartSeconds);
        Debug.Log($"QTE: {qteData.type}, Target clicks: {targetClicks}, Duration: {duration}, Delay: {delay}");
        if (qteInstructionText != null)
        {
            if (targetClicks > 0)
                qteInstructionText.text = $"在{duration:0.#}秒内点击{targetClicks}次";
            else
                qteInstructionText.text = $"在{duration:0.#}秒内尽可能多点击";
        }

        // Optional delay before QTE becomes active (show panel after delay)
        float wait = delay;
        while (wait > 0f)
        {
            // You may show a pre-countdown here if desired
            wait -= Time.deltaTime;
            yield return null;
        }

        // Now show the panel right when the QTE starts
        StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));

        float timeRemaining = duration;
        int clicks = 0;

        while (timeRemaining > 0f)
        {
            // Update progress bar
            if (qteProgressBar != null && duration > 0f)
            {
                qteProgressBar.value = 1f - (timeRemaining / duration);
            }

            // Count clicks: mouse/touch/space
            bool clicked = false;
            if (Input.GetMouseButtonDown(0)) clicked = true;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.Space)) clicked = true;
#endif
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        clicked = true;
                        break;
                    }
                }
            }
            if (clicked)
            {
                clicks++;
                if (qteKeyText != null)
                {
                    if (targetClicks > 0)
                        qteKeyText.text = $"{clicks}/{targetClicks}";
                    else
                        qteKeyText.text = clicks.ToString();
                }
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        HideQTE();
        // Return total clicks; Branching manager can map this via NextNodeMap
        onQTECompleted?.Invoke(clicks);
    }


    private IEnumerator TimingQTE(QTEData qteData)
    {
        StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));

        if (qteInstructionText != null)
            qteInstructionText.text = "Press SPACE at the right moment!";

        float timeRemaining = qteData.duration;
        bool inputReceived = false;
        float inputTime = 0f;

        while (timeRemaining > 0)
        {
            if (qteProgressBar != null)
                qteProgressBar.value = 1f - (timeRemaining / qteData.duration);

            if (!inputReceived && Input.GetKeyDown(KeyCode.Space))
            {
                inputReceived = true;
                inputTime = qteData.duration - timeRemaining;
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Success if input was received in the middle 40% of the duration
        bool success = inputReceived &&
                      inputTime >= qteData.duration * 0.3f &&
                      inputTime <= qteData.duration * 0.7f;

        HideQTE();
        onQTECompleted?.Invoke(1);
    }

    private IEnumerator FadeInPanel(CanvasGroup canvasGroup, GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float elapsedTime = 0f;

                while (elapsedTime < fadeInDuration)
                {
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                canvasGroup.alpha = 1f;
            }
        }
    }

    private IEnumerator FadeOutPanel(CanvasGroup canvasGroup, GameObject panel)
    {
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < fadeOutDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void HideAllCanvasGroup()
    {
        Debug.Log("Hiding all canvas groups");
        mainCanvasGroup.alpha = 0f;
    }

    public void ShowAllCanvasGroup()
    {
        StartCoroutine(FadeInCanvas(mainCanvasGroup));
    }


    private IEnumerator FadeInCanvas(CanvasGroup canvasGroup)
    {
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            float startAlpha = 0;

            while (elapsedTime < 0.5f)
            {
                //canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / 2);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Debug.Log("Fade in completed");
            canvasGroup.alpha = 1f;
        }
    }

    public void ShowTreeView()
    {
        VideoPlayerController.Instance.PauseVideo();
        if (TreeView == null)
        {
            Debug.LogError("TreeView is not assigned!");
            return;
        }
        TreeView.gameObject.SetActive(true);
        TreeView.Show();
    }
    public void HideTreeView()
    {
        TreeView.gameObject.SetActive(false);
    }

    public void BackToMainMenu()
    {
        HideTreeView();
        MainView.gameObject.SetActive(false);
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("OnPlayButtonClicked");
        MainView.gameObject.SetActive(true);
        BranchingVideoGameManager.Instance.StartGame();
    }
}
