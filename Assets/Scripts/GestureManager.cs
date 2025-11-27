/*
using System;
using UnityEngine;
using DigitalRubyShared;


public class GestureManager : NexgenDragon.MonoSingleton<GestureManager>
{
    [Header("Enable Gestures")] public bool EnableTap = true;
    public bool EnableDoubleTap = true;
    public bool EnableSwipe = true;
    public bool EnablePan = true;
    public bool EnableScale = true;
    public bool EnableRotate = true;
    public bool EnableLongPress = true;

    [Header("Gesture Settings")] public int DoubleTapCount = 2;
    public int PanMinTouches = 1;
    public int PanMaxTouches = 2;

    // Events to subscribe to
    public event Action<Vector2> OnTap;
    public event Action<Vector2> OnDoubleTap;
    public event Action<Vector2, Vector2, Vector2> OnSwipe; // start, end, velocity
    public event Action<Vector2, Vector2> OnPan;            // focus, delta
    public event Action<float, Vector2> OnScale;            // scaleMultiplier, focus
    public event Action<float, Vector2> OnRotate;           // deltaRadians, focus

    public event Action<Vector2> OnLongPressBegan;
    public event Action<Vector2> OnLongPressExecuting;
    public event Action<Vector2, Vector2> OnLongPressEnded; // focus, velocity

    private TapGestureRecognizer tapGesture;
    private TapGestureRecognizer doubleTapGesture;
    private SwipeGestureRecognizer swipeGesture;
    private PanGestureRecognizer panGesture;
    private ScaleGestureRecognizer scaleGesture;
    private RotateGestureRecognizer rotateGesture;
    private LongPressGestureRecognizer longPressGesture;

    private void Awake()
    {
        // Ensure FingersScript exists in scene
        if (FingersScript.Instance == null)
        {
            Debug.LogError("FingersScript.Instance is null. Please add the FingersScript prefab to your scene.");
        }
    }

    private void Start()
    {
        if (EnableTap) CreateTapGesture();
        if (EnableDoubleTap) CreateDoubleTapGesture();
        if (EnableSwipe) CreateSwipeGesture();
        if (EnablePan) CreatePanGesture();
        if (EnableScale) CreateScaleGesture();
        if (EnableRotate) CreateRotateGesture();
        if (EnableLongPress) CreateLongPressGesture();

        // Allow common simultaneous gestures
        if (panGesture != null && scaleGesture != null) panGesture.AllowSimultaneousExecution(scaleGesture);
        if (panGesture != null && rotateGesture != null) panGesture.AllowSimultaneousExecution(rotateGesture);
        if (scaleGesture != null && rotateGesture != null) scaleGesture.AllowSimultaneousExecution(rotateGesture);
    }

    private void OnDestroy()
    {
        var f = FingersScript.Instance;
        if (f == null) return;
        if (tapGesture != null) f.RemoveGesture(tapGesture);
        if (doubleTapGesture != null) f.RemoveGesture(doubleTapGesture);
        if (swipeGesture != null) f.RemoveGesture(swipeGesture);
        if (panGesture != null) f.RemoveGesture(panGesture);
        if (scaleGesture != null) f.RemoveGesture(scaleGesture);
        if (rotateGesture != null) f.RemoveGesture(rotateGesture);
        if (longPressGesture != null) f.RemoveGesture(longPressGesture);
    }

    private void CreateTapGesture()
    {
        Debug.Log("Creating tap gesture");
        tapGesture = new TapGestureRecognizer();
        tapGesture.StateUpdated += g =>
        {
            if (g.State == GestureRecognizerState.Ended)
            {
                Debug.Log("Tap at " + g.FocusX + ", " + g.FocusY);
                OnTap?.Invoke(new Vector2(g.FocusX, g.FocusY));
            }
        };
        FingersScript.Instance.AddGesture(tapGesture);
    }

    private void CreateDoubleTapGesture()
    {
        doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = Mathf.Max(2, DoubleTapCount) };
        doubleTapGesture.StateUpdated += g =>
        {
            if (g.State == GestureRecognizerState.Ended)
            {
                Debug.Log("Double tap at " + g.FocusX + ", " + g.FocusY);
                OnDoubleTap?.Invoke(new Vector2(g.FocusX, g.FocusY));
            }
        };
        FingersScript.Instance.AddGesture(doubleTapGesture);

        if (tapGesture != null)
        {
            tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
        }
    }

    private void CreateSwipeGesture()
    {
        swipeGesture = new SwipeGestureRecognizer
        {
            Direction = SwipeGestureRecognizerDirection.Any,
            DirectionThreshold = 1.0f
        };
        swipeGesture.StateUpdated += g =>
        {
            if (g.State == GestureRecognizerState.Ended)
            {
                Debug.Log("Swipe at " + g.FocusX + ", " + g.FocusY);
                var start = new Vector2(swipeGesture.StartFocusX, swipeGesture.StartFocusY);
                var end = new Vector2(g.FocusX, g.FocusY);
                var velocity = new Vector2(swipeGesture.VelocityX, swipeGesture.VelocityY);
                OnSwipe?.Invoke(start, end, velocity);
            }
        };
        FingersScript.Instance.AddGesture(swipeGesture);
    }

    private void CreatePanGesture()
    {
        panGesture = new PanGestureRecognizer
        {
            MinimumNumberOfTouchesToTrack = Mathf.Max(1, PanMinTouches),
            MaximumNumberOfTouchesToTrack = Mathf.Max(1, PanMaxTouches)
        };
        panGesture.StateUpdated += g =>
        {
            if (g.State == GestureRecognizerState.Executing)
            {
                Debug.Log("Pan at " + g.FocusX + ", " + g.FocusY);
                OnPan?.Invoke(new Vector2(g.FocusX, g.FocusY), new Vector2(g.DeltaX, g.DeltaY));
            }
        };
        FingersScript.Instance.AddGesture(panGesture);
    }

    private void CreateScaleGesture()
    {
        scaleGesture = new ScaleGestureRecognizer();
        scaleGesture.StateUpdated += g =>
        {
            if (g.State == GestureRecognizerState.Executing)
            {
                Debug.Log("Scale at " + g.FocusX + ", " + g.FocusY);
                OnScale?.Invoke(scaleGesture.ScaleMultiplier, new Vector2(g.FocusX, g.FocusY));
            }
        };
        FingersScript.Instance.AddGesture(scaleGesture);
    }

    private void CreateRotateGesture()
    {
        rotateGesture = new RotateGestureRecognizer();
        rotateGesture.StateUpdated += g =>
        {
            if (g.State == GestureRecognizerState.Executing)
            {
                Debug.Log("Rotate at " + g.FocusX + ", " + g.FocusY);
                OnRotate?.Invoke(rotateGesture.RotationRadiansDelta, new Vector2(g.FocusX, g.FocusY));
            }
        };
        FingersScript.Instance.AddGesture(rotateGesture);
    }

    private void CreateLongPressGesture()
    {
        longPressGesture = new LongPressGestureRecognizer { MaximumNumberOfTouchesToTrack = 1 };
        longPressGesture.StateUpdated += g =>
        {
            if (g.State == GestureRecognizerState.Began)
            {
                Debug.Log("Long press at " + g.FocusX + ", " + g.FocusY);
                OnLongPressBegan?.Invoke(new Vector2(g.FocusX, g.FocusY));
            }
            else if (g.State == GestureRecognizerState.Executing)
            {
                Debug.Log("Long press executing at " + g.FocusX + ", " + g.FocusY);
                OnLongPressExecuting?.Invoke(new Vector2(g.FocusX, g.FocusY));
            }
            else if (g.State == GestureRecognizerState.Ended)
            {
                Debug.Log("Long press ended at " + g.FocusX + ", " + g.FocusY);
                OnLongPressEnded?.Invoke(new Vector2(g.FocusX, g.FocusY), new Vector2(longPressGesture.VelocityX, longPressGesture.VelocityY));
            }
        };
        FingersScript.Instance.AddGesture(longPressGesture);
    }
}

*/
using System;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

