namespace Gallop.ImageEffect
{
    [System.Serializable]
    public class RadialBlurParam
    {
        public enum MoveBlurType
        {
            None = 0,
            Circle = 1,
            Horizontal = 2,
            Vertical = 3,
            Ellipse = 4,
            Roll = 5
        }
    }
}
