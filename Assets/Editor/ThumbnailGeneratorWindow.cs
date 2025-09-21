#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ThumbnailGeneratorWindow : EditorWindow
{
    private string jsonInput = "";
    private string outputSubfolder = "Thumbnails"; // under StreamingAssets
    private bool preferDownloadFromCdn = true;
    private bool overwriteExisting = true;
    private int textureWidth = 512;
    private int textureHeight = 288;

    [MenuItem("Tools/Thumbnails/Generate From JSON...")]
    public static void Open()
    {
        GetWindow<ThumbnailGeneratorWindow>(true, "Generate Thumbnails", true);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Generate/Download Thumbnails To StreamingAssets", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("JSON (GameData)");
        jsonInput = EditorGUILayout.TextArea(jsonInput, GUILayout.MinHeight(160));

        outputSubfolder = EditorGUILayout.TextField(new GUIContent("Streaming subfolder", "Relative folder under StreamingAssets to save images"), outputSubfolder);
        preferDownloadFromCdn = EditorGUILayout.Toggle(new GUIContent("Try CDN download first", "If true, try to download <cdnBase>/<nodeId>.png, else generate colored placeholder."), preferDownloadFromCdn);
        overwriteExisting = EditorGUILayout.Toggle(new GUIContent("Overwrite existing files", "If false, existing files will be kept."), overwriteExisting);

        EditorGUILayout.BeginHorizontal();
        textureWidth = EditorGUILayout.IntField(new GUIContent("Width"), textureWidth);
        textureHeight = EditorGUILayout.IntField(new GUIContent("Height"), textureHeight);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Process JSON and Create Thumbnails", GUILayout.Height(32)))
        {
            ProcessJson();
        }
    }

    private void ProcessJson()
    {
        if (string.IsNullOrWhiteSpace(jsonInput))
        {
            EditorUtility.DisplayDialog("Error", "Please paste a valid GameData JSON.", "OK");
            return;
        }

        GameData data = null;
        try
        {
            data = JsonUtility.FromJson<GameData>(jsonInput);
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Parse Error", "Failed to parse JSON: " + e.Message, "OK");
            return;
        }

        if (data == null || data.nodes == null || data.nodes.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No nodes found in JSON.", "OK");
            return;
        }

        string streamingRoot = Application.streamingAssetsPath;
        if (string.IsNullOrEmpty(streamingRoot))
        {
            streamingRoot = Path.Combine(Application.dataPath, "StreamingAssets");
        }
        string outDir = string.IsNullOrEmpty(outputSubfolder) ? streamingRoot : Path.Combine(streamingRoot, outputSubfolder);
        Directory.CreateDirectory(outDir);

        try
        {
            int total = data.nodes.Count;
            for (int i = 0; i < total; i++)
            {
                var node = data.nodes[i];
                if (node == null || string.IsNullOrEmpty(node.id)) continue;

                string outPath = Path.Combine(outDir, node.id + ".png");
                if (!overwriteExisting && File.Exists(outPath))
                {
                    EditorUtility.DisplayProgressBar("Thumbnails", $"Skip existing: {node.id}.png", (float)(i + 1) / total);
                    continue;
                }

                bool saved = false;
                if (preferDownloadFromCdn && data.meta != null && !string.IsNullOrEmpty(data.meta.cdnBase))
                {
                    saved = TryDownload(data.meta.cdnBase, node.id, outPath, (float)i / total, (float)(i + 1) / total);
                }

                if (!saved)
                {
                    saved = GeneratePlaceholder(node.id, outPath, textureWidth, textureHeight);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Done", "Thumbnail generation complete.", "OK");
    }

    private bool TryDownload(string cdnBase, string nodeId, string outPath, float progressStart, float progressEnd)
    {
        string url = cdnBase.EndsWith("/") ? (cdnBase + nodeId + ".png") : ($"{cdnBase}/{nodeId}.png");
        try
        {
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
            {
                var op = req.SendWebRequest();
                while (!op.isDone)
                {
                    float t = progressStart + (progressEnd - progressStart) * op.progress;
                    EditorUtility.DisplayProgressBar("Downloading", $"{nodeId}.png\n{url}", Mathf.Clamp01(t));
                    System.Threading.Thread.Sleep(10);
                }
#if UNITY_2020_2_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    Debug.LogWarning($"Download failed for {nodeId}: {req.error} ({url})");
                    return false;
                }

                var tex = DownloadHandlerTexture.GetContent(req);
                if (tex == null)
                {
                    Debug.LogWarning($"No texture for {nodeId} from {url}");
                    return false;
                }

                byte[] png = tex.EncodeToPNG();
                File.WriteAllBytes(outPath, png);
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Exception downloading {nodeId} from {url}: {e.Message}");
            return false;
        }
    }

    private bool GeneratePlaceholder(string nodeId, string outPath, int width, int height)
    {
        try
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            // Background color derived from nodeId hash for variety
            var color = HashColor(nodeId);
            var pixels = new Color32[width * height];
            var c32 = (Color32)color;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
            tex.SetPixels32(pixels);

            // Add a simple diagonal stripe pattern for visibility
            var stripe = new Color32(255, 255, 255, 32);
            for (int y = 0; y < height; y += 8)
            {
                for (int x = 0; x < width; x++)
                {
                    int sx = (x + y) % 16;
                    if (sx < 2)
                        tex.SetPixel(x, y, stripe);
                }
            }

            tex.Apply();
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to generate placeholder for {nodeId}: {e.Message}");
            return false;
        }
    }

    private static Color HashColor(string s)
    {
        unchecked
        {
            int h = 23;
            for (int i = 0; i < s.Length; i++) h = h * 31 + s[i];
            float r = ((h >> 0) & 255) / 255f;
            float g = ((h >> 8) & 255) / 255f;
            float b = ((h >> 16) & 255) / 255f;
            // brighten a bit
            r = 0.25f + 0.75f * r;
            g = 0.25f + 0.75f * g;
            b = 0.25f + 0.75f * b;
            return new Color(r, g, b, 1f);
        }
    }
}
#endif
