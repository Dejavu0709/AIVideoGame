using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootQTE : MonoBehaviour
{
    public Camera ShootCamera;
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
    private System.Action<int> onComplete;
    private Coroutine wobbleCo;
    private QTEData currentData;
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
        if (ShootCamera != null)
        {
            ShootCamera.transform.localRotation = Quaternion.identity;
        }
        if (wobbleCo != null)
        {
            StopCoroutine(wobbleCo);
        }
        wobbleCo = StartCoroutine(WobbleRoutine());
    }

    public void HideQTE()
    {
        if (wobbleCo != null)
        {
            StopCoroutine(wobbleCo);
            wobbleCo = null;
        }
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        float t = 0f;
        while (t < hideDelaySeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }
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
            if (ShootCamera != null)
            {
                float rx = Mathf.Sin((Time.time + seedX) * wobbleSpeedX) * wobbleAmplitudeX;
                float ry = Mathf.Sin((Time.time + seedY) * wobbleSpeedY) * wobbleAmplitudeY;
                var e = new Vector3(rx, ry, 0f);
                ShootCamera.transform.localRotation = Quaternion.Euler(e);
            }
            yield return null;
        }
    }

    private void OnShootClicked()
    {
        Vector2 center = new Vector2(0.5f, 0.5f);
        Vector2 rectCenter = new Vector2(0.5f, 0.5f);
        Vector2 rectSize = new Vector2(0.1f, 0.1f);
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
        if (currentData != null)
        {
            if (!string.IsNullOrEmpty(currentData.param1))
            {
                var parts = currentData.param1.Split(',');
                if (parts.Length >= 2)
                {
                    float.TryParse(parts[0], out rectCenter.x);
                    float.TryParse(parts[1], out rectCenter.y);
                }
            }
            if (!string.IsNullOrEmpty(currentData.param2))
            {
                var parts = currentData.param2.Split(',');
                if (parts.Length >= 2)
                {
                    float.TryParse(parts[0], out rectSize.x);
                    float.TryParse(parts[1], out rectSize.y);
                }
            }
            if (!string.IsNullOrEmpty(currentData.param3))
            {
                var parts = currentData.param3.Split(',');
                if (parts.Length >= 4)
                {
                    float ax, ay, sx, sy;
                    if (float.TryParse(parts[0], out ax)) wobbleAmplitudeX = ax;
                    if (float.TryParse(parts[1], out ay)) wobbleAmplitudeY = ay;
                    if (float.TryParse(parts[2], out sx)) wobbleSpeedX = sx;
                    if (float.TryParse(parts[3], out sy)) wobbleSpeedY = sy;
                }
            }
        }
        Vector2 half = rectSize * 0.5f;
        bool inside = Mathf.Abs(center.x - rectCenter.x) <= half.x && Mathf.Abs(center.y - rectCenter.y) <= half.y;
        onComplete?.Invoke(inside ? 1 : 0);
        HideQTE();
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
}
