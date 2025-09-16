using System;
using UnityEngine;
using UnityEngine.Events;
using RenderHeads.Media.AVProVideo;

public class VideoPlayerController : MonoBehaviour
{
    [Header("AVPro Video Components")]
    public MediaPlayer mediaPlayer;
    public DisplayUGUI displayUGUI;
    
    [Header("Events")]
    public UnityEvent OnVideoFinished;
    public UnityEvent OnVideoStarted;
    
    private bool isVideoPlaying = false;
    private string currentVideoUrl = "";
    
    void Start()
    {
        if (mediaPlayer == null)
            mediaPlayer = GetComponent<MediaPlayer>();
            
        if (mediaPlayer != null)
        {
            mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
        }
    }
    
    void OnDestroy()
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
        }
    }
    
    public void PlayVideo(string videoUrl)
    {
        if (mediaPlayer == null)
        {
            Debug.LogError("MediaPlayer is not assigned!");
            return;
        }
        
        currentVideoUrl = videoUrl;
        Debug.Log($"Playing video: {videoUrl}");
        
        // Stop current video if playing
        if (isVideoPlaying)
        {
            mediaPlayer.Stop();
        }
        
        // Load and play new video
        if (mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, videoUrl, true))
        {
            isVideoPlaying = true;
            OnVideoStarted?.Invoke();
        }
        else
        {
            Debug.LogError($"Failed to load video: {videoUrl}");
        }
    }
    
    public void StopVideo()
    {
        if (mediaPlayer != null && isVideoPlaying)
        {
            mediaPlayer.Stop();
            isVideoPlaying = false;
        }
    }
    
    public void PauseVideo()
    {
        if (mediaPlayer != null && isVideoPlaying)
        {
            mediaPlayer.Pause();
            isVideoPlaying = false;
        }
    }
    
    public void ResumeVideo()
    {
        if (mediaPlayer != null && !isVideoPlaying)
        {
            mediaPlayer.Play();
        }
    }
    
    public bool IsVideoPlaying()
    {
        return isVideoPlaying && mediaPlayer != null;
    }
    
    public float GetVideoProgress()
    {
        if (mediaPlayer != null && mediaPlayer.Info != null)
        {
            return (float)(mediaPlayer.Control.GetCurrentTime() / mediaPlayer.Info.GetDuration());
        }
        return 0f;
    }
    
    private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                Debug.Log("Video ready to play");
                break;
                
            case MediaPlayerEvent.EventType.Started:
                Debug.Log("Video started");
                isVideoPlaying = true;
                break;
                
            case MediaPlayerEvent.EventType.FinishedPlaying:
                Debug.Log("Video finished playing");
                isVideoPlaying = false;
                OnVideoFinished?.Invoke();
                break;
                
            case MediaPlayerEvent.EventType.Error:
                Debug.LogError($"Video player error: {errorCode}");
                isVideoPlaying = false;
                break;
                
            case MediaPlayerEvent.EventType.Stalled:
                Debug.LogWarning("Video playback stalled");
                break;
        }
    }
}
