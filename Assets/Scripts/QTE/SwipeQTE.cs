using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwipeQTE : BaseQTE
{
    public TextMeshProUGUI qteInstructionText;
    public Image qteProgressBar;
    public TextMeshProUGUI qteKeyText;
    public GameObject qtePanel;
    public CanvasGroup qtePanelCanvasGroup;
    public float fadeInDuration = 0.25f;
    public RawImage VideoImage;
    public RectTransform Content;
    public GameObject dirUp;
    public GameObject dirDown;
    public GameObject dirLeft;
    public GameObject dirRight;
    public Image progressLeft;
    public Image progressRight;
    public Image progressUp;
    public Image progressDown;
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

        // Direction settings
        Vector2 expectedDir = ParseDirection(qteData.param1, out string dirLabel);
        float minDistance = 100f; // pixels
        float angleTolerance = 45f; // degrees
        if (!string.IsNullOrEmpty(qteData.param2)) float.TryParse(qteData.param2, out minDistance);
        if (!string.IsNullOrEmpty(qteData.param3)) float.TryParse(qteData.param3, out angleTolerance);
        minDistance = Mathf.Max(5f, minDistance);
        angleTolerance = Mathf.Clamp(angleTolerance, 1f, 89f);

        // Optional: place Content relative to VideoImage using qteData.position and param3 as size
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
            }
        }
        // Reuse param3 as region size if formatted as w,h in [0..1] (optional)
        // If you already use param3 for angleTolerance, ignore this sizing by leaving default
        // Here we only size UI container; swipe detection logic stays unchanged
        if (qteData != null && qteData.param3 != null && qteData.param3.Contains(","))
        {
            var parts = qteData.param3.Split(',');
            if (parts.Length >= 2)
            {
                float w, h;
                if (float.TryParse(parts[0], out w)) regionSize01.x = Mathf.Clamp01(Mathf.Abs(w));
                if (float.TryParse(parts[1], out h)) regionSize01.y = Mathf.Clamp01(Mathf.Abs(h));
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

        if (qteInstructionText != null)
        {
            if (!string.IsNullOrEmpty(dirLabel))
                qteInstructionText.text = $"按方向滑动：{dirLabel}";
            else
                qteInstructionText.text = "按提示方向滑动";
        }

        SetDirectionIndicators(expectedDir, dirLabel);

        if (qtePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));
        }

        float timeRemaining = window > 0 ? window : float.PositiveInfinity;
        bool pointerDown = false;
        Vector2 startPos = Vector2.zero;
        Vector2 currentPos = Vector2.zero;
        qteProgressBar.gameObject.SetActive(window > 0);
        while (timeRemaining > 0f)
        {
            // Progress by time (optional)
            if (qteProgressBar != null && window > 0f)
            {
                qteProgressBar.fillAmount = (timeRemaining / window);
            }

            // Begin
            if (!pointerDown)
            {
                bool down = false;
                Vector2 pos = Vector2.zero;
                if (Input.GetMouseButtonDown(0)) { down = true; pos = Input.mousePosition; }
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        if (Input.GetTouch(i).phase == TouchPhase.Began)
                        {
                            down = true; pos = Input.GetTouch(i).position; break;
                        }
                    }
                }
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetKeyDown(KeyCode.Space)) { down = true; pos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f); }
#endif
                if (down)
                {
                    pointerDown = true;
                    startPos = pos;
                    currentPos = pos;
                    if (qteKeyText != null) qteKeyText.text = "";
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
                    // use first active touch
                    Touch t = Input.GetTouch(0);
                    pos = t.position;
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) up = true;
                }
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetKeyUp(KeyCode.Space)) up = true;
#endif
                currentPos = pos;

                Vector2 delta = currentPos - startPos;
                float dist = delta.magnitude;
                if (qteKeyText != null)
                {
                    qteKeyText.text = $"{(int)dist}/{(int)minDistance}px";
                }

                if (up)
                {
                    if (dist >= minDistance)
                    {
                        if (expectedDir == Vector2.zero)
                        {
                            CompleteQTE(1, onComplete, HideQTE);
                            yield break;
                        }
                        else
                        {
                            Vector2 nd = delta.normalized;
                            Vector2 ne = expectedDir.normalized;
                            float cos = Vector2.Dot(nd, ne);
                            float passCos = Mathf.Cos(angleTolerance * Mathf.Deg2Rad);
                            if (cos >= passCos)
                            {
                                CompleteQTE(1, onComplete, HideQTE);
                                yield break;
                            }
                        }
                    }
                    // reset for next attempt within time window
                    pointerDown = false;
                    startPos = currentPos;
                }
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        CompleteQTE(0, onComplete, HideQTE);
    }

    private void SetDirectionIndicators(Vector2 expectedDir, string dirLabel)
    {
        if (dirUp != null) dirUp.SetActive(false);
        if (dirDown != null) dirDown.SetActive(false);
        if (dirLeft != null) dirLeft.SetActive(false);
        if (dirRight != null) dirRight.SetActive(false);

        if (expectedDir == Vector2.zero) return;

        if (!string.IsNullOrEmpty(dirLabel))
        {
            if (dirLabel == "上" && dirUp != null) { dirUp.SetActive(true);qteProgressBar = progressUp; return; }
            if (dirLabel == "下" && dirDown != null) { dirDown.SetActive(true);qteProgressBar = progressDown; return; }
            if (dirLabel == "左" && dirLeft != null) { dirLeft.SetActive(true);qteProgressBar = progressLeft; return; }
            if (dirLabel == "右" && dirRight != null) { dirRight.SetActive(true);qteProgressBar = progressRight; return; }
        }

        Vector2 v = expectedDir.normalized;
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
        {
            if (v.x >= 0f) { if (dirRight != null) dirRight.SetActive(true); }
            else { if (dirLeft != null) dirLeft.SetActive(true); }
        }
        else
        {
            if (v.y >= 0f) { if (dirUp != null) dirUp.SetActive(true); }
            else { if (dirDown != null) dirDown.SetActive(true); }
        }
    }

    private Vector2 ParseDirection(string s, out string label)
    {
        label = null;
        if (string.IsNullOrEmpty(s)) return Vector2.zero;
        string lower = s.Trim().ToLowerInvariant();
        switch (lower)
        {
            case "up": label = "上"; return Vector2.up;
            case "down": label = "下"; return Vector2.down;
            case "left": label = "左"; return Vector2.left;
            case "right": label = "右"; return Vector2.right;
        }
        var parts = lower.Split(',');
        if (parts.Length >= 2)
        {
            float x, y;
            if (float.TryParse(parts[0], out x) && float.TryParse(parts[1], out y))
            {
                Vector2 v = new Vector2(x, y);
                if (v.sqrMagnitude > 0.0001f) return v;
            }
        }
        return Vector2.zero;
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
    }
}
