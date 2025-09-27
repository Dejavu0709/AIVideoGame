using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Playback rate script
/// </summary>
public class PlaybackRate : MonoBehaviour {
    public bool isSaveToPrefs = true; // Is need to save to PlayerPrefs (Set from main script)
    public VideoManager videoManager; // Reference to video manager
    public TextMeshProUGUI playbackRateText; // Reference to playback speed text

    public float[] allowedRates; // Allowed playback speeds (Set from main script)

    int currentRate; // Current playback speed

    /// <summary>
    /// Initialize playback rate script
    /// </summary>
    public void Init() {
        if (isSaveToPrefs) {
            if (PlayerPrefs.HasKey("VideoPlaybackSpeed")) {
                currentRate = PlayerPrefs.GetInt("VideoPlaybackSpeed");
            } else {
                PlayerPrefs.SetInt("VideoPlaybackSpeed", 0);
                currentRate = 0;
            }
        } else {
            currentRate = 0;
        }
        SetPlaybackSpeed();
    }

    /// <summary>
    /// Change playback speed
    /// </summary>
    public void ChangeSpeed() {
        currentRate = (currentRate + 1) % allowedRates.Length;
        if (isSaveToPrefs) {
            PlayerPrefs.SetInt("VideoPlaybackSpeed", currentRate);
        }
        SetPlaybackSpeed();
    }

    /// <summary>
    /// Set playback speed to video manager and text
    /// </summary>
    public void SetPlaybackSpeed() {
        videoManager.SetPlaybackSpeed(allowedRates[currentRate]);
        playbackRateText.text = "x" + allowedRates[currentRate];
    }
}
