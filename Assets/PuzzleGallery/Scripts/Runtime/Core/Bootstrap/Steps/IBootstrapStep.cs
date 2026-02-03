using System;
using Cysharp.Threading.Tasks;

namespace PuzzleGallery.Core.Bootstrap
{
    public interface IBootstrapStep
    {
        string StepId { get; }

        string DisplayName { get; }

        string[] Dependencies { get; }

        float Weight { get; }

        UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null);
    }
}
