public class PoseLoadOptions
{
    public bool
        Root,
        Position,
        Rotation,
        Scale,
        Morphs,
        FaceBones,
        Physics;

    public static PoseLoadOptions None()
    {
        return new PoseLoadOptions()
        {
            Root        = false,
            Position    = false,
            Rotation    = false,
            Scale       = false,
            Morphs      = false,
            FaceBones   = false,
            Physics     = false
        };
    }

    public static PoseLoadOptions All()
    {
        return new PoseLoadOptions()
        {
            Root        = true,
            Position    = true,
            Rotation    = true,
            Scale       = true,
            Morphs      = true,
            FaceBones   = true,
            Physics     = true
        };
    }
}