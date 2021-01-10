using System;
using System.Collections.Generic;
using UnityEditor;

namespace Dman.ReactiveVariables
{
    [CustomPropertyDrawer(typeof(IntReference))]
    public class IntReferenceDrawer : GenericReferenceDrawer
    {
        protected override List<string> GetValidNamePaths(VariableInstantiator instantiator)
        {
            throw new NotImplementedException();
        }
    }
}
