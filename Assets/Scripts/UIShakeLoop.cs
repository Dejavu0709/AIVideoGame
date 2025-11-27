using UnityEngine;
using DG.Tweening;

/// <summary>
/// 对 UI 的 RectTransform 做往返晃动的效果。
/// 例如：方向设为 Down，图片就会上下往返移动（指示向下）。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIShakeLoop : MonoBehaviour
{
    public enum ShakeDirection
    {
        Up,
        Down,
        Left,
        Right,
        Custom
    }

    [Header("Settings")]
    public ShakeDirection direction = ShakeDirection.Down;
    /// <summary>
    /// 晃动位移的幅度（单位：像素）。
    /// </summary>
    public float distance = 20f;
    /// <summary>
    /// 往返一次（来回）的时长，秒。
    /// </summary>
    public float duration = 0.6f;
    /// <summary>
    /// 是否在启用时自动播放。
    /// </summary>
    public bool playOnEnable = true;
    /// <summary>
    /// 自定义方向向量（仅在 direction = Custom 时使用）。
    /// 会自动归一化。
    /// </summary>
    public Vector2 customDirection = Vector2.down;

    private RectTransform _rectTransform;
    private Tween _tween;
    private Vector2 _originAnchoredPos;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originAnchoredPos = _rectTransform.anchoredPosition;
    }

    void OnEnable()
    {
        _originAnchoredPos = _rectTransform.anchoredPosition;
        if (playOnEnable)
        {
            Play();
        }
    }

    void OnDisable()
    {
        Stop();
        // 还原到初始位置
        _rectTransform.anchoredPosition = _originAnchoredPos;
    }

    /// <summary>
    /// 开始或重新开始晃动。
    /// </summary>
    public void Play()
    {
        Stop();

        Vector2 dir = GetDirectionVector();
        if (dir.sqrMagnitude < 0.0001f)
            return;

        Vector2 targetPos = _originAnchoredPos + dir.normalized * distance;

        // 使用 DOAnchorPos 做往返循环
        _tween = _rectTransform.DOAnchorPos(targetPos, duration * 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// 停止晃动动画。
    /// </summary>
    public void Stop()
    {
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
            _tween = null;
        }
    }

    private Vector2 GetDirectionVector()
    {
        switch (direction)
        {
            case ShakeDirection.Up:
                return Vector2.up;
            case ShakeDirection.Down:
                return Vector2.down;
            case ShakeDirection.Left:
                return Vector2.left;
            case ShakeDirection.Right:
                return Vector2.right;
            case ShakeDirection.Custom:
                return customDirection.normalized;
            default:
                return Vector2.down;
        }
    }
}
