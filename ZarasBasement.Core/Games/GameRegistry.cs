using System;
using System.Collections.Generic;
using System.Linq;

namespace Zara_s_Basement.Core.Games;

/// <summary>
/// Registry of all available mini-games.
/// </summary>
public static class GameRegistry
{
    private static readonly Dictionary<string, Func<IMinigame>> _gameFactories = new();
    private static readonly Dictionary<string, GameInfo> _gameInfos = new();

    /// <summary>
    /// Get all registered game IDs.
    /// </summary>
    public static IEnumerable<string> GameIds => _gameFactories.Keys;

    /// <summary>
    /// Get all registered game infos.
    /// </summary>
    public static IEnumerable<GameInfo> Games => _gameInfos.Values;

    /// <summary>
    /// Number of registered games.
    /// </summary>
    public static int Count => _gameFactories.Count;

    /// <summary>
    /// Register a mini-game.
    /// </summary>
    /// <param name="info">Game metadata.</param>
    /// <param name="factory">Factory function to create game instance.</param>
    public static void Register(GameInfo info, Func<IMinigame> factory)
    {
        _gameFactories[info.Id] = factory;
        _gameInfos[info.Id] = info;
    }

    /// <summary>
    /// Check if a game is registered.
    /// </summary>
    public static bool IsRegistered(string gameId) => _gameFactories.ContainsKey(gameId);

    /// <summary>
    /// Get info for a specific game.
    /// </summary>
    public static GameInfo? GetInfo(string gameId) => 
        _gameInfos.TryGetValue(gameId, out var info) ? info : null;

    /// <summary>
    /// Create an instance of a game.
    /// </summary>
    public static IMinigame? CreateGame(string gameId) =>
        _gameFactories.TryGetValue(gameId, out var factory) ? factory() : null;

    /// <summary>
    /// Get games filtered by tag.
    /// </summary>
    public static IEnumerable<GameInfo> GetGamesByTag(string tag) =>
        _gameInfos.Values.Where(g => g.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Clear all registrations (mainly for testing).
    /// </summary>
    public static void Clear()
    {
        _gameFactories.Clear();
        _gameInfos.Clear();
    }
}
