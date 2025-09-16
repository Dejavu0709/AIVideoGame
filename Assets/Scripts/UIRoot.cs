using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using System.Runtime.InteropServices;
public class UIRoot : MonoBehaviour
{
	public float referenceWidth = 1280;
	public float referenceHeight = 720;

    private int _width;
    private int _height;

    public static int OriginWidth = 0;
    public static int OriginHeight = 0;

    public static void Init()
    {
        OriginWidth = Screen.width;
        OriginHeight = Screen.height;
        Debug.Log("Origin Screen.width:" + Screen.width);
        Debug.Log("Origin Screen.height:" + Screen.height);
    }


    void Awake()
    {
        Init();
        OnResize();
        TryAdaptToScreen(this.GetComponent<RectTransform>());
    }

    void LateUpdate()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying) return;
        #endif
        int w = Screen.width;
        int h = Screen.height;

        if (w != _width || h != _height)
        {
            OnResize();
        }
    }

    public void OnResize()
    {
        transform.position = new Vector3(10000f,10000f,0f);

        _width = Screen.width;
        _height = Screen.height;
        CanvasScaler canScaler = GetComponent<CanvasScaler>();
        canScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canScaler.scaleFactor = 1f;

		float widthScale = Screen.width / referenceWidth;
		float heightScale = Screen.height / referenceHeight;

        float realScale = Mathf.Min(widthScale, heightScale);
//        if (realScale < 1.3 && realScale > 0.9)
//        {
//            realScale = 1f;
//        }

        canScaler.scaleFactor = realScale;
    }

    public static float marginToLeft = 0.0f;
    public static float marginToRight = 0.0f;
    public static float marginToButtom = 0.0f;
    public static float marginToTop = -77.0f;
	public static float marginToButtomForDialog = 20.0f;
    [DllImport("__Internal")]
    private static extern int CheckForNotch();
    public static void TryAdaptToScreen(RectTransform transform)
    {
#if UNITY_EDITOR
        return;
        #endif
        if (1 == CheckForNotch())
        {
            transform.offsetMin = new Vector2(-marginToLeft, -marginToButtom);
            transform.offsetMax = new Vector2(marginToRight, marginToTop);
        }
    }
}
