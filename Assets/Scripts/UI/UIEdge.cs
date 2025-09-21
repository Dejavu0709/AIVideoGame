using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a straight line (UI Image) between two UI points (RectTransforms) within the same Canvas space.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIEdge : MonoBehaviour
{
    public Image lineImage; // Assign a 1x1 white sprite or a 9-sliced line sprite

    private RectTransform _rt;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        /*
        if (lineImage == null)
        {
            lineImage = gameObject.AddComponent<Image>();
            lineImage.color = Color.white;
        }
        lineImage.raycastTarget = false;
        */
    }

    /// <summary>
    /// Connect two UI nodes using side midpoints (left/right/top/bottom) depending on relative layout.
    /// </summary>
    public void Connect(RectTransform from, RectTransform to)
    {
        if (from == null || to == null) return;

        var parentRt = _rt.parent as RectTransform;
        if (parentRt == null) parentRt = _rt; // fallback

        // Determine relative positions in parent local space
        Vector2 fromCenterLocal = WorldToLocalIn(parentRt, from.TransformPoint(from.rect.center));
        Vector2 toCenterLocal = WorldToLocalIn(parentRt, to.TransformPoint(to.rect.center));
        Vector2 delta = toCenterLocal - fromCenterLocal;

        // Choose connection sides: prefer horizontal if |dx| >= |dy|, else vertical
        Vector3 fromWorld;
        Vector3 toWorld;
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            // Horizontal connection
            fromWorld = GetSideWorldPoint(from, delta.x >= 0 ? Side.Right : Side.Left);
            toWorld = GetSideWorldPoint(to, delta.x >= 0 ? Side.Left : Side.Right);
        }
        else
        {
            // Vertical connection
            fromWorld = GetSideWorldPoint(from, delta.y >= 0 ? Side.Top : Side.Bottom);
            toWorld = GetSideWorldPoint(to, delta.y >= 0 ? Side.Bottom : Side.Top);
        }

        Vector2 localA = WorldToLocalIn(parentRt, fromWorld);
        Vector2 localB = WorldToLocalIn(parentRt, toWorld);

        Vector2 dir = (localB - localA);
        float length = dir.magnitude;
        Vector2 mid = (localA + localB) * 0.5f;

        // Place the edge in parent space
        _rt.anchoredPosition = mid;
        _rt.sizeDelta = new Vector2(length, _rt.sizeDelta.y == 0 ? 2f : _rt.sizeDelta.y);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rt.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private enum Side { Left, Right, Top, Bottom }

    private static Vector3 GetSideWorldPoint(RectTransform rt, Side side)
    {
        Rect r = rt.rect;
        Vector2 local;
        switch (side)
        {
            case Side.Left:
                local = new Vector2(r.xMin, (r.yMin + r.yMax) * 0.5f);
                break;
            case Side.Right:
                local = new Vector2(r.xMax, (r.yMin + r.yMax) * 0.5f);
                break;
            case Side.Top:
                local = new Vector2((r.xMin + r.xMax) * 0.5f, r.yMax);
                break;
            case Side.Bottom:
            default:
                local = new Vector2((r.xMin + r.xMax) * 0.5f, r.yMin);
                break;
        }
        return rt.TransformPoint(local);
    }

    private static Vector2 WorldToLocalIn(RectTransform parent, Vector3 world)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, world),
            null,
            out local);
        return local;
    }
}
