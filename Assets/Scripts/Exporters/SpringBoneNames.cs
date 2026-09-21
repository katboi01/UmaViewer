using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// A VMD bone track addresses its bone by a fixed 15-byte name field, so any bone a baked
// motion needs to drive must have a PMX name that fits. CySpring's names do not: they run
// to 20 characters (Sp_Hi_MSkirt0_FLL_00), and on chr1127 82 of 90 overflow the field while
// truncation collapses 11 separate skirt bones onto "Sp_Hi_MSkirt0_F".
//
// The names are strictly structured -- Sp_<region>_<part><n>_<side>_<index> -- so they can be
// abbreviated component-wise rather than hashed, which keeps the result readable in PMXEditor
// and stable across exports. Sp_Hi_MSkirt0_FLL_00 becomes HiMS0FLL0 (9 bytes).
//
// Only the PMX primary name is shortened; NameEn keeps the original, both so the full name
// stays visible and so lookups that key off NameEn (CySpringPhysicsExporter) still resolve.
public static class SpringBoneNames
{
    public const int VmdNameBytes = 15;

    public static bool IsSpringBone(string name) =>
        !string.IsNullOrEmpty(name) && name.StartsWith("Sp_", StringComparison.Ordinal);

    /// Short name for every spring bone in <paramref name="boneNames"/>, keyed by the original.
    /// Built over the whole set at once so uniqueness can be guaranteed rather than hoped for;
    /// iteration is ordinal-sorted so the same model always yields the same map.
    public static Dictionary<string, string> BuildMap(IEnumerable<string> boneNames)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in boneNames.Where(IsSpringBone).Distinct(StringComparer.Ordinal)
                                      .OrderBy(x => x, StringComparer.Ordinal))
        {
            string shortName = Fit(Abbreviate(name));
            if (used.Contains(shortName))
            {
                // Abbreviation is injective for the naming scheme this targets, but a model
                // that breaks the scheme must still export something addressable.
                for (int n = 2; ; n++)
                {
                    string cand = Fit(shortName, n.ToString());
                    if (used.Add(cand)) { shortName = cand; break; }
                }
            }
            else used.Add(shortName);
            map[name] = shortName;
        }
        return map;
    }

    // Sp_Hi_MSkirt0_FLL_00 -> Hi + MS0 + FLL + 0
    //   region : kept whole (Ch/He/Hi), 2 bytes
    //   part   : first two letters plus the trailing digits (MSkirt0 -> MS0, Hair2 -> Ha2)
    //   side   : kept whole (B/BL/BR/F/FL/FLL/FR/L/R), 1-3 bytes
    //   index  : leading zeros dropped (00 -> 0)
    static string Abbreviate(string name)
    {
        var parts = name.Split('_');
        if (parts.Length != 5) return name.Substring(3); // not the expected shape; Fit() trims it

        string region = parts[1];
        string part = AbbreviatePart(parts[2]);
        string side = parts[3];
        string index = parts[4].TrimStart('0');
        if (index.Length == 0) index = "0";
        return region + part + side + index;
    }

    static string AbbreviatePart(string part)
    {
        int split = part.Length;
        while (split > 0 && char.IsDigit(part[split - 1])) split--;
        string letters = part.Substring(0, split);
        string digits = part.Substring(split);
        if (letters.Length > 2) letters = letters.Substring(0, 2);
        return letters + digits;
    }

    /// Trim to the VMD field, reserving room for an optional suffix.
    static string Fit(string name, string suffix = "")
    {
        int budget = VmdNameBytes - Encoding.GetEncoding("shift_jis").GetByteCount(suffix);
        while (Encoding.GetEncoding("shift_jis").GetByteCount(name) > budget && name.Length > 0)
            name = name.Substring(0, name.Length - 1);
        return name + suffix;
    }
}
