using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用数值变化 Toast：包含 Icon、文字和数值变化，支持渐显 + 上移 + 停留 + 渐隐。
/// 使用方法：
/// 1. 在 Canvas 下创建一个 Toast 预制体，挂上本脚本。
/// 2. 把 iconImage、messageText、valueText、canvasGroup 绑定到对应组件。
/// 3. 运行时调用 Show(iconSprite, "说明文字", deltaValue)。
/// </summary>
public class StatChangeToast : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Text messageText;
    public Text valueText;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.2f;
    public float moveUpDistance = 50f;
    public float moveDuration = 0.4f;
    public float holdDuration = 0.6f;
    public float fadeOutDuration = 0.3f;

    private RectTransform _rectTransform;
    private Coroutine _playRoutine;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        // 初始隐藏
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 显示一个 Toast。
    /// </summary>
    /// <param name="icon">显示的图标，可以为 null 表示不显示/用默认图标。</param>
    /// <param name="message">文字说明，如 "好感度"、"金钱" 等。</param>
    /// <param name="deltaValue">数值变化，比如 +10 / -5。</param>
    public void Show(Sprite icon, string message, int deltaValue)
    {
        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else
            {
                // 如果没有传 icon，可以选择隐藏图标
                iconImage.enabled = false;
            }
        }

        if (messageText != null)
        {
            messageText.text = message ?? string.Empty;
        }

        if (valueText != null)
        {
            string sign = deltaValue > 0 ? "+" : string.Empty;
            valueText.text = sign + deltaValue.ToString();
        }

        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
        }
        _playRoutine = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        // 起始位置（以当前 anchoredPosition 为基准）
        Vector2 startPos = _rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, moveUpDistance);

        // 渐显 + 上移
        float t = 0f;
        while (t < Mathf.Max(fadeInDuration, moveDuration))
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(fadeInDuration <= 0f ? 1f : t / fadeInDuration);
            float m = Mathf.Clamp01(moveDuration <= 0f ? 1f : t / moveDuration);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = f;
            }
            _rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, m);

            yield return null;
        }

        // 停留一段时间
        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        // 渐隐
        if (fadeOutDuration > 0f && canvasGroup != null)
        {
            float t2 = 0f;
            float startAlpha = canvasGroup.alpha;
            while (t2 < fadeOutDuration)
            {
                t2 += Time.deltaTime;
                float f2 = 1f - Mathf.Clamp01(t2 / fadeOutDuration);
                canvasGroup.alpha = startAlpha * f2;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        // 动画结束后恢复到起始位置，方便重复使用
        _rectTransform.anchoredPosition = startPos;

        _playRoutine = null;
    }
}
