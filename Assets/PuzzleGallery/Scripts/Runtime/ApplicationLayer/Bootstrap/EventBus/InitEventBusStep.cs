using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Core.EventBus;

namespace PuzzleGallery.Bootstrap
{
    public sealed class InitEventBusStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.EventBus;
        public override string DisplayName => "Initializing Events";
        public override float Weight => 0.1f;

        public override UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            var eventBus = new GlobalEventBus();
            context.ServiceRegistry.Register<GlobalEventBus>(eventBus);
            ReportProgress(progress, 1f);
            return UniTask.CompletedTask;
        }
    }
}
