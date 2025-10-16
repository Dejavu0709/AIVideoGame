using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using BestHTTP;
using BestHTTP.Caching;
#if UNITY_WEBGL || WEIXINMINIGAME || PLATFORM_WEIXINMINIGAME
using WeChatWASM;
#endif

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
        #if UNITY_EDITOR
        return null;
        #endif

        string path = WX.env.USER_DATA_PATH;
        Debug.Log("path" + path);
        string cachePath = Path.Combine(path, fileName);
        Debug.Log("cachePath" + cachePath);
        return cachePath;
        //return Path.Combine(CacheDir, fileName);
    }

    public static bool TryLoadLocal(string nodeId, string streamingSubfolder, out Sprite sprite)
    {
        sprite = null;
        // Prefer .png, fallback to .jpg for legacy assets
        string[] candidates = new[] { nodeId + ".png",nodeId + ".jpg" };
        
        // 1) Check StreamingAssets (direct file paths only)
        foreach (var fn in candidates)
        {
            string streamingPath = StreamingFilePath(streamingSubfolder, fn);
            bool streamingIsUri = streamingPath.Contains("://") || streamingPath.Contains(":///");
            Debug.Log("thumbnail streamingPath" + streamingPath + " " + streamingIsUri);
            if (!streamingIsUri && File.Exists(streamingPath))
            {
                if (TryLoadSpriteFromFile(streamingPath, out sprite))
                    return true;
            }
        }

        // 2) Check local cache folder
        foreach (var fn in candidates)
        {
            string cachedPath = CacheFilePath(fn);
            if (File.Exists(cachedPath))
            {
                if (TryLoadSpriteFromFile(cachedPath, out sprite))
                {
                    Debug.Log($"TryLoadLocal sprite for '{nodeId}'" + sprite.name + " " + sprite.texture.name + sprite != null);
                    return true;
                }
            }
        }

        return false;
    }

    public static IEnumerator LoadOrDownload(string videoFileName, string url, string streamingSubfolder, Action<Sprite> onLoaded)
    {
        Debug.Log("videoFileName" + videoFileName);
        // Try local first (StreamingAssets -> Cache)
        if (TryLoadLocal(videoFileName, streamingSubfolder, out var localSprite))
        {
            Debug.Log($"LoadOrDownload local sprite for '{videoFileName}'" + localSprite.name + " " + localSprite.texture.name);
            onLoaded?.Invoke(localSprite);
            yield break;
        }
        Debug.Log($"LoadOrDownload download  for '{url}'");
        /*
        // If StreamingAssets path is not a regular file (e.g., Android/WebGL), try to load it via UnityWebRequest
        string fileName = videoFileName + ".png";
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
                    var texFromSA = DownloadHandlerTexture.GetContent(req);
                    if (texFromSA != null)
                    {
                        var sp = Sprite.Create(texFromSA, new Rect(0, 0, texFromSA.width, texFromSA.height), new Vector2(0.5f, 0.5f));
                        onLoaded?.Invoke(sp);
                        yield break;
                    }
                }
            }
        }
        */
        Debug.Log($"Thumbnail download  for '{url}'");

#if UNITY_WEBGL || WEIXINMINIGAME && !UNITY_EDITOR
        // WeChat Mini Game/WebGL path: explicitly use wx.downloadFile via WeChatWASM.WX.DownloadFile
        if (!string.IsNullOrEmpty(url))
        {
            bool wxDone = false;
            bool wxSuccess = false;
            string wxLocalPath = null;
            try
            {
                var option = new DownloadFileOption()
                {
                    url = url,
                    filePath = CacheFilePath(videoFileName + ".png"),
                    success = (res) =>
                    {
                        Debug.Log($"WX.DownloadFile success for '{url}'" + res.filePath + " local:" + res.tempFilePath);
                        // prefer filePath if provided, otherwise tempFilePath
                        wxLocalPath = res.filePath;
                        Debug.Log("1File downloaded to: "+  WX.GetFileSystemManager().AccessSync(wxLocalPath));
                        Debug.Log("2File downloaded to: "+ File.Exists(res.filePath));
                        wxSuccess = !string.IsNullOrEmpty(wxLocalPath) && res.statusCode == 200;
                        wxDone = true;
                    },
                    fail = (err) =>
                    {
                        Debug.LogWarning($"WX.DownloadFile fail for '{url}': {err.errMsg}");
                        wxDone = true;
                        wxSuccess = false;
                    }
                };

                // Start download task
                var task = WX.DownloadFile(option);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"WX.DownloadFile exception for '{url}': {e.Message}");
            }

            // wait for download to complete
            while (!wxDone)
                yield return null;

            if (wxSuccess && !string.IsNullOrEmpty(wxLocalPath))
            {
                Debug.Log($"string.IsNullOrEmpty(wxLocalPath) WX success for '{url}'" + wxLocalPath);

                //WX.GetFileSystemManager().AccessSync(wxLocalPath);
                //WX.GetFileSystemManager().ReadFileSync(wxLocalPath);

                //if (WX.GetFileSystemManager().AccessSync(wxLocalPath))
                {
                    byte[] fileData = WX.GetFileSystemManager().ReadFileSync(wxLocalPath);
                    
                    Texture2D texture = new Texture2D(2, 2); // �ߴ�ᱻ���ص�ͼƬ����
                    if (texture.LoadImage(fileData)) // �Զ�����ͼƬ���ݣ���JPG, PNG��
                    {
                        var spriteWx = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        onLoaded?.Invoke(spriteWx);
                        Debug.Log("ͼƬ���سɹ���");
                    }
                    else
                    {
                        Debug.LogError("����ͼƬ����ʧ�ܡ�");
                    }
                }
            }
        }
