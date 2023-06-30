using UnityEngine;

namespace LibMMD.Model
{
    public class SkinningOperator
    {
        public enum SkinningType : byte
        {
            SkinningBdef1 = 0,
            SkinningBdef2 = 1,
            SkinningBdef4 = 2,
            SkinningSdef = 3
        }

        public abstract class SkinningParam
        {
        }

        public class Bdef1 : SkinningParam
        {
            public int BoneId { get; set; }
        }

        public class Bdef2 : SkinningParam
        {
            public Bdef2()
            {
                BoneId = new int[2];
            }

            public int[] BoneId { get; set; }
            public float BoneWeight { get; set; }
        }

        public class Bdef4 : SkinningParam
        {
            public Bdef4()
            {
                BoneId = new int[4];
                BoneWeight = new float[4];
            }

            public int[] BoneId { get; set; }
            public float[] BoneWeight { get; set; }
        }

        public class Sdef : SkinningParam
        {
            public Sdef()
            {
                BoneId = new int[2];
            }

            public int[] BoneId { get; set; }
            public float BoneWeight { get; set; }
            public Vector3 C { get; set; }
            public Vector3 R0 { get; set; }
            public Vector3 R1 { get; set; }
        }

        public SkinningType Type { get; set; }
        public SkinningParam Param { get; set; }
    }
}