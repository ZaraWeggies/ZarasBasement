using System;

namespace Zara_s_Basement.Core.Games;

/// <summary>
/// Statistics for a mini-game that persist across sessions.
/// </summary>
public class GameStats
{
    /// <summary>
    /// Game ID this stats object belongs to.
    /// </summary>
    public required string GameId { get; init; }

    /// <summary>
    /// Highest score achieved.
    /// </summary>
    public int HighScore { get; set; }

    /// <summary>
    /// Total number of times the game has been played.
    /// </summary>
    public int TimesPlayed { get; set; }

    /// <summary>
    /// Total number of wins.
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Last time the game was played.
    /// </summary>
    public DateTime? LastPlayed { get; set; }

    /// <summary>
    /// Best completion time (for timed games), in seconds.
    /// </summary>
    public float? BestTime { get; set; }

    /// <summary>
    /// Create empty stats for a game.
    /// </summary>
    public static GameStats CreateNew(string gameId) => new()
    {
        GameId = gameId,
        HighScore = 0,
        TimesPlayed = 0,
        Wins = 0,
        LastPlayed = null,
        BestTime = null
    };
}
