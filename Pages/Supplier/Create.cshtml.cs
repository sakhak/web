using AssetManagement.Application.Features.Suppliers;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Supplier;

[Authorize(Policy = "SUPPLIER_MANAGE")]
public class CreateModel(ISender sender) : PageModel
{
    [BindProperty]
    public SupplierForm Form { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new CreateSupplierCommand(
                new SupplierInput(Form.FullName, Form.ContactPerson, Form.AddressLine1, Form.AddressLine2,
                    Form.PhoneNumber, Form.Email, Form.Description),
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Supplier {result.Data.FullName} created.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }
}
