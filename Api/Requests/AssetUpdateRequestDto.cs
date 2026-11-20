using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Web.Api.Requests;

// Mirrors exactly the columns AssetService.UpdateAsync persists via ExecuteUpdateAsync -
// Currency, LastMaintenanceDate and the image are not part of that update path.
public class AssetUpdateRequestDto
{
    [Required, StringLength(150)]
    public required string AssetNumber { get; set; }

    [Required, StringLength(150)]
    public required string AssetName { get; set; }

    [Required, StringLength(150)]
    public required string Department { get; set; }

    [Required, StringLength(255)]
    public required string Description { get; set; }

    public DateTime PurchaseDate { get; set; }

    public decimal PurchaseCost { get; set; }

    [Required, StringLength(150)]
    public required string Status { get; set; }

    [Required, StringLength(150)]
    public required string Location { get; set; }

    public int? CategoryId { get; set; }
}
