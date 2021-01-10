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
                Debug.Log("Successfully reset all object IDs. All save files may be invalid.");
            }
        }
    }
}