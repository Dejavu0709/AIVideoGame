using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Change time by double pressing on the sides of the video or by pressing Left/Right arrows
/// </summary>
public class ChangeTime : MonoBehaviour, IPointerClickHandler {
    public bool isPlusTime; // Is need to add seconds or minus seconds

    public float doubleClickDuration = 0.5f; // Duration of double click
    int countClick = 0; // Count of clicks
    float currentTimer; // Timer to check current duration

    public GameObject targetObject; // Target object that appears when changing time
    public Animator arrowAnimator; // Arrow animator in a target object
    public TextMeshProUGUI textSeconds; // Text for display seconds

    public VideoManager videoManager; // Reference to a video manager

    /// <summary>
    /// Update method
    /// </summary>
    private void Update() {
        if (currentTimer > 0) {
            currentTimer -= Time.deltaTime;
        } else {
            if (countClick > 0) {
                countClick = 0;
                targetObject.GetComponent<Animator>().SetTrigger("Fade");
            }
        }
    }

    /// <summary>
    /// Check count of clicks on left or right sides
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData) {
        countClick++;
        currentTimer = doubleClickDuration;
        if (countClick == 2) {
            targetObject.SetActive(false);
            targetObject.SetActive(true);
        }
        if (countClick > 1) {
            textSeconds.text = (countClick - 1) * videoManager.advancedVideoManager.timeStepOffset + " seconds";
            arrowAnimator.SetTrigger("Go");
            if (isPlusTime) {
                videoManager.AddSeconds();
            } else {
                videoManager.MinusSeconds();
            }
        }
    }

    /// <summary>
    /// Change time by pressing Left/Right arrows
    /// </summary>
    public void AddByButton() {
        if (countClick == 0) {
            countClick = 1;
        }
        countClick++;
        currentTimer = doubleClickDuration;
        if (countClick == 2) {
            targetObject.SetActive(false);
            targetObject.SetActive(true);
        }
        textSeconds.text = (countClick - 1) * videoManager.advancedVideoManager.timeStepOffset + " seconds";
        arrowAnimator.SetTrigger("Go");
        if (isPlusTime) {
            videoManager.AddSeconds();
        } else {
            videoManager.MinusSeconds();
        }
    }
}
