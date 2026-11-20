using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Web.Models;

public class AssetCategoryForm
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int? DefaultProfileId { get; set; }

    public bool Active { get; set; } = true;
}
