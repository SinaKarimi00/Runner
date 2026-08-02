using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Runner3d.Features.RunnerPrototype.Infrastructure.Configuration
{
    public static class RunnerGameplayConfigLoader
    {
        public const string AddressableKey = "RunnerGameplayConfig";

        public static AsyncOperationHandle<RunnerGameplayConfig> LoadAsync(Action<RunnerGameplayConfig> onLoaded)
        {
            AsyncOperationHandle<RunnerGameplayConfig> handle = Addressables.LoadAssetAsync<RunnerGameplayConfig>(AddressableKey);
            handle.Completed += handleResult =>
            {
                onLoaded?.Invoke(handleResult.Status == AsyncOperationStatus.Succeeded ? handleResult.Result : null);
            };

            return handle;
        }
    }
}
