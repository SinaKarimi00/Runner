using System;
using Studio.Runner3d.Features.RunnerPrototype.Domain.Entities;

namespace Studio.Runner3d.Features.RunnerPrototype.Application.Services
{
    public sealed class RunnerGameFlowService
    {
        public event Action<RunnerGameState> StateChanged;

        public RunnerGameState State { get; private set; } = RunnerGameState.Ready;

        public bool StartRun()
        {
            if (State != RunnerGameState.Ready)
            {
                return false;
            }

            State = RunnerGameState.Running;
            StateChanged?.Invoke(State);
            return true;
        }

        public bool TriggerGameOver()
        {
            if (State != RunnerGameState.Running)
            {
                return false;
            }

            State = RunnerGameState.GameOver;
            StateChanged?.Invoke(State);
            return true;
        }
    }
}
