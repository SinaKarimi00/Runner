using UnityEngine;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Camera
{
    public sealed class ImpactEffectSpawner : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _impactParticlePrefab;

        public void Play(Vector3 position, Vector3 normal)
        {
            if (_impactParticlePrefab == null)
            {
                return;
            }

            Quaternion rotation = normal.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(normal)
                : Quaternion.identity;

            ParticleSystem instance = Instantiate(_impactParticlePrefab, position, rotation);
            var lifetime = instance.main.duration + instance.main.startLifetime.constantMax;
            Destroy(instance.gameObject, lifetime);
        }
    }
}