namespace Studio.Runner3d.Features.RunnerPrototype.Application.Services
{
    public sealed class TutorialGateService
    {
        // Static so the flag survives SceneReloader's SceneManager.LoadScene (in-session restart),
        // but resets on a fresh Play session / app launch (domain reload).
        private static bool _hasShownObstacleTutorial;

        public bool ShouldShowObstacleTutorial => !_hasShownObstacleTutorial;

        public void MarkObstacleTutorialShown()
        {
            _hasShownObstacleTutorial = true;
        }

        public static void ResetSession()
        {
            _hasShownObstacleTutorial = false;
        }
    }
}
