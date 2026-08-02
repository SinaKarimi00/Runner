using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Player;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.PlayMode
{
    public class RunnerPlayerControllerPlayModeTests
    {
        private GameObject _playerObject;
        private RunnerGameplayConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<RunnerGameplayConfig>();

            _playerObject = new GameObject("TestPlayer", typeof(Rigidbody), typeof(RunnerPlayerController));
            _playerObject.GetComponent<Rigidbody>().useGravity = false;

            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            var configField = typeof(RunnerPlayerController).GetField("_config",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField.SetValue(controller, _config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_playerObject);
            Object.Destroy(_config);
        }

        private static IEnumerator SettleFixedUpdates(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTest]
        public IEnumerator MoveToLane_FromCenter_SingleRightSwipe_MovesExactlyOneLaneRight()
        {
            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            controller.SetRunning(true);

            controller.MoveToLane(1);
            yield return SettleFixedUpdates(60);

            Assert.AreEqual(_config.LaneOffset, _playerObject.transform.position.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator MoveToLane_FromCenter_SingleLeftSwipe_MovesExactlyOneLaneLeft()
        {
            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            controller.SetRunning(true);

            controller.MoveToLane(-1);
            yield return SettleFixedUpdates(60);

            Assert.AreEqual(-_config.LaneOffset, _playerObject.transform.position.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator MoveToLane_AtRightmostLane_AdditionalRightSwipe_StaysClampedAtRightLane()
        {
            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            controller.SetRunning(true);

            controller.MoveToLane(1);
            controller.MoveToLane(1);
            yield return SettleFixedUpdates(60);

            Assert.AreEqual(_config.LaneOffset, _playerObject.transform.position.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator MoveToLane_FromLeftLane_TwoRightSwipes_EndsAtRightLane()
        {
            // Matches the exact scenario requested: starting in the left lane, two
            // right swipes must cross the center lane and land in the right lane.
            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            controller.SetRunning(true);

            controller.MoveToLane(-1);
            yield return SettleFixedUpdates(60);
            Assert.AreEqual(-_config.LaneOffset, _playerObject.transform.position.x, 0.05f);

            controller.MoveToLane(1);
            yield return SettleFixedUpdates(60);
            Assert.AreEqual(0f, _playerObject.transform.position.x, 0.05f);

            controller.MoveToLane(1);
            yield return SettleFixedUpdates(60);
            Assert.AreEqual(_config.LaneOffset, _playerObject.transform.position.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator MoveToLane_WhenNotRunning_DoesNothing()
        {
            var controller = _playerObject.GetComponent<RunnerPlayerController>();

            controller.MoveToLane(1);
            yield return SettleFixedUpdates(10);

            Assert.AreEqual(0f, _playerObject.transform.position.x, 0.001f);
        }

        [UnityTest]
        public IEnumerator MoveToLane_AtRightmostLane_AdditionalRightSwipe_TemporarilyReducesForwardSpeed()
        {
            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            controller.SetRunning(true);

            controller.MoveToLane(1);
            controller.MoveToLane(1); // Already at rightmost lane: this should trigger the edge bump.

            yield return SettleFixedUpdates(10);

            float expectedPenalizedZ = _config.ForwardSpeed * _config.EdgeBumpSpeedMultiplier * Time.fixedDeltaTime * 10;
            Assert.AreEqual(expectedPenalizedZ, _playerObject.transform.position.z, 0.05f);
        }

        [UnityTest]
        public IEnumerator MoveToLane_AfterEdgeBumpRecoveryDistance_ForwardSpeedReturnsToNormal()
        {
            var recoveryDistanceField = typeof(RunnerGameplayConfig).GetField("_edgeBumpRecoveryDistance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            recoveryDistanceField.SetValue(_config, 1f);

            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            controller.SetRunning(true);

            controller.MoveToLane(1);
            controller.MoveToLane(1); // Triggers the edge bump with a short 1-unit recovery distance.

            yield return SettleFixedUpdates(30);
            float zAfterRecovery = _playerObject.transform.position.z;

            yield return SettleFixedUpdates(10);
            float actualStep = _playerObject.transform.position.z - zAfterRecovery;
            float expectedNormalStep = _config.ForwardSpeed * Time.fixedDeltaTime * 10;

            Assert.AreEqual(expectedNormalStep, actualStep, 0.05f);
        }

        [UnityTest]
        public IEnumerator SetRunning_False_StopsForwardMovement()
        {
            var controller = _playerObject.GetComponent<RunnerPlayerController>();
            controller.SetRunning(true);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            controller.SetRunning(false);
            float zAfterStop = _playerObject.transform.position.z;

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.AreEqual(zAfterStop, _playerObject.transform.position.z, 0.001f);
        }
    }
}
