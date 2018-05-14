using System.Collections.Generic;
using UnityEngine;

namespace MorphController
{
    public static class MorphEditorTool
    {
        private static readonly string[] ProhibitiveName = new string[] { "", "Null", "null", "<None>" };

        public static bool BoneNameIsAllow(Transform[] bones, string name)
        {
            for (int i = 0; i < ProhibitiveName.Length; i++)
            {
                if (ProhibitiveName[i] == name)
                {
                    return false;
                }
            }
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].name == name)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ClipNameIsAllow(List<MorphAnimationClip> clips, string name)
        {
            for (int i = 0; i < ProhibitiveName.Length; i++)
            {
                if (ProhibitiveName[i] == name)
                {
                    return false;
                }
            }
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].Name == name)
                {
                    return false;
                }
            }
            return true;
        }

        public static void SetGUIColor(Color color)
        {
            GUI.color = color;
        }

        public static void SetGUIContentColor(Color color)
        {
            GUI.contentColor = color;
        }

        public static void SetGUIBackgroundColor(Color color)
        {
            GUI.backgroundColor = color;
        }

        public static void SetGUIEnabled(bool enabled)
        {
            GUI.enabled = enabled;
        }
    }
}
