using AssetManagement.Application.Contracts.Reports;

namespace AssetManagement.Web.Models;

public static class ReportFormat
{
    public static string Cell(ReportColumnKind kind, object? value) => value switch
    {
        null => string.Empty,
        decimal m when kind == ReportColumnKind.Percent => m.ToString("P1"),
        decimal m => m.ToString("N2"),
        int i => i.ToString("N0"),
        DateTime d => d.ToString("dd MMM yyyy"),
        _ => value.ToString() ?? string.Empty
    };

    public static bool IsNumeric(ReportColumnKind kind) =>
        kind is ReportColumnKind.Money or ReportColumnKind.Integer or ReportColumnKind.Percent;
}
