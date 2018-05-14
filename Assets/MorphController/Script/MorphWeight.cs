using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorphController
{
    [Serializable]
    public class MorphWeight
    {
        public Transform bone0;
        public Transform bone1;
        public Transform bone2;
        public Transform bone3;
        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;

        public MorphWeight()
        {
            bone0 = null;
            bone1 = null;
            bone2 = null;
            bone3 = null;
            weight0 = 1;
            weight1 = 0;
            weight2 = 0;
            weight3 = 0;
        }

        public MorphWeight(Transform[] bones, BoneWeight weight)
        {
            bone0 = bones[weight.boneIndex0];
            bone1 = bones[weight.boneIndex1];
            bone2 = bones[weight.boneIndex2];
            bone3 = bones[weight.boneIndex3];
            weight0 = weight.weight0;
            weight1 = weight.weight1;
            weight2 = weight.weight2;
            weight3 = weight.weight3;
        }

        public float GetWeights()
        {
            return weight0 + weight1 + weight2 + weight3;
        }

        public float GetWeight(Transform bone)
        {
            float weight = 0;
            if (bone0 == bone && weight0 > weight)
                weight = weight0;
            if (bone1 == bone && weight1 > weight)
                weight = weight1;
            if (bone2 == bone && weight2 > weight)
                weight = weight2;
            if (bone3 == bone && weight3 > weight)
                weight = weight3;
            return weight;
        }

        public BoneWeight ToBoneWeight(List<Transform> bones)
        {
            BoneWeight weight = new BoneWeight();
            weight.boneIndex0 = bone0 ? bones.IndexOf(bone0) : 0;
            weight.boneIndex1 = bone1 ? bones.IndexOf(bone1) : 0;
            weight.boneIndex2 = bone2 ? bones.IndexOf(bone2) : 0;
            weight.boneIndex3 = bone3 ? bones.IndexOf(bone3) : 0;
            weight.weight0 = weight0;
            weight.weight1 = weight1;
            weight.weight2 = weight2;
            weight.weight3 = weight3;

            return weight;
        }

        public void AstrictWeightsToOne()
        {
            float oldValue = GetWeights();
            if (oldValue <= 0f) oldValue = 1f;

            weight0 = weight0 / oldValue;
            weight1 = weight1 / oldValue;
            weight2 = weight2 / oldValue;
            weight3 = weight3 / oldValue;
        }
        
        public void AstrictWeightsToOneExclude0()
        {
            float oldValue = weight1 + weight2 + weight3;
            if (oldValue <= 0f) oldValue = 1f;

            float percent = 1.0f - weight0;
            weight1 = weight1 / oldValue * percent;
            weight2 = weight2 / oldValue * percent;
            weight3 = weight3 / oldValue * percent;
        }
        
        public void AstrictWeightsToOneExclude1()
        {
            float oldValue = weight0 + weight2 + weight3;
            if (oldValue <= 0f) oldValue = 1f;

            float percent = 1.0f - weight1;
            weight0 = weight0 / oldValue * percent;
            weight2 = weight2 / oldValue * percent;
            weight3 = weight3 / oldValue * percent;
        }
        
        public void AstrictWeightsToOneExclude2()
        {
            float oldValue = weight0 + weight1 + weight3;
            if (oldValue <= 0f) oldValue = 1f;

            float percent = 1.0f - weight2;
            weight0 = weight0 / oldValue * percent;
            weight1 = weight1 / oldValue * percent;
            weight3 = weight3 / oldValue * percent;
        }
        
        public void AstrictWeightsToOneExclude3()
        {
            float oldValue = weight0 + weight1 + weight2;
            if (oldValue <= 0f) oldValue = 1f;

            float percent = 1.0f - weight3;
            weight0 = weight0 / oldValue * percent;
            weight1 = weight1 / oldValue * percent;
            weight2 = weight2 / oldValue * percent;
        }
    }
}
