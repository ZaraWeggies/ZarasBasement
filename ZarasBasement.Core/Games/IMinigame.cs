using System;
using System.Text.Json;
using ZarasBasement.Core.Screens;

namespace ZarasBasement.Core.Games;

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
    
    /// <summary>
    /// Whether this game supports save/resume functionality.
    /// Default implementation returns false.
    /// </summary>
    bool SupportsSaveState => false;
    
    /// <summary>
    /// Get current game state for saving. Returns null if nothing to save.
    /// Games should return a serializable object with their current state.
    /// </summary>
    object? GetSaveState() => null;
    
    /// <summary>
    /// Restore game from previously saved state.
    /// Called before Initialize() if there's saved state.
    /// The JsonElement can be deserialized to the game's specific state type.
    /// </summary>
    void RestoreSaveState(JsonElement stateJson) { }
    
    /// <summary>
    /// Clear saved state (called on game completion - win or lose).
    /// </summary>
    event Action? ClearSaveState;
}
