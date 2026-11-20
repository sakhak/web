namespace AssetManagement.Web.Api.Requests;

public class MaintenanceScheduleRequest
{
    public int AssetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int IntervalMonths { get; set; } = 12;

    /// <summary>Create only: first due date. Defaults to one interval after the last maintenance (or today).</summary>
    public DateTime? FirstDueDate { get; set; }

    /// <summary>Update only: the next due date (defaults to the current one).</summary>
    public DateTime? NextDueDate { get; set; }

    public int LeadDays { get; set; } = 14;

    /// <summary>Update only.</summary>
    public bool Active { get; set; } = true;
}

public class PlanMaintenanceRequest
{
    public int AssetId { get; set; }
    public int? ScheduleId { get; set; }
    public string Type { get; set; } = "Preventive";
    public string? Title { get; set; }
    public DateTime PlannedDate { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
    public bool TakesAssetOutOfService { get; set; }
    public Guid? SupplierId { get; set; }
    public string? VendorName { get; set; }
}

public class CompleteMaintenanceRequest
{
    public DateTime PerformedDate { get; set; } = DateTime.Today;
    public string? PerformedBy { get; set; }
    public decimal? Cost { get; set; }
    public string? Currency { get; set; }
    public Guid? SupplierId { get; set; }
    public string? VendorName { get; set; }
    public string? Notes { get; set; }
}

public class CancelMaintenanceRequest
{
    public string? Reason { get; set; }
}

public class RecordMaintenanceWorkRequest
{
    public int? AssetId { get; set; }
    public int? ScheduleId { get; set; }
    public string Type { get; set; } = "Preventive";
    public string? Title { get; set; }
    public DateTime PerformedDate { get; set; } = DateTime.Today;
    public string? PerformedBy { get; set; }
    public decimal? Cost { get; set; }
    public string? Currency { get; set; }
    public Guid? SupplierId { get; set; }
    public string? VendorName { get; set; }
    public string? Notes { get; set; }
}
