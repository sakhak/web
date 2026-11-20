namespace AssetManagement.Web.Api;

public static class AppVersion
{
    /// <summary>Parses "1.2.0", "1.2.0+45" or "1.2.0-beta" (build and pre-release suffixes are ignored).</summary>
    public static bool TryParse(string? value, out Version version)
    {
        version = new Version(0, 0);
        if (string.IsNullOrWhiteSpace(value)) return false;

        var core = value.Trim().TrimStart('v', 'V');
        var cut = core.IndexOfAny(['+', '-', ' ']);
        if (cut >= 0) core = core[..cut];

        if (!Version.TryParse(core, out var parsed))
        {
            if (!int.TryParse(core, out var major)) return false;
            parsed = new Version(major, 0);
        }

        version = parsed;
        return true;
    }

    public static bool IsOlder(string? candidate, string? reference) =>
        TryParse(candidate, out var a) && TryParse(reference, out var b) && Normalize(a) < Normalize(b);

    private static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0), Math.Max(v.Revision, 0));
}
