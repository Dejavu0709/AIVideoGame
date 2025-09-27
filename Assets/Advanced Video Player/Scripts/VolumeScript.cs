using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Volume script
/// </summary>
public class VolumeScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isSaveToPrefs = true; // Is need to save to PlayerPrefs (Set from main script)

    public Image[] volumeIcons; // Volume icon list
    public Slider volumeSlider; // Volume slider

    public RectTransform sliderParent; // Slider parent rect
    public VideoManager videoManager; // Reference to a video manager

    float maxVolumeExpand = 225f;
    float minVolumeExpand = 71f;

    public float speedOfLerp = 70; // Speed of expand/shrink slider parent
    public AnimationCurve curveLerp; // Curve for set up a trajectory of expand/shrink

    float targetPos;
    float startPos;

    float ratio;

    RectTransform selfRect;

    /// <summary>
    /// Initialize volume script
    /// </summary>
    public void Init() {
        selfRect = GetComponent<RectTransform>();
        ratio = 0;
        targetPos = minVolumeExpand;
        startPos = selfRect.sizeDelta.x;

        if (isSaveToPrefs) {
            if (PlayerPrefs.HasKey("VideoVolume")) {
                volumeSlider.value = PlayerPrefs.GetInt("VideoVolume");
                ChangeVolume();
            } else {
                PlayerPrefs.SetInt("VideoVolume", 50);
                volumeSlider.value = 50;
                ChangeVolume();
            }
        } else {
            volumeSlider.value = 50;
            ChangeVolume();
        }
    }

    /// <summary>
    /// Change volume when slider value change
    /// </summary>
    public void ChangeVolume() {
        if (isSaveToPrefs) {
            PlayerPrefs.SetInt("VideoVolume", (int)volumeSlider.value);
        }
        videoManager.SetVolume((int)volumeSlider.value);
        if(volumeSlider.value < 1) {
            volumeIcons[0].gameObject.SetActive(true);
            volumeIcons[1].gameObject.SetActive(false);
            volumeIcons[2].gameObject.SetActive(false);
            volumeIcons[3].gameObject.SetActive(false);
        } else if(volumeSlider.value < 40) {
            volumeIcons[0].gameObject.SetActive(false);
            volumeIcons[1].gameObject.SetActive(true);
            volumeIcons[2].gameObject.SetActive(false);
            volumeIcons[3].gameObject.SetActive(false);
        }else if(volumeSlider.value < 80) {
            volumeIcons[0].gameObject.SetActive(false);
            volumeIcons[1].gameObject.SetActive(false);
            volumeIcons[2].gameObject.SetActive(true);
            volumeIcons[3].gameObject.SetActive(false);
        } else {
            volumeIcons[0].gameObject.SetActive(false);
            volumeIcons[1].gameObject.SetActive(false);
            volumeIcons[2].gameObject.SetActive(false);
            volumeIcons[3].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Mute video
    /// </summary>
    public void PressToMute() {
        volumeSlider.value = 0;
    }

    /// <summary>
    /// Unmute video
    /// </summary>
    public void PressToUnmute() {
        volumeSlider.value = 50;
    }

    /// <summary>
    /// Enter pointer for expand slider parent
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData) {
        ratio = 0;
        targetPos = maxVolumeExpand;
        startPos = selfRect.sizeDelta.x;
        isInTheArea = true;
    }

    /// <summary>
    /// Exit pointer for shrink slider parent
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData) {
        isInTheArea = false;
        if (isUsingSlider) {
            return;
        }
        ratio = 0;
        targetPos = minVolumeExpand;
        startPos = selfRect.sizeDelta.x;
    }

    public bool isUsingSlider; // Is using volume slider
    public bool isInTheArea; // Is pointer in the area

    /// <summary>
    /// Update method
    /// </summary>
    private void Update() {
        ratio = Mathf.Clamp(ratio + (speedOfLerp * Time.deltaTime) * curveLerp.Evaluate(ratio), 0, 1);
        selfRect.sizeDelta = new Vector2(Mathf.Lerp(startPos, targetPos, ratio), selfRect.sizeDelta.y);
    }
}
