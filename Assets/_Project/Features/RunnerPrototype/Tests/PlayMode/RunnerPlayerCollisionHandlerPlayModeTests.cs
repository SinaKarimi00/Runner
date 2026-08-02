using NUnit.Framework;
using UnityEngine;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Player;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.PlayMode
{
    public class RunnerPlayerCollisionHandlerPlayModeTests
    {
        private GameObject _playerObject;

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.Destroy(_playerObject);
            }
        }

        [Test]
        public void HandleObstacleContact_CalledTwice_OnlyInvokesCallbackOnce()
        {
            _playerObject = new GameObject("TestPlayer", typeof(RunnerPlayerCollisionHandler));
            var handler = _playerObject.GetComponent<RunnerPlayerCollisionHandler>();

            int callCount = 0;
            handler.Initialize(() => callCount++);

            handler.HandleObstacleContact(Vector3.zero, Vector3.up);
            handler.HandleObstacleContact(Vector3.zero, Vector3.up);

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void HandleObstacleContact_WhenSetInactive_DoesNotInvokeCallback()
        {
            _playerObject = new GameObject("TestPlayer", typeof(RunnerPlayerCollisionHandler));
            var handler = _playerObject.GetComponent<RunnerPlayerCollisionHandler>();

            int callCount = 0;
            handler.Initialize(() => callCount++);
            handler.SetActive(false);

            handler.HandleObstacleContact(Vector3.zero, Vector3.up);

            Assert.AreEqual(0, callCount);
        }
    }
}
