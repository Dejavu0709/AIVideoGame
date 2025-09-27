using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Change volume by Up/Down arrows animation
/// </summary>
public class ChangeVolumeAnim : MonoBehaviour
{
    public Image[] volumeIcons; // Volume icon list
    public TextMeshProUGUI volumeText; // Volume text

    /// <summary>
    /// Deactivate from animation
    /// </summary>
    public void Deactivate() {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Open animation
    /// </summary>
    /// <param name="volume">Target volume (0% - 100%)</param>
    public void Open(int volume) {
        volumeText.text = volume + "%";
        if (volume < 1) {
            volumeIcons[0].gameObject.SetActive(true);
            volumeIcons[1].gameObject.SetActive(false);
            volumeIcons[2].gameObject.SetActive(false);
            volumeIcons[3].gameObject.SetActive(false);
        } else if (volume < 40) {
            volumeIcons[0].gameObject.SetActive(false);
            volumeIcons[1].gameObject.SetActive(true);
            volumeIcons[2].gameObject.SetActive(false);
            volumeIcons[3].gameObject.SetActive(false);
        } else if (volume < 80) {
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
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}
