using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Volume slider handle
/// </summary>
public class VolumeSliderHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public VolumeScript volumeScript; // Reference to a volume script


    public void OnPointerDown(PointerEventData eventData) {
        volumeScript.isUsingSlider = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
        volumeScript.isUsingSlider = false;
        if (!volumeScript.isInTheArea) {
            volumeScript.OnPointerExit(eventData);
        }
    }
}
