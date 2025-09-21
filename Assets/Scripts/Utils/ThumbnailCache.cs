using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
public static class ThumbnailCache
{
        // Cache directory under persistent data path
        private static string CacheDir => Path.Combine(Application.persistentDataPath, "ThumbnailCache");

        // Build full path helpers
        private static string StreamingFilePath(string streamingSubfolder, string fileName)
        {
            if (string.IsNullOrEmpty(streamingSubfolder))
                return Path.Combine(Application.streamingAssetsPath, fileName);
            return Path.Combine(Application.streamingAssetsPath, streamingSubfolder, fileName);
        }

        private static string CacheFilePath(string fileName)
        {
            return Path.Combine(CacheDir, fileName);
        }

        public static bool TryLoadLocal(string nodeId, string streamingSubfolder, out Sprite sprite)
        {
            sprite = null;
            string fileName = nodeId + ".png";

            // 1) Check StreamingAssets
            string streamingPath = StreamingFilePath(streamingSubfolder, fileName);
            bool streamingIsUri = streamingPath.Contains("://") || streamingPath.Contains(":///");
            if (!streamingIsUri && File.Exists(streamingPath))
            {
                if (TryLoadSpriteFromFile(streamingPath, out sprite))
                    return true;
            }

            // 2) Check Cache
            string cachedPath = CacheFilePath(fileName);
            if (File.Exists(cachedPath))
            {
                if (TryLoadSpriteFromFile(cachedPath, out sprite))
                    return true;
            }

            return false;
        }

        public static IEnumerator LoadOrDownload(string nodeId, string cdnBase, string streamingSubfolder, Action<Sprite> onLoaded)
        {
            // Try local first (StreamingAssets -> Cache)
            if (TryLoadLocal(nodeId, streamingSubfolder, out var localSprite))
            {
                onLoaded?.Invoke(localSprite);
                yield break;
            }

            // If StreamingAssets path is not a regular file (e.g., Android/WebGL), try to load it via UnityWebRequest
            string fileName = nodeId + ".png";
            string streamingPath = StreamingFilePath(streamingSubfolder, fileName);
            bool streamingIsUri = streamingPath.Contains("://") || streamingPath.Contains(":///");
            if (streamingIsUri)
            {
                using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(streamingPath))
                {
                    yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                    if (req.result == UnityWebRequest.Result.Success)
#else
                    if (!(req.isNetworkError || req.isHttpError))
#endif
                    {
                        var tex = DownloadHandlerTexture.GetContent(req);
                        if (tex != null)
                        {
                            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                            onLoaded?.Invoke(sprite);
                            yield break;
                        }
                    }
                }
            }

            // 3) Download from CDN if available
            if (!string.IsNullOrEmpty(cdnBase))
            {
                string url = cdnBase.EndsWith("/") ? (cdnBase + nodeId + ".png") : ($"{cdnBase}/{nodeId}.png");
                using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                    if (req.result != UnityWebRequest.Result.Success)
#else
                    if (req.isNetworkError || req.isHttpError)
#endif
                    {
                        Debug.LogWarning($"Thumbnail download failed for '{nodeId}': {req.error}");
                        yield break;
                    }

                    var tex = DownloadHandlerTexture.GetContent(req);
                    if (tex != null)
                    {
                        // Save to cache
                        try
                        {
                            Directory.CreateDirectory(CacheDir);
                            byte[] png = tex.EncodeToPNG();
                            File.WriteAllBytes(CacheFilePath(nodeId + ".png"), png);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to write thumbnail cache for '{nodeId}': {e.Message}");
                        }

                        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        onLoaded?.Invoke(sprite);
                        yield break;
                    }
                }
            }
        }

        private static bool TryLoadSpriteFromFile(string path, out Sprite sprite)
        {
            sprite = null;
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load sprite from '{path}': {e.Message}");
            }
            return false;
        }
}
