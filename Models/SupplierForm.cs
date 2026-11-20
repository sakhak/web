using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Web.Models;

public class SupplierForm
{
    public Guid Id { get; set; }

    [Required, StringLength(250)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? ContactPerson { get; set; }

    [Required, StringLength(500)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AddressLine2 { get; set; }

    [Required, StringLength(30)]
    public string? PhoneNumber { get; set; }

    [EmailAddress, StringLength(250)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool Active { get; set; } = true;
}
