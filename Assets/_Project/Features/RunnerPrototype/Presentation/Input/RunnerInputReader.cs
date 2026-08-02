using System;
using UnityEngine;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Input
{
    public sealed class RunnerInputReader : MonoBehaviour
    {
        private readonly struct PointerState
        {
            public readonly bool Present;
            public readonly bool PressedThisFrame;
            public readonly bool ReleasedThisFrame;
            public readonly float ScreenX;

            public PointerState(bool present, bool pressedThisFrame, bool releasedThisFrame, float screenX)
            {
                Present = present;
                PressedThisFrame = pressedThisFrame;
                ReleasedThisFrame = releasedThisFrame;
                ScreenX = screenX;
            }
        }

        private const float DefaultSwipeThreshold = 0.08f;

        [SerializeField] private RunnerGameplayConfig config;

        public event Action<int> SwipeDetected;

        public bool IsDragging { get; private set; }

        private bool _inputEnabled = true;
        private bool _pointerActive;
        private bool _swipeConsumedThisDrag;
        private float _dragStartScreenX;

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;

            if (enabled)
            {
                return;
            }

            _pointerActive = false;
            IsDragging = false;
        }

        private void Update()
        {
            if (!_inputEnabled)
            {
                return;
            }

            PointerState pointer = ReadPointerState();

            UpdateDragState(pointer);
            TryDetectSwipe(pointer);
        }

        private static PointerState ReadPointerState()
        {
            return UnityEngine.Input.touchCount > 0 ? ReadTouchPointer() : ReadMousePointer();
        }

        private static PointerState ReadTouchPointer()
        {
            Touch touch = UnityEngine.Input.GetTouch(0);
            bool releasedThisFrame = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;

            return new PointerState(true, touch.phase == TouchPhase.Began, releasedThisFrame, touch.position.x);
        }

        private static PointerState ReadMousePointer()
        {
            return new PointerState(
                UnityEngine.Input.GetMouseButton(0),
                UnityEngine.Input.GetMouseButtonDown(0),
                UnityEngine.Input.GetMouseButtonUp(0),
                UnityEngine.Input.mousePosition.x);
        }

        private void UpdateDragState(PointerState pointer)
        {
            if (pointer.PressedThisFrame)
            {
                BeginDrag(pointer.ScreenX);
                return;
            }

            if (pointer.ReleasedThisFrame || !pointer.Present)
            {
                EndDrag();
            }
        }

        private void BeginDrag(float screenX)
        {
            _pointerActive = true;
            _swipeConsumedThisDrag = false;
            _dragStartScreenX = screenX;
            IsDragging = true;
        }

        private void EndDrag()
        {
            _pointerActive = false;
            IsDragging = false;
        }

        private void TryDetectSwipe(PointerState pointer)
        {
            if (!_pointerActive || _swipeConsumedThisDrag)
            {
                return;
            }

            float threshold = config != null ? config.SwipeThreshold : DefaultSwipeThreshold;
            float normalizedDelta = (pointer.ScreenX - _dragStartScreenX) / Mathf.Max(1f, Screen.width);

            int direction = ResolveSwipeDirection(normalizedDelta, threshold);
            if (direction == 0)
            {
                return;
            }

            _swipeConsumedThisDrag = true;
            SwipeDetected?.Invoke(direction);
        }

        private static int ResolveSwipeDirection(float normalizedDelta, float threshold)
        {
            if (normalizedDelta >= threshold)
            {
                return 1;
            }

            if (normalizedDelta <= -threshold)
            {
                return -1;
            }

            return 0;
        }
    }
}