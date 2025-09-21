using System;
using BestHTTP;
using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;

namespace NexgenDragon.BestHTTP
{
    public partial class ImageSetter : MonoBehaviour
    {
        private Image _image;
        public Image image => object.ReferenceEquals(_image, null) ? (_image = GetComponent<Image>()) : _image;

        public bool disableImageWhenLoading
        {
            get;
            private set;
        }

        private Sprite _originSprite;
        public Sprite originSprite
        {
            get => _originSprite;
            set => _originSprite = value;
        }

        private Sprite _responseSprite;
        public Sprite responseSprite => _responseSprite;
        
        private HTTPRequest _request;

        private long _requestDownloaded;
        private long _requestDownloadLength;
        
        public float downloadProgress
        {
            get
            {
                if (_request == null)
                    return 0f;
                return _requestDownloadLength > 0 ? (float)_requestDownloaded / (float)_requestDownloadLength : 0f;
            }
        }

        [SerializeField]
        private bool _setNativeSize;
        public bool setNativeSize
        {
            get => _setNativeSize;
            set
            {
                _setNativeSize = value;
                if (_setNativeSize 
                    && _image)
                {
                    _image.SetNativeSize();
                }
            }
        }

        private void Awake()
        {
            if (image)
            {
                _originSprite = _image.sprite;
            }
        }

        private void OnDestroy()
        {
            CancelDownloadImageThenSetTexture();
            
            _originSprite = null;
        }

        public void CancelDownloadImageThenSetTexture()
        {
            if (_request != null)
                DoDestroyHttpRequest();

            if (image)
            {
                SpriteManager.Instance.UnLoadSprite(_image);
                _image = null;
            }
            
            _responseSprite = null;
            _requestDownloaded = _requestDownloadLength = 0L;
        }

        public void SetOriginSprite()
            => DownloadImageThenSetTexture(null, null, 10f, false);
        
        public void SetLocalSprite(string spriteName)
            => DownloadImageThenSetTexture(null, spriteName, 10f, false);

        public void SetRemoteSprite(string remotePath, string fallbackLocalSpriteName = null)
        {
            Uri uri;
            if (remotePath.StartsWith("http:", StringComparison.Ordinal))
                uri = new Uri(remotePath);
            else
                uri = BestHTTPUtils.GenUri(remotePath);
            DownloadImageThenSetTexture(uri, fallbackLocalSpriteName);
        }

        public void DownloadImageThenSetTexture(Uri uri, string fallbackSpriteName = null, float requestTimeout = 10f, bool disableImageWhenLoading = true)
        {
            if (uri != null)
            {
                var spritePool = SpritePoolContainer.GetObjectPool(uri.OriginalString, false);
                if (spritePool)
                {
                    var cachedSprite = spritePool.presetAsset as Sprite;
                    if (cachedSprite)
                    {
                        if (image)
                        {
                            DoImageSetSprite(cachedSprite);
                            SpritePoolContainer.AddPoolNameLRU(uri.OriginalString);
                            return;
                        }
                    }
                }
            }

            this.disableImageWhenLoading = disableImageWhenLoading;
            if (disableImageWhenLoading)
            {
                if (_image)
                    _image.enabled = false;
            }
            
            if (_request != null)
            {
                if (_request.Uri == uri && uri != null)
                {
                    if (_request.State == HTTPRequestStates.Finished)
                    {
                        var response = _request.Response;
                        if (response != null && response.IsSuccess)
                        {
                            DoUpdateGraphicWithRespondData();
                            return;
                        }
                    }
                }
            }

            DoDestroyHttpRequest();
            SpriteManager.Instance.UnLoadSprite(_image);

            if (uri != null)
            {
                _request = DoCreateHttpRequest(uri, requestTimeout);
                _request.Tag = new RequestTag
                {
                    downloadThenSet = true,
                    fallbackSpriteName = fallbackSpriteName,
                };
                _request.Send();
                return;
            }

            if (string.IsNullOrEmpty(fallbackSpriteName))
                DoUpdateGraphicWithOriginSprite();
            else
                DoUpdateGraphicWithFallbackSprite(fallbackSpriteName);
        }

        HTTPRequest DoCreateHttpRequest(Uri uri, float requestTimeout)
        {
            var request = new HTTPRequest(uri, HTTPMethods.Get, HTTPManager.KeepAliveDefaultValue, false, OnImageDownloaded);
            if (requestTimeout > 0f)
                request.Timeout = TimeSpan.FromSeconds(requestTimeout);
            request.OnDownloadProgress = OnDownloadProgress;
            return request;
        }

        void DoDestroyHttpRequest()
        {
            if (_responseSprite)
                _responseSprite = null;

            if (_request != null)
            {
                if (_request.Tag != null)
                {
                    _request.Tag = null;
                }

                _request.OnDownloadProgress = null;
                _request.Abort();
                _request = null;
            }
        }

        void OnImageDownloaded(HTTPRequest req, HTTPResponse resp)
        {
            if (!(req.Tag is RequestTag requestTag))
                return;

            if (req.State == HTTPRequestStates.Finished)
            {
                if (resp.IsSuccess)
                {
                    if (!requestTag.downloadThenSet)
                        return;
                    if (DoUpdateGraphicWithRespondData())
                        return;
                }
                else
                {
                    Debug.LogWarning(string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                        resp.StatusCode,
                        resp.Message,
                        resp.DataAsText));
                }
            }
            
            DoUpdateGraphicWithFallbackSprite(requestTag.fallbackSpriteName);

            switch (req.State)
            {
                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogWarning("Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("Request Aborted!");
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogWarning("Connection Timed Out!");
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogWarning("Processing the request Timed Out!");
                    break;
            }
        }

        void OnDownloadProgress(HTTPRequest originalRequest, long downloaded, long downloadLength)
        {
            if (originalRequest == _request)
            {
                _requestDownloaded = downloaded;
                _requestDownloadLength = downloadLength;
            }
        }

        Sprite DoCreateResponseSprite(HTTPResponse response)
        {
            var texture = response.DataAsTexture2D;
            return Sprite.Create(texture, Rect.MinMaxRect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        bool DoUpdateGraphicWithRespondData()
        {
            if (!image)
                return false;

            if (_request == null || _request.State != HTTPRequestStates.Finished)
                return false;

            var response = _request.Response;
            if (response == null || !response.IsSuccess)
                return false;

            if (_responseSprite == null)
            {
                _responseSprite = DoCreateResponseSprite(response);
                if (_responseSprite)
                {
                    var poolName = _request.Uri.OriginalString;
                    SpritePoolContainer.AddObjectPool(poolName, _responseSprite);
                    SpritePoolContainer.AddPoolNameLRU(poolName);
                }
            }
            DoImageSetSprite(_responseSprite);
            if (this.disableImageWhenLoading)
                _image.enabled = true;
            return true;
        }

        bool DoUpdateGraphicWithOriginSprite()
        {
            if (!image)
                return false;
            DoImageSetSprite(originSprite);
            if (this.disableImageWhenLoading)
                _image.enabled = true;
            return true;
        }

        bool DoUpdateGraphicWithFallbackSprite(string fallbackSpriteName)
        {
            if (!string.IsNullOrEmpty(fallbackSpriteName) && image)
            {
                if (transform.TryGetComponent(out SpriteReference spriteReference)
                    && spriteReference.targetSpriteName == fallbackSpriteName)
                {
                    // 已经加载过重复的Sprite, 避免多次UnLoadSprite/LoadSprite.
                    return true;
                }
                
                SpriteManager.Instance.UnLoadSprite(_image);
                Action<Sprite> callback = null;
                if (this.disableImageWhenLoading)
                {
                    callback = (Sprite theSprite) =>
                    {
                        if (this && this.disableImageWhenLoading)
                        {
                            if (_image)
                                _image.enabled = true;
                        }
                    };
                }
                SpriteManager.Instance.LoadSprite(fallbackSpriteName, _image, "sp_icon_missing_2", callback);
                return true;
            }
            return false;
        }

        void DoImageSetSprite(Sprite sprite)
        {
            if (_image)
            {
                _image.sprite = sprite;
                if (_setNativeSize)
                {
                    _setNativeSize = true;
                }
            }
        }
    }
}