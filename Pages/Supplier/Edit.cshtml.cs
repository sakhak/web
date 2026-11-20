using AssetManagement.Application.Features.Suppliers;
using AssetManagement.Domain.Enums;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Supplier;

[Authorize(Policy = "SUPPLIER_MANAGE")]
public class EditModel(ISender sender) : PageModel
{
    [BindProperty]
    public SupplierForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var result = await sender.Send(new GetSupplierByIdQuery(id));
        if (!result.Succeeded) return NotFound();

        var s = result.Data;
        Form = new SupplierForm
        {
            Id = s.Id,
            FullName = s.FullName,
            ContactPerson = s.ContactPerson,
            AddressLine1 = s.AddressLine1,
            AddressLine2 = s.AddressLine2,
            PhoneNumber = s.PhoneNumber,
            Email = s.Email,
            Description = s.Description,
            Active = s.Status == BaseStatus.Active
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new UpdateSupplierCommand(
                id,
                new SupplierInput(Form.FullName, Form.ContactPerson, Form.AddressLine1, Form.AddressLine2,
                    Form.PhoneNumber, Form.Email, Form.Description),
                Form.Active,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (result.Status == HttpStatus.NotFound) return NotFound();
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Supplier {result.Data.FullName} updated.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }
}
