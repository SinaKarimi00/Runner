using NUnit.Framework;
using Studio.Runner3d.Features.RunnerPrototype.Domain.Entities;
using Studio.Runner3d.Features.RunnerPrototype.Application.Services;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.EditMode
{
    public class RunnerGameFlowServiceTests
    {
        [Test]
        public void InitialState_IsReady()
        {
            var service = new RunnerGameFlowService();

            Assert.AreEqual(RunnerGameState.Ready, service.State);
        }

        [Test]
        public void StartRun_FromReady_TransitionsToRunningAndReturnsTrue()
        {
            var service = new RunnerGameFlowService();

            bool result = service.StartRun();

            Assert.IsTrue(result);
            Assert.AreEqual(RunnerGameState.Running, service.State);
        }

        [Test]
        public void StartRun_WhenAlreadyRunning_ReturnsFalseAndDoesNotChangeState()
        {
            var service = new RunnerGameFlowService();
            service.StartRun();

            bool result = service.StartRun();

            Assert.IsFalse(result);
            Assert.AreEqual(RunnerGameState.Running, service.State);
        }

        [Test]
        public void TriggerGameOver_WhenRunning_TransitionsToGameOverAndReturnsTrue()
        {
            var service = new RunnerGameFlowService();
            service.StartRun();

            bool result = service.TriggerGameOver();

            Assert.IsTrue(result);
            Assert.AreEqual(RunnerGameState.GameOver, service.State);
        }

        [Test]
        public void TriggerGameOver_WhenNotRunning_ReturnsFalseAndDoesNotFireEventAgain()
        {
            var service = new RunnerGameFlowService();
            service.StartRun();
            service.TriggerGameOver();

            int eventCount = 0;
            service.StateChanged += _ => eventCount++;

            bool secondResult = service.TriggerGameOver();

            Assert.IsFalse(secondResult);
            Assert.AreEqual(0, eventCount);
            Assert.AreEqual(RunnerGameState.GameOver, service.State);
        }

        [Test]
        public void StateChanged_InvokedWithNewState_OnStartRun()
        {
            var service = new RunnerGameFlowService();
            RunnerGameState? observed = null;
            service.StateChanged += state => observed = state;

            service.StartRun();

            Assert.AreEqual(RunnerGameState.Running, observed);
        }
    }
}
