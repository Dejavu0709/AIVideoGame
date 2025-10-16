using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and displays a branching story tree UI from GameData.
/// Place this on a GameObject under a Canvas. Provide containers and prefabs in the Inspector.
/// </summary>
public class StoryTreeView : MonoBehaviour
{
    [Header("Containers")]
    public RectTransform nodesContainer; // Parent for node instances
    public RectTransform edgesContainer; // Parent for edge instances (full-size overlay)

    [Header("Prefabs")]
    public NodeUI nodePrefab; // Prefab with NodeUI, layout, background, thumbnailImage, titleText
    public UIEdge edgePrefab; // Prefab with UIEdge + Image for line (optional). If null, created at runtime.

    [Header("Layout")]
    public float horizontalSpacing = 220f;
    public float verticalSpacing = 160f;
    public Vector2 startPosition = new Vector2(100f, -100f);
    [Tooltip("Extra padding on the left so the first node is fully visible")] public float leftPadding = 120f;

    [Header("Thumbnails")]
    public string thumbnailsResourcesFolder = "Thumbnails"; // Resources/Thumbnails
    [Tooltip("Folder name under StreamingAssets to look for prepacked thumbnails (e.g., StreamingAssets/Thumbnails)")]
    public string streamingThumbnailsFolder = "Thumbnails";
    [Tooltip("Placeholder shown before thumbnail finishes loading")] public Sprite defaultThumbnail;



    public Transform Content;

    public Button BackButton;
    
    [Header("Options")]
    public bool showAll = false; // When true, show the full storyline regardless of visited

    private readonly Dictionary<string, NodeUI> _nodeUIs = new Dictionary<string, NodeUI>();
    private readonly List<UIEdge> _edges = new List<UIEdge>();


