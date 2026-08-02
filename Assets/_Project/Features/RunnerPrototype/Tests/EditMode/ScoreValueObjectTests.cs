using NUnit.Framework;
using Studio.Runner3d.Features.RunnerPrototype.Domain.ValueObjects;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.EditMode
{
    public class ScoreValueObjectTests
    {
        [Test]
        public void Zero_HasValueZero()
        {
            Assert.AreEqual(0, Score.Zero.Value);
        }

        [Test]
        public void Constructor_NegativeValue_ClampsToZero()
        {
            var score = new Score(-10);

            Assert.AreEqual(0, score.Value);
        }

        [Test]
        public void Add_PositiveAmount_IncreasesValue()
        {
            var score = Score.Zero.Add(10);

            Assert.AreEqual(10, score.Value);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Add_NonPositiveAmount_ReturnsUnchangedValue(int amount)
        {
            var score = new Score(10).Add(amount);

            Assert.AreEqual(10, score.Value);
        }
    }
}
