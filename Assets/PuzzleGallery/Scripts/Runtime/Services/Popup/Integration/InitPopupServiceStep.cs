using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Features.Premium;
using PuzzleGallery.Services.Asset;
using PuzzleGallery.Services.ResourcesAccess;

namespace PuzzleGallery.Services.Popup
{
    public sealed class InitPopupServiceStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.PopupService;
        public override string DisplayName => "Initializing Popups";
        public override string[] Dependencies => new[] { BootstrapStepIds.AssetService, BootstrapStepIds.ResourcesService };
        public override float Weight => 0.1f;

        public override UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            var assetService = context.ServiceRegistry.Get<IAssetService>();
            var resourcesService = context.ServiceRegistry.Get<IResourcesService>();

            // Get config from ServiceRegistry (registered by Bootstrapper)
            var premiumPopupConfig = context.ServiceRegistry.Get<PremiumPopupConfig>();

            var popupService = new PopupService(assetService, resourcesService, premiumPopupConfig);
            context.ServiceRegistry.Register<IPopupService>(popupService);

            popupService.RegisterPopup<PremiumPurchasePopup>(premiumPopupConfig.PremiumPurchaseAddress);

            ReportProgress(progress, 1f);
            return UniTask.CompletedTask;
        }
    }
}