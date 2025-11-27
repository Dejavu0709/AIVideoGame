using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单的手指点击提示动画脚本：
/// 让一张 UI 图片（手指）循环做“按下-抬起”的动画，看起来像在不断点击。
/// 挂在含有 RectTransform 的 UI 对象上即可。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UITapHint : MonoBehaviour
{
    [Header("Tap Motion")]
    [Tooltip("手指按下时的位移（本地坐标），例如 (0, -20) 表示向下移动 20 像素")]
    public Vector2 downOffset = new Vector2(0f, -20f);

    [Tooltip("按下时的缩放比例，例如 0.9 表示稍微变小一点")]
    [Range(0.5f, 1.5f)]
    public float downScale = 0.9f;

    [Tooltip("一次按下/抬起动画的时间（秒），越小越快")]
    public float tapDuration = 0.15f;

    [Tooltip("两次点击之间的间隔（秒）")]
    public float interval = 0.4f;

    [Tooltip("启用时是否自动开始动画")]
    public bool playOnEnable = true;

    private RectTransform rectTransform;
    private Vector2 initialAnchoredPos;
    private Vector3 initialScale;
    private Coroutine playRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialAnchoredPos = rectTransform.anchoredPosition;
        initialScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        // 重新记录初始状态，避免外部改动后偏移错误
        initialAnchoredPos = rectTransform.anchoredPosition;
        initialScale = rectTransform.localScale;

        if (playOnEnable)
        {
            StartTap();
        }
    }

    private void OnDisable()
    {
        StopTap();
        // 还原到初始状态，避免停用时停在中间帧
        rectTransform.anchoredPosition = initialAnchoredPos;
        rectTransform.localScale = initialScale;
    }

    public void StartTap()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }
        playRoutine = StartCoroutine(TapLoop());
    }

    public void StopTap()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }

    private IEnumerator TapLoop()
    {
        while (true)
        {
            // 按下
            yield return StartCoroutine(AnimateTo(initialAnchoredPos + downOffset, initialScale * downScale, tapDuration));
            // 抬起
            yield return StartCoroutine(AnimateTo(initialAnchoredPos, initialScale, tapDuration));

            if (interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator AnimateTo(Vector2 targetPos, Vector3 targetScale, float duration)
    {
        if (duration <= 0f)
        {
            rectTransform.anchoredPosition = targetPos;
            rectTransform.localScale = targetScale;
            yield break;
        }

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;
        float t = 0f;
        while (t < duration)
        {
            float lerp = t / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, lerp);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, lerp);
            t += Time.unscaledDeltaTime; // 使用 unscaled 时间，避免受暂停/慢动作影响
            yield return null;
        }
        rectTransform.anchoredPosition = targetPos;
        rectTransform.localScale = targetScale;
    }
}
