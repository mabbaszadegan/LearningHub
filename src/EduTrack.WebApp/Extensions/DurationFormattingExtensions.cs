namespace EduTrack.WebApp.Extensions;

public static class DurationFormattingExtensions
{
    public static string ToReadableDuration(this int totalMinutes)
    {
        if (totalMinutes <= 0)
        {
            return "0 دقیقه";
        }

        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;

        if (hours == 0)
        {
            return $"{minutes} دقیقه";
        }

        if (minutes == 0)
        {
            return $"{hours} ساعت";
        }

        return $"{hours} ساعت و {minutes} دقیقه";
    }
}

