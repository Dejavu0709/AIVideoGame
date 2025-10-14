using System;
using UnityEngine;
using UnityEngine.Events;
using NexgenDragon;
using com.ootii.Messages;

public class VideoPlayerController : MonoSingleton<VideoPlayerController>
{
    [Header("Events")]
    public UnityEvent OnVideoFinished;
    public UnityEvent OnVideoStarted;
    

    public VideoManager videoManager;
    private bool isVideoPlaying = false;
    private string currentVideoUrl = "";

    void OnEnable()
    {
        MessageDispatcher.AddListener("VideoEnded", OnVideoEnded);
        MessageDispatcher.AddListener("VideoStarted", OnVideoStart);
    }

    void OnDisable()
    {
        MessageDispatcher.RemoveListener("VideoEnded", OnVideoEnded);
        MessageDispatcher.RemoveListener("VideoStarted", OnVideoStart);
    }

    
    void Start()
    {

            
    }
    
    private void OnVideoEnded(IMessage message)
    {
        isVideoPlaying = false;
        OnVideoFinished?.Invoke();
    }

    private void OnVideoStart(IMessage message)
    {
        isVideoPlaying = true;
        OnVideoStarted?.Invoke();
    }
    
    public void PlayVideo(string videoUrl)
    {
        currentVideoUrl = videoUrl;
        Debug.Log($"Playing video: {videoUrl}");

        // Stop current video if playing
        if (isVideoPlaying)
        {
            videoManager.Pause();
        }
        videoManager.InitVideo(videoUrl);
    }
    
    public void StopVideo()
    {
        if (isVideoPlaying)
        {
            videoManager.Pause();
            isVideoPlaying = false;
        }
    }
    
    public void PauseVideo()
    {
        Debug.Log("PauseVideo");
        if (isVideoPlaying)
        {
            videoManager.Pause();
            isVideoPlaying = false;
        }
    }
    
    public void ResumeVideo()
    {
        if (!isVideoPlaying)
        {
            videoManager.Play();
        }
    }
    
    public bool IsVideoPlaying()
    {
        return isVideoPlaying && videoManager != null;
    }
    
    /*
    private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                Debug.Log("Video ready to play");
                break;
                
            case MediaPlayerEvent.EventType.Started:
                Debug.Log("Video started");
                OnVideoStarted?.Invoke();
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
    */
}
