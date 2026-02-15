namespace ZarasBasement.Core.Games;

/// <summary>
/// Metadata about a mini-game displayed in the hub.
/// </summary>
public class GameInfo
{
    /// <summary>
    /// Unique identifier for the game.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display title of the game.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Short description shown on hover.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Content path to the thumbnail image (without extension).
    /// </summary>
    public required string ThumbnailPath { get; init; }

    /// <summary>
    /// Difficulty rating (1-5 stars).
    /// </summary>
    public int Difficulty { get; init; } = 3;

    /// <summary>
    /// Estimated play time in minutes.
    /// </summary>
    public int EstimatedMinutes { get; init; } = 5;

    /// <summary>
    /// Category/genre tags.
    /// </summary>
    public string[] Tags { get; init; } = [];
}
