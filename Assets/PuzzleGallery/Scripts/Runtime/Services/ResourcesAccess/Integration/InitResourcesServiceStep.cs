using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;

namespace PuzzleGallery.Services.ResourcesAccess
{
    public sealed class InitResourcesServiceStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.ResourcesService;
        public override string DisplayName => "Initializing Resources";
        public override float Weight => 0.05f;

        public override UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            var resourcesService = new ResourcesService();
            context.ServiceRegistry.Register<IResourcesService>(resourcesService);

            ReportProgress(progress, 1f);
            return UniTask.CompletedTask;
        }
    }
}
