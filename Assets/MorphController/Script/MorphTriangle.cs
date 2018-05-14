using System;

namespace MorphController
{
    [Serializable]
    public class MorphTriangle
    {
        public int Vertex1;
        public int Vertex2;
        public int Vertex3;

        public MorphTriangle(int vertex1, int vertex2, int vertex3)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;
        }
    }
}
