using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    public class SaveTreeContext
    {
        /// <summary>
        /// set to true if the root of the current scope tree is a global scope
        ///     used to determine if an error should be thrown if a Global Flag is encountered
        /// -- if the root is already global, a global flag component will not cause an error
        /// </summary>
        public bool isGlobal;
    }
}
