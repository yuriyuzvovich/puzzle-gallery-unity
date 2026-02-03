using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Services.Logging;

namespace PuzzleGallery.Core.StateMachine.Runtime
{
    public sealed class StateMachine : IStateMachine
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
        private IState _currentState;
        private bool _isTransitioning;

        public IState CurrentState => _currentState;

        public void RegisterState<T>(T state) where T : IState
        {
            _states[typeof(T)] = state;
        }

        public async UniTask TransitionToAsync<T>() where T : IState
        {
            if (_isTransitioning)
            {
                Logs.Warning($"Already transitioning, ignoring transition to {typeof(T).Name}");
                return;
            }

            if (!_states.TryGetValue(typeof(T), out var nextState))
            {
                throw new InvalidOperationException($"State {typeof(T).Name} is not registered");
            }

            await PerformTransitionAsync(nextState);
        }

        public async UniTask TransitionToAsync<T>(T state) where T : IState
        {
            if (_isTransitioning)
            {
                Logs.Warning($"Already transitioning, ignoring transition to {typeof(T).Name}");
                return;
            }

            var type = typeof(T);
            if (!_states.ContainsKey(type))
            {
                _states[type] = state;
            }

            await PerformTransitionAsync(state);
        }

        private async UniTask PerformTransitionAsync(IState nextState)
        {
            _isTransitioning = true;

            try
            {
                if (_currentState != null)
                {
                    await _currentState.ExitAsync();
                }

                _currentState = nextState;
                await _currentState.EnterAsync();
            }
            catch (Exception ex)
            {
                Logs.Exception(ex, "State transition failed.");
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}
