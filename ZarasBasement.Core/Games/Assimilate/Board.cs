using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Zara_s_Basement.Core.Games.Assimilate;

/// <summary>
/// The game board for Assimilate! (flood-it style game).
/// </summary>
public class Board
{
    private readonly int[,] _cells;
    private readonly int _width;
    private readonly int _height;
    private readonly Random _random;

    /// <summary>
    /// Board width in cells.
    /// </summary>
    public int Width => _width;

    /// <summary>
    /// Board height in cells.
    /// </summary>
    public int Height => _height;

    /// <summary>
    /// Number of different colors on the board.
    /// </summary>
    public int ColorCount { get; }

    /// <summary>
    /// Available colors for the game.
    /// </summary>
    public static readonly Color[] Colors = new[]
    {
        new Color(255, 0, 0),      // Red
        new Color(102, 204, 0),    // Green
        new Color(34, 51, 244),    // Blue
        new Color(255, 204, 0),    // Yellow
        new Color(187, 119, 255),  // Purple
        new Color(255, 255, 255),  // White
    };

    /// <summary>
    /// Get the color index at a position.
    /// </summary>
    public int this[int x, int y]
    {
        get => _cells[x, y];
        set => _cells[x, y] = value;
    }

    public Board(int width, int height, int colorCount, int? seed = null)
    {
        _width = width;
        _height = height;
        ColorCount = Math.Min(colorCount, Colors.Length);
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _cells = new int[width, height];
        
        Randomize();
    }

    /// <summary>
    /// Fill the board with random colors.
    /// </summary>
    public void Randomize()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _cells[x, y] = _random.Next(ColorCount);
            }
        }
    }

    /// <summary>
    /// Perform a flood fill from the top-left corner with the given color.
    /// Returns the number of cells that changed.
    /// </summary>
    public int Flood(int newColor)
    {
        int oldColor = _cells[0, 0];
        
        // If same color, no change needed
        if (oldColor == newColor)
            return 0;
        
        // Find all cells currently connected to top-left
        var connected = GetConnectedCells(0, 0);
        
        // Change all connected cells to new color
        foreach (var (x, y) in connected)
        {
            _cells[x, y] = newColor;
        }
        
        return connected.Count;
    }

    /// <summary>
    /// Get all cells connected to the given cell (same color, orthogonally adjacent).
    /// </summary>
    private HashSet<(int x, int y)> GetConnectedCells(int startX, int startY)
    {
        var result = new HashSet<(int x, int y)>();
        var toVisit = new Queue<(int x, int y)>();
        int targetColor = _cells[startX, startY];
        
        toVisit.Enqueue((startX, startY));
        
        while (toVisit.Count > 0)
        {
            var (x, y) = toVisit.Dequeue();
            
            // Skip if out of bounds
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                continue;
            
            // Skip if already visited
            if (result.Contains((x, y)))
                continue;
            
            // Skip if different color
            if (_cells[x, y] != targetColor)
                continue;
            
            // Add to result
            result.Add((x, y));
            
            // Add neighbors
            toVisit.Enqueue((x - 1, y));
            toVisit.Enqueue((x + 1, y));
            toVisit.Enqueue((x, y - 1));
            toVisit.Enqueue((x, y + 1));
        }
        
        return result;
    }

    /// <summary>
    /// Check if the entire board is filled with one color (win condition).
    /// </summary>
    public bool IsComplete()
    {
        int firstColor = _cells[0, 0];
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_cells[x, y] != firstColor)
                    return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Get the percentage of the board that matches the top-left color.
    /// </summary>
    public float GetCompletionPercentage()
    {
        int targetColor = _cells[0, 0];
        int count = 0;
        int total = _width * _height;
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_cells[x, y] == targetColor)
                    count++;
            }
        }
        
        return count / (float)total;
    }

    /// <summary>
    /// Get the color at the top-left (player's current color).
    /// </summary>
    public int CurrentColor => _cells[0, 0];

    /// <summary>
    /// Get the actual Color value at a position.
    /// </summary>
    public Color GetColor(int x, int y)
    {
        int colorIndex = _cells[x, y];
        return Colors[colorIndex];
    }
}
