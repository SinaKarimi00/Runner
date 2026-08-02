using UnityEngine;

namespace Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration
{
    [CreateAssetMenu(fileName = "RunnerGameplayConfig", menuName = "RunnerPrototype/Gameplay Config")]
    public sealed class RunnerGameplayConfig : ScriptableObject
    {
        [Header("Tags")] [SerializeField] private string _playerTag = "Player";

        [Header("Movement")] [Min(0f)] [SerializeField]
        private float _forwardSpeed = 6f;

        [Range(0.02f, 0.3f)] [SerializeField] private float _swipeThreshold = 0.08f;

        [Min(0.1f)] [SerializeField] private float _laneOffset = 3f;

        [Range(0.01f, 1f)] [SerializeField] private float _horizontalSmoothTime = 0.12f;

        [Header("Edge Bump")] [Range(0.1f, 1f)] [SerializeField]
        private float _edgeBumpSpeedMultiplier = 0.5f;

        [Min(0.1f)] [SerializeField] private float _edgeBumpRecoveryDistance = 20f;

        [Range(0.05f, 1f)] [SerializeField] private float _edgeBumpCameraShakeForce = 0.35f;

        [Header("Visual Tilt")] [Range(0f, 60f)] [SerializeField]
        private float _maxTiltAngle = 20f;

        [Min(0.01f)] [SerializeField] private float _tiltSmoothSpeed = 8f;

        [Header("Camera Feedback")] [Min(0f)] [SerializeField]
        private float _cameraShakeDuration = 0.3f;

        [Min(0f)] [SerializeField] private float _cameraShakeIntensity = 1.6f;

        [Header("Game Over UI")] [Min(0f)] [SerializeField]
        private float _gameOverTransitionDuration = 0.35f;

        [Header("Score Feedback")] [Min(0f)] [SerializeField]
        private float _scorePunchScale = 0.3f;

        [Min(0f)] [SerializeField] private float _scorePunchDuration = 0.25f;

        [Header("Collectible Feedback")] [Range(0.05f, 1f)] [SerializeField]
        private float _collectibleAnimationDuration = 0.25f;

        [Min(0f)] [SerializeField] private float _collectibleBounceHeight = 0.4f;

        [Min(0f)] [SerializeField] private float _collectiblePunchScale = 0.3f;

        [Range(1, 10)] [SerializeField] private int _collectiblePunchVibrato = 4;

        [Range(0f, 1f)] [SerializeField] private float _collectiblePunchElasticity = 0.6f;

        [Min(0f)] [SerializeField] private float _collectibleRotationAmount = 360f;

        [Header("Obstacle Tutorial")] [Min(0f)] [SerializeField]
        private float _obstacleTutorialFadeDuration = 0.3f;

        public string PlayerTag => _playerTag;
        public float ForwardSpeed => _forwardSpeed;
        public float SwipeThreshold => _swipeThreshold;
        public float LaneOffset => _laneOffset;
        public float HorizontalSmoothTime => _horizontalSmoothTime;
        public float EdgeBumpSpeedMultiplier => _edgeBumpSpeedMultiplier;
        public float EdgeBumpRecoveryDistance => _edgeBumpRecoveryDistance;
        public float EdgeBumpCameraShakeForce => _edgeBumpCameraShakeForce;
        public float MaxTiltAngle => _maxTiltAngle;
        public float TiltSmoothSpeed => _tiltSmoothSpeed;
        public float CameraShakeDuration => _cameraShakeDuration;
        public float CameraShakeIntensity => _cameraShakeIntensity;
        public float GameOverTransitionDuration => _gameOverTransitionDuration;
        public float ScorePunchScale => _scorePunchScale;
        public float ScorePunchDuration => _scorePunchDuration;
        public float CollectibleAnimationDuration => _collectibleAnimationDuration;
        public float CollectibleBounceHeight => _collectibleBounceHeight;
        public float CollectiblePunchScale => _collectiblePunchScale;
        public int CollectiblePunchVibrato => _collectiblePunchVibrato;
        public float CollectiblePunchElasticity => _collectiblePunchElasticity;
        public float CollectibleRotationAmount => _collectibleRotationAmount;
        public float ObstacleTutorialFadeDuration => _obstacleTutorialFadeDuration;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_playerTag))
            {
                _playerTag = "Player";
            }

            _forwardSpeed = Mathf.Max(0f, _forwardSpeed);
            _swipeThreshold = Mathf.Clamp(_swipeThreshold, 0.02f, 0.3f);
            _laneOffset = Mathf.Max(0.1f, _laneOffset);
            _horizontalSmoothTime = Mathf.Clamp(_horizontalSmoothTime, 0.01f, 1f);
            _edgeBumpSpeedMultiplier = Mathf.Clamp(_edgeBumpSpeedMultiplier, 0.1f, 1f);
            _edgeBumpRecoveryDistance = Mathf.Max(0.1f, _edgeBumpRecoveryDistance);
            _edgeBumpCameraShakeForce = Mathf.Clamp(_edgeBumpCameraShakeForce, 0.05f, 1f);
            _maxTiltAngle = Mathf.Clamp(_maxTiltAngle, 0f, 60f);
            _tiltSmoothSpeed = Mathf.Max(0.01f, _tiltSmoothSpeed);
            _cameraShakeDuration = Mathf.Max(0f, _cameraShakeDuration);
            _cameraShakeIntensity = Mathf.Max(0f, _cameraShakeIntensity);
            _gameOverTransitionDuration = Mathf.Max(0f, _gameOverTransitionDuration);
            _scorePunchScale = Mathf.Max(0f, _scorePunchScale);
            _scorePunchDuration = Mathf.Max(0f, _scorePunchDuration);
            _collectibleAnimationDuration = Mathf.Clamp(_collectibleAnimationDuration, 0.05f, 1f);
            _collectibleBounceHeight = Mathf.Max(0f, _collectibleBounceHeight);
            _collectiblePunchScale = Mathf.Max(0f, _collectiblePunchScale);
            _collectiblePunchVibrato = Mathf.Clamp(_collectiblePunchVibrato, 1, 10);
            _collectiblePunchElasticity = Mathf.Clamp(_collectiblePunchElasticity, 0f, 1f);
            _collectibleRotationAmount = Mathf.Max(0f, _collectibleRotationAmount);
            _obstacleTutorialFadeDuration = Mathf.Max(0f, _obstacleTutorialFadeDuration);
        }
    }
}