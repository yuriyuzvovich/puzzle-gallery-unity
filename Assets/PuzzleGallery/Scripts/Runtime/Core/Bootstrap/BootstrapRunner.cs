using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Services.Logging;

namespace PuzzleGallery.Core.Bootstrap
{
    public sealed class BootstrapRunner
    {
        private readonly List<IBootstrapStep> _steps = new List<IBootstrapStep>();
        private readonly IBootstrapContext _context;
        private IBootstrapProgressReporter _progressReporter;

        public BootstrapRunner(IBootstrapContext context)
        {
            _context = context;
        }

        public BootstrapRunner AddStep(IBootstrapStep step)
        {
            _steps.Add(step);
            return this;
        }

        public BootstrapRunner SetProgressReporter(IBootstrapProgressReporter reporter)
        {
            _progressReporter = reporter;
            return this;
        }

        public async UniTask RunAsync()
        {
            var sortedSteps = TopologicalSort(_steps);
            var totalWeight = sortedSteps.Sum(s => s.Weight);
            var completedWeight = 0f;

            foreach (var step in sortedSteps)
            {
                _progressReporter?.OnProgressChanged(
                    completedWeight / totalWeight,
                    step.DisplayName);

                try
                {
                    var stepProgress = new Progress<float>(p =>
                    {
                        var overallProgress = (completedWeight + p * step.Weight) / totalWeight;
                        _progressReporter?.OnProgressChanged(overallProgress, step.DisplayName);
                    });

                    await step.ExecuteAsync(_context, stepProgress);
                }
                catch (Exception ex)
                {
                    Logs.Exception(ex, $"Bootstrap step '{step.StepId}' failed.");
                    _progressReporter?.OnError(step.StepId, ex);
                    throw;
                }

                completedWeight += step.Weight;
            }

            _progressReporter?.OnProgressChanged(1f, "Complete");
            _progressReporter?.OnComplete();
        }

        private List<IBootstrapStep> TopologicalSort(List<IBootstrapStep> steps)
        {
            var stepMap = steps.ToDictionary(s => s.StepId);
            var inDegree = steps.ToDictionary(s => s.StepId, s => 0);
            var adjacency = steps.ToDictionary(s => s.StepId, s => new List<string>());

            foreach (var step in steps)
            {
                foreach (var dep in step.Dependencies)
                {
                    if (!stepMap.ContainsKey(dep))
                    {
                        throw new InvalidOperationException(
                            $"Step '{step.StepId}' depends on unknown step '{dep}'");
                    }
                    adjacency[dep].Add(step.StepId);
                    inDegree[step.StepId]++;
                }
            }

            var queue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var result = new List<IBootstrapStep>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(stepMap[current]);

                foreach (var neighbor in adjacency[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (result.Count != steps.Count)
            {
                throw new InvalidOperationException(
                    "Circular dependency detected in bootstrap steps");
            }

            return result;
        }
    }
}
