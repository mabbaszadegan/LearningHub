using System.Diagnostics.CodeAnalysis;

namespace EduTrack.WebApp.Models;

/// <summary>
/// Standard API response wrapper for student-facing endpoints that depend on an active student profile.
/// </summary>
/// <typeparam name="T">Payload type.</typeparam>
public sealed class ProfileAwareResponse<T>
{
    /// <summary>
    /// Indicates whether the request was processed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Indicates that the caller must select an active student profile before retrying.
    /// </summary>
    public bool RequiresProfile { get; set; }

    /// <summary>
    /// Optional error message for client display.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Response payload. May be null when the request fails or requires profile selection.
    /// </summary>
    [AllowNull]
    public T? Data { get; set; }
}

