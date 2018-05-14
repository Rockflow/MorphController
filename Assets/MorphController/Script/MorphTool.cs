using UnityEngine;

namespace MorphController
{
    public static class MorphTool
    {
        public static MorphVertex GetVertexByIndex(this MorphAnimationData morph, int index)
        {
            for (int i = 0; i < morph.Vertexs.Count; i++)
            {
                if (morph.Vertexs[i].VertexIndexs.Contains(index))
                {
                    return morph.Vertexs[i];
                }
            }
            return null;
        }

        public static MorphVertex GetVertexByClick(this MorphAnimationData morph, MorphTriangle triangle, Vector3 clickPoint)
        {
            float distance1 = Vector3.Distance(morph.Vertexs[triangle.Vertex1].Vertex, clickPoint);
            float distance2 = Vector3.Distance(morph.Vertexs[triangle.Vertex2].Vertex, clickPoint);
            float distance3 = Vector3.Distance(morph.Vertexs[triangle.Vertex3].Vertex, clickPoint);

            if (distance1 < distance2 && distance1 < distance3)
                return morph.Vertexs[triangle.Vertex1];
            if (distance2 < distance1 && distance2 < distance3)
                return morph.Vertexs[triangle.Vertex2];
            if (distance3 < distance1 && distance3 < distance2)
                return morph.Vertexs[triangle.Vertex3];
            return morph.Vertexs[triangle.Vertex1];
        }

        public static int GetVertexIndexByIndex(this MorphAnimationData morph, int index)
        {
            for (int i = 0; i < morph.Vertexs.Count; i++)
            {
                if (morph.Vertexs[i].VertexIndexs.Contains(index))
                {
                    return i;
                }
            }
            return 0;
        }
    }

    public enum MorphEditType
    {
        Vertex,
        Bone
    }

    public enum MorphEditTool
    {
        Move,
        Rotate,
        Scale,
        Transform,
        None
    }

    public enum UpdateType
    {
        Update,
        FixedUpdate
    }
}
