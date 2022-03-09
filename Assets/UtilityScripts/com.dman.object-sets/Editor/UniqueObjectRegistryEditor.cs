using Dman.Utilities;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dman.ObjectSets
{
    [CustomEditor(typeof(UniqueObjectRegistry), true)]
    public class UniqueObjectRegistryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Reassign unique IDs"))
            {
                var registry = serializedObject.targetObject as UniqueObjectRegistry;
                registry.AssignAllIDs();
                foreach (var registryObject in registry.AllObjects)
                {
                    EditorUtility.SetDirty(registryObject);
                }
                EditorUtility.SetDirty(registry);
                Debug.Log("Successfully reset all object IDs. All save files may be invalid.");
            }
            if (GUILayout.Button("Auto populate"))
            {
                var registry = serializedObject.targetObject as UniqueObjectRegistry;
                var baseType = serializedObject.targetObject.GetType();
                var targetAssembly = typeof(UniqueObjectRegistry).Assembly;
                while (baseType.Assembly != targetAssembly && baseType != null)
                {
                    baseType = baseType.BaseType;
                }
                var genericOptions = baseType.GenericTypeArguments;
                var trueType = genericOptions[0];

                var allObjects = AssetUtilities.GetAllInstances(trueType);
                registry.AllObjects = allObjects.Cast<IDableObject>().ToList();

                registry.AssignAllIDs();
                foreach (var registryObject in registry.AllObjects)
                {
                    EditorUtility.SetDirty(registryObject);
                }
                EditorUtility.SetDirty(registry);
                Debug.Log("Successfully reset all object IDs. All save files may be invalid.");
            }
        }
    }
}