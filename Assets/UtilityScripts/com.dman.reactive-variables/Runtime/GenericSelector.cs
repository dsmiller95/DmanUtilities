using System;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    public abstract class GenericSelector<T> : ScriptableObject
    {
        public abstract T GetCurrentValue(VariableInstantiator instancer);
        public abstract IObservable<T> ValueChanges(VariableInstantiator instancer);
    }
}
