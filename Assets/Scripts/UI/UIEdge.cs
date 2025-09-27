using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a straight line (UI Image) between two UI points (RectTransforms) within the same Canvas space.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIEdge : MonoBehaviour
{
    public Image lineImage; // Assign a 1x1 white sprite or a 9-sliced line sprite
    [Tooltip("Push the line endpoints slightly outside the node bounds to avoid overlapping the visuals.")]
    public float endpointMargin = 8f;

    private RectTransform _rt;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        if (lineImage != null)
        {
            lineImage.raycastTarget = false;
        }
    }

    public enum Axis { Auto, Horizontal, Vertical }

    /// <summary>
    /// Connect two UI nodes using side midpoints. If axis is specified, force side selection accordingly.
    /// </summary>
    public void Connect(RectTransform from, RectTransform to, Axis axis = Axis.Auto)
    {
        if (from == null || to == null) return;

        var parentRt = _rt.parent as RectTransform;
        if (parentRt == null) parentRt = _rt; // fallback

        // Determine bounds rects (prefer NodeUI background image if provided)
        RectTransform fromBounds = ResolveBoundsRect(from);
        RectTransform toBounds = ResolveBoundsRect(to);

        // Determine relative positions in parent local space using bounds rects
        Vector2 fromCenterLocal = WorldToLocalIn(parentRt, fromBounds.TransformPoint(fromBounds.rect.center));
        Vector2 toCenterLocal = WorldToLocalIn(parentRt, toBounds.TransformPoint(toBounds.rect.center));
        Vector2 delta = toCenterLocal - fromCenterLocal;

        // Decide axis if Auto
        Axis chosen = axis;
        if (chosen == Axis.Auto)
        {
            chosen = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? Axis.Horizontal : Axis.Vertical;
        }

        // Choose connection sides based on axis
        Vector3 fromWorld;
        Vector3 toWorld;
        if (chosen == Axis.Horizontal)
        {
            fromWorld = GetSideWorldPoint(fromBounds, delta.x >= 0 ? Side.Right : Side.Left);
            toWorld = GetSideWorldPoint(toBounds, delta.x >= 0 ? Side.Left : Side.Right);
        }
        else // Vertical
        {
            fromWorld = GetSideWorldPoint(fromBounds, delta.y >= 0 ? Side.Top : Side.Bottom);
            toWorld = GetSideWorldPoint(toBounds, delta.y >= 0 ? Side.Bottom : Side.Top);
        }

        Vector2 localA = WorldToLocalIn(parentRt, fromWorld);
        Vector2 localB = WorldToLocalIn(parentRt, toWorld);

        Vector2 dir = (localB - localA);
        float length = dir.magnitude;
        
        // Apply endpoint margin by shortening the line from both ends
        if (length > endpointMargin * 2f)
        {
            Vector2 dirNormalized = dir.normalized;
            localA += dirNormalized * endpointMargin;
            localB -= dirNormalized * endpointMargin;
            dir = (localB - localA);
            length = dir.magnitude;
        }
        
        Vector2 mid = (localA + localB) * 0.5f;

        // Place the edge in parent space
        _rt.anchoredPosition = mid;
        _rt.sizeDelta = new Vector2(length, _rt.sizeDelta.y == 0 ? 2f : _rt.sizeDelta.y);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rt.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private enum Side { Left, Right, Top, Bottom }

    private Vector3 GetSideWorldPoint(RectTransform rt, Side side)
    {
        Rect r = rt.rect;
        Vector2 local;
        switch (side)
        {
            case Side.Left:
                // Connect to the exact left edge center
                local = new Vector2(r.xMin, (r.yMin + r.yMax) * 0.5f);
                break;
            case Side.Right:
                // Connect to the exact right edge center
                local = new Vector2(r.xMax, (r.yMin + r.yMax) * 0.5f);
                break;
            case Side.Top:
                // Connect to the exact top edge center
                local = new Vector2((r.xMin + r.xMax) * 0.5f, r.yMax);
                break;
            case Side.Bottom:
            default:
                // Connect to the exact bottom edge center
                local = new Vector2((r.xMin + r.xMax) * 0.5f, r.yMin);
                break;
        }
        return rt.TransformPoint(local);
    }

    private static RectTransform ResolveBoundsRect(RectTransform rt)
    {
        if (rt == null) return null;
        var node = rt.GetComponent<NodeUI>();
        if (node == null)
            node = rt.GetComponentInParent<NodeUI>();
        if (node != null)
        {
            var b = node.GetBoundsRect();
            if (b != null) return b;
        }
        return rt;
    }

    private static Vector2 WorldToLocalIn(RectTransform parent, Vector3 world)
    {
        // Prefer using the Canvas's worldCamera when available (Screen Space - Camera)
        Canvas canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
        }

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(cam, world),
            cam,
            out local);
        return local;
    }
}
