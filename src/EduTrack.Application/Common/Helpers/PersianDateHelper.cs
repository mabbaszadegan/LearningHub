using System;
using System.Globalization;

namespace EduTrack.Application.Common.Helpers;

public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Converts DateTimeOffset to Persian date string
    /// </summary>
    public static string ToPersianDateString(this DateTimeOffset dateTime, string format = "yyyy/MM/dd")
    {
        return PersianDateHelper.DateTimeOffsetToPersian(dateTime, false);
    }

    /// <summary>
    /// Converts DateTimeOffset to Persian date and time string
    /// </summary>
    public static string ToPersianDateTimeString(this DateTimeOffset dateTime, string dateFormat = "yyyy/MM/dd", string timeFormat = "HH:mm")
    {
        return PersianDateHelper.DateTimeOffsetToPersian(dateTime, true);
    }

    /// <summary>
    /// Converts DateTimeOffset to Persian date with month name (e.g., "5 مهر 1404")
    /// </summary>
    public static string ToPersianDateWithMonthName(this DateTimeOffset dateTime)
    {
        var persianDateString = PersianDateHelper.DateTimeOffsetToPersian(dateTime, false);
        var parts = persianDateString.Split('/');
        
        if (parts.Length == 3 && 
            int.TryParse(parts[0], out int year) &&
            int.TryParse(parts[1], out int month) &&
            int.TryParse(parts[2], out int day))
        {
            var monthName = PersianDateHelper.GetPersianMonthName(month);
            return $"{day} {monthName} {year}";
        }
        
        return persianDateString; // Fallback to original format
    }

    /// <summary>
    /// Converts DateTimeOffset to Persian date with day name
    /// </summary>
    public static string ToPersianDateWithDayName(this DateTimeOffset dateTime)
    {
        var persianDate = PersianDateHelper.DateTimeOffsetToPersian(dateTime, false);
        var tehranTime = TimeZoneInfo.ConvertTime(dateTime, PersianDateHelper.GetTehranTimeZone());
        var dayOfWeek = GetPersianDayName(tehranTime.DayOfWeek);
        return $"{dayOfWeek}، {persianDate}";
    }

    private static string GetPersianDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Saturday => "شنبه",
            DayOfWeek.Sunday => "یکشنبه",
            DayOfWeek.Monday => "دوشنبه",
            DayOfWeek.Tuesday => "سه‌شنبه",
            DayOfWeek.Wednesday => "چهارشنبه",
            DayOfWeek.Thursday => "پنج‌شنبه",
            DayOfWeek.Friday => "جمعه",
            _ => ""
        };
    }
}

public static class PersianDateHelper
{
    private static readonly PersianCalendar _persianCalendar = new();
    private static readonly TimeZoneInfo _tehranTimeZone = ResolveTehranTimeZone();

    private static TimeZoneInfo ResolveTehranTimeZone()
    {
        var candidates = new[] { "Asia/Tehran", "Iran Standard Time" };

        foreach (var candidate in candidates)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidate);
            }
            catch (TimeZoneNotFoundException)
            {
                // Try next candidate
            }
            catch (InvalidTimeZoneException)
            {
                // Try next candidate
            }
        }

        return TimeZoneInfo.Utc;
    }

    public static TimeZoneInfo GetTehranTimeZone() => _tehranTimeZone;

    /// <summary>
    /// Converts Persian date string to DateTimeOffset
    /// </summary>
    /// <param name="persianDate">Persian date string in format "yyyy/MM/dd"</param>
    /// <param name="timeString">Time string in format "HH:mm" (optional)</param>
    /// <returns>DateTimeOffset in UTC</returns>
    public static DateTimeOffset PersianToDateTimeOffset(string persianDate, string? timeString = null)
    {
        if (string.IsNullOrWhiteSpace(persianDate))
            throw new ArgumentException("Persian date cannot be null or empty", nameof(persianDate));

        var parts = persianDate.Split('/');
        if (parts.Length != 3)
            throw new ArgumentException("Persian date must be in format 'yyyy/MM/dd'", nameof(persianDate));

        if (!int.TryParse(parts[0], out int year) ||
            !int.TryParse(parts[1], out int month) ||
            !int.TryParse(parts[2], out int day))
        {
            throw new ArgumentException("Invalid Persian date format", nameof(persianDate));
        }

        // Convert Persian date to Gregorian
        var gregorianDate = _persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
        var tehranDateTime = DateTime.SpecifyKind(gregorianDate, DateTimeKind.Unspecified);

        // Parse time if provided
        if (!string.IsNullOrWhiteSpace(timeString) && TimeSpan.TryParse(timeString, out var time))
        {
            tehranDateTime = tehranDateTime.Date.Add(time);
        }

        var offset = _tehranTimeZone.GetUtcOffset(tehranDateTime);
        return new DateTimeOffset(tehranDateTime, offset);
    }

    /// <summary>
    /// Converts DateTimeOffset to Persian date string
    /// </summary>
    /// <param name="dateTimeOffset">DateTimeOffset to convert</param>
    /// <param name="includeTime">Whether to include time in the result</param>
    /// <returns>Persian date string in format "yyyy/MM/dd" or "yyyy/MM/dd HH:mm"</returns>
    public static string DateTimeOffsetToPersian(DateTimeOffset dateTimeOffset, bool includeTime = false)
    {
        var tehranTime = TimeZoneInfo.ConvertTime(dateTimeOffset, _tehranTimeZone);
        var localDateTime = tehranTime.DateTime;
        
        var year = _persianCalendar.GetYear(localDateTime);
        var month = _persianCalendar.GetMonth(localDateTime);
        var day = _persianCalendar.GetDayOfMonth(localDateTime);

        var result = $"{year:D4}/{month:D2}/{day:D2}";

        if (includeTime)
        {
            var hour = tehranTime.Hour;
            var minute = tehranTime.Minute;
            result += $" {hour:D2}:{minute:D2}";
        }

        return result;
    }

    /// <summary>
    /// Validates Persian date string
    /// </summary>
    /// <param name="persianDate">Persian date string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidPersianDate(string persianDate)
    {
        if (string.IsNullOrWhiteSpace(persianDate))
            return false;

        var parts = persianDate.Split('/');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out int year) ||
            !int.TryParse(parts[1], out int month) ||
            !int.TryParse(parts[2], out int day))
        {
            return false;
        }

        try
        {
            _persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts Persian date string to DateTimeOffset (alias for PersianToDateTimeOffset)
    /// </summary>
    /// <param name="persianDate">Persian date string in format "yyyy/MM/dd"</param>
    /// <param name="timeString">Time string in format "HH:mm" (optional)</param>
    /// <returns>DateTimeOffset in UTC</returns>
    public static DateTimeOffset FromPersianDateString(string persianDate, string? timeString = null)
    {
        return PersianToDateTimeOffset(persianDate, timeString);
    }

    /// <summary>
    /// Gets Persian month name by month number
    /// </summary>
    /// <param name="month">Month number (1-12)</param>
    /// <returns>Persian month name</returns>
    public static string GetPersianMonthName(int month)
    {
        return month switch
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
            _ => ""
        };
    }
}