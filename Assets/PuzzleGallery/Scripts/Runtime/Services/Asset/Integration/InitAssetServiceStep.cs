using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;
using UnityEngine.AddressableAssets;

namespace PuzzleGallery.Services.Asset
{
    public sealed class InitAssetServiceStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.AssetService;
        public override string DisplayName => "Initializing Assets";
        public override float Weight => 0.2f;

        public override async UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            ReportProgress(progress, 0f);

            var assetService = new AddressablesAssetService();
            context.ServiceRegistry.Register<IAssetService>(assetService);

            await Addressables.InitializeAsync().ToUniTask();

            ReportProgress(progress, 1f);
        }
    }
}
