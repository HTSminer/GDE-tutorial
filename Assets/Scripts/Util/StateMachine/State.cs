using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace PKMNUtils.StateMachine
{
    public class State<T> : MonoBehaviour
    {
        public virtual void Enter(T owner) { }

        public virtual void Execute() { }

        public virtual void Exit() { }
    }
}
