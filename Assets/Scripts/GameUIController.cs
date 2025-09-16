using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameUIController : MonoBehaviour
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
    
    public GameObject functionPanel;


    private List<Button> currentChoiceButtons = new List<Button>();
    private System.Action<string> onChoiceSelected;
    private System.Action<bool> onQTECompleted;
    private CanvasGroup choicePanelCanvasGroup;
    private CanvasGroup qtePanelCanvasGroup;
    
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

        functionPanel.SetActive(false);
        
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
    
    public void ShowQTE(QTEData qteData, System.Action<bool> onComplete)
    {
        if (qtePanel == null)
        {
            Debug.LogError("QTE Panel not assigned!");
            return;
        }
        
        onQTECompleted = onComplete;
        
        // Setup QTE UI based on type
        switch (qteData.type.ToLower())
        {
            case "button":
                StartCoroutine(ButtonQTE(qteData));
                break;
            case "sequence":
                StartCoroutine(SequenceQTE(qteData));
                break;
            case "timing":
                StartCoroutine(TimingQTE(qteData));
                break;
            default:
                Debug.LogWarning($"Unknown QTE type: {qteData.type}");
                onQTECompleted?.Invoke(false);
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
        onQTECompleted?.Invoke(success);
    }
    
    private IEnumerator SequenceQTE(QTEData qteData)
    {
        StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));
        
        if (qteData.sequence == null || qteData.sequence.Count == 0)
        {
            onQTECompleted?.Invoke(false);
            yield break;
        }
        
        int currentIndex = 0;
        float timePerKey = qteData.duration / qteData.sequence.Count;
        bool success = true;
        
        while (currentIndex < qteData.sequence.Count && success)
        {
            string expectedKey = qteData.sequence[currentIndex];
            
            if (qteInstructionText != null)
                qteInstructionText.text = $"Press: {expectedKey}";
            
            if (qteKeyText != null)
                qteKeyText.text = expectedKey;
            
            float keyTimeRemaining = timePerKey;
            bool keyPressed = false;
            
            while (keyTimeRemaining > 0 && !keyPressed)
            {
                if (qteProgressBar != null)
                    qteProgressBar.value = 1f - (keyTimeRemaining / timePerKey);
                
                if (Input.inputString.ToUpper().Contains(expectedKey.ToUpper()))
                {
                    keyPressed = true;
                    currentIndex++;
                    break;
                }
                
                keyTimeRemaining -= Time.deltaTime;
                yield return null;
            }
            
            if (!keyPressed)
            {
                success = false;
            }
        }
        
        HideQTE();
        onQTECompleted?.Invoke(success);
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
        onQTECompleted?.Invoke(success);
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
}
