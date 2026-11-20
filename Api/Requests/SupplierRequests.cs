using AssetManagement.Application.Features.Suppliers;

namespace AssetManagement.Web.Api.Requests;

public sealed class SupplierRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; } = true;

    public SupplierInput ToInput() => new(FullName, ContactPerson, AddressLine1, AddressLine2, PhoneNumber, Email, Description);
}

public sealed class SupplierStatusRequest
{
    public bool Active { get; set; }
}
