using System;

namespace Dman.ReactiveVariables
{
    [Serializable]
    public class BooleanReference : GenericReference<bool>
    {
        public BooleanReference(bool value) : base(value)
        {
        }

        public override GenericVariable<bool> GetFromInstancer(VariableInstantiator Instancer, string NamePath)
        {
            return Instancer.GetBooleanValue(NamePath);
        }

        public static implicit operator BooleanReference(bool value) => new BooleanReference(value);
    }
}
