using System;
using Zara_s_Basement.Core.Screens;

namespace Zara_s_Basement.Core.Games;

/// <summary>
/// Interface for mini-games in the hub.
/// Games should implement both IScreen (for lifecycle) and IMinigame (for hub integration).
/// </summary>
public interface IMinigame : IScreen
{
    /// <summary>
    /// Metadata about this game.
    /// </summary>
    GameInfo Info { get; }

    /// <summary>
    /// Get current stats for this game.
    /// </summary>
    GameStats GetStats();

    /// <summary>
    /// Update stats (called by SaveManager on load).
    /// </summary>
    void SetStats(GameStats stats);

    /// <summary>
    /// Event fired when the player wants to quit back to the hub.
    /// </summary>
    event Action? RequestQuit;

    /// <summary>
    /// Event fired when stats change (win, new high score, etc.)
    /// </summary>
    event Action<GameStats>? StatsChanged;
}