namespace NexgenDragon
{
    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public class GestureManager : MonoSingleton<GestureManager>
    {
        [Header("Enable Gestures")] public bool EnableTap = true;
        public bool EnableSwipe = true;
        public bool EnableLongPress = true;

        [Header("Swipe Settings")] public float SwipeThresholdPixels = 50f;

        [Header("Long Press Settings")] public float LongPressDuration = 0.5f;
        public float LongPressMoveTolerancePixels = 10f;

        public event Action<Vector2> OnTap;
        public event Action<Vector2, SwipeDirection> OnSwipe;
        public event Action<Vector2> OnLongPressBegan;
        public event Action<Vector2> OnLongPressExecuting;
        public event Action<Vector2> OnLongPressEnded;

        private readonly Dictionary<int, Vector2> longPressStartPos = new Dictionary<int, Vector2>();
        private readonly HashSet<int> longPressStarted = new HashSet<int>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            LeanTouch.OnFingerTap += HandleFingerTap;
            LeanTouch.OnFingerSwipe += HandleFingerSwipe;
            LeanTouch.OnFingerDown += HandleFingerDown;
            LeanTouch.OnFingerUpdate += HandleFingerUpdate;
            LeanTouch.OnFingerUp += HandleFingerUp;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerTap -= HandleFingerTap;
            LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
            LeanTouch.OnFingerDown -= HandleFingerDown;
            LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
            LeanTouch.OnFingerUp -= HandleFingerUp;
            longPressStartPos.Clear();
            longPressStarted.Clear();
        }

