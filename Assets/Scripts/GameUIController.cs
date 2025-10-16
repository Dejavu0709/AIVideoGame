using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using NexgenDragon;
using Michsky.UI.Dark;
using QTESystem;
using UnityEditor.Experimental.GraphView;
public class GameUIController : MonoSingleton<GameUIController>
{
    [Header("UI Elements")]
    public GameObject choicePanel;
    public TextMeshProUGUI questionText;
    public Transform choiceButtonContainer;
    public Button choiceButtonPrefab;

    [Header("QTE UI")]
    public GameObject qtePanel;
    //public ClickQTE clickQTE;
    public MashingQTE mashingQTE;
    public ShootQTE shootQTE;

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
    public ModalWindowManager DeadView;
    [Header("Death View")]
    public TextMeshProUGUI deadDescriptionText; // Assign to DeadView's description label

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
            //case "button":
                //StartCoroutine(ButtonQTE(qteData));
                //break;
            //case "timing":
            //    StartCoroutine(TimingQTE(qteData));
            //    break;
            case "clicks":
            /*
            qtePanel.gameObject.SetActive(true);
            mashingQTE.gameObject.SetActive(true);
            mashingQTE.timesToHit = int.Parse(qteData.param1);
            mashingQTE.time = qteData.duration;
            mashingQTE.timesToHit = 20;
            mashingQTE.time = 5;
            mashingQTE.BeginQTE();
            //clickQTE.ShowQTE(qteData, onQTECompleted);
                break;
            case "shoot":
            */
                if (shootQTE != null)
                {
                    qtePanel.gameObject.SetActive(true);
                    shootQTE.ShowQTE(qteData, onQTECompleted);
                }
                else
                {
                    Debug.LogWarning("ShootQTE is not assigned on GameUIController");
                    onQTECompleted?.Invoke(0);
                }
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
/*
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
*/
/*

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
*/
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
        DeadView.ModalWindowOut();
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

    // Death flow
    public void ShowDeath(GameNode currentNode)
    {
        Debug.Log("ShowDeath");
        // Pause any playing video and hide interactive UI
        VideoPlayerController.Instance.PauseVideo();
        HideChoices();
        HideQTE();
        // Show death modal/page
        if (DeadView != null)
        {
            DeadView.gameObject.SetActive(true);
            DeadView.ModalWindowIn();
            
        }
        // Set death description if available
        if (DeadView.description != null)
        {
            DeadView.description = (currentNode != null && !string.IsNullOrEmpty(currentNode.deathdesc))
                ? currentNode.deathdesc
                : string.Empty;
        }
    }

    // Hook this to a close button on DeadView
    public void CloseDeathAndShowTree()
    {
        Debug.Log("CloseDeathAndShowTree");
        if (DeadView != null)
        {
            DeadView.gameObject.SetActive(false);
        }
        // Open the story tree to review progress
        ShowTreeView();
    }
}
