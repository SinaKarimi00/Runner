using NUnit.Framework;
using Studio.Runner3d.Features.RunnerPrototype.Application.Services;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.EditMode
{
    public class TutorialGateServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            TutorialGateService.ResetSession();
        }

        [TearDown]
        public void TearDown()
        {
            TutorialGateService.ResetSession();
        }

        [Test]
        public void ShouldShowObstacleTutorial_NewSession_IsTrue()
        {
            var service = new TutorialGateService();

            Assert.IsTrue(service.ShouldShowObstacleTutorial);
        }

        [Test]
        public void MarkObstacleTutorialShown_SuppressesFurtherShowing()
        {
            var service = new TutorialGateService();

            service.MarkObstacleTutorialShown();

            Assert.IsFalse(service.ShouldShowObstacleTutorial);
        }

        [Test]
        public void MarkObstacleTutorialShown_PersistsAcrossNewServiceInstances()
        {
            var service = new TutorialGateService();
            service.MarkObstacleTutorialShown();

            var serviceAfterReload = new TutorialGateService();

            Assert.IsFalse(serviceAfterReload.ShouldShowObstacleTutorial);
        }

        [Test]
        public void ResetSession_RestoresShouldShowObstacleTutorial()
        {
            var service = new TutorialGateService();
            service.MarkObstacleTutorialShown();

            TutorialGateService.ResetSession();

            Assert.IsTrue(service.ShouldShowObstacleTutorial);
        }
    }
}
