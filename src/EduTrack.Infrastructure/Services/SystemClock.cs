using EduTrack.Application.Common.Interfaces;

namespace EduTrack.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
