using UnityEngine;
using Cinemachine;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Input;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RunnerPlayerController : MonoBehaviour
    {
        [SerializeField] private RunnerGameplayConfig config;
        [SerializeField] private RunnerInputReader inputReader;
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Animator animator;
        [SerializeField] private CinemachineImpulseSource impulseSource;

        private static readonly int MoveSpeedParam = Animator.StringToHash("MoveSpeed");
        private static readonly int GroundedParam = Animator.StringToHash("Grounded");

        private const int MinLaneIndex = -1;
        private const int MaxLaneIndex = 1;

        private bool _isRunning;
        private int _laneIndex;
        private float _targetX;
        private float _horizontalVelocityRef;
        private float _currentTilt;

        private bool _isSpeedPenalized;
        private float _speedPenaltyDistanceRemaining;

        private void Reset()
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            if (playerRigidbody == null)
            {
                playerRigidbody = GetComponent<Rigidbody>();
            }

            _targetX = playerRigidbody.position.x;
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.SwipeDetected += HandleSwipeDetected;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.SwipeDetected -= HandleSwipeDetected;
            }
        }

        public void SetRunning(bool isRunning)
        {
            _isRunning = isRunning;

            if (isRunning)
            {
                _laneIndex = 0;
                _targetX = 0f;
                _isSpeedPenalized = false;
                _speedPenaltyDistanceRemaining = 0f;
            }
            else
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }


            animator.SetBool(GroundedParam, true);
            animator.SetFloat(MoveSpeedParam, isRunning ? 1f : 0f);
        }


        private void HandleSwipeDetected(int direction)
        {
            MoveToLane(direction);
        }

        public void MoveToLane(int direction)
        {
            if (!_isRunning || config == null)
            {
                return;
            }

            int desiredLaneIndex = _laneIndex + direction;

            if (desiredLaneIndex < MinLaneIndex || desiredLaneIndex > MaxLaneIndex)
            {
                TriggerEdgeBump();
                return;
            }

            _laneIndex = desiredLaneIndex;
            _targetX = _laneIndex * config.LaneOffset;
        }

        private void TriggerEdgeBump()
        {
            _isSpeedPenalized = true;
            _speedPenaltyDistanceRemaining = config.EdgeBumpRecoveryDistance;

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(config.EdgeBumpCameraShakeForce);
            }
        }

        private void Update()
        {
            UpdateVisualTilt();
        }

        private void FixedUpdate()
        {
            if (!_isRunning || config == null)
            {
                return;
            }

            Vector3 currentPosition = playerRigidbody.position;
            float smoothedX = Mathf.SmoothDamp(
                currentPosition.x,
                _targetX,
                ref _horizontalVelocityRef,
                config.HorizontalSmoothTime);

            float currentForwardSpeed = config.ForwardSpeed;
            if (_isSpeedPenalized)
            {
                currentForwardSpeed *= config.EdgeBumpSpeedMultiplier;
            }

            float step = currentForwardSpeed * Time.fixedDeltaTime;

            Vector3 nextPosition = currentPosition;
            nextPosition.x = smoothedX;
            nextPosition.z += step;

            playerRigidbody.MovePosition(nextPosition);

            if (_isSpeedPenalized)
            {
                _speedPenaltyDistanceRemaining -= step;
                if (_speedPenaltyDistanceRemaining <= 0f)
                {
                    _isSpeedPenalized = false;
                }
            }
        }

        private void UpdateVisualTilt()
        {
            if (visualRoot == null || config == null)
            {
                return;
            }

            float maxLateralSpeed = config.LaneOffset / Mathf.Max(0.01f, config.HorizontalSmoothTime);
            float normalizedLateralSpeed =
                Mathf.Clamp(_horizontalVelocityRef / Mathf.Max(0.01f, maxLateralSpeed), -1f, 1f);
            float targetTilt = _isRunning ? -normalizedLateralSpeed * config.MaxTiltAngle : 0f;

            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * config.TiltSmoothSpeed);
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, _currentTilt);
        }
    }
}