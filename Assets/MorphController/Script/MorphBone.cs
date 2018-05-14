using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorphController
{
    [Serializable]
    [DisallowMultipleComponent]
    public class MorphBone : MonoBehaviour
    {
        public float ExternalRange = 1;
        public float InternalRange = 0.5f;
        public List<int> ExternalRangeVertexs = new List<int>();
        public List<int> InternalRangeVertexs = new List<int>();
        public List<int> ErrorVertexs = new List<int>();
        public bool IsShowInInspector = false;
    }
}
