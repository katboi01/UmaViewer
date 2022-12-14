
public static class ManifestCategory
{
    public const int APP_DEFINED_KIND_BASE = 10;
    public const string ASSET_MANIFEST = "manifest";
    public const string PLATFORM_MANIFEST = "manifest2";
    public const string ROOT_MANIFEST = "manifest3";

    public enum Kind : int
    {
        Default = 0,
        AssetManifest = 1,
        PlatformManifest = 2,
        RootManifest = 3
    }
}