#else
   // 3) Fallback: Download from CDN if available (via BestHTTP with HTTPCache)
            if (!string.IsNullOrEmpty(url))
            {
                //string url = cdnBase.EndsWith("/") ? (cdnBase + nodeId + ".png") : ($"{cdnBase}/{nodeId}.png");
                bool done = false;
                Texture2D downloaded = null;

                // BestHTTP will use its own HTTPCache if supported. No special flags needed for GET.
                var request = new HTTPRequest(new Uri(url), (req, resp) =>
                {
                    try
                    {
                        Debug.Log("resp " + resp.IsSuccess);
                        if (resp != null && resp.IsSuccess && resp.Data != null && resp.Data.Length > 0)
                        {
                            var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                            if (t.LoadImage(resp.Data))
                                downloaded = t;
                            else
                                Debug.LogError($"BestHTTP image decode failed for '{videoFileName}'.");
                        }
                        else
                        {
                            string err = req.Exception != null ? req.Exception.Message : (resp != null ? $"HTTP {resp.StatusCode}" : "No response");
                            Debug.LogError($"Thumbnail download failed for '{videoFileName}': {err}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Exception while processing thumbnail for '{videoFileName}': {e.Message}");
                    }
                    finally { done = true; }
                });

                request.DisableCache = false; // allow HTTPCache
                request.MethodType = HTTPMethods.Get;
                request.Send();

                while (!done)
                    yield return null;

                if (downloaded != null)
                {
                    // Save to our simple disk cache as PNG
                    try
                    {
                        Directory.CreateDirectory(CacheDir);
                        byte[] png = downloaded.EncodeToPNG();
                        File.WriteAllBytes(CacheFilePath(videoFileName + ".png"), png);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to write thumbnail cache for '{videoFileName}': {e.Message}");
                    }

                    var sprite = Sprite.Create(downloaded, new Rect(0, 0, downloaded.width, downloaded.height), new Vector2(0.5f, 0.5f));
                    onLoaded?.Invoke(sprite);
                    yield break;
                }
            }
#endif


     }

    private static bool TryLoadSpriteFromFile(string path, out Sprite sprite)
    {
        sprite = null;
        Debug.Log($"TryLoadSpriteFromFile: {path}");
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(256, 256, TextureFormat.ASTC_6x6, false);
            if (tex.LoadImage(bytes))
            {
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Debug.Log($"TryLoadSpriteFromFile sprite: {sprite} sprite != null: {sprite != null}");
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
