using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Services.Logging;

namespace PuzzleGallery.Services.Asset
{
    public sealed class PreloadAssetsStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.PreloadAssets;
        public override string DisplayName => "Loading Assets";
        public override string[] Dependencies => new[] { BootstrapStepIds.AssetService };
        public override float Weight => 0.5f;

        public override async UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            // Get assets to preload from bootstrap config
            var assets = context.Config.AssetsToPreload;
            if (assets == null || assets.Length == 0)
            {
                ReportProgress(progress, 1f);
                return;
            }

            var assetService = context.ServiceRegistry.Get<IAssetService>();
            var step = 1f / assets.Length;

            for (int i = 0; i < assets.Length; i++)
            {
                try
                {
                    await assetService.LoadAssetAsync<UnityEngine.Object>(assets[i]);
                }
                catch (Exception ex)
                {
                    Logs.Warning($"Failed to preload asset '{assets[i]}': {ex.Message}");
                }

                ReportProgress(progress, (i + 1) * step);
            }
        }
    }
}
