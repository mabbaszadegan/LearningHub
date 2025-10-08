using EduTrack.Application.Common.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace EduTrack.WebApp.ModelBinders;

/// <summary>
/// Custom model binder for DateTimeOffset with Persian date support
/// </summary>
public class PersianDateTimeOffsetModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        try
        {
            // Try to parse as Persian date first
            if (IsPersianDate(value))
            {
                var dateTimeOffset = PersianDateHelper.FromPersianDateString(value);
                bindingContext.Result = ModelBindingResult.Success(dateTimeOffset);
                return Task.CompletedTask;
            }
            
            // Try to parse as standard DateTimeOffset
            if (DateTimeOffset.TryParse(value, out var standardDateTime))
            {
                bindingContext.Result = ModelBindingResult.Success(standardDateTime);
                return Task.CompletedTask;
            }
            
            // Try to parse as DateTime and convert to DateTimeOffset
            if (DateTime.TryParse(value, out var dateTime))
            {
                var dateTimeOffset = new DateTimeOffset(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime));
                bindingContext.Result = ModelBindingResult.Success(dateTimeOffset);
                return Task.CompletedTask;
            }
            
            bindingContext.ModelState.TryAddModelError(modelName, "فرمت تاریخ نامعتبر است");
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.TryAddModelError(modelName, $"خطا در تبدیل تاریخ: {ex.Message}");
        }

        return Task.CompletedTask;
    }
    
    private static bool IsPersianDate(string value)
    {
        // Check if the string contains Persian digits or follows Persian date pattern
        return value.Contains('/') && (value.Contains('۱') || value.Contains('۲') || value.Contains('۳') || 
                                      value.Contains('۴') || value.Contains('۵') || value.Contains('۶') || 
                                      value.Contains('۷') || value.Contains('۸') || value.Contains('۹') || 
                                      value.Contains('۰') || char.IsDigit(value[0]));
    }
}
