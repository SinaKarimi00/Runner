using System;
using UnityEngine;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Environment
{
    [RequireComponent(typeof(Collider))]
    public sealed class ObstacleTutorialTrigger : MonoBehaviour
    {
        [SerializeField] private RunnerGameplayConfig _config;

        private Action _onTriggered;
        private bool _consumed;

        public void Initialize(Action onTriggered)
        {
            _onTriggered = onTriggered;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_consumed || _config == null || !other.CompareTag(_config.PlayerTag))
            {
                return;
            }

            _consumed = true;
            _onTriggered?.Invoke();
        }
    }
}
