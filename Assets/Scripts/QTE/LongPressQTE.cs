using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LongPressQTE : BaseQTE
{
    public TextMeshProUGUI qteInstructionText;
    public Image qteProgressBar;
    public Image finishProgressBar;
    public TextMeshProUGUI qteKeyText;
    public GameObject qtePanel;
    public CanvasGroup qtePanelCanvasGroup;
    public float fadeInDuration = 0.25f;
    public RawImage VideoImage;
    //public RectTransform pressPrompt;
    public RectTransform Content;

    private Coroutine routine;

    public void ShowQTE(QTEData qteData, System.Action<int> onComplete)
    {
        if (qtePanel != null)
        {
            qtePanel.SetActive(true);
        }
        ResetResultIndicators();
        gameObject.SetActive(true);
        routine = StartCoroutine(Run(qteData, onComplete));
    }

    private IEnumerator Run(QTEData qteData, System.Action<int> onComplete)
    {
        float window = Mathf.Max(0f, qteData.duration);
        float requiredHold = 0f;
        float.TryParse(qteData.param1, NumberStyles.Float, CultureInfo.InvariantCulture, out requiredHold);
        requiredHold = Mathf.Max(0.05f, requiredHold);

        // Region parameters: position = "x,y" normalized [0..1] for center (preferred), param2 fallback; param3 = "w,h" normalized [0..1] for size
        Vector2 regionCenter01 = new Vector2(0.5f, 0.5f);
        Vector2 regionSize01 = new Vector2(0.2f, 0.2f);
        if (!string.IsNullOrEmpty(qteData.position))
        {
            var parts = qteData.position.Split(',');
            if (parts.Length >= 2)
            {
                float cx, cy;
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out cx)) regionCenter01.x = Mathf.Clamp01(cx);
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out cy)) regionCenter01.y = Mathf.Clamp01(cy);
            }
        }
        else if (!string.IsNullOrEmpty(qteData.param2))
        {
            var parts = qteData.param2.Split(',');
            if (parts.Length >= 2)
            {
                float cx, cy;
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out cx)) regionCenter01.x = Mathf.Clamp01(cx);
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out cy)) regionCenter01.y = Mathf.Clamp01(cy);
            }
        }
        if (!string.IsNullOrEmpty(qteData.param3))
        {
            var parts = qteData.param3.Split(',');
            if (parts.Length >= 2)
            {
                float w, h;
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out w)) regionSize01.x = Mathf.Clamp01(Mathf.Abs(w));
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out h)) regionSize01.y = Mathf.Clamp01(Mathf.Abs(h));
            }
        }

        // Position the Content relative to VideoImage if available; otherwise fallback to canvas
        RectTransform canvasRect = null;
        if (Content != null)
        {
            var parentRect = Content.transform.parent as RectTransform;
            if (VideoImage != null && VideoImage.rectTransform != null && parentRect != null)
            {
                var videoRect = VideoImage.rectTransform;
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parentRect, videoRect);
                var videoSize = (Vector2)bounds.size;
                var videoCenter = (Vector2)bounds.center;
                Vector2 sizePixels = new Vector2(regionSize01.x * videoSize.x, regionSize01.y * videoSize.y);
                Vector2 halfSize = sizePixels * 0.5f;
                Vector2 targetPos = videoCenter + new Vector2((regionCenter01.x - 0.5f) * videoSize.x, (regionCenter01.y - 0.5f) * videoSize.y);
                // Clamp inside video bounds
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
                if (canvasRect == null && Content != null) canvasRect = Content.GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    var canvasSize = canvasRect.rect.size;
                    Vector2 localCenter = new Vector2((regionCenter01.x - 0.5f) * canvasSize.x, (regionCenter01.y - 0.5f) * canvasSize.y);
                    Vector2 sizePixels = new Vector2(regionSize01.x * canvasSize.x, regionSize01.y * canvasSize.y);
                    Vector2 halfSize = sizePixels * 0.5f;
                    // Canvas local space centered at (0,0); clamp within canvas rect
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

        if (qteInstructionText != null)
        {
            qteInstructionText.text = $"长按{requiredHold:0.#}秒";
        }

        if (qtePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));
        }

        float timeRemaining = window > 0 ? window : float.PositiveInfinity;
        bool pointerDown = false;
        float holdTime = 0f;
        // Initialize progress bars
        if (qteProgressBar != null)
        {
            qteProgressBar.gameObject.SetActive(window > 0f);
            if (window > 0f)
            {
                qteProgressBar.fillAmount = 1f; // countdown starts full
            }
        }
        if (finishProgressBar != null)
        {
            finishProgressBar.fillAmount = 0f; // hold progress starts empty
        }
        System.Func<Vector2, bool> isInsideRegion = (Vector2 screenPoint) =>
        {
            // Prefer testing against VideoImage bounds if available
            RectTransform parentRect = Content != null ? Content.transform.parent as RectTransform : null;
            if (VideoImage != null && VideoImage.rectTransform != null && parentRect != null)
            {
                // Determine camera for proper screen-to-local conversion
                Camera uiCam = null;
                var parentCanvas = parentRect.GetComponentInParent<Canvas>();
                if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay) uiCam = parentCanvas.worldCamera;

                Vector2 localPoint;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, uiCam, out localPoint))
                    return false;

                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parentRect, VideoImage.rectTransform);
                Vector2 videoSize = (Vector2)bounds.size;
                Vector2 videoCenter = (Vector2)bounds.center;
                Vector2 regionCenterLocal = videoCenter + new Vector2((regionCenter01.x - 0.5f) * videoSize.x, (regionCenter01.y - 0.5f) * videoSize.y);
                Vector2 half = new Vector2(regionSize01.x * videoSize.x * 0.5f, regionSize01.y * videoSize.y * 0.5f);
                return Mathf.Abs(localPoint.x - regionCenterLocal.x) <= half.x && Mathf.Abs(localPoint.y - regionCenterLocal.y) <= half.y;
            }

            // Fallback to canvas-based region
            if (canvasRect == null) return true;
            Camera fallbackCam = null;
            var canvas = canvasRect.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) fallbackCam = canvas.worldCamera;
            Vector2 localPointFB;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, fallbackCam, out localPointFB))
                return false;
            var canvasSize = canvasRect.rect.size;
            Vector2 localCenter = new Vector2((regionCenter01.x - 0.5f) * canvasSize.x, (regionCenter01.y - 0.5f) * canvasSize.y);
            Vector2 halfFB = new Vector2(regionSize01.x * canvasSize.x * 0.5f, regionSize01.y * canvasSize.y * 0.5f);
            return Mathf.Abs(localPointFB.x - localCenter.x) <= halfFB.x && Mathf.Abs(localPointFB.y - localCenter.y) <= halfFB.y;
        };

        while (timeRemaining > 0f)
        {
            if (!pointerDown)
            {
                bool down = false;
                if (Input.GetMouseButtonDown(0) && isInsideRegion(Input.mousePosition)) down = true;
#if UNITY_EDITOR || UNITY_STANDALONE
                // Keyboard press is not location-aware; ignore to enforce region constraint
#endif
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        if (Input.GetTouch(i).phase == TouchPhase.Began && isInsideRegion(Input.GetTouch(i).position))
                        {
                            down = true;
                            break;
                        }
                    }
                }
                if (down)
                {
                    pointerDown = true;
                    holdTime = 0f;
                }
            }
            else
            {
                bool stillDown = Input.GetMouseButton(0) && isInsideRegion(Input.mousePosition);
                if (Input.touchCount > 0)
                {
                    bool anyTouchHolding = false;
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        var ph = Input.GetTouch(i).phase;
                        if ((ph == TouchPhase.Stationary || ph == TouchPhase.Moved) && isInsideRegion(Input.GetTouch(i).position))
                        {
                            anyTouchHolding = true;
                            break;
                        }
                    }
                    if (anyTouchHolding) stillDown = true;
                }

                if (stillDown)
                {
                    holdTime += Time.deltaTime;
                    if (finishProgressBar != null && requiredHold > 0f)
                    {
                        finishProgressBar.fillAmount = Mathf.Clamp01(holdTime / requiredHold);
                    }
                    if (qteKeyText != null)
                    {
                        qteKeyText.text = $"{Mathf.Max(0f, requiredHold - holdTime):0.0}s";
                    }
                    if (holdTime >= requiredHold)
                    {
                        if (finishProgressBar != null) finishProgressBar.fillAmount = 1f;
                        CompleteQTE(1, onComplete, HideQTE);
                        yield break;
                    }
                }
                else
                {
                    pointerDown = false;
                    holdTime = 0f;
                    if (finishProgressBar != null) finishProgressBar.fillAmount = 0f;
                    if (qteKeyText != null) qteKeyText.text = string.Empty;
                }
            }

            if (window > 0f)
            {
                timeRemaining -= Time.deltaTime;
                if (qteProgressBar != null)
                {
                    qteProgressBar.fillAmount = Mathf.Clamp01(timeRemaining / window);
                }
            }
            yield return null;
        }

        if (qteProgressBar != null) qteProgressBar.fillAmount = 0f;
        CompleteQTE(0, onComplete, HideQTE);
    }

    private IEnumerator FadeInPanel(CanvasGroup canvasGroup, GameObject panel)
    {
        if (panel != null) panel.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                t += Time.deltaTime;
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
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        gameObject.SetActive(false);
        if (qtePanel != null) qtePanel.SetActive(false);
        if (qtePanelCanvasGroup != null) qtePanelCanvasGroup.alpha = 0f;
        if (qteProgressBar != null) qteProgressBar.fillAmount = 0f;
        if (finishProgressBar != null) finishProgressBar.fillAmount = 0f;
    }
}
