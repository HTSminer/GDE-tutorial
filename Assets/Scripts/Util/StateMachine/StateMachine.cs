using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PKMNUtils.StateMachine
{
    public class StateMachine<T>
    {
        private State<T> _currentState;
        public State<T> CurrentState 
        {
            get => _currentState;
            private set
            {
                _currentState = value;
                OnStateChanged?.Invoke(_currentState);
            }
        }

        public event Action<State<T>> OnStateChanged;

        public Stack<State<T>> StateStack { get; private set; }

        T owner;
        public StateMachine(T owner)
        {
            this.owner = owner;
            StateStack = new Stack<State<T>>();
        }

        public void Execute() => CurrentState?.Execute();

        public void Push(State<T> newState)
        {
            StateStack.Push(newState);
            CurrentState = newState;
            CurrentState.Enter(owner);
        }

        public void Pop()
        {
            StateStack.Pop();
            CurrentState.Exit();
            CurrentState = StateStack.Peek();
        }

        public void ChangeState(State<T> newState)
        {
            if (CurrentState != null)
            {
                CurrentState.Exit();
                StateStack.Pop();
            }

            StateStack.Push(newState);
            CurrentState = newState;
            CurrentState.Enter(owner);
        }

        public IEnumerator PushAndWait(State<T> newState)
        {
            var oldState = CurrentState;
            Push(newState);
            yield return new WaitUntil(() => CurrentState == oldState);
        }

        public State<T> GetPrevState()
        {
            if (StateStack.Count > 1)
                return StateStack.ElementAt(1);

            return null;
        }

        public bool IsStateInStack(State<T> state) => StateStack.Contains(state);
    }
}
