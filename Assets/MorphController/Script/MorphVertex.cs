using UnityEngine;
using System.Collections.Generic;
using System;

namespace MorphController
{
    [Serializable]
    public class MorphVertex
    {
        public Vector3 Vertex;
        public List<int> VertexIndexs;
        public MorphWeight VertexWeight;

        public MorphVertex(Vector3 vertex, List<int> vertexIndexs)
        {
            Vertex = vertex;
            VertexIndexs = vertexIndexs;
            VertexWeight = new MorphWeight();
        }
    }
}
