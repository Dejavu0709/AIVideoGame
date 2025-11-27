using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ShootQTE : BaseQTE
{
    public Camera ShootCamera;
    public GameObject Gun;
    public Transform GunStart;
    public Transform GunEnd;
    public GameObject scopeOverlay;
    public Button shootButton;
    public GameObject qtePanel;
    public CanvasGroup qtePanelCanvasGroup;
    public float fadeInDuration = 0.25f;
    public float wobbleAmplitudeX = 1.5f;
    public float wobbleAmplitudeY = 1.5f;
    public float wobbleSpeedX = 0.7f;
    public float wobbleSpeedY = 1.1f;
    public float hideDelaySeconds = 2f;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gunshotClip;
    [Header("UI Shake (Canvas)")]
    public RectTransform uiCanvasToShake;
    public float shakeDuration = 0.2f; // seconds
    public float shakeMagnitude = 25f; // pixels
    public Image qteProgressBar;
    public GameObject target;
    private System.Action<int> onComplete;
    private Coroutine wobbleCo;
    private Coroutine timeoutCo;
    private QTEData currentData;
    private bool completed;
    // Start is called before the first frame update
    void Start()
    {
        if (shootButton != null)
        {
            shootButton.onClick.AddListener(OnShootClicked);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowQTE(QTEData qteData, System.Action<int> onComplete)
    {
        this.onComplete = onComplete;
        currentData = qteData;
        completed = false;
        ResetResultIndicators();
        if (qtePanel != null)
        {
            qtePanel.SetActive(true);
        }
        gameObject.SetActive(true);
        if (scopeOverlay != null) scopeOverlay.SetActive(true);
        if (qtePanelCanvasGroup != null)
        {
            StartCoroutine(FadeInPanel(qtePanelCanvasGroup, qtePanel));
        }
        if (Gun != null)
        {
            Gun.transform.localRotation = Quaternion.identity;
        }
        else if (ShootCamera != null)
        {
            ShootCamera.transform.localRotation = Quaternion.identity;
        }
        // Position target via parameters: param1 = "x,y" in world units (x in [-20,20], y in [-10,10]);
        // If absent, random within bounds. z remains unchanged.
        if (target != null)
        {
            float x = 0f, y = 0f;
            bool hasXY = false;
            if (currentData != null && !string.IsNullOrEmpty(currentData.param1))
            {
                var parts = currentData.param1.Split(',');
                if (parts.Length >= 2)
                {
                    if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                    {
                        hasXY = true;
                    }
                }
            }
            if (!hasXY)
            {
                x = Random.Range(-20f, 20f);
                y = Random.Range(-10f, 10f);
            }
            x = Mathf.Clamp(x, -20f, 20f);
            y = Mathf.Clamp(y, -10f, 10f);
            var pos = target.transform.localPosition;
            pos.x = x;
            pos.y = y;
            target.transform.localPosition = pos;
        }
        if (wobbleCo != null)
        {
            StopCoroutine(wobbleCo);
        }
        wobbleCo = StartCoroutine(WobbleRoutine());

        if (timeoutCo != null)
        {
            StopCoroutine(timeoutCo);
            timeoutCo = null;
        }
        if (currentData != null && currentData.duration > 0f)
        {
            timeoutCo = StartCoroutine(TimeoutRoutine(currentData.duration));
        }
        // Setup progress bar visibility and initial fill based on duration
        if (qteProgressBar != null)
        {
            if (currentData != null && currentData.duration > 0f)
            {
                qteProgressBar.gameObject.SetActive(true);
                qteProgressBar.fillAmount = 1f;
            }
            else
            {
                qteProgressBar.gameObject.SetActive(false);
            }
        }
    }

    public void HideQTE()
    {
        if (wobbleCo != null)
        {
            StopCoroutine(wobbleCo);
            wobbleCo = null;
        }
        if (timeoutCo != null)
        {
            StopCoroutine(timeoutCo);
            timeoutCo = null;
        }
        if (qteProgressBar != null)
        {
            qteProgressBar.gameObject.SetActive(false);
        }
        HideAfterDelay();
    }

    private void HideAfterDelay()
    {
        if (scopeOverlay != null) scopeOverlay.SetActive(false);
        if (qtePanel != null) qtePanel.SetActive(false);
        if (qtePanelCanvasGroup != null) qtePanelCanvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private IEnumerator FadeInPanel(CanvasGroup canvasGroup, GameObject panel)
    {
        if (panel != null)
            panel.SetActive(true);
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

    private IEnumerator WobbleRoutine()
    {
        float seedX = Random.value * 100f;
        float seedY = Random.value * 100f;
        while (true)
        {
            Transform target = null;
            if (Gun != null) target = Gun.transform;
            else if (ShootCamera != null) target = ShootCamera.transform;

            if (target != null)
            {
                float rx = Mathf.Sin((Time.time + seedX) * wobbleSpeedX) * wobbleAmplitudeX;
                float ry = Mathf.Sin((Time.time + seedY) * wobbleSpeedY) * wobbleAmplitudeY;
                var e = new Vector3(rx, ry, 0f);
                target.localRotation = Quaternion.Euler(e);
            }
            yield return null;
        }
    }

    private void OnShootClicked()
    {
        if (completed) return;
        bool inside = false;
        // Determine hit by checking if line from GunStart to GunEnd intersects target's 2D collider
        if (GunStart != null && GunEnd != null && target != null)
        {
            Debug.Log("GunStart: " + GunStart.position);
            Debug.Log("GunEnd: " + GunEnd.position);
            var col2D = target.GetComponent<CircleCollider2D>();
            if (col2D != null)
            {
                Vector2 a = (Vector2)GunStart.position;
                Vector2 b = (Vector2)GunEnd.position;
                var hit = Physics2D.Linecast(a, b);
                if (hit.collider != null && hit.collider == col2D)
                {
                    Debug.Log("Hit target");
                    inside = true;
                }
                else
                {
                    Debug.Log("Miss target");
                    inside = false;
                    return;
                }
            }
            else
            {
                Debug.LogError("Target has no CircleCollider2D");
                return;
            }
        }
        // Play audio
        if (audioSource != null && gunshotClip != null)
        {
            audioSource.PlayOneShot(gunshotClip);
        }
        // Shake UI canvas
        if (uiCanvasToShake != null)
        {
            StartCoroutine(ShakeUICanvas(uiCanvasToShake, shakeDuration, shakeMagnitude));
        }
        // Optional: allow wobble params via param3: ax,ay,sx,sy
        if (currentData != null && !string.IsNullOrEmpty(currentData.param3))
        {
            var parts = currentData.param3.Split(',');
            if (parts.Length >= 4)
            {
                float ax, ay, sx, sy;
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out ax)) wobbleAmplitudeX = ax;
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out ay)) wobbleAmplitudeY = ay;
                if (float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out sx)) wobbleSpeedX = sx;
                if (float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out sy)) wobbleSpeedY = sy;
            }
        }
        completed = true;
        CompleteQTE(inside ? 1 : 0, onComplete, HideQTE);
    }

    private IEnumerator ShakeUICanvas(RectTransform target, float duration, float magnitude)
    {
        if (target == null) yield break;
        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Random inside unit circle, scaled by magnitude (pixels)
            Vector2 offset = Random.insideUnitCircle * magnitude;
            target.anchoredPosition = originalPos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.anchoredPosition = originalPos;
    }

    private IEnumerator TimeoutRoutine(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            if (completed) yield break;
            t += Time.deltaTime;
            if (qteProgressBar != null && duration > 0f)
            {
                qteProgressBar.fillAmount = Mathf.Clamp01(1f - (t / duration));
            }
            yield return null;
        }
        if (!completed)
        {
            completed = true;
            if (qteProgressBar != null)
            {
                qteProgressBar.fillAmount = 0f;
            }
            CompleteQTE(0, onComplete, HideQTE);
        }
    }
}