        private void HandleFingerTap(LeanFinger finger)
        {
            if (!EnableTap) return;
            Debug.Log("Tap at " + finger.ScreenPosition);
            OnTap?.Invoke(finger.ScreenPosition);
        }

        private void HandleFingerSwipe(LeanFinger finger)
        {
            if (!EnableSwipe) return;

            var delta = finger.SwipeScreenDelta;
            if (delta.magnitude < SwipeThresholdPixels)
            {
                return;
            }

            var dir = GetSwipeDirection(delta);
            Debug.Log("Swipe at " + finger.ScreenPosition + " direction: " + dir);
            OnSwipe?.Invoke(finger.ScreenPosition, dir);
        }

        private void HandleFingerDown(LeanFinger finger)
        {
            if (!EnableLongPress) return;
            longPressStartPos[finger.Index] = finger.ScreenPosition;
            longPressStarted.Remove(finger.Index);
        }

        private void HandleFingerUpdate(LeanFinger finger)
        {
            if (!EnableLongPress) return;

            Vector2 startPos;
            if (!longPressStartPos.TryGetValue(finger.Index, out startPos))
            {
                return;
            }

            float move = (finger.ScreenPosition - startPos).magnitude;
            if (move > LongPressMoveTolerancePixels)
            {
                return;
            }

            if (!longPressStarted.Contains(finger.Index))
            {
                if (finger.Age >= LongPressDuration)
                {
                    longPressStarted.Add(finger.Index);
                    Debug.Log("Long press began at " + finger.ScreenPosition);
                    OnLongPressBegan?.Invoke(finger.ScreenPosition);
                }
            }
            else
            {
                OnLongPressExecuting?.Invoke(finger.ScreenPosition);
            }
        }

        private void HandleFingerUp(LeanFinger finger)
        {
            if (!EnableLongPress) return;

            if (longPressStarted.Contains(finger.Index))
            {
                Debug.Log("Long press ended at " + finger.ScreenPosition);
                OnLongPressEnded?.Invoke(finger.ScreenPosition);
            }

            longPressStarted.Remove(finger.Index);
            longPressStartPos.Remove(finger.Index);
        }

        private static SwipeDirection GetSwipeDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }
    }
}