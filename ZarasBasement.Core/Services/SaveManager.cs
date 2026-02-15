using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zara_s_Basement.Core.Games;

namespace Zara_s_Basement.Core.Services;

/// <summary>
/// Manages saving and loading game statistics and game state.
/// </summary>
public class SaveManager
{
    private readonly string _savePath;
    private readonly Dictionary<string, GameStats> _statsCache = new();
    private readonly Dictionary<string, JsonElement> _gameStateCache = new();
    private SaveData? _saveData;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SaveManager()
    {
        // Get platform-appropriate save location
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string saveDir = Path.Combine(appData, "ZarasBasement");
        
        // Ensure directory exists
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        
        _savePath = Path.Combine(saveDir, "save.json");
        LoadAll();
    }

    /// <summary>
    /// Load save data from disk.
    /// </summary>
    private void LoadAll()
    {
        _statsCache.Clear();
        _gameStateCache.Clear();
        
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                _saveData = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
                
                if (_saveData?.GameStats != null)
                {
                    foreach (var stats in _saveData.GameStats)
                    {
                        _statsCache[stats.GameId] = stats;
                    }
                }
                
                if (_saveData?.GameStates != null)
                {
                    foreach (var kvp in _saveData.GameStates)
                    {
                        _gameStateCache[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load save data: {ex.Message}");
                _saveData = new SaveData();
            }
        }
        else
        {
            _saveData = new SaveData();
        }
    }

    /// <summary>
    /// Save all data to disk.
    /// </summary>
    private void SaveAll()
    {
        try
        {
            _saveData ??= new SaveData();
            _saveData.GameStats = new List<GameStats>(_statsCache.Values);
            _saveData.GameStates = new Dictionary<string, JsonElement>(_gameStateCache);
            _saveData.LastSaved = DateTime.UtcNow;
            
            string json = JsonSerializer.Serialize(_saveData, JsonOptions);
            File.WriteAllText(_savePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save data: {ex.Message}");
        }
    }

    /// <summary>
    /// Load stats for a specific game.
    /// </summary>
    public GameStats LoadStats(string gameId)
    {
        if (_statsCache.TryGetValue(gameId, out var stats))
        {
            return stats;
        }
        
        // Return empty stats
        return GameStats.CreateNew(gameId);
    }

    /// <summary>
    /// Save stats for a specific game.
    /// </summary>
    public void SaveStats(GameStats stats)
    {
        _statsCache[stats.GameId] = stats;
        SaveAll();
    }

    /// <summary>
    /// Check if a game has saved state.
    /// </summary>
    public bool HasSavedGame(string gameId)
    {
        return _gameStateCache.ContainsKey(gameId);
    }

    /// <summary>
    /// Save game state for a specific game.
    /// </summary>
    public void SaveGameState<T>(string gameId, T state) where T : class
    {
        var jsonElement = JsonSerializer.SerializeToElement(state, JsonOptions);
        _gameStateCache[gameId] = jsonElement;
        SaveAll();
    }

    /// <summary>
    /// Load game state for a specific game.
    /// </summary>
    public T? LoadGameState<T>(string gameId) where T : class
    {
        if (_gameStateCache.TryGetValue(gameId, out var jsonElement))
        {
            try
            {
                return jsonElement.Deserialize<T>(JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize game state for {gameId}: {ex.Message}");
            }
        }
        return null;
    }
    
    /// <summary>
    /// Load raw game state as JsonElement (for generic interface usage).
    /// </summary>
    public JsonElement? LoadGameStateRaw(string gameId)
    {
        if (_gameStateCache.TryGetValue(gameId, out var jsonElement))
        {
            return jsonElement;
        }
        return null;
    }

    /// <summary>
    /// Clear saved game state (after game completion).
    /// </summary>
    public void ClearGameState(string gameId)
    {
        if (_gameStateCache.Remove(gameId))
        {
            SaveAll();
        }
    }

    /// <summary>
    /// Get all saved stats.
    /// </summary>
    public IEnumerable<GameStats> GetAllStats() => _statsCache.Values;

    /// <summary>
    /// Clear all save data (for testing or reset).
    /// </summary>
    public void ClearAll()
    {
        _statsCache.Clear();
        _gameStateCache.Clear();
        _saveData = new SaveData();
        
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
        }
    }
}

/// <summary>
/// Root save data structure.
/// </summary>
internal class SaveData
{
    public int Version { get; set; } = 1;
    public DateTime LastSaved { get; set; }
    public List<GameStats> GameStats { get; set; } = new();
    public Dictionary<string, JsonElement> GameStates { get; set; } = new();
}
