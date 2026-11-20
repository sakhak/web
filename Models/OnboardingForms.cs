using System.ComponentModel.DataAnnotations;
using AssetManagement.Application.Features.Onboarding;

namespace AssetManagement.Web.Models;

public class PurchaseOrderForm
{
    public int Id { get; set; }

    [Required, StringLength(250)]
    public string VendorName { get; set; } = string.Empty;

    public DateTime? OrderDate { get; set; } = DateTime.Today;

    [Required, StringLength(10)]
    public string Currency { get; set; } = "USD";

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public List<PurchaseOrderLineInput> Lines { get; set; } = [new() { Quantity = 1 }];
}

public class GoodsReceiptForm
{
    public int PurchaseOrderId { get; set; }
    public DateTime? ReceivedDate { get; set; } = DateTime.Today;

    [StringLength(100)]
    public string? ReceivedBy { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public List<GoodsReceiptLineInput> Lines { get; set; } = [];
}

public class TagAssetsForm
{
    public int? DepreciationProfileId { get; set; }
    public DateTime? StartDate { get; set; }
    public List<TagAssetInput> Assets { get; set; } = [];
}

public class DepreciationProfileForm
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1200)]
    public int UsefulLifeMonths { get; set; } = 36;

    [Range(0, 100)]
    public decimal SalvagePercent { get; set; }

    public bool Active { get; set; } = true;
}

public record HeadingAction(string Text, string Page, object? RouteValues = null, bool Primary = false);

public class HeadingModel
{
    public string Crumb { get; set; } = "Asset Onboarding";
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public List<HeadingAction> Actions { get; set; } = [];
}

public static class OnboardingUi
{
    public static string StatusClass(string? status) => status switch
    {
        "Draft" => "disposed",
        "Approved" => "available",
        "PartiallyReceived" => "in-transit",
        "Received" => "in-use",
        "Cancelled" => "maintenance",
        "ACTIVE" => "active",
        "INACTIVE" => "inactive",
        _ => "disposed"
    };

    public static string StatusText(string? status) => status switch
    {
        "PartiallyReceived" => "Partially Received",
        "ACTIVE" => "Active",
        "INACTIVE" => "Inactive",
        _ => status ?? string.Empty
    };
}
