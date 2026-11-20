using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AssetManagement.Web.Api.Requests;

public class AssetCreateRequestDto
{
    [Required, StringLength(150)]
    public required string AssetNumber { get; set; }

    [Required, StringLength(150)]
    public required string AssetName { get; set; }

    [Required, StringLength(150)]
    public required string Department { get; set; }

    [Required, StringLength(255)]
    public required string Description { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.Now;

    public decimal PurchaseCost { get; set; }

    [StringLength(50)]
    public string Currency { get; set; } = "USD";

    [Required, StringLength(150)]
    public required string Status { get; set; }

    [Required, StringLength(150)]
    public required string Location { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }

    public int? CategoryId { get; set; }

    public IFormFile? AssetImage { get; set; }
}
