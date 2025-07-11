namespace Gallop.RenderPipeline
{
    public enum CutoffType
    {
        Invalid = -1,
        UnSupport = 0,
        UnSupportButDraw = 1,
        DiscardSrcTexA = 2,
        DiscardSrcTexB = 3,
        AlphaBlend = 4,
        AlphaBlendSrcTexR = 5,
        Max = 6,
    }
}
