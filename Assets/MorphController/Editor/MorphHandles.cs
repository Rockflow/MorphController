using UnityEngine;
using UnityEditor;

namespace MorphController
{
    public static class MorphHandles
    {
        public static void SetHandlesColor(Color color)
        {
            Handles.color = color;
        }

        public static Vector3 PositionHandle(Vector3 position)
        {
            return Handles.PositionHandle(position, Quaternion.identity);
        }

        public static Quaternion RotationHandle(Quaternion rotation, Vector3 position)
        {
            return Handles.RotationHandle(rotation, position);
        }

        public static Vector3 ScaleHandle(Vector3 scale, Vector3 position)
        {
            return Handles.ScaleHandle(scale, position, Quaternion.identity, 0.5f);
        }

        public static void DrawMorphVertex(Vector3 position, float size)
        {
            Handles.DotHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);
        }

        public static void DrawTransition(Vector2 clip, Vector2 transitionClip)
        {
            Handles.DrawBezier(clip, transitionClip, new Vector2(clip.x + 200, clip.y), new Vector2(transitionClip.x - 200, transitionClip.y), Color.white, null, 2);
        }
        
        public static void DrawMorphBone(Transform rootBone, Transform selectBone, float size)
        {
            for (int i = 0; i < rootBone.childCount; i++)
            {
                Transform bone = rootBone.GetChild(i);
                SetHandlesColor(bone == selectBone ? Color.red : Color.white);
                Handles.DrawWireCube(bone.position, new Vector3(size, size, size));
                DrawMorphSubBone(bone, selectBone, size * 0.8f);
            }
            SetHandlesColor(Color.white);
        }
        
        private static void DrawMorphSubBone(Transform rootBone, Transform selectBone, float size)
        {
            for (int i = 0; i < rootBone.childCount; i++)
            {
                Transform bone = rootBone.GetChild(i);
                SetHandlesColor(bone == selectBone ? Color.red : Color.white);
                Handles.DrawWireCube(bone.position, new Vector3(size, size, size));
                DrawJoint(rootBone.position, bone.position, size / 2);
                DrawMorphSubBone(bone, selectBone, size * 0.8f);
            }
        }

        private static void DrawJoint(Vector3 parent, Vector3 child, float radius)
        {
            Vector3 up = parent + new Vector3(0, radius, 0);
            Vector3 down = parent + new Vector3(0, -radius, 0);
            Vector3 left = parent + new Vector3(-radius, 0, 0);
            Vector3 right = parent + new Vector3(radius, 0, 0);
            Vector3 front = parent + new Vector3(0, 0, radius);
            Vector3 back = parent + new Vector3(0, 0, -radius);

            Handles.DrawLine(up, child);
            Handles.DrawLine(down, child);
            Handles.DrawLine(left, child);
            Handles.DrawLine(right, child);
            Handles.DrawLine(front, child);
            Handles.DrawLine(back, child);

            Handles.DrawLine(up, front);
            Handles.DrawLine(up, back);
            Handles.DrawLine(up, left);
            Handles.DrawLine(up, right);
            Handles.DrawLine(down, front);
            Handles.DrawLine(down, back);
            Handles.DrawLine(down, left);
            Handles.DrawLine(down, right);
            Handles.DrawLine(left, front);
            Handles.DrawLine(right, front);
            Handles.DrawLine(left, back);
            Handles.DrawLine(right, back);
        }
    }
}
