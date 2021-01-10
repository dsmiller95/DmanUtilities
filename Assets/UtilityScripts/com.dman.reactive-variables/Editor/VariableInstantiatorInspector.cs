using UnityEditor;

namespace Dman.ReactiveVariables
{
    [CustomEditor(typeof(VariableInstantiator))]
    public class VariableInstantiatorInspector : Editor
    {
        SerializedProperty booleanStates;
        SerializedProperty floatStates;

        void OnEnable()
        {
            booleanStates = serializedObject.FindProperty("booleanStateConfig");
            floatStates = serializedObject.FindProperty("floatStateConfig");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(booleanStates);
            EditorGUILayout.PropertyField(floatStates);
            serializedObject.ApplyModifiedProperties();

            var instantiator = serializedObject.targetObject as VariableInstantiator;
            if (instantiator == null)
            {
                return;
            }
            if (instantiator.instancedBooleans != null)
            {
                foreach (var kvp in instantiator.instancedBooleans)
                {
                    ShowBooleanVariable(kvp.Key, kvp.Value);
                }
            }
            if (instantiator.instancedFloats != null)
            {
                foreach (var kvp in instantiator.instancedFloats)
                {
                    ShowFloatVariable(kvp.Key, kvp.Value);
                }
            }
        }

        private void ShowBooleanVariable(string variableName, GenericVariable<bool> variable)
        {
            EditorGUILayout.LabelField(variableName, variable.CurrentValue ? "true" : "false");
        }
        private void ShowFloatVariable(string variableName, GenericVariable<float> variable)
        {
            var formattedFloat = $"{variable.CurrentValue:F1}";
            EditorGUILayout.LabelField(variableName, formattedFloat);
        }
    }
}
