using UnityEditor;
using UnityEngine;

namespace Dman.Utilities
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var topRowPosition = new Rect(position);

            DrawSceneSelection(topRowPosition, property, label);
        }

        /// <summary>
        ///  draw the first row
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        private void DrawSceneSelection(Rect position, SerializedProperty property, GUIContent label)
        {
            var scenePathProperty = property.FindPropertyRelative(nameof(SceneReference.scenePath));
            var scenePath = scenePathProperty.stringValue;
            var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            var newScene = EditorGUI.ObjectField(position, property.displayName, oldScene, typeof(SceneAsset), false) as SceneAsset;
            if (GUI.changed)
            {
                var newPath = AssetDatabase.GetAssetPath(newScene);
                scenePathProperty.stringValue = newPath;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}