using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    public abstract class GenericReferenceDrawer : PropertyDrawer
    {
        const int dataPathHeight = 18;
        const int referenceDropdownHeight = 18;
        const int dropdownWidth = 100;

        // Here you must define the height of your property drawer. Called by Unity.
        public override float GetPropertyHeight(SerializedProperty prop,
                                                 GUIContent label)
        {
            ReferenceDataSource dataSource = DataSource(prop);
            if (dataSource == ReferenceDataSource.INSTANCER)
                return base.GetPropertyHeight(prop, label) + dataPathHeight;
            else if (dataSource == ReferenceDataSource.SINGLETON_VARIABLE)
                return base.GetPropertyHeight(prop, label) + (prop.isExpanded ? referenceDropdownHeight : 0);
            else
                return base.GetPropertyHeight(prop, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReferenceDataSource dataSource = DataSource(property);
            var topRowPosition = new Rect(position);
            if (dataSource == ReferenceDataSource.INSTANCER)
            {
                topRowPosition.yMax -= dataPathHeight;

                label = EditorGUI.BeginProperty(topRowPosition, label, property);
                DrawObjectSelection(topRowPosition, property, label);

                var bottomRowPosition = new Rect(position);
                bottomRowPosition.yMin = bottomRowPosition.yMax - dataPathHeight;
                DrawPathSelection(bottomRowPosition, property);
                EditorGUI.EndProperty();
            }
            else if (dataSource == ReferenceDataSource.SINGLETON_VARIABLE)
            {
                if (property.isExpanded)
                {
                    topRowPosition.yMax -= referenceDropdownHeight;
                }

                var genericVariable = property.FindPropertyRelative("Variable");
                var variableValue = genericVariable.objectReferenceValue;
                // Draw foldout arrow
                if (variableValue != null)
                {
                    property.isExpanded = EditorGUI.Foldout(topRowPosition, property.isExpanded, GUIContent.none);
                }
                else
                {
                    property.isExpanded = false;
                }


                label = EditorGUI.BeginProperty(topRowPosition, label, property);
                DrawObjectSelection(topRowPosition, property, label);
                EditorGUI.EndProperty();


                if (property.isExpanded)
                {
                    var bottomRowPosition = new Rect(position);
                    bottomRowPosition.yMin = bottomRowPosition.yMax - referenceDropdownHeight;
                    DrawCurrentSingletonValue(bottomRowPosition, genericVariable);
                }
            }
            else
            {
                label = EditorGUI.BeginProperty(topRowPosition, label, property);
                DrawObjectSelection(topRowPosition, property, label);
                EditorGUI.EndProperty();
            }
        }

        protected abstract List<string> GetValidNamePaths(VariableInstantiator instantiator);


        // Cached scriptable object editor
        private Editor editor = null;

        private void DrawCurrentSingletonValue(Rect position, SerializedProperty genericVariable)
        {
            var variableValue = genericVariable.objectReferenceValue;
            if (!editor)
                Editor.CreateCachedEditor(variableValue, null, ref editor);

            var inspectorValue = editor.serializedObject.FindProperty("InspectableValue");

            var label = new GUIContent("Current Value");
            //var changed = EditorGUILayout.PropertyField(inspectorValue, label);

            EditorGUI.indentLevel++;
            position = EditorGUI.IndentedRect(position);
            EditorGUI.indentLevel--;

            EditorGUI.PropertyField(position,
                inspectorValue,
                label);
            editor.serializedObject.ApplyModifiedProperties();
        }


        /// <summary>
        /// draws a path selector for an instancer type
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        private void DrawPathSelection(Rect position, SerializedProperty property)
        {
            SerializedProperty instancer = property.FindPropertyRelative("Instancer");
            var instancerObj = instancer.objectReferenceValue as VariableInstantiator;
            if (instancerObj == null)
            {
                EditorGUI.HelpBox(position, "Set the instantiator", MessageType.Error);
                return;
            }

            SerializedProperty namePath = property.FindPropertyRelative("NamePath");

            EditorGUI.BeginChangeCheck();
            var selectionOptions = GetValidNamePaths(instancerObj);
            var currentPathIndex = selectionOptions.IndexOf(namePath.stringValue);

            int newPathIndex = EditorGUI.Popup(position, currentPathIndex, selectionOptions.ToArray());
            // var newpath = EditorGUI.TextField(position, namePath.stringValue);

            if (newPathIndex < 0)// && !instancerObj.variableInstancingConfig.Any(x => x.name == newpath))
            {
                namePath.stringValue = "";
            }
            else
            {
                namePath.stringValue = selectionOptions[newPathIndex];
            }
            EditorGUI.EndChangeCheck();
        }

        /// <summary>
        ///  draw the first row: a dropdown to select the type of refernce this is, and a value field to set the primary property of the reference
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        private void DrawObjectSelection(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            // Get properties
            SerializedProperty dataSource = property.FindPropertyRelative("DataSource");

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.width = dropdownWidth;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            ReferenceDataSource dataSourceEnum = (ReferenceDataSource)dataSource.enumValueIndex;
            dataSourceEnum = (ReferenceDataSource)EditorGUI.EnumPopup(buttonRect, dataSourceEnum);

            //int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);

            dataSource.enumValueIndex = (int)dataSourceEnum;

            switch (dataSourceEnum)
            {
                case ReferenceDataSource.CONSTANT:
                    SerializedProperty constantValue = property.FindPropertyRelative("ConstantValue");
                    if (constantValue == null)
                    {
                        EditorGUI.HelpBox(position, "Cannot set constant value", MessageType.Error);
                    }
                    else
                    {
                        EditorGUI.PropertyField(position,
                            constantValue,
                            GUIContent.none);
                    }
                    break;
                case ReferenceDataSource.SINGLETON_VARIABLE:
                    SerializedProperty variable = property.FindPropertyRelative("Variable");
                    EditorGUI.PropertyField(position,
                        variable,
                        GUIContent.none);
                    break;
                case ReferenceDataSource.INSTANCER:
                    SerializedProperty instancer = property.FindPropertyRelative("Instancer");
                    EditorGUI.PropertyField(position,
                        instancer,
                        GUIContent.none);
                    break;
                default:
                    break;
            }

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
        }



        private ReferenceDataSource DataSource(SerializedProperty prop)
        {
            SerializedProperty dataSource = prop.FindPropertyRelative("DataSource");
            return (ReferenceDataSource)dataSource.enumValueIndex;
        }
    }
}
