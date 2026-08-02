using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.UI
{
    public sealed class GameOverView : MonoBehaviour
    {
        [SerializeField] private RunnerGameplayConfig _config;
        [SerializeField] private CanvasGroup _panelCanvasGroup;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private Button _restartButton;

        private Vector3 _originalScale;
        private Sequence _showSequence;
        private Action _onRestartRequested;
        private bool _restartRequested;

        private void Awake()
        {
            if (_panelRect != null)
            {
                _originalScale = _panelRect.localScale;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClicked);
            }

            HideImmediate();
        }

        public void Initialize(Action onRestartRequested)
        {
            _onRestartRequested = onRestartRequested;
        }

        public void HideImmediate()
        {
            _showSequence?.Kill();
            _restartRequested = false;

            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.alpha = 0f;
                _panelCanvasGroup.interactable = false;
                _panelCanvasGroup.blocksRaycasts = false;
            }

            if (_panelRect != null)
            {
                _panelRect.localScale = _originalScale * 0.9f;
            }

            if (_restartButton != null)
            {
                _restartButton.interactable = false;
            }
        }

        public void Show()
        {
            _showSequence?.Kill();

            if (_panelRect != null)
            {
                _panelRect.localScale = _originalScale * 0.9f;
            }

            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.alpha = 0f;
            }

            float duration = _config != null ? _config.GameOverTransitionDuration : 0.35f;

            Sequence sequence = DOTween.Sequence();

            if (_panelCanvasGroup != null)
            {
                CanvasGroup canvasGroup = _panelCanvasGroup;
                sequence.Join(DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, duration));
            }

            if (_panelRect != null)
            {
                sequence.Join(_panelRect.DOScale(_originalScale, duration).SetEase(Ease.OutBack));
            }

            sequence.OnComplete(HandleShowComplete);
            sequence.SetTarget(this);

            _showSequence = sequence;
        }

        private void HandleShowComplete()
        {
            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.interactable = true;
                _panelCanvasGroup.blocksRaycasts = true;
            }

            if (_restartButton != null)
            {
                _restartButton.interactable = true;
            }
        }

        private void HandleRestartClicked()
        {
            if (_restartRequested)
            {
                return;
            }

            _restartRequested = true;

            if (_restartButton != null)
            {
                _restartButton.interactable = false;
            }

            _onRestartRequested?.Invoke();
        }

        private void OnDestroy()
        {
            _showSequence?.Kill();

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
        }
    }
}