    void OnEnable()
    {
        BackButton.onClick.AddListener(BackToVideo);
    }
    void OnDisable()
    {
        BackButton.onClick.RemoveListener(BackToVideo);
    }
    /// <summary>
    /// Build the tree from GameData, showing ONLY visited nodes/edges.
    /// Clears previous UI children.
    /// </summary>
    private void Build(GameData data)
    {
        if (data == null || data.nodes == null || data.nodes.Count == 0)
        {
            Debug.LogWarning("StoryTreeView.Build: No data or empty nodes.");
            return;
        }

        // Get visited set
        var mgr = BranchingVideoGameManager.Instance;
        var visitedSet = (mgr != null ? mgr.GetVisitedNodes() : null) ?? Array.Empty<string>();
        var visited = new HashSet<string>(visitedSet);
        if (showAll && data.nodes != null)
        {
            // Treat all nodes as visited to display the entire storyline
            visited = new HashSet<string>(data.nodes.Select(n => n.id));
        }
        if (visited.Count == 0)
        {
            Clear();
            return;
        }

        string startId = data.meta != null ? data.meta.startNodeId : data.nodes[0].id;
        if (string.IsNullOrEmpty(startId)) startId = data.nodes[0].id;
        // If start not visited, pick first visited node to anchor layout
        if (!visited.Contains(startId))
        {
            var firstVisited = data.nodes.FirstOrDefault(n => visited.Contains(n.id));
            if (firstVisited == null)
            {
                Clear();
                return;
            }
            startId = firstVisited.id;
        }

        Clear();

        // Ensure containers are centered at (0,0) and not shifted by layout
        if (nodesContainer != null)
        {
            nodesContainer.anchorMin = new Vector2(0.5f, 0.5f);
            nodesContainer.anchorMax = new Vector2(0.5f, 0.5f);
            nodesContainer.pivot = new Vector2(0.5f, 0.5f);
            nodesContainer.anchoredPosition = Vector2.zero;
        }
        if (edgesContainer != null)
        {
            edgesContainer.anchorMin = new Vector2(0.5f, 0.5f);
            edgesContainer.anchorMax = new Vector2(0.5f, 0.5f);
            edgesContainer.pivot = new Vector2(0.5f, 0.5f);
            edgesContainer.anchoredPosition = Vector2.zero;
        }

        // Ensure edges are rendered behind nodes
        if (edgesContainer != null && nodesContainer != null)
        {
            edgesContainer.SetSiblingIndex(0);
            nodesContainer.SetSiblingIndex(1);
        }

        // Index nodes by id (only visited)
        var nodeById = data.nodes.Where(n => visited.Contains(n.id)).ToDictionary(n => n.id, n => n);

        // Build adjacency & in-degree for BFS layering
        var children = new Dictionary<string, HashSet<string>>();
        foreach (var node in data.nodes)
        {
            if (!visited.Contains(node.id)) continue; // only visited parent nodes
            if (!children.ContainsKey(node.id)) children[node.id] = new HashSet<string>();
            if (node.choices != null)
            {
                foreach (var c in node.choices)
                {
                    if (!string.IsNullOrEmpty(c.next))
                    {
                        if (c.next != node.id && visited.Contains(c.next)) children[node.id].Add(c.next);
                    }
                }
            }
            if (node.qte != null && node.qte.NextNodeMap != null)
            {
                foreach (var kv in node.qte.NextNodeMap)
                {
                    if (!string.IsNullOrEmpty(kv.Value) && kv.Value != node.id && visited.Contains(kv.Value))
                    {
                        children[node.id].Add(kv.Value);
                        Debug.Log(node.id + ":" + kv.Value);
                    }
                }


                //if (!string.IsNullOrEmpty(node.qte.successNext) && node.qte.successNext != node.id) children[node.id].Add(node.qte.successNext);
                //if (!string.IsNullOrEmpty(node.qte.failNext) && node.qte.failNext != node.id) children[node.id].Add(node.qte.failNext);
            }
            if(!string.IsNullOrEmpty(node.next))
            {
                children[node.id].Add(node.next);
            }
        }

        // BFS to assign levels (depths)
        var level = new Dictionary<string, int>();
        var bfsVisited = new HashSet<string>();
        var q = new Queue<string>();

        if (!nodeById.ContainsKey(startId))
        {
            Debug.LogWarning($"StoryTreeView.Build: start id '{startId}' not found, using first node.");
            startId = data.nodes[0].id;
        }

        q.Enqueue(startId);
        level[startId] = 0;
        bfsVisited.Add(startId);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (!children.TryGetValue(cur, out var ch)) continue;
            foreach (var nx in ch)
            {
                if (!nodeById.ContainsKey(nx)) continue; // skip dangling
                int nextLevel = level[cur] + 1;
                if (!level.ContainsKey(nx) || nextLevel < level[nx])
                {
                    level[nx] = nextLevel;
                }
                if (bfsVisited.Add(nx))
                {
                    q.Enqueue(nx);
                }
            }
        }

        // Add any disconnected visited nodes after BFS
        foreach (var id in nodeById.Keys)
        {
            if (!level.ContainsKey(id))
                level[id] = 0; // place at root level if unreachable
        }

        // Group by level
        var groups = level
            .GroupBy(kv => kv.Value)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

        // Instantiate nodes and layout (visited only)
        _nodeUIs.Clear();

