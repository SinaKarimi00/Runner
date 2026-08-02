using DG.Tweening;
using UnityEngine;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.UI
{
    public sealed class ObstacleTutorialView : MonoBehaviour
    {
        [SerializeField] private RunnerGameplayConfig _config;
        [SerializeField] private CanvasGroup _panelCanvasGroup;

        private Sequence _showSequence;
        private bool _isVisible;

        private void Awake()
        {
            HideImmediate();
        }

        public void HideImmediate()
        {
            _showSequence?.Kill();
            _isVisible = false;

            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.alpha = 0f;
            }
        }

        public void Show()
        {
            if (_isVisible || _panelCanvasGroup == null)
            {
                return;
            }

            _isVisible = true;
            _showSequence?.Kill();

            float fadeDuration = _config != null ? _config.ObstacleTutorialFadeDuration : 0.3f;

            CanvasGroup canvasGroup = _panelCanvasGroup;
            canvasGroup.alpha = 0f;

            // Unscaled time: the game is paused (Time.timeScale = 0) for the whole time this is visible.
            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            sequence.Append(DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeDuration)
                .SetUpdate(true));
            sequence.SetTarget(this);

            _showSequence = sequence;
        }

        public void Hide()
        {
            if (!_isVisible)
            {
                return;
            }

            _isVisible = false;
            _showSequence?.Kill();

            if (_panelCanvasGroup == null)
            {
                return;
            }

            float fadeDuration = _config != null ? _config.ObstacleTutorialFadeDuration : 0.3f;
            CanvasGroup canvasGroup = _panelCanvasGroup;

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            sequence.Append(DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeDuration)
                .SetUpdate(true));
            sequence.SetTarget(this);

            _showSequence = sequence;
        }

        private void OnDestroy()
        {
            _showSequence?.Kill();
        }
    }
}
