using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PuzzleGallery.Core.Bootstrap
{
    public abstract class BootstrapStepBase : IBootstrapStep
    {
        public abstract string StepId { get; }
        public abstract string DisplayName { get; }
        public virtual string[] Dependencies => Array.Empty<string>();
        public virtual float Weight => 1f;

        public abstract UniTask ExecuteAsync(IBootstrapContext context, IProgress<float> progress = null);

        protected void ReportProgress(IProgress<float> progress, float value)
        {
            progress?.Report(Mathf.Clamp01(value));
        }
    }
}
