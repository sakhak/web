namespace AssetManagement.Web.Options;

public sealed class MobileOptions
{
    public const string SectionName = "Mobile";

    /// <summary>Oldest app version the server still supports. Older apps are told to update.</summary>
    public string MinimumAppVersion { get; set; } = "1.0.0";

    /// <summary>Newest released app version (optional). Apps older than this are told an update is available.</summary>
    public string? LatestAppVersion { get; set; }

    /// <summary>Where the user can get the update (store link, optional).</summary>
    public string? UpdateUrl { get; set; }
}
