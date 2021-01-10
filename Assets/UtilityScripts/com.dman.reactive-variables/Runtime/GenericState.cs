using UnityEngine;

namespace Dman.ReactiveVariables
{
    public abstract class GenericState<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif

        public string IdentifierInInstantiator;

        public abstract GenericVariable<T> GenerateNewVariable();

        public abstract object GetSaveObjectFromVariable(GenericVariable<T> variable);

        public abstract void SetSaveObjectIntoVariable(GenericVariable<T> variable, object savedValue);
    }
}
