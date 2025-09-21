using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour
{
    [Header("UI Refs")]
    public Image thumbnailImage;
    public Text titleText;
    [Tooltip("Background image used to determine the node's visual bounds for edge connections")] public Image backgroundImage;

    [Header("Data")]
    public string nodeId;

    /// <summary>
    /// Set the visual info for this node.
    /// </summary>
    public void SetInfo(string id, string title, Sprite thumbnail)
    {
        nodeId = id;
        if (titleText != null)
            titleText.text = title ?? id;
        if (thumbnailImage != null)
            thumbnailImage.sprite = thumbnail;
    }

    /// <summary>
    /// Returns the RectTransform representing the visual bounds of this node for edge connections.
    /// Prefers the backgroundImage's rect if assigned; falls back to this object's RectTransform.
    /// </summary>
    public RectTransform GetBoundsRect()
    {
        if (backgroundImage != null && backgroundImage.rectTransform != null)
            return backgroundImage.rectTransform;
        return GetComponent<RectTransform>();
    }
}
