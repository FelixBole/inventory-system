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

        protected Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        protected void DrawTextureInBox(Texture2D background, Texture2D preview, int size, int padding = 4)
        {
            GUIStyle backgroundStyle = new GUIStyle(GUI.skin.box);
            backgroundStyle.normal.background = background;
            EditorGUILayout.BeginVertical(backgroundStyle, GUILayout.Width(size + padding), GUILayout.Height(size + padding));
            GUI.DrawTexture(GUILayoutUtility.GetRect(size, size), preview, ScaleMode.ScaleToFit, true, 1);
            EditorGUILayout.EndVertical();
        }
    }
}