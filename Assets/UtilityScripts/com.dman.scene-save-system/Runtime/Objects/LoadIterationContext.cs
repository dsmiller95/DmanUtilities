namespace Dman.SceneSaveSystem.Objects
{
    internal struct LoadIterationContext
    {
        /// <summary>
        /// set to true if the root of the current scope tree is a global scope
        ///     used to determine if an error should be thrown if a Global Flag is encountered
        /// -- if the root is already global, a global flag component will not cause an error
        /// </summary>
        public bool isGlobal;
        public SaveScopeData currentScope;
        public SaveScopeData globalScope;
        public SaveablePrefabRegistry prefabRegistry;

        public LoadIterationContext ForChildScope(SaveScopeData childScope)
        {
            return new LoadIterationContext
            {
                isGlobal = isGlobal,
                currentScope = childScope,
                globalScope = globalScope,
                prefabRegistry = prefabRegistry
            };
        }
    }
}
