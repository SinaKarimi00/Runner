using System;
using UnityEngine;
using Cinemachine;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Camera;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Player
{
    public sealed class RunnerPlayerCollisionHandler : MonoBehaviour
    {
        [SerializeField] private RunnerGameplayConfig _config;
        [SerializeField] private string _obstacleTag = "Obstacle";
        [SerializeField] private ImpactEffectSpawner _impactEffectSpawner;
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        private Action _onObstacleHit;
        private bool _isActive = true;

        public void Initialize(Action onObstacleHit)
        {
            _onObstacleHit = onObstacleHit;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isActive || !collision.collider.CompareTag(_obstacleTag))
            {
                return;
            }

            ContactPoint contact = collision.GetContact(0);
            HandleObstacleContact(contact.point, contact.normal);
        }

        public void HandleObstacleContact(Vector3 point, Vector3 normal)
        {
            if (!_isActive)
            {
                return;
            }

            _isActive = false;

            if (_impactEffectSpawner != null)
            {
                _impactEffectSpawner.Play(point, normal);
            }

            if (_impulseSource != null)
            {
                float shakeForce = _config != null ? _config.CameraShakeIntensity : 1f;
                _impulseSource.GenerateImpulse(shakeForce);
            }

            _onObstacleHit?.Invoke();
        }
    }
}
