using System;
using UnityEngine;
using UnityEngine.Events;
using NexgenDragon;
using com.ootii.Messages;
using HISPlayer;

public class VideoPlayerController : MonoSingleton<VideoPlayerController>
{
    [Header("Events")]
    public UnityEvent OnVideoFinished;
    public UnityEvent OnVideoStarted;
    
#if ADV_PLAYER
    public VideoManager videoManager;
#endif
    public HISVideoPlayerManager hisPlayerController;

    public bool IsLocalVideo;







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
        Debug.Log("Video ended");
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
        //videoUrl = "asdasadsads";
        // Stop current video if playing
        if (isVideoPlaying)
        {
            Debug.Log("Stop current video if playing");
            #if ADV_PLAYER
            videoManager.Pause();
            #else
            hisPlayerController.PauseVideo();
            #endif
        }
        #if ADV_PLAYER
        videoManager.InitVideo(videoUrl);
        #else
        hisPlayerController.PlayVideo(videoUrl);
        #endif
        isVideoPlaying = true;
    }
    
    public void StopVideo()
    {
        if (isVideoPlaying)
        {
            #if ADV_PLAYER
            videoManager.Pause();
            #else
            hisPlayerController.CloseVideo();
            #endif
            isVideoPlaying = false;
        }
    }
    
    public void PauseVideo()
    {
        Debug.Log("PauseVideo");
        if (isVideoPlaying)
        {
            #if ADV_PLAYER
            videoManager.Pause();
            #else
            hisPlayerController.PauseVideo();
            #endif
            isVideoPlaying = false;
        }
    }
    
    public void ResumeVideo()
    {
        Debug.Log("ResumeVideo");
        if (!isVideoPlaying)
        {
            #if ADV_PLAYER
            videoManager.Play();
            #else
            hisPlayerController.ResumeVideo();
            #endif
            isVideoPlaying = true;
        }
    }
    
    public bool IsVideoPlaying()
    {
        #if ADV_PLAYER
        return isVideoPlaying && videoManager != null;
        #endif
        return isVideoPlaying;
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
