using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AssetManagement.Domain.Constants;

namespace AssetManagement.Web.Models;

public class MaintenanceScheduleForm
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string AssetNumber { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 120)]
    public int IntervalMonths { get; set; } = 12;

    public DateTime? NextDueDate { get; set; }

    [Range(0, 365)]
    public int LeadDays { get; set; } = 14;

    public bool Active { get; set; } = true;
}

public class PlanMaintenanceForm
{
    [StringLength(150)]
    public string? AssetNumber { get; set; }

    public int? ScheduleId { get; set; }

    public string Type { get; set; } = MaintenanceType.Preventive;

    [StringLength(200)]
    public string? Title { get; set; }

    public DateTime? PlannedDate { get; set; } = DateTime.Today;

    [StringLength(2000)]
    public string? Notes { get; set; }

    public bool TakesAssetOutOfService { get; set; }

    public Guid? SupplierId { get; set; }

    [StringLength(250)]
    public string? VendorName { get; set; }
}

public class CompleteWorkForm
{
    public DateTime? PerformedDate { get; set; } = DateTime.Today;

    [StringLength(200)]
    public string? PerformedBy { get; set; }

    [Range(0, 1_000_000_000)]
    public decimal? Cost { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    public Guid? SupplierId { get; set; }

    [StringLength(250)]
    public string? VendorName { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public class RecordWorkForm : CompleteWorkForm
{
    [StringLength(150)]
    public string? AssetNumber { get; set; }

    public int? ScheduleId { get; set; }

    public string Type { get; set; } = MaintenanceType.Preventive;

    [StringLength(200)]
    public string? Title { get; set; }
}

public static class MaintenanceUi
{
    public static string StateClass(string? state) => state switch
    {
        MaintenanceDueState.Overdue => "maintenance",
        MaintenanceDueState.DueSoon => "in-transit",
        _ => "in-use"
    };

    public static string StateText(string? state) => state switch
    {
        MaintenanceDueState.DueSoon => "Due soon",
        _ => state ?? string.Empty
    };

    public static string StatusClass(string? status) => status switch
    {
        MaintenanceStatus.Planned => "available",
        MaintenanceStatus.InProgress => "in-transit",
        MaintenanceStatus.Done => "in-use",
        _ => "disposed"
    };

    public static string StatusText(string? status) => status == MaintenanceStatus.InProgress ? "In progress" : status ?? string.Empty;

    public static string DueText(int daysUntilDue) => daysUntilDue switch
    {
        0 => "today",
        < 0 => $"{-daysUntilDue} day{(daysUntilDue == -1 ? "" : "s")} overdue",
        _ => $"in {daysUntilDue} day{(daysUntilDue == 1 ? "" : "s")}"
    };
}

public static class PrincipalExtensions
{
    public static string? ActorName(this ClaimsPrincipal user) => user.FindFirst("DisplayName")?.Value ?? user.Identity?.Name;
}
