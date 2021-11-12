using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dman.SceneSaveSystem.PlaymodeTests
{
    public class SimpleSaveable : MonoBehaviour, ISaveableData
    {
        public string MySavedData;
        public string uniqueNameInScope = "name";

        public string UniqueSaveIdentifier => "SimpleSaveableData" + uniqueNameInScope;

        public int loadOrder = 0;
        public int LoadOrderPriority { get => loadOrder; set => loadOrder = value; }

        public static event Action<SimpleSaveable> OnLoad;

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }

        public object GetSaveObject()
        {
            return MySavedData;
        }

        public void SetupFromSaveObject(object save)
        {
            if(save is string saved)
            {
                MySavedData = saved;
                OnLoad?.Invoke(this);
            }
        }
    }
}
