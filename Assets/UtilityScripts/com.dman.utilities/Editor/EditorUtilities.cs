using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dman.Utilities
{
    public static class EditorUtilities
    {
        public static void DrawSettingsEditor(UnityEngine.Object settings, ref bool foldout, ref Editor editor)
        {
            if (settings != null)
            {
                foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
                if (foldout)
                {
                    Editor.CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();
                }

            }
        }

        public static void DrawButton(string name, Action onClicked)
        {
            if (GUILayout.Button(name))
            {
                onClicked();
            }
        }
    }
}
