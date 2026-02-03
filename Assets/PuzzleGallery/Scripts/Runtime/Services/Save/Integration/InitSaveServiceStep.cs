using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Services.Save.Runtime;

namespace PuzzleGallery.Services.Save.Integration
{
    public sealed class InitSaveServiceStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.SaveService;
        public override string DisplayName => "Initializing Save";
        public override float Weight => 0.1f;

        public override UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            var saveService = new LocalStorageService(new JsonSaveStrategy());
            context.ServiceRegistry.Register<ILocalStorageService>(saveService);
            ReportProgress(progress, 1f);
            return UniTask.CompletedTask;
        }
    }
}
