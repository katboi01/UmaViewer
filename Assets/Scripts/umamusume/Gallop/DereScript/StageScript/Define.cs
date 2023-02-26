using System;
using UnityEngine;

public class Define
{
    public enum RenderOrder
    {
        Geometry = 2000,
        Transparent = 3000,
        Overlay = 4000
    }

    public const float NUM_SKILLTIMELIMIT = 3f;

    public const int NUM_DIFFICULTY_COUNT = 22;

    public static Vector3 hidePosition = new Vector3(99999f, 99999f);

    public static Color black = new Color(0.1764706f, 0.1764706f, 0.1764706f, 1f);

    public static Color white = new Color(1f, 1f, 1f, 1f);

    public static Color yellow = new Color(1f, 1f, 0f, 1f);

    public static Color pink = new Color(211f / 255f, 61f / 255f, 152f / 255f, 1f);

    public static Color blue = new Color(61f / 255f, 161f / 255f, 254f / 255f, 1f);

    public static Color gray = new Color(0.6f, 0.6f, 0.6f, 1f);

    public static Color red = new Color(0.882352948f, 19f / 85f, 19f / 85f, 1f);
}
