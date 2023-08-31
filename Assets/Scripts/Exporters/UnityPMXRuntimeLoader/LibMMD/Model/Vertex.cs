using UnityEngine;

namespace LibMMD.Model
{
    public class Vertex
    {
        public Vector3 Coordinate { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 UvCoordinate { get; set; }
        public Vector4[] ExtraUvCoordinate { get; set; }
        public SkinningOperator SkinningOperator { get; set; }
        public float EdgeScale { get; set; }
    }
}