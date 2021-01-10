using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Dman.ReactiveVariables
{
    [CustomPropertyDrawer(typeof(BooleanReference))]
    public class BooleanReferenceDrawer : GenericReferenceDrawer
    {
        protected override List<string> GetValidNamePaths(VariableInstantiator instantiator)
        {
            return instantiator.booleanStateConfig.Select(x => x.IdentifierInInstantiator).ToList();
        }
    }
}
