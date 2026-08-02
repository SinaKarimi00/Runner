using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.UI
{
    public sealed class StartMenuView : MonoBehaviour
    {
        [SerializeField] private RunnerGameplayConfig _config;
        [SerializeField] private CanvasGroup _panelCanvasGroup;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private Button _playButton;

        private Vector3 _originalScale = Vector3.one;
        private Sequence _showSequence;
        private Action _onPlayRequested;
        private bool _playRequested;

        private void Awake()
        {
            if (_panelRect != null)
            {
                _originalScale = _panelRect.localScale;
            }

            if (_playButton != null)
            {
                _playButton.onClick.AddListener(HandlePlayClicked);
            }
        }

        public void Initialize(Action onPlayRequested)
        {
            _onPlayRequested = onPlayRequested;
        }

        public void ShowImmediate()
        {
            _showSequence?.Kill();
            _playRequested = false;

            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.alpha = 1f;
                _panelCanvasGroup.interactable = true;
                _panelCanvasGroup.blocksRaycasts = true;
            }

            if (_panelRect != null)
            {
                _panelRect.localScale = _originalScale;
            }

            if (_playButton != null)
            {
                _playButton.interactable = true;
            }
        }

        public void Hide()
        {
            _showSequence?.Kill();

            float duration = _config != null ? _config.GameOverTransitionDuration : 0.35f;

            Sequence sequence = DOTween.Sequence();

            if (_panelCanvasGroup != null)
            {
                CanvasGroup canvasGroup = _panelCanvasGroup;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                sequence.Join(DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, duration));
            }

            if (_panelRect != null)
            {
                sequence.Join(_panelRect.DOScale(_originalScale * 0.9f, duration).SetEase(Ease.InBack));
            }

            sequence.OnComplete(() => gameObject.SetActive(false));
            sequence.SetTarget(this);

            _showSequence = sequence;
        }

        private void HandlePlayClicked()
        {
            if (_playRequested)
            {
                return;
            }

            _playRequested = true;

            if (_playButton != null)
            {
                _playButton.interactable = false;
            }

            _onPlayRequested?.Invoke();
        }

        private void OnDestroy()
        {
            _showSequence?.Kill();

            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(HandlePlayClicked);
            }
        }
    }
}