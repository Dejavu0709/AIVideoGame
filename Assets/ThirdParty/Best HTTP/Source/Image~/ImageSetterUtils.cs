using System;
using UnityEngine.UI;

namespace NexgenDragon.BestHTTP
{
    public static class ImageSetterUtils
    {
        public static ImageSetter CancelDownloadImageThenSetTexture(this Image image)
        {
            if (!image)
                return null;
            
            var imageSetter = image.gameObject.GetComponent<ImageSetter>();
            if (imageSetter)
            {
                imageSetter.CancelDownloadImageThenSetTexture();
            }
            else
            {
                SpriteManager.Instance.UnLoadSprite(image);
            }

            return imageSetter;
        }

        public static ImageSetter DownloadImageThenSetTexture(this Image image, Uri uri, string fallbackSpriteName = null, float requestTimeout = 10f)
        {
            if (!image)
                return null;

            var imageSetter = image.gameObject.GetOrAddComponent<ImageSetter>();

            if (imageSetter)
            {
                imageSetter.DownloadImageThenSetTexture(uri, fallbackSpriteName, requestTimeout);
                return imageSetter;
            }

            return null;
        }

        public static ImageSetter DownloadImageThenSetTexture(this Image image, string remotePath, string fallbackSpriteName = null, float requestTimeout = 10f)
        {
            Uri uri;

            if (string.IsNullOrEmpty(remotePath))
            {
                uri = null;
            }
            else
            {
                if (remotePath.StartsWith("http", StringComparison.Ordinal))
                    uri = new Uri(remotePath);
                else
                    uri = BestHTTPUtils.GenUri(remotePath);
            }
            
            return DownloadImageThenSetTexture(image, uri, fallbackSpriteName, requestTimeout);
        }
        
        public static ImageSetter RequestCustomIcon(this Image image, string remotePath, string fallbackSpriteName = null, float requestTimeout = 10f)
        {
            Uri uri;

            if (string.IsNullOrEmpty(remotePath))
            {
                uri = null;
            }
            else
            {
                var iconServerUrl = CustomIconLoader.Instance.GetIconServerUrl();
                if (remotePath.StartsWith(iconServerUrl) || remotePath.StartsWith("http", StringComparison.Ordinal))
                    uri = new Uri(remotePath);
                else
                    uri = BestHTTPUtils.GenUri(BestHTTPUtils.PathCombine("/assets/icon/", remotePath), CustomIconLoader.Instance.GetIconServerUrl());
            }
            
            return DownloadImageThenSetTexture(image, uri, fallbackSpriteName, requestTimeout);
        }

        public static ImageSetter GetOrAddImageSetter(this Image image)
        {
            return image == null ? null : image.gameObject.GetOrAddComponent<ImageSetter>();
        }
    }
}
