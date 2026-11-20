using AssetManagement.Application.Features.Onboarding;

namespace AssetManagement.Web.Api.Requests;

public class DepreciationProfileRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UsefulLifeMonths { get; set; }
    public decimal SalvagePercent { get; set; }
    public bool Active { get; set; } = true;
}

public class AssignDepreciationProfileRequest
{
    public int ProfileId { get; set; }
    public DateTime? StartDate { get; set; }
}

public class PurchaseOrderRequest
{
    public string VendorName { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Currency { get; set; }
    public string? Remarks { get; set; }
    public List<PurchaseOrderLineInput> Lines { get; set; } = [];
}

public class GoodsReceiptRequest
{
    public int PurchaseOrderId { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Remarks { get; set; }
    public List<GoodsReceiptLineInput> Lines { get; set; } = [];
}

public class TagAssetsRequest
{
    public List<TagAssetInput> Assets { get; set; } = [];
    public int? DepreciationProfileId { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
}

public class AssetCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DefaultProfileId { get; set; }
    public bool Active { get; set; } = true;
}

public class AssignAssetCategoryRequest
{
    public int? CategoryId { get; set; }
}
