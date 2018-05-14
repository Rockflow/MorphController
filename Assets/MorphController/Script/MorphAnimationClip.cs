using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorphController
{
    [Serializable]
    public class MorphAnimationClip
    {
        public string Name;
        public int BoneNumber;
        public Vector2 Anchor;
        public List<MorphAnimationKeyframe> Keyframes;
        public bool Eligible;
        public bool Valid;
        public int TransitionClip;
        
        public MorphAnimationClip(string name, int number, Vector2 anchor)
        {
            Name = name;
            BoneNumber = number;
            Anchor = anchor;
            Keyframes = new List<MorphAnimationKeyframe>();
            Eligible = false;
            Valid = true;
            TransitionClip = -1;
        }
    }
}
