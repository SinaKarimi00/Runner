using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Collectibles
{
    public sealed class CollectibleView : MonoBehaviour
    {
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private Transform visualTransform;
        [SerializeField] private Collider triggerCollider;
        [SerializeField] private ParticleSystem collectionParticle;

        private RunnerGameplayConfig _config;
        private AsyncOperationHandle<RunnerGameplayConfig> _configHandle;
        private Action<int> _onCollected;
        private bool _collected;
        private Sequence _activeSequence;
        private Vector3 _originalLocalPosition;

        private void Awake()
        {
            ResolveChildReferences();

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            _configHandle = RunnerGameplayConfigLoader.LoadAsync(loadedConfig =>
            {
                _config = loadedConfig;
                if (triggerCollider != null && !_collected)
                {
                    triggerCollider.enabled = true;
                }
            });

            if (visualTransform != null)
            {
                _originalLocalPosition = visualTransform.localPosition;
            }
        }

        private void ResolveChildReferences()
        {
            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<Collider>();
            }

            if (visualTransform == null)
            {
                MeshRenderer visualRenderer = GetComponentInChildren<MeshRenderer>(true);
                if (visualRenderer != null)
                {
                    visualTransform = visualRenderer.transform;
                }
            }

            if (collectionParticle == null)
            {
                collectionParticle = GetComponentInChildren<ParticleSystem>(true);
            }
        }

        public void Initialize(Action<int> onCollected)
        {
            _onCollected = onCollected;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_config != null && other.CompareTag(_config.PlayerTag))
            {
                HandleTriggerEnter();
            }
        }

        public void HandleTriggerEnter()
        {
            if (_collected)
            {
                return;
            }

            _collected = true;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            _onCollected?.Invoke(scoreValue);

            if (collectionParticle != null)
            {
                collectionParticle.transform.SetParent(null, true);
                collectionParticle.Play();
                var lifetime = collectionParticle.main.duration + collectionParticle.main.startLifetime.constantMax;
                Destroy(collectionParticle.gameObject, lifetime);
            }

            PlayCollectionAnimation();
        }

        private void PlayCollectionAnimation()
        {
            _activeSequence?.Kill();

            if (visualTransform == null || _config == null)
            {
                gameObject.SetActive(false);
                return;
            }

            var duration = _config.CollectibleAnimationDuration;
            var bounceTargetY = _originalLocalPosition.y + _config.CollectibleBounceHeight;
            var punchScale = Vector3.one * _config.CollectiblePunchScale;
            var rotationAmount = new Vector3(0f, _config.CollectibleRotationAmount, 0f);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(visualTransform.DOLocalMoveY(bounceTargetY, duration)
                .SetEase(Ease.OutQuad));
            sequence.Join(visualTransform.DOPunchScale(punchScale, duration, _config.CollectiblePunchVibrato,
                _config.CollectiblePunchElasticity));
            sequence.Join(visualTransform.DOLocalRotate(rotationAmount, duration, RotateMode.FastBeyond360));
            sequence.OnComplete(() => gameObject.SetActive(false));
            sequence.SetTarget(this);

            _activeSequence = sequence;
        }

        private void OnDisable()
        {
            _activeSequence?.Kill();
        }

        private void OnDestroy()
        {
            if (_configHandle.IsValid())
            {
                UnityEngine.AddressableAssets.Addressables.Release(_configHandle);
            }
        }
    }
}
