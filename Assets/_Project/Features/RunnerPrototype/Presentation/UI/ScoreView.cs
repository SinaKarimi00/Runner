using TMPro;
using UnityEngine;
using DG.Tweening;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.UI
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private RunnerGameplayConfig _config;
        [SerializeField] private TMP_Text _scoreText;

        private Vector3 _originalScale;
        private Tween _punchTween;

        private void Awake()
        {
            if (_scoreText != null)
            {
                _originalScale = _scoreText.transform.localScale;
            }
        }

        public void ResetView()
        {
            _punchTween?.Kill();

            if (_scoreText == null)
            {
                return;
            }

            _scoreText.text = "0";
            _scoreText.transform.localScale = _originalScale;
        }

        public void OnScoreChanged(int newScore)
        {
            if (_scoreText == null)
            {
                return;
            }

            _scoreText.text = newScore.ToString();

            _punchTween?.Kill();
            _scoreText.transform.localScale = _originalScale;

            float punchScale = _config != null ? _config.ScorePunchScale : 0.3f;
            float punchDuration = _config != null ? _config.ScorePunchDuration : 0.25f;

            _punchTween = _scoreText.transform
                .DOPunchScale(Vector3.one * punchScale, punchDuration, 4, 0.7f)
                .SetTarget(_scoreText.transform);
        }

        private void OnDisable()
        {
            _punchTween?.Kill();
        }
    }
}