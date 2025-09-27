using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Main script to set up the video player
/// </summary>
public class AdvancedVideoManager : MonoBehaviour
{
    public VideoClip templateVideo; //Video clip that uses in video manager
    public string templateUrl; //Video url that uses in video manager
    public bool isLocalVideo = true; //Is use video clip or video from url

    public bool isLoop = false; //Is video should be looping

    public bool needToClickToShowInterface = false; //Is need to click on video to show an interface with the buttons
    public bool neverHideInterfaceWhileHover = false; //Is never hide interface while mouse is hovering the video
    public bool saveVolumeInPlayerPrefs = true; //Is need to save volume settings in PlayerPrefs
    public bool savePlaybackSpeedInPlayerPrefs = true; //Is need to save playback speed settings in PlayerPrefs

    public int masterVolume = 100; //Master volume (0% - 100%)

    public int volumeStepOffset = 5; //Volume step by pressing Up/Down arrows (0% - 100%)
    public int timeStepOffset = 10; //Time step by pressing Left/Right arrows or double pressing on video Left/Right sides (0 seconds - infinity seconds)

    public float[] allowedPlaybackSpeeds = new float[4] { 1, 1.5f, 2f, 0.5f }; //Allowed playback speed list

    public VideoManager videoManager;
    public VolumeScript volumeManager;
    public PlaybackRate playbackSpeedManager;

    /// <summary>
    /// Start method
    /// </summary>
    private void Awake() {
        //videoManager = transform.GetComponentInChildren<VideoManager>();
        //volumeManager = transform.GetComponentInChildren<VolumeScript>();
       // playbackSpeedManager = transform.GetComponentInChildren<PlaybackRate>();


        InitAllSettings();
    }

    /// <summary>
    /// Save settings and initialize all scripts
    /// </summary>
    public void InitAllSettings() {
        volumeManager.isSaveToPrefs = saveVolumeInPlayerPrefs;
        playbackSpeedManager.isSaveToPrefs = savePlaybackSpeedInPlayerPrefs;
        playbackSpeedManager.allowedRates = allowedPlaybackSpeeds;
        //videoManager.InitVideo(templateUrl);
        playbackSpeedManager.Init();
        volumeManager.Init();
    }

    /// <summary>
    /// Load video clip
    /// </summary>
    /// <param name="targetClip">Target video clip</param>
    public void LoadVideoClip(VideoClip targetClip) {
        templateVideo = targetClip;
        isLocalVideo = true;
        InitAllSettings();
    }

    /// <summary>
    /// Load video from url
    /// </summary>
    /// <param name="targetUrl">Target url (local or http/https)</param>
    public void LoadVideoFromUrl(string targetUrl) {
        templateUrl = targetUrl;
        isLocalVideo = false;
        InitAllSettings();
    }
}
