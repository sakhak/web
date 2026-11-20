using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Assets;
using AssetManagement.Application.Features.Categories;
using AssetManagement.Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Assets;

[Authorize(Policy = "ASSET_CREATE")]
public class CreateModel(ISender sender, IMapper mapper) : PageModel
{
    [BindProperty]
    public Asset Asset { get; set; } = null!;

    [BindProperty]
    public IFormFile? AssetImage { get; set; }

    public List<SelectListItem> CategoryItems { get; private set; } = [];

    public async Task OnGetAsync() => await LoadCategoriesAsync();

    private async Task LoadCategoriesAsync() =>
        CategoryItems = ((await sender.Send(new GetAssetCategoriesQuery())).Data ?? [])
            .Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString())).ToList();

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();
        if (!ModelState.IsValid) return Page();

        if (AssetImage is { Length: > CreateAssetCommandValidator.MaxImageSizeBytes })
        {
            ModelState.AddModelError("AssetImage", "File size exceeds the 5MB limit.");
            return Page();
        }

        byte[]? imageBytes = null;
        if (AssetImage is { Length: > 0 })
        {
            using var ms = new MemoryStream();
            await AssetImage.CopyToAsync(ms);
            imageBytes = ms.ToArray();
        }

        var command = mapper.Map<CreateAssetCommand>(Asset) with
        {
            ImageData = imageBytes,
            ImageFileName = AssetImage?.FileName,
            ImageContentType = AssetImage?.ContentType
        };

        var result = await sender.Send(command);

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded)
        {
            ModelState.AddModelError("Internal_Error", result.Message);
            return Page();
        }

        return RedirectToPage("/Assets/Index");
    }
}
