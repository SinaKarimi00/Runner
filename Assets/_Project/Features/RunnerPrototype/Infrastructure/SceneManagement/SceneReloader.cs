using UnityEngine;
using UnityEngine.SceneManagement;
using Studio.Runner3d.Features.RunnerPrototype.Application.Interfaces;

namespace Studio.Runner3d.Features.RunnerPrototype.Infrastructure.SceneManagement
{
    public sealed class SceneReloader : ISceneReloader
    {
        public void Reload()
        {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
