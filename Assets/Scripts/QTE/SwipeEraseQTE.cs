using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwipeEraseQTE : BaseQTE
{
    public TextMeshProUGUI qteInstructionText;
    public Image qteProgressBar;
    public TextMeshProUGUI qteKeyText;
    public GameObject qtePanel;
    public CanvasGroup qtePanelCanvasGroup;
    public float fadeInDuration = 0.25f;
    public RawImage VideoImage;
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

        // Orientation: "horizontal" (default) or "vertical" in param1
        bool vertical = false;
        if (!string.IsNullOrEmpty(qteData.param1))
        {
            string p1 = qteData.param1.Trim().ToLowerInvariant();
            if (p1 == "vertical" || p1 == "v" || p1 == "y") vertical = true;
        }

        // Required back-and-forth count in param2
        int requiredSwipes = 0;
        if (!string.IsNullOrEmpty(qteData.param2)) int.TryParse(qteData.param2, out requiredSwipes);
        requiredSwipes = Mathf.Max(1, requiredSwipes);

        // Minimum distance (pixels) per swipe
        float minDistance = 100f;
        if (!string.IsNullOrEmpty(qteData.param3) && !qteData.param3.Contains(","))
        {
            float.TryParse(qteData.param3, out minDistance);
        }
        minDistance = Mathf.Max(5f, minDistance);

        // Region placement (similar to LongPressQTE)
        Vector2 regionCenter01 = new Vector2(0.5f, 0.5f);
        Vector2 regionSize01 = new Vector2(0.3f, 0.3f);
        if (!string.IsNullOrEmpty(qteData.position))
        {
            var parts = qteData.position.Split(',');
            if (parts.Length >= 2)
            {
                float cx, cy;
                if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out cx)) regionCenter01.x = Mathf.Clamp01(cx);
                if (float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out cy)) regionCenter01.y = Mathf.Clamp01(cy);
            }
        }

        // Optional: use param3 as region size when formatted as "w,h" in [0..1]
        if (!string.IsNullOrEmpty(qteData.param3) && qteData.param3.Contains(","))
        {
            var parts = qteData.param3.Split(',');
            if (parts.Length >= 2)
            {
                float w, h;
                if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out w)) regionSize01.x = Mathf.Clamp01(Mathf.Abs(w));
                if (float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out h)) regionSize01.y = Mathf.Clamp01(Mathf.Abs(h));
            }
        }

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
                    Vector2 minPos = new Vector2(-canvasSize.x * 0.5f + halfSize.x, -canvasSize.y * 0.5f + halfSize.y);
                    Vector2 maxPos = new Vector2(canvasSize.x * 0.5f - halfSize.x, canvasSize.y * 0.5f - halfSize.y);
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

        System.Func<Vector2, bool> isInsideRegion = (Vector2 screenPoint) =>
        {
            RectTransform parentRect = Content != null ? Content.transform.parent as RectTransform : null;
            if (VideoImage != null && VideoImage.rectTransform != null && parentRect != null)
            {
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

        if (qteInstructionText != null)
        {
            string axisLabel = vertical ? "上下" : "左右";
            qteInstructionText.text = $"在{axisLabel}方向来回滑动 {requiredSwipes} 次";
        }

        Debug.Log($"[SwipeEraseQTE] Start QTE - axis={(vertical ? "vertical" : "horizontal")}, requiredSwipes={requiredSwipes}, minDistance={minDistance}, window={window}");

        if (qtePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));
        }

        float timeRemaining = window > 0 ? window : float.PositiveInfinity;
        bool pointerDown = false;
        Vector2 lastPos = Vector2.zero;
        Vector2 currentPos = Vector2.zero;
        int completedSwipes = 0;
        int currentDir = 0;           // 当前移动方向（-1 / +1）
        float traveledInDir = 0f;     // 在当前方向上累计的位移（绝对值）

        if (qteProgressBar != null)
        {
            qteProgressBar.gameObject.SetActive(window > 0f);
            if (window > 0f)
            {
                qteProgressBar.fillAmount = 1f;
            }
        }

        while (timeRemaining > 0f)
        {
            if (window > 0f && qteProgressBar != null)
            {
                qteProgressBar.fillAmount = Mathf.Clamp01(timeRemaining / window);
            }

            if (!pointerDown)
            {
                bool down = false;
                Vector2 pos = Vector2.zero;
                if (Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; down = isInsideRegion(pos); }
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        if (Input.GetTouch(i).phase == TouchPhase.Began)
                        {
                            pos = Input.GetTouch(i).position;
                            down = isInsideRegion(pos);
                            break;
                        }
                    }
                }
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetKeyDown(KeyCode.Space)) { down = true; pos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f); }
#endif
                if (down)
                {
                    pointerDown = true;
                    lastPos = pos;
                    currentPos = pos;
                    currentDir = 0;
                    traveledInDir = 0f;
                    Debug.Log($"[SwipeEraseQTE] Pointer down inside region at {pos}, reset progress. completedSwipes={completedSwipes}/{requiredSwipes}");
                    if (qteKeyText != null)
                    {
                        qteKeyText.text = $"0/{requiredSwipes}";
                    }
                }
            }
            else
            {
                bool up = false;
                Vector2 pos = currentPos;
                if (Input.GetMouseButton(0)) pos = Input.mousePosition;
                if (Input.GetMouseButtonUp(0)) { up = true; pos = Input.mousePosition; }
                if (Input.touchCount > 0)
                {
                    Touch t = Input.GetTouch(0);
                    pos = t.position;
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) up = true;
                }
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetKeyUp(KeyCode.Space)) up = true;
#endif
                currentPos = pos;

                // 在按住期间，根据每一帧的位置差来累计位移，并基于方向变化计次
                Vector2 step = currentPos - lastPos;
                float axisStep = vertical ? step.y : step.x;
                float stepAbs = Mathf.Abs(axisStep);

                if (stepAbs > 0.01f)
                {
                    int stepDir = axisStep > 0f ? 1 : -1;
                    if (currentDir == 0)
                    {
                        // 第一次确定方向
                        currentDir = stepDir;
                        traveledInDir = 0f;
                        Debug.Log($"[SwipeEraseQTE] First move dir={currentDir}, axisStep={axisStep}");
                    }

                    if (stepDir == currentDir)
                    {
                        traveledInDir += stepAbs;
                        if (traveledInDir >= minDistance)
                        {
                            // 在同一方向上累计位移达到阈值，计一次，然后要求反向擦才算下一次
                            completedSwipes++;
                            traveledInDir = 0f;
                            currentDir = -currentDir;

                            Debug.Log($"[SwipeEraseQTE] Progress tick - completedSwipes={completedSwipes}/{requiredSwipes}, nextRequiredDir={currentDir}");

                            if (qteKeyText != null)
                            {
                                qteKeyText.text = $"{completedSwipes}/{requiredSwipes}";
                            }

                            if (completedSwipes >= requiredSwipes)
                            {
                                Debug.Log("[SwipeEraseQTE] QTE SUCCESS - reached required swipes");
                                CompleteQTE(1, onComplete, HideQTE);
                                yield break;
                            }
                        }
                    }
                    else
                    {
                        // 方向反转，重置计数到新方向
                        currentDir = stepDir;
                        traveledInDir = stepAbs;
                        Debug.Log($"[SwipeEraseQTE] Direction flip - newDir={currentDir}, traveledInDir={traveledInDir}");
                    }
                }

                lastPos = currentPos;

                if (up)
                {
                    // 松手仅用于结束当前按压，不再基于 Up 判定擦除
                    Debug.Log($"[SwipeEraseQTE] Pointer up - final progress {completedSwipes}/{requiredSwipes}");
                    pointerDown = false;
                    currentDir = 0;
                    traveledInDir = 0f;
                }
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        Debug.Log($"[SwipeEraseQTE] QTE FAILED - timeout, final progress {completedSwipes}/{requiredSwipes}");
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
    }
}
