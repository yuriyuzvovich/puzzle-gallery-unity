using System;

namespace PuzzleGallery.Core.Bootstrap
{
    public interface IBootstrapProgressReporter
    {
        void OnProgressChanged(float progress, string currentStepName);
        void OnComplete();
        void OnError(string stepId, Exception error);
    }
}