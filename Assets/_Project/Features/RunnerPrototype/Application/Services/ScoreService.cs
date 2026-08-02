using System;
using Studio.Runner3d.Features.RunnerPrototype.Domain.ValueObjects;

namespace Studio.Runner3d.Features.RunnerPrototype.Application.Services
{
    public sealed class ScoreService
    {
        public event Action<int> ScoreChanged;

        private Score _score = Score.Zero;

        public int CurrentScore => _score.Value;

        public void AddScore(int amount)
        {
            Score updated = _score.Add(amount);
            if (updated.Value == _score.Value)
            {
                return;
            }

            _score = updated;
            ScoreChanged?.Invoke(_score.Value);
        }

        public void ResetScore()
        {
            _score = Score.Zero;
            ScoreChanged?.Invoke(_score.Value);
        }
    }
}
