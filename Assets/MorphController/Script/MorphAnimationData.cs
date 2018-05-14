using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorphController
{
    [Serializable]
    [DisallowMultipleComponent]
    public class MorphAnimationData : MonoBehaviour
    {
        [HideInInspector]
        public bool Identity = false;
        public List<MorphTriangle> Triangles = new List<MorphTriangle>();
        public List<MorphVertex> Vertexs = new List<MorphVertex>();
        public float VertexIconSize = 0.01f;
        public float BoneIconSize = 0.1f;
        public List<Vector3> BindposesPosition = new List<Vector3>();
        public List<Quaternion> BindposesRotation = new List<Quaternion>();
        public List<Vector3> BindposesScale = new List<Vector3>();
    }
}
