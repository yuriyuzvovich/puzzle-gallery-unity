using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Bootstrap;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Services.Logging.Data;

namespace PuzzleGallery.Services.Logging
{
    public sealed class InitLogsServiceStep : BootstrapStepBase
    {
        public override string StepId => BootstrapStepIds.LogsService;
        public override string DisplayName => "Initializing Logs";
        public override float Weight => 0.05f;

        public override UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null)
        {
            var logsConfig = context.ServiceRegistry.Get<LogsConfig>();
            var logsService = new LogsService(logsConfig);
            context.ServiceRegistry.Register<ILogsService>(logsService);
            ReportProgress(progress, 1f);
            return UniTask.CompletedTask;
        }
    }
}
