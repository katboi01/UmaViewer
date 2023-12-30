namespace Gallop.ImageEffect
{
    [System.Serializable]
    public class DofDiffusionBloomOverlayParam
    {
        public enum DofDiffusionBloomType
        {
            None = 0,
            DofBloom = 1,
            DiffusionDofBloom = 2,
            Bloom = 3,
            DiffusionBloom = 4,
            Dof = 5,
            OldDof = 6,
            OldDofFastBloom = 7,
            OverlayOnly = 8
        }

        public enum BloomScreenBlendMode
        {
            Screen = 0,
            Add = 1
        }
    }
}
