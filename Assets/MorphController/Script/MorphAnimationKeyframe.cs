using System;
using UnityEngine;

namespace MorphController
{
    [Serializable]
    public class MorphAnimationKeyframe
    {
        public Vector3[] Positions;
        public Quaternion[] Rotations;
        public Vector3[] Scales;
        public MorphCallBack EventCallBack;
        public float Time;

        public MorphAnimationKeyframe(MorphAnimationData data)
        {
            Positions = data.BindposesPosition.ToArray();
            Rotations = data.BindposesRotation.ToArray();
            Scales = data.BindposesScale.ToArray();
            EventCallBack = new MorphCallBack();
            Time = 1f;
        }

        public void CopyBy(MorphAnimationKeyframe frame)
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                Positions[i] = frame.Positions[i];
                Rotations[i] = frame.Rotations[i];
                Scales[i] = frame.Scales[i];
            }
            EventCallBack = new MorphCallBack();
            Time = frame.Time;
        }
    }
}
