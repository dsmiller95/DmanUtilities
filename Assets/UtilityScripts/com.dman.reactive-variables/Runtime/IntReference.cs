using System;

namespace Dman.ReactiveVariables
{
    [Serializable]
    public class IntReference : GenericReference<int>
    {
        public IntReference(int value) : base(value)
        {
        }

        public override GenericVariable<int> GetFromInstancer(VariableInstantiator Instancer, string NamePath)
        {
            throw new NotImplementedException();
        }
    }
}
