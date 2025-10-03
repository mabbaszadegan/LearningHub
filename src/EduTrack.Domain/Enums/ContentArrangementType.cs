namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents how content items are arranged within a stage
/// </summary>
public enum ContentArrangementType
{
    /// <summary>
    /// Content items displayed sequentially (one after another)
    /// </summary>
    Sequential = 1,
    
    /// <summary>
    /// Content items displayed side by side
    /// </summary>
    SideBySide = 2,
    
    /// <summary>
    /// Content items displayed in a grid layout
    /// </summary>
    Grid = 3,
    
    /// <summary>
    /// Content items displayed in tabs
    /// </summary>
    Tabs = 4
}
