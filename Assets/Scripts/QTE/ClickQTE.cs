using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ClickQTE : MonoBehaviour
{
    public TextMeshProUGUI qteInstructionText;
    public Image qteProgressBar;
    public Image finishedProgress;
    public TextMeshProUGUI qteKeyText;
    public GameObject qtePanel;
    public CanvasGroup qtePanelCanvasGroup;
    public float fadeInDuration = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowQTE(QTEData qteData, System.Action<int> onComplete)
    {
        if (qtePanel != null)
        {
            qtePanel.SetActive(true);
        }
        this.gameObject.SetActive(true);
        StartCoroutine(ClicksQTE(qteData, onComplete));
    }

      // New QTE: Count clicks within duration (after optional start delay)
    private IEnumerator ClicksQTE(QTEData qteData, System.Action<int> onComplete)
    {
        // Parse optional target clicks from param1 for UI hint only
        int targetClicks = 0;
        finishedProgress.color = Color.blue;
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

    
        if (qtePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));
        }

        float timeRemaining = duration;
        int clicks = 0;

        while (timeRemaining > 0f)
        {
            // Update progress bar
            if (qteProgressBar != null && duration > 0f)
            {
                qteProgressBar.fillAmount = 1f - (timeRemaining / duration);
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

                finishedProgress.fillAmount = (float)clicks / targetClicks;
                if (qteKeyText != null)
                {
                    if (targetClicks > 0)
                        qteKeyText.text = $"{clicks}/{targetClicks}";
                    else
                        qteKeyText.text = clicks.ToString();
                }
                if(clicks >= targetClicks)
                {
                    finishedProgress.color = Color.green;
                    onComplete?.Invoke(1);
                    HideQTE();
                    yield return null;
                }
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        HideQTE();
        // Return total clicks; Branching manager can map this via NextNodeMap
        onComplete?.Invoke(clicks);
    }

    private IEnumerator FadeInPanel(CanvasGroup canvasGroup, GameObject panel)
    {
        if (panel != null)
            panel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        else
        {
            yield return null;
        }
    }

    public void HideQTE()
    {
        this.gameObject.SetActive(false);
        if (qtePanel != null)
            qtePanel.SetActive(false);
        if (qtePanelCanvasGroup != null)
            qtePanelCanvasGroup.alpha = 0f;
    }

}
