using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ClickQTE : BaseQTE
{
    public TextMeshProUGUI qteInstructionText;
    public Image qteProgressBar;
    public Image finishedProgress;
    public TextMeshProUGUI qteKeyText;
    public GameObject qtePanel;
    public CanvasGroup qtePanelCanvasGroup;
    public float fadeInDuration = 0.25f;
    public RawImage VideoImage;
    public RectTransform Content;
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
        ResetResultIndicators();
        finishedProgress.fillAmount = 0;
        this.gameObject.SetActive(true);
        StartCoroutine(ClicksQTE(qteData, onComplete));
    }

      // New QTE: Count clicks within duration (after optional start delay)
    private IEnumerator ClicksQTE(QTEData qteData, System.Action<int> onComplete)
    {
        // Parse optional target clicks from param1 for UI hint only
        int targetClicks = 0;
        //finishedProgress.color = Color.blue;
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

        // Position Content relative to VideoImage if provided
        Vector2 regionCenter01 = new Vector2(0.5f, 0.5f);
        Vector2 regionSize01 = new Vector2(0.2f, 0.2f);
        if (qteData != null && !string.IsNullOrEmpty(qteData.position))
        {
            var parts = qteData.position.Split(',');
            if (parts.Length >= 2)
            {
                float cx, cy;
                if (float.TryParse(parts[0], out cx)) regionCenter01.x = Mathf.Clamp01(cx);
                if (float.TryParse(parts[1], out cy)) regionCenter01.y = Mathf.Clamp01(cy);
                Debug.Log($"QTE: {qteData.type}, Target clicks: {targetClicks}, Duration: {duration}, Delay: {delay}, Position: {regionCenter01}");
            }
        }
        if (qteData != null && !string.IsNullOrEmpty(qteData.param3))
        {
            var parts = qteData.param3.Split(',');
            if (parts.Length >= 2)
            {
                float w, h;
                if (float.TryParse(parts[0], out w)) regionSize01.x = Mathf.Clamp01(Mathf.Abs(w));
                if (float.TryParse(parts[1], out h)) regionSize01.y = Mathf.Clamp01(Mathf.Abs(h));
                Debug.Log($"QTE: {qteData.type}, Target clicks: {targetClicks}, Duration: {duration}, Delay: {delay}, Position: {regionCenter01}, Size: {regionSize01}");
            }
        }
        if (Content != null)
        {
            var parentRect = Content.transform.parent as RectTransform;
            if (VideoImage != null && VideoImage.rectTransform != null && parentRect != null)
            {
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parentRect, VideoImage.rectTransform);
                var videoSize = (Vector2)bounds.size;
                var videoCenter = (Vector2)bounds.center;
                Vector2 sizePixels = new Vector2(regionSize01.x * videoSize.x, regionSize01.y * videoSize.y);
                Vector2 halfSize = sizePixels * 0.5f;
                Vector2 targetPos = videoCenter + new Vector2((regionCenter01.x - 0.5f) * videoSize.x, (regionCenter01.y - 0.5f) * videoSize.y);
                Vector2 minPos = videoCenter - (videoSize * 0.5f) + halfSize;
                Vector2 maxPos = videoCenter + (videoSize * 0.5f) - halfSize;
                targetPos.x = Mathf.Clamp(targetPos.x, minPos.x, maxPos.x);
                targetPos.y = Mathf.Clamp(targetPos.y, minPos.y, maxPos.y);
                Content.anchoredPosition = targetPos;
                Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizePixels.x);
                Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizePixels.y);
            }
            else
            {
                var canvasRect = Content.GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    var canvasSize = canvasRect.rect.size;
                    Vector2 localCenter = new Vector2((regionCenter01.x - 0.5f) * canvasSize.x, (regionCenter01.y - 0.5f) * canvasSize.y);
                    Vector2 sizePixels = new Vector2(regionSize01.x * canvasSize.x, regionSize01.y * canvasSize.y);
                    Vector2 halfSize = sizePixels * 0.5f;
                    Vector2 minPos = new Vector2(-canvasSize.x * 0.5f + halfSize.x, -canvasSize.y * 0.5f + halfSize.y);
                    Vector2 maxPos = new Vector2( canvasSize.x * 0.5f - halfSize.x,  canvasSize.y * 0.5f - halfSize.y);
                    Vector2 clamped = new Vector2(
                        Mathf.Clamp(localCenter.x, minPos.x, maxPos.x),
                        Mathf.Clamp(localCenter.y, minPos.y, maxPos.y)
                    );
                    Content.anchoredPosition = clamped;
                    Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizePixels.x);
                    Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizePixels.y);
                }
            }
        }

        if (qtePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));
        }

        float timeRemaining = duration;
        int clicks = 0;
        qteProgressBar.gameObject.SetActive(duration > 0);
        while (timeRemaining > 0f)
        {
            // Update progress bar
            if (qteProgressBar != null && duration > 0f)
            {
                qteProgressBar.fillAmount = (timeRemaining / duration);
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
                if(targetClicks > 0 && clicks >= targetClicks)
                {
                    finishedProgress.color = Color.green;
                    CompleteQTE(1, onComplete, HideQTE);
                    yield break;
                }
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        CompleteQTE(0, onComplete, HideQTE);
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
