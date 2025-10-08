using System.Globalization;

namespace EduTrack.Application.Common.Helpers;

/// <summary>
/// Helper class for Persian date operations
/// </summary>
public static class PersianDateHelper
{
    private static readonly PersianCalendar _persianCalendar = new();
    
    /// <summary>
    /// Converts DateTimeOffset to Persian date string
    /// </summary>
    /// <param name="dateTime">The DateTimeOffset to convert</param>
    /// <param name="format">The format string (default: "yyyy/MM/dd")</param>
    /// <returns>Persian date string</returns>
    public static string ToPersianDateString(this DateTimeOffset dateTime, string format = "yyyy/MM/dd")
    {
        var persianYear = _persianCalendar.GetYear(dateTime.DateTime);
        var persianMonth = _persianCalendar.GetMonth(dateTime.DateTime);
        var persianDay = _persianCalendar.GetDayOfMonth(dateTime.DateTime);
        
        var persianDate = new DateTime(persianYear, persianMonth, persianDay);
        
        return persianDate.ToString(format, CultureInfo.InvariantCulture);
    }
    
    /// <summary>
    /// Converts DateTimeOffset to Persian date and time string
    /// </summary>
    /// <param name="dateTime">The DateTimeOffset to convert</param>
    /// <param name="dateFormat">The date format string (default: "yyyy/MM/dd")</param>
    /// <param name="timeFormat">The time format string (default: "HH:mm")</param>
    /// <returns>Persian date and time string</returns>
    public static string ToPersianDateTimeString(this DateTimeOffset dateTime, string dateFormat = "yyyy/MM/dd", string timeFormat = "HH:mm")
    {
        var persianDate = dateTime.ToPersianDateString(dateFormat);
        var time = dateTime.ToString(timeFormat);
        
        return $"{persianDate} {time}";
    }
    
    /// <summary>
    /// Converts Persian date string to DateTimeOffset
    /// </summary>
    /// <param name="persianDateString">Persian date string (format: yyyy/MM/dd)</param>
    /// <param name="time">Optional time (default: current time)</param>
    /// <returns>DateTimeOffset</returns>
    public static DateTimeOffset FromPersianDateString(string persianDateString, TimeSpan? time = null)
    {
        var parts = persianDateString.Split('/');
        if (parts.Length != 3)
            throw new ArgumentException("Invalid Persian date format. Expected: yyyy/MM/dd");
        
        var year = int.Parse(parts[0]);
        var month = int.Parse(parts[1]);
        var day = int.Parse(parts[2]);
        
        var persianDate = new DateTime(year, month, day, _persianCalendar);
        var targetTime = time ?? DateTime.Now.TimeOfDay;
        
        return new DateTimeOffset(persianDate.Date.Add(targetTime), TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
    }
    
    /// <summary>
    /// Gets current Persian date string
    /// </summary>
    /// <param name="format">The format string (default: "yyyy/MM/dd")</param>
    /// <returns>Current Persian date string</returns>
    public static string GetCurrentPersianDate(string format = "yyyy/MM/dd")
    {
        return DateTimeOffset.Now.ToPersianDateString(format);
    }
    
    /// <summary>
    /// Gets current Persian date and time string
    /// </summary>
    /// <param name="dateFormat">The date format string (default: "yyyy/MM/dd")</param>
    /// <param name="timeFormat">The time format string (default: "HH:mm")</param>
    /// <returns>Current Persian date and time string</returns>
    public static string GetCurrentPersianDateTime(string dateFormat = "yyyy/MM/dd", string timeFormat = "HH:mm")
    {
        return DateTimeOffset.Now.ToPersianDateTimeString(dateFormat, timeFormat);
    }
    
    /// <summary>
    /// Converts Persian month number to Persian month name
    /// </summary>
    /// <param name="monthNumber">Month number (1-12)</param>
    /// <returns>Persian month name</returns>
    public static string GetPersianMonthName(int monthNumber)
    {
        return monthNumber switch
        {
            1 => "فروردین",
            2 => "اردیبهشت",
            3 => "خرداد",
            4 => "تیر",
            5 => "مرداد",
            6 => "شهریور",
            7 => "مهر",
            8 => "آبان",
            9 => "آذر",
            10 => "دی",
            11 => "بهمن",
            12 => "اسفند",
            _ => "نامشخص"
        };
    }
    
    /// <summary>
    /// Converts Persian day of week to Persian day name
    /// </summary>
    /// <param name="dateTime">The DateTimeOffset</param>
    /// <returns>Persian day name</returns>
    public static string GetPersianDayName(this DateTimeOffset dateTime)
    {
        var dayOfWeek = _persianCalendar.GetDayOfWeek(dateTime.DateTime);
        
        return dayOfWeek switch
        {
            DayOfWeek.Saturday => "شنبه",
            DayOfWeek.Sunday => "یکشنبه",
            DayOfWeek.Monday => "دوشنبه",
            DayOfWeek.Tuesday => "سه‌شنبه",
            DayOfWeek.Wednesday => "چهارشنبه",
            DayOfWeek.Thursday => "پنج‌شنبه",
            DayOfWeek.Friday => "جمعه",
            _ => "نامشخص"
        };
    }
    
    /// <summary>
    /// Gets Persian date with day name
    /// </summary>
    /// <param name="dateTime">The DateTimeOffset</param>
    /// <param name="format">The format string (default: "yyyy/MM/dd")</param>
    /// <returns>Persian date with day name</returns>
    public static string ToPersianDateWithDayName(this DateTimeOffset dateTime, string format = "yyyy/MM/dd")
    {
        var persianDate = dateTime.ToPersianDateString(format);
        var dayName = dateTime.GetPersianDayName();
        
        return $"{dayName} {persianDate}";
    }
}
