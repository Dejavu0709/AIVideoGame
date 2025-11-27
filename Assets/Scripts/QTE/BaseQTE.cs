using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public abstract class BaseQTE : MonoBehaviour
{
    [Header("Common QTE UI")]
    public RectTransform commonRoot;
    public GameObject rightIndicator;
    public GameObject wrongIndicator;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    [Header("QTE Result Display")]
    public float resultDisplayDuration = 0.5f;

    protected void ResetResultIndicators()
    {
        if (rightIndicator != null) rightIndicator.SetActive(false);
        if (wrongIndicator != null) wrongIndicator.SetActive(false);

        // Reset colors on all common UI elements so each QTE starts from a clean state
        if (commonRoot != null)
        {
            var images = commonRoot.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img != null) img.color = Color.white;
            }
            var texts = commonRoot.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (t != null) t.color = Color.white;
            }
            var texts2 = commonRoot.GetComponentsInChildren<Text>(true);
            foreach (var t in texts2)
            {
                if (t != null) t.color = Color.white;
            }
        }
    }

    protected void ApplyResultVisual(bool success)
    {
        if (commonRoot != null)
        {
            Color targetColor = success ? successColor : failColor;
            var images = commonRoot.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img != null) img.color = targetColor;
            }
            var texts = commonRoot.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (t != null) t.color = targetColor;
            }
            var texts2 = commonRoot.GetComponentsInChildren<Text>(true);
            foreach (var t in texts2)
            {
                if (t != null) t.color = targetColor;
            }
        }

        if (rightIndicator != null) rightIndicator.SetActive(success);
        if (wrongIndicator != null) wrongIndicator.SetActive(!success);
    }

    protected void CompleteQTE(int score, System.Action<int> onComplete, System.Action afterDelay = null)
    {
        StartCoroutine(CompleteQTERoutine(score, onComplete, afterDelay));
    }

    private IEnumerator CompleteQTERoutine(int score, System.Action<int> onComplete, System.Action afterDelay)
    {
        bool success = score > 0;
        ApplyResultVisual(success);

        if (resultDisplayDuration > 0f)
        {
            yield return new WaitForSeconds(resultDisplayDuration);
        }

        onComplete?.Invoke(score);
        if (afterDelay != null)
        {
            afterDelay();
        }
    }
}
