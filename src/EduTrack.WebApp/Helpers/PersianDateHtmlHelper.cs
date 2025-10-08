using EduTrack.Application.Common.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduTrack.WebApp.Helpers;

/// <summary>
/// HTML Helper extensions for Persian date display
/// </summary>
public static class PersianDateHtmlHelper
{
    /// <summary>
    /// Displays Persian date
    /// </summary>
    /// <param name="htmlHelper">HTML Helper</param>
    /// <param name="dateTime">DateTimeOffset to display</param>
    /// <param name="format">Date format (default: "yyyy/MM/dd")</param>
    /// <param name="includeTime">Whether to include time</param>
    /// <returns>HTML string</returns>
    public static IHtmlContent PersianDate(this IHtmlHelper htmlHelper, DateTimeOffset? dateTime, string format = "yyyy/MM/dd", bool includeTime = false)
    {
        if (!dateTime.HasValue)
            return new HtmlString("<span class='text-muted'>-</span>");
            
        var persianDate = includeTime 
            ? dateTime.Value.ToPersianDateTimeString(format, "HH:mm")
            : dateTime.Value.ToPersianDateString(format);
            
        return new HtmlString($"<span class='persian-date'>{persianDate}</span>");
    }
    
    /// <summary>
    /// Displays Persian date with day name
    /// </summary>
    /// <param name="htmlHelper">HTML Helper</param>
    /// <param name="dateTime">DateTimeOffset to display</param>
    /// <param name="format">Date format (default: "yyyy/MM/dd")</param>
    /// <returns>HTML string</returns>
    public static IHtmlContent PersianDateWithDayName(this IHtmlHelper htmlHelper, DateTimeOffset? dateTime, string format = "yyyy/MM/dd")
    {
        if (!dateTime.HasValue)
            return new HtmlString("<span class='text-muted'>-</span>");
            
        var persianDateWithDay = dateTime.Value.ToPersianDateWithDayName(format);
        return new HtmlString($"<span class='persian-date-with-day'>{persianDateWithDay}</span>");
    }
    
    /// <summary>
    /// Displays relative Persian date (e.g., "2 روز پیش")
    /// </summary>
    /// <param name="htmlHelper">HTML Helper</param>
    /// <param name="dateTime">DateTimeOffset to display</param>
    /// <returns>HTML string</returns>
    public static IHtmlContent PersianRelativeDate(this IHtmlHelper htmlHelper, DateTimeOffset? dateTime)
    {
        if (!dateTime.HasValue)
            return new HtmlString("<span class='text-muted'>-</span>");
            
        var now = DateTimeOffset.Now;
        var diff = now - dateTime.Value;
        
        string relativeText;
        if (diff.TotalDays >= 1)
        {
            var days = (int)diff.TotalDays;
            relativeText = $"{days} روز پیش";
        }
        else if (diff.TotalHours >= 1)
        {
            var hours = (int)diff.TotalHours;
            relativeText = $"{hours} ساعت پیش";
        }
        else if (diff.TotalMinutes >= 1)
        {
            var minutes = (int)diff.TotalMinutes;
            relativeText = $"{minutes} دقیقه پیش";
        }
        else
        {
            relativeText = "همین الان";
        }
        
        return new HtmlString($"<span class='persian-relative-date' title='{dateTime.Value.ToPersianDateString()}'>{relativeText}</span>");
    }
    
    /// <summary>
    /// Creates a Persian date input field
    /// </summary>
    /// <param name="htmlHelper">HTML Helper</param>
    /// <param name="name">Field name</param>
    /// <param name="value">Current value</param>
    /// <param name="htmlAttributes">HTML attributes</param>
    /// <returns>HTML string</returns>
    public static IHtmlContent PersianDateInput(this IHtmlHelper htmlHelper, string name, DateTimeOffset? value = null, object? htmlAttributes = null)
    {
        var persianValue = value?.ToPersianDateString() ?? string.Empty;
        var attributes = htmlAttributes != null ? 
            string.Join(" ", htmlAttributes.GetType().GetProperties()
                .Select(p => $"{p.Name.ToLower()}=\"{p.GetValue(htmlAttributes)}\"")) : string.Empty;
                
        return new HtmlString($@"
            <div class='persian-date-input-wrapper'>
                <input type='text' name='{name}' value='{persianValue}' class='form-control persian-datepicker' {attributes} />
                <input type='hidden' name='{name}' value='{value?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? ""}' />
            </div>");
    }
}
