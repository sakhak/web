using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AssetManagement.Web.Models;

public static class ModelStateExtensions
{
    public static void AddFluentErrors(this ModelStateDictionary modelState, ValidationException exception, string prefix)
    {
        foreach (var error in exception.Errors)
        {
            var key = string.IsNullOrEmpty(error.PropertyName) ? string.Empty : $"{prefix}.{error.PropertyName}";
            modelState.AddModelError(key, error.ErrorMessage);
        }
    }
}
