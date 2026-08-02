using UnityEngine;
using Studio.Runner3d.Features.RunnerPrototype.Domain.Entities;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.UI;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Input;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Player;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Environment;
using Studio.Runner3d.Features.RunnerPrototype.Application.Services;
using Studio.Runner3d.Features.RunnerPrototype.Application.Interfaces;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Collectibles;
using Studio.Runner3d.Features.RunnerPrototype.Infrastructure.SceneManagement;

namespace Studio.Runner3d.Features.RunnerPrototype.Presentation.Bootstrap
{
    public sealed class RunnerCompositionRoot : MonoBehaviour
    {
        [SerializeField] private RunnerPlayerController _playerController;
        [SerializeField] private RunnerInputReader _inputReader;
        [SerializeField] private RunnerPlayerCollisionHandler _collisionHandler;
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private GameOverView _gameOverView;
        [SerializeField] private StartMenuView _startMenuView;
        [SerializeField] private ObstacleTutorialView _obstacleTutorialView;
        [SerializeField] private ObstacleTutorialTrigger _obstacleTutorialTrigger;
        [SerializeField] private Transform _collectiblesContainer;

        private RunnerGameFlowService _gameFlowService;
        private ScoreService _scoreService;
        private TutorialGateService _tutorialGateService;
        private ISceneReloader _sceneReloader;
        private bool _isPausedForTutorial;

        private void Awake()
        {
            _gameFlowService = new RunnerGameFlowService();
            _scoreService = new ScoreService();
            _tutorialGateService = new TutorialGateService();
            _sceneReloader = new SceneReloader();

            _gameFlowService.StateChanged += HandleStateChanged;
            _scoreService.ScoreChanged += HandleScoreChanged;
            _inputReader.SwipeDetected += HandleSwipeDetected;

            _gameOverView.Initialize(HandleRestartRequested);
            _startMenuView.Initialize(HandlePlayRequested);
            _collisionHandler.Initialize(HandleObstacleHit);
            _obstacleTutorialTrigger.Initialize(HandleObstacleTutorialTriggered);

            if (_collectiblesContainer != null)
            {
                CollectibleView[] collectibles = _collectiblesContainer.GetComponentsInChildren<CollectibleView>(true);

                foreach (var collectible in collectibles)
                {
                    collectible.Initialize(_scoreService.AddScore);
                }
            }

            _scoreView.ResetView();
            _gameOverView.HideImmediate();
            _startMenuView.ShowImmediate();
            _obstacleTutorialView.HideImmediate();
            _playerController.SetRunning(false);
            _inputReader.SetInputEnabled(false);
        }

        private void HandleStateChanged(RunnerGameState state)
        {
            var isRunning = state == RunnerGameState.Running;

            _playerController.SetRunning(isRunning);
            _inputReader.SetInputEnabled(isRunning);
            _collisionHandler.SetActive(isRunning);

            if (state == RunnerGameState.GameOver)
            {
                _gameOverView.Show();
            }
        }

        private void HandleScoreChanged(int newScore)
        {
            _scoreView.OnScoreChanged(newScore);
        }

        private void HandleObstacleHit()
        {
            _gameFlowService.TriggerGameOver();
        }

        private void HandleObstacleTutorialTriggered()
        {
            if (!_tutorialGateService.ShouldShowObstacleTutorial)
            {
                return;
            }

            _tutorialGateService.MarkObstacleTutorialShown();
            _obstacleTutorialView.Show();

            _isPausedForTutorial = true;
            Time.timeScale = 0f;
        }

        private void HandleSwipeDetected(int direction)
        {
            if (!_isPausedForTutorial)
            {
                return;
            }

            _isPausedForTutorial = false;
            Time.timeScale = 1f;
            _obstacleTutorialView.Hide();
        }

        private void HandleRestartRequested()
        {
            _sceneReloader.Reload();
        }

        private void HandlePlayRequested()
        {
            _startMenuView.Hide();
            _gameFlowService.StartRun();
        }

        private void OnDestroy()
        {
            if (_gameFlowService != null)
            {
                _gameFlowService.StateChanged -= HandleStateChanged;
            }

            if (_scoreService != null)
            {
                _scoreService.ScoreChanged -= HandleScoreChanged;
            }

            if (_inputReader != null)
            {
                _inputReader.SwipeDetected -= HandleSwipeDetected;
            }
        }
    }
}