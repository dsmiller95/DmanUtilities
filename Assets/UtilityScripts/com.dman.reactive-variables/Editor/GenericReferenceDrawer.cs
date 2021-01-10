using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    public abstract class GenericReferenceDrawer : PropertyDrawer
    {
        const int dataPathHeight = 18;
        const int dropdownWidth = 100;

        // Here you must define the height of your property drawer. Called by Unity.
        public override float GetPropertyHeight(SerializedProperty prop,
                                                 GUIContent label)
        {
            ReferenceDataSource dataSource = DataSource(prop);
            if (dataSource == ReferenceDataSource.INSTANCER)
                return base.GetPropertyHeight(prop, label) + dataPathHeight;
            else
                return base.GetPropertyHeight(prop, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReferenceDataSource dataSource = DataSource(property);
            if (dataSource != ReferenceDataSource.INSTANCER)
            {
                var topRowPosition = new Rect(position);

                label = EditorGUI.BeginProperty(topRowPosition, label, property);

                DrawObjectSelection(topRowPosition, property, label);
            }
            else
            {
                var topRowPosition = new Rect(position);
                topRowPosition.yMax -= dataPathHeight;

                label = EditorGUI.BeginProperty(topRowPosition, label, property);

                DrawObjectSelection(topRowPosition, property, label);

                var bottomRowPosition = new Rect(position);
                bottomRowPosition.yMin = bottomRowPosition.yMax - dataPathHeight;
                DrawPathSelection(bottomRowPosition, property);
            }
            EditorGUI.EndProperty();
        }

        protected abstract List<string> GetValidNamePaths(VariableInstantiator instantiator);

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
        ///  draw the first row
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
