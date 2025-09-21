using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour
{
    [Header("UI Refs")]
    public Image thumbnailImage;
    public Text titleText;

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
}
