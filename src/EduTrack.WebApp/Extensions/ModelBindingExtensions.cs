using EduTrack.Application.Common.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace EduTrack.WebApp.Extensions;

/// <summary>
/// Extension methods for model binding with Persian date support
/// </summary>
public static class ModelBindingExtensions
{
    /// <summary>
    /// Binds Persian date string to DateTimeOffset
    /// </summary>
    /// <param name="modelState">ModelStateDictionary</param>
    /// <param name="key">The key for the model property</param>
    /// <param name="persianDateString">Persian date string</param>
    /// <param name="timeString">Optional time string (HH:mm format)</param>
    /// <returns>True if binding was successful</returns>
    public static bool TryBindPersianDate(this ModelStateDictionary modelState, string key, string? persianDateString, string? timeString = null)
    {
        if (string.IsNullOrWhiteSpace(persianDateString))
        {
            modelState.AddModelError(key, "تاریخ الزامی است");
            return false;
        }

        try
        {
            TimeSpan? time = null;
            if (!string.IsNullOrWhiteSpace(timeString))
            {
                if (TimeSpan.TryParse(timeString, out var parsedTime))
                {
                    time = parsedTime;
                }
                else
                {
                    modelState.AddModelError(key, "فرمت زمان نامعتبر است");
                    return false;
                }
            }

            var dateTimeOffset = PersianDateHelper.FromPersianDateString(persianDateString, time);
            modelState.SetModelValue(key, dateTimeOffset, persianDateString);
            return true;
        }
        catch (Exception ex)
        {
            modelState.AddModelError(key, $"خطا در تبدیل تاریخ: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Converts DateTimeOffset to Persian date string for display
    /// </summary>
    /// <param name="dateTime">DateTimeOffset to convert</param>
    /// <param name="includeTime">Whether to include time</param>
    /// <returns>Persian date string</returns>
    public static string ToPersianDisplayString(this DateTimeOffset? dateTime, bool includeTime = false)
    {
        if (!dateTime.HasValue)
            return string.Empty;
            
        return includeTime 
            ? dateTime.Value.ToPersianDateTimeString()
            : dateTime.Value.ToPersianDateString();
    }
    
    /// <summary>
    /// Converts DateTimeOffset to Persian date string for display
    /// </summary>
    /// <param name="dateTime">DateTimeOffset to convert</param>
    /// <param name="includeTime">Whether to include time</param>
    /// <returns>Persian date string</returns>
    public static string ToPersianDisplayString(this DateTimeOffset dateTime, bool includeTime = false)
    {
        return includeTime 
            ? dateTime.ToPersianDateTimeString()
            : dateTime.ToPersianDateString();
    }
}
