﻿using System;

namespace Dman.ReactiveVariables
{
    [Serializable]
    public class FloatReference : GenericReference<float>
    {
        public FloatReference(float value) : base(value)
        {
        }

        public override GenericVariable<float> GetFromInstancer(VariableInstantiator Instancer, string NamePath)
        {
            return Instancer.GetFloatValue(NamePath);
        }
    }
}
