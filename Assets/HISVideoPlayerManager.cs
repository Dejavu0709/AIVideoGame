using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HISPlayerAPI;
using com.ootii.Messages;

public class HISVideoPlayerManager : HISPlayerManager
{
    private bool isVideoPlaying = false;
    protected override void Awake()
    {
        base.Awake();
        SetUpPlayer();
    }
    
    // Start is called before the first frame update
    void Start()
    {

    }
    public void PlayVideo(string url = null)
    {
        Debug.Log("PlayVideo LoopPlayback" + multiStreamProperties[0].LoopPlayback);
        string targetUrl = url;
        if (!string.IsNullOrEmpty(targetUrl))
        {
            if (multiStreamProperties[0].url.Count == 0)
            {
                AddVideoContent(0, targetUrl);
            }
            else
            {
                ChangeVideoContent(0, targetUrl);
            }
        }

        Play(0);
    }

    public void PauseVideo()
    {
        Pause(0);
    }

    public void ResumeVideo()
    {
        Debug.Log("ResumeVideo");
        Play(0);
    }

    public void CloseVideo()
    {
        Stop(0);
    }

    public bool IsVideoPlaying()
    {
        return isVideoPlaying;
    }

    public void playvideo(string url = null)
    {
        PlayVideo(url);
    }

    public void pausevideo()
    {
        PauseVideo();
    }

    public void resumevideo()
    {
        ResumeVideo();
    }

    public void closevideo()
    {
        CloseVideo();
    }

    protected override void EventPlaybackPlay(HISPlayerEventInfo eventInfo)
    {
        Debug.Log("EventPlaybackPlay " + eventInfo);
        base.EventPlaybackPlay(eventInfo);
        if (isVideoPlaying == false)
        {
            Debug.Log("EventPlaybackPlay");
            isVideoPlaying = true;
            MessageDispatcher.SendMessage("VideoStarted");
           // Play(0);
        }
    }

    protected override void EventPlaybackPause(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackPause(eventInfo);
        Debug.Log("EventPlaybackPause");
       // Play(0);
        isVideoPlaying = false;
    }

    protected override void EventPlaybackReady(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackReady(eventInfo);
        Debug.Log("EventPlaybackReady");
         //Play(0);
         SetVolume(0,1);
    }

    protected override void EventPlaybackStop(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackStop(eventInfo);
        Debug.Log("EventPlaybackStop");
    }

   protected override void EventEndOfContent(HISPlayerEventInfo eventInfo)
    {
        Debug.Log("EventEndOfContent");
        base.EventEndOfContent(eventInfo);
        //if (isVideoPlaying)
        {
            isVideoPlaying = false;
            MessageDispatcher.SendMessage("VideoEnded");
        }
    }
    protected override void EventEndOfPlaylist(HISPlayerEventInfo eventInfo)
    {
        Debug.Log("EventEndOfPlaylist");
        base.EventEndOfPlaylist(eventInfo);

    }

    protected override void EventPlaybackSeek(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackSeek(eventInfo);
    }

    protected override void EventPlaybackBuffering(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackBuffering(eventInfo);
    }

    protected override void EventOnTrackChange(HISPlayerEventInfo eventInfo)
    {
        base.EventOnTrackChange(eventInfo);
    }

    protected override void EventVideoSizeChange(HISPlayerEventInfo eventInfo)
    {
        base.EventVideoSizeChange(eventInfo);
    }

    protected override void ErrorInfo(HISPlayerErrorInfo errorInfo)
    {
        base.ErrorInfo(errorInfo);
    }

    protected override void OnDestroy()
    {
        Release();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Returns current playback position in seconds for the primary stream (index 0)
    public float GetCurrentTimeSeconds()
    {
        // base.GetVideoPosition returns milliseconds
        return GetVideoPosition(0) / 1000f;
    }
}
