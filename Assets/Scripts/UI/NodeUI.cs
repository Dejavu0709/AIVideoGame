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

    public Button VideoButton;

    // Video data
    private string videoFileName;
    private string cdnBase;

    private void Awake()
    {
        if (VideoButton != null)
        {
            VideoButton.onClick.RemoveListener(OnVideoButtonClicked);
            VideoButton.onClick.AddListener(OnVideoButtonClicked);
        }
    }

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
    /// Assign the video file and CDN base for this node. Wire the button to play.
    /// </summary>
    public void SetVideo(string video, string cdnBase)
    {
        this.videoFileName = video;
        this.cdnBase = cdnBase;
        if (VideoButton != null)
        {
            VideoButton.onClick.RemoveListener(OnVideoButtonClicked);
            VideoButton.onClick.AddListener(OnVideoButtonClicked);
        }
    }

    private void OnVideoButtonClicked()
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            Debug.LogWarning($"NodeUI '{nodeId}' has no video file assigned.");
            return;
        }

        string url = BranchingVideoGameManager.Instance.GetVideoUrl(videoFileName);

        var player = GameObject.FindObjectOfType<VideoPlayerController>();
        if (player == null)
        {
            Debug.LogError("VideoPlayerController not found in scene.");
            return;
        }
        player.PlayVideo(url);
        GameUIController.Instance.HideTreeView();
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
