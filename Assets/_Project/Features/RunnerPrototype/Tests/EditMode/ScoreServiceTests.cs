using NUnit.Framework;
using Studio.Runner3d.Features.RunnerPrototype.Application.Services;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.EditMode
{
    public class ScoreServiceTests
    {
        [Test]
        public void InitialScore_IsZero()
        {
            var service = new ScoreService();

            Assert.AreEqual(0, service.CurrentScore);
        }

        [Test]
        public void AddScore_PositiveAmount_IncreasesScoreAndFiresEvent()
        {
            var service = new ScoreService();
            int? observed = null;
            service.ScoreChanged += value => observed = value;

            service.AddScore(10);

            Assert.AreEqual(10, service.CurrentScore);
            Assert.AreEqual(10, observed);
        }

        [Test]
        public void AddScore_AccumulatesAcrossMultipleCalls()
        {
            var service = new ScoreService();

            service.AddScore(10);
            service.AddScore(5);

            Assert.AreEqual(15, service.CurrentScore);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void AddScore_NonPositiveAmount_DoesNotChangeScoreOrFireEvent(int amount)
        {
            var service = new ScoreService();
            int eventCount = 0;
            service.ScoreChanged += _ => eventCount++;

            service.AddScore(amount);

            Assert.AreEqual(0, service.CurrentScore);
            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void ResetScore_SetsScoreToZeroAndFiresEvent()
        {
            var service = new ScoreService();
            service.AddScore(25);
            int? observed = null;
            service.ScoreChanged += value => observed = value;

            service.ResetScore();

            Assert.AreEqual(0, service.CurrentScore);
            Assert.AreEqual(0, observed);
        }
    }
}
