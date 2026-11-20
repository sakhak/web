namespace AssetManagement.Web.Api.Requests;

public sealed class SettingResponse
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? ShortName { get; set; }
    public string? NameEn { get; set; }
    public string? NameKh { get; set; }
    public string? Description { get; set; }
}
