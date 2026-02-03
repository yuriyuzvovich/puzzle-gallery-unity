using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Features.MainMenu;
using PuzzleGallery.Services.Screen.Runtime;

namespace PuzzleGallery.Services.Screen.Integration
{
    public sealed class InitScreenServiceStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.ScreenService;
        public override string DisplayName => "Detecting Device";
        public override float Weight => 0.1f;

        public override UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            // Get config from ServiceRegistry (registered by Bootstrapper)
            var screenConfig = context.ServiceRegistry.Get<ScreenConfig>();
            var screenService = new ScreenService(screenConfig);
            context.ServiceRegistry.Register<IScreenService>(screenService);
            ReportProgress(progress, 1f);
            return UniTask.CompletedTask;
        }
    }
}
