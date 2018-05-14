using UnityEditor;
using UnityEngine;

namespace MorphController
{
    public static class MorphStyle
    {
        public static GUIContent IconGUIContent(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName);
        }

        public static GUIContent ObjectGUIContent(Object obj, System.Type type)
        {
            return EditorGUIUtility.ObjectContent(obj, type);
        }
    }
}
