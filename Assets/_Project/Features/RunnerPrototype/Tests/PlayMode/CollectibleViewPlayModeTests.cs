using NUnit.Framework;
using UnityEngine;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Collectibles;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.PlayMode
{
    public class CollectibleViewPlayModeTests
    {
        private GameObject _collectibleObject;

        [TearDown]
        public void TearDown()
        {
            if (_collectibleObject != null)
            {
                Object.Destroy(_collectibleObject);
            }
        }

        [Test]
        public void HandleTriggerEnter_CalledTwice_OnlyInvokesCallbackOnce()
        {
            _collectibleObject = new GameObject("TestCollectible", typeof(SphereCollider), typeof(CollectibleView));
            var view = _collectibleObject.GetComponent<CollectibleView>();

            int callCount = 0;
            view.Initialize(_ => callCount++);

            view.HandleTriggerEnter();
            view.HandleTriggerEnter();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void HandleTriggerEnter_DisablesTriggerCollider()
        {
            _collectibleObject = new GameObject("TestCollectible", typeof(SphereCollider), typeof(CollectibleView));
            var collider = _collectibleObject.GetComponent<SphereCollider>();
            var view = _collectibleObject.GetComponent<CollectibleView>();

            var triggerField = typeof(CollectibleView).GetField("triggerCollider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            triggerField.SetValue(view, collider);

            view.Initialize(_ => { });
            view.HandleTriggerEnter();

            Assert.IsFalse(collider.enabled);
        }
    }
}
