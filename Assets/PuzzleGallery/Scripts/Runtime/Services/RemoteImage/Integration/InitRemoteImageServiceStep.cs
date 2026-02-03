using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Features.MainMenu;

namespace PuzzleGallery.Services.RemoteImage
{
    public sealed class InitRemoteImageServiceStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.RemoteImageService;
        public override string DisplayName => "Initializing Images";
        public override float Weight => 0.1f;

        public override UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            var remoteImageConfig = context.ServiceRegistry.Get<RemoteImageConfig>();
            var imageService = new RemoteImageService(remoteImageConfig);
            context.ServiceRegistry.Register<IRemoteImageService>(imageService);
            ReportProgress(progress, 1f);
            return UniTask.CompletedTask;
        }
    }
}
