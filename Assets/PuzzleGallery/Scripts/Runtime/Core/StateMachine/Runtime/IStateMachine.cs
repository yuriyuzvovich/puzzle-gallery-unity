using Cysharp.Threading.Tasks;

namespace PuzzleGallery.Core.StateMachine.Runtime
{
    public interface IStateMachine
    {
        IState CurrentState { get; }

        UniTask TransitionToAsync<T>() where T : IState;

        UniTask TransitionToAsync<T>(T state) where T : IState;
    }
}