        foreach (var kv in groups)
        {
            int l = kv.Key;
            var ids = kv.Value;

            // Order stable (by id) for consistent layout
            ids.Sort(StringComparer.Ordinal);

            // Vertically center nodes within the same level for even distribution
            float baseY = -((ids.Count - 1) * 0.5f) * verticalSpacing;

            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (!nodeById.ContainsKey(id)) continue;
                var n = nodeById[id];
                var nodeUI = Instantiate(nodePrefab, nodesContainer);
                nodeUI.name = $"Node_{id}";

                // Set info: title and thumbnail
                Sprite sprite = null;
                string title = !string.IsNullOrEmpty(n.question) ? n.question : n.id;
                //var sprite = LoadThumbnailForNode(n, data?.meta?.cdnBase);
                //if (sprite == null)
                {
                    // Ensure thumbnail is loaded or downloaded and cached, then applied when ready
                    string URL = BranchingVideoGameManager.Instance.GetThumbnailUrl($"{n.video.Replace(".mp4", ".png")}");
                    Debug.Log($"Thumbnail URL: {URL}");
                    StartCoroutine(ThumbnailCache.LoadOrDownload(
                    n.video.Split('.').First(),
                    URL,
                    streamingThumbnailsFolder,
                    loaded =>
                    {
                        Debug.Log($"Thumbnail loaded for" + loaded != null);
                        if (nodeUI != null && nodeUI.thumbnailImage != null && loaded != null)
                        {
                            Debug.Log($"Thumbnail loaded for" + loaded != null);
                            nodeUI.thumbnailImage.sprite = loaded;
                        }
                    }));

                }

                nodeUI.SetInfo(n.id, title);
                // Provide video info so the button can play the corresponding video
                nodeUI.SetVideo(n.video, data?.meta?.cdnBase);


                // Position: x by level, y evenly distributed and centered within this level
                var rt = nodeUI.GetComponent<RectTransform>();
                Vector2 pos = new Vector2(l * horizontalSpacing,
                                           baseY + i * verticalSpacing);
                rt.anchoredPosition = pos;

                _nodeUIs[id] = nodeUI;
            }
        }

        // After nodes are placed, ensure Content is large enough and shift nodes so min corner has padding inside Content
        FitContentAndOffsetNodes();

        // Draw edges (only between visited nodes)
        var addedEdges = new HashSet<string>();
        foreach (var fromId in children.Keys)
        {
            if (!_nodeUIs.ContainsKey(fromId)) continue;
            // Use GetBoundsRect() to get the actual bounds rectangle used for edge calculations
            var fromRt = _nodeUIs[fromId].GetBoundsRect();

            foreach (var toId in children[fromId])
            {
                if (!_nodeUIs.ContainsKey(toId)) continue;
                if (!level.TryGetValue(fromId, out var lf) || !level.TryGetValue(toId, out var lt)) continue;
                // Use GetBoundsRect() to get the actual bounds rectangle used for edge calculations
                var toRt = _nodeUIs[toId].GetBoundsRect();

                string key = fromId + "->" + toId;
                if (addedEdges.Contains(key)) continue;
                addedEdges.Add(key);

                var edge = CreateEdge();
                edge.name = $"Edge_{fromId}_to_{toId}";
                // Draw horizontal edges for next-level links, vertical for same-level links.
                if (lt == lf + 1)
                {
                    edge.Connect(fromRt, toRt, UIEdge.Axis.Horizontal);
                }
                else if (lt == lf)
                {
                    edge.Connect(fromRt, toRt, UIEdge.Axis.Vertical);
                }
                else
                {
                    // Skip cross-level non-adjacent edges to avoid clutter
                    DestroyImmediate(edge.gameObject);
                    addedEdges.Remove(key);
                    continue;
                }
                _edges.Add(edge);
            }
        }

        // Content already sized prior to drawing edges
    }

    public void Clear()
    {
        // Clear nodes
        if (nodesContainer != null)
        {
            for (int i = nodesContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(nodesContainer.GetChild(i).gameObject);
            }
        }
        // Clear edges
        if (edgesContainer != null)
        {
            for (int i = edgesContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(edgesContainer.GetChild(i).gameObject);
            }
        }
        _nodeUIs.Clear();
        _edges.Clear();
    }

    private UIEdge CreateEdge()
    {
        UIEdge edge;
        if (edgePrefab != null)
        {
            edge = Instantiate(edgePrefab, edgesContainer);
        }
        else
        {
            var go = new GameObject("Edge", typeof(RectTransform), typeof(Image), typeof(UIEdge));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(edgesContainer, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            //img.color = Color.white;
            edge = go.GetComponent<UIEdge>();
            edge.lineImage = img;
        }
        // Default thin line height
        var ert = edge.GetComponent<RectTransform>();
        ert.sizeDelta = new Vector2(10f, 3f);
        return edge;
    }

    private Sprite LoadThumbnailForNode(GameNode n, string cdnBase)
    {
        // 1) Try local files: StreamingAssets/Thumbnails and then persistent cache
        //if (ThumbnailCache.TryLoadLocal(n.video.Substring(0, n.video.LastIndexOf('.')), streamingThumbnailsFolder, out var localSprite))
       // {
        //    return localSprite;
        //}

        // 2) Try by Resources (fallback)
       // string pathById = string.IsNullOrEmpty(thumbnailsResourcesFolder) ? n.video.Substring(0, n.video.LastIndexOf('.')) : ($"{thumbnailsResourcesFolder}/{n.id}");
      //  var sprite = Resources.Load<Sprite>(pathById);
        //if (sprite != null) return sprite;

        // 3) Try by video base name (without extension) in Resources
        Sprite sprite = null;
        string baseName = n.video.Substring(0, n.video.LastIndexOf('.'));
        sprite = Resources.Load<Sprite>(baseName);
        if (sprite != null) return sprite;
        Debug.Log(baseName);
        if (!string.IsNullOrEmpty(baseName))
        {
            int dot = baseName.LastIndexOf('.');
            if (dot >= 0) baseName = baseName.Substring(0, dot);
            string pathByVideo = string.IsNullOrEmpty(thumbnailsResourcesFolder) ? baseName : ($"{thumbnailsResourcesFolder}/{baseName}");
            sprite = Resources.Load<Sprite>(pathByVideo);
            if (sprite != null) return sprite;
        }

        return null;
    }

    /// <summary>
    /// Compute bounds of the placed nodes, set Content size, and shift nodes so the tree starts within Content with padding.
    /// This should be called BEFORE drawing edges so the edge positions are correct post-shift.
    /// </summary>
    private void FitContentAndOffsetNodes()
    {
        var contentRt = (Content as RectTransform) ?? nodesContainer; // fallback to nodesContainer if Content unassigned
        if (contentRt == null || _nodeUIs.Count == 0)
            return;

        // Compute bounds in Content's local space using world corners
        bool hasAny = false;
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (var kv in _nodeUIs)
        {
            var rt = kv.Value.GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
            {
                Vector3 local = contentRt.InverseTransformPoint(corners[i]);
                if (!hasAny)
                {
                    min = max = local;
                    hasAny = true;
                }
                else
                {
                    min.x = Mathf.Min(min.x, local.x);
                    min.y = Mathf.Min(min.y, local.y);
                    max.x = Mathf.Max(max.x, local.x);
                    max.y = Mathf.Max(max.y, local.y);
                }
            }
        }

        if (!hasAny) return;

        // Padding around the tree (use configurable left padding so the first node is fully visible)
        float paddingX = Mathf.Max(0f, leftPadding);
        const float paddingY = 64f;

        // Determine required size with extra 300px width buffer
        float width = (max.x - min.x) + paddingX * 2f + 300f;
        float height = (max.y - min.y) + paddingY * 2f;

        // Update Content size first (so its rect reflects the final left/bottom edges)
        var current = contentRt.sizeDelta;
        float newW = Mathf.Max(current.x, width);
        float newH = Mathf.Max(current.y, height);
        contentRt.sizeDelta = new Vector2(newW, newH);

        // Compute left/bottom edges in Content local space
        float leftEdge = -contentRt.sizeDelta.x * 0.5f;
        float bottomEdge = -contentRt.sizeDelta.y * 0.5f;

        // Shift each node so the min corner aligns to (leftEdge + paddingX, bottomEdge + paddingY)
        Vector2 offset = new Vector2((leftEdge + paddingX) - min.x, (bottomEdge + paddingY) - min.y);
        foreach (var kv in _nodeUIs)
        {
            var rt = kv.Value != null ? kv.Value.GetComponent<RectTransform>() : null;
            if (rt == null) continue;
            rt.anchoredPosition += offset;
        }

        // Ensure edges container matches content area so lines can span across
        if (edgesContainer != null)
        {
            var er = edgesContainer;
            er.sizeDelta = contentRt.sizeDelta;
        }

        // Auto-scroll ScrollRect (if any) to the far left
        Canvas.ForceUpdateCanvases();
        var scroll = contentRt.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            // Ensure horizontal scroll left; keep vertical position unchanged
            scroll.horizontalNormalizedPosition = 0f;
        }
    }


    public void Show()
    {
        Clear();
        Build(BranchingVideoGameManager.GameData);
    }

    public void Hide()
    {
        
    }
    

    public void BackToVideo()
    {
        this.gameObject.SetActive(false);
        VideoPlayerController.Instance.ResumeVideo();
    }
}
