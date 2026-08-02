using UnityEngine;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Environment
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MovingObstacle : MonoBehaviour
    {
        [Min(0f)] [SerializeField] private float amplitude = 3f;

        [Min(0f)] [SerializeField] private float speed = 2f;

        private Rigidbody _rigidbody;
        private float _baseX;
        private float _phaseOffset;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _baseX = transform.position.x;
            _phaseOffset = transform.position.z;
        }

        private void FixedUpdate()
        {
            Vector3 position = _rigidbody.position;
            position.x = _baseX + Mathf.Sin((Time.time + _phaseOffset) * speed) * amplitude;
            _rigidbody.MovePosition(position);
        }
    }
}