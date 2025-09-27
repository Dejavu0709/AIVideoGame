using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Time slider animator
/// </summary>
public class SliderAnimator : MonoBehaviour, IPointerDownHandler {
    public Animator targetAnimator; // Reference to an interface animator
    bool isPointerDown; // Is pressing slider

    public VideoManager videoManager; // Reference to a video manager
    public bool isVideoPlaying; // Is video playing

    /// <summary>
    /// Update method
    /// </summary>
    private void Update() {
        if (Input.GetMouseButtonUp(0)) {
            if (isPointerDown) {
                targetAnimator.SetTrigger("StartHoldingOpenAgain");
                isPointerDown = false;
                targetAnimator.speed = 1;
                videoManager.isSliderDown = false;
                videoManager.WaitUntilLoading();
                if (isVideoPlaying && !videoManager.IsVideoEnded()) {
                    videoManager.PlayForSlider();
                }
            }
        }
    }

    /// <summary>
    /// Is pointer down on slider
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData) {
        isPointerDown = true;
        targetAnimator.speed = 0;
        isVideoPlaying = videoManager.isPlaying;
        videoManager.isSliderDown = true;
        if (isVideoPlaying) {
            videoManager.PauseForSlider();
            videoManager.replayButton.SetActive(false);
            videoManager.playButton.SetActive(false);
            videoManager.stopButton.SetActive(true);
        } else {
            videoManager.replayButton.SetActive(false);
            videoManager.playButton.SetActive(true);
            videoManager.stopButton.SetActive(false);
        }
    }
}
