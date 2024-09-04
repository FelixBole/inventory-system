using UnityEditor;
using UnityEngine;

namespace Slax.Inventory
{
    public class EditorWithQuickAccessMethods: UnityEditor.Editor
    {
        protected void BV(bool helpBox = false)
        {
            if (helpBox)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            }
            else
            {
                EditorGUILayout.BeginVertical();
            }
        }

        protected void EV()
        {
            EditorGUILayout.EndVertical();
        }

        protected void BH(bool helpBox = false)
        {
            if (helpBox)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
            }
        }

        protected void EH()
        {
            EditorGUILayout.EndHorizontal();
        }

        protected void Space(int space = 5)
        {
            EditorGUILayout.Space(space);
        }

        protected void BSV(ref Vector2 scrollPos)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        }

        protected void ESV()
        {
            EditorGUILayout.EndScrollView();
        }

        protected void PropertyFieldFor(string propertyName, string displayName, bool includeChildren = true)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(displayName), includeChildren);
        }

        protected void BoldLabel(string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }
    }
}