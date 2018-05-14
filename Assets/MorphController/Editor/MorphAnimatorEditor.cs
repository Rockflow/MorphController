using UnityEditor;
using UnityEngine;

namespace MorphController
{
    [CustomEditor(typeof(MorphAnimator))]
    public class MorphAnimatorEditor : Editor
    {
        private MorphAnimator _morphAnimator;

        private void OnEnable()
        {
            _morphAnimator = target as MorphAnimator;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Update Mode", GUILayout.Width(120));
            _morphAnimator.UpdateMode = (UpdateType)EditorGUILayout.EnumPopup(_morphAnimator.UpdateMode);
            GUILayout.EndHorizontal();

            if (_morphAnimator.UpdateMode == UpdateType.Update)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ignore TimeScale", GUILayout.Width(120));
                _morphAnimator.IgnoreTimeScale = GUILayout.Toggle(_morphAnimator.IgnoreTimeScale, "");
                GUILayout.EndHorizontal();
            }
            else
            {
                _morphAnimator.IgnoreTimeScale = false;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed", GUILayout.Width(120));
            _morphAnimator.Speed = EditorGUILayout.FloatField(_morphAnimator.Speed);
            GUILayout.EndHorizontal();

            string defaultClip = (_morphAnimator.DefaultClipIndex == -1 ? "<None>" : _morphAnimator.Clips[_morphAnimator.DefaultClipIndex].Name);
            EditorGUILayout.HelpBox("Clip Count:" + _morphAnimator.Clips.Count + "\r\n" + "Default Clip:" + defaultClip, MessageType.Info);
        }
    }
}
