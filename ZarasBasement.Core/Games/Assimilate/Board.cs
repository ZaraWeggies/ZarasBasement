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
    /// Available colors for the game (matching original HTML5 version).
    /// Order: Green, Purple, Blue, White, Red, Yellow
    /// </summary>
    public static readonly Color[] Colors = new[]
    {
        new Color(0x66, 0xCC, 0x00),  // 0: Green   #66CC00
        new Color(0xBB, 0x77, 0xFF),  // 1: Purple  #BB77FF
        new Color(0x22, 0x33, 0xF4),  // 2: Blue    #2233F4
        new Color(0xFF, 0xFF, 0xFF),  // 3: White   #FFFFFF
        new Color(0xFF, 0x00, 0x00),  // 4: Red     #FF0000
        new Color(0xFF, 0xCC, 0x00),  // 5: Yellow  #FFCC00
    };

    /// <summary>
    /// Color names for hint display.
    /// </summary>
    public static readonly string[] ColorNames = new[]
    {
        "Green", "Purple", "Blue", "White", "Red", "Yellow"
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
    public HashSet<(int x, int y)> GetConnectedCells(int startX, int startY)
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

    /// <summary>
    /// AI hint: find the color that would capture the most adjacent cells.
    /// Matches the original HTML5 game's findMostTouching algorithm.
    /// </summary>
    public int GetBestMove()
    {
        int currentColor = _cells[0, 0];
        var colorCounts = new int[ColorCount];
        
        // Get all cells currently in the player's region
        var connected = GetConnectedCells(0, 0);
        var counted = new HashSet<(int, int)>(connected);
        
        // For each cell adjacent to the connected region, count cells by color
        foreach (var (cx, cy) in connected)
        {
            CountAdjacentGroup(cx - 1, cy, counted, colorCounts);
            CountAdjacentGroup(cx + 1, cy, counted, colorCounts);
            CountAdjacentGroup(cx, cy - 1, counted, colorCounts);
            CountAdjacentGroup(cx, cy + 1, counted, colorCounts);
        }
        
        // Don't suggest current color
        colorCounts[currentColor] = 0;
        
        // Find the color with the most adjacent cells
        int bestColor = 0;
        int bestCount = 0;
        for (int i = 0; i < ColorCount; i++)
        {
            if (colorCounts[i] > bestCount)
            {
                bestCount = colorCounts[i];
                bestColor = i;
            }
        }
        
        return bestColor;
    }

    private void CountAdjacentGroup(int x, int y, HashSet<(int, int)> counted, int[] colorCounts)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return;
        
        if (counted.Contains((x, y)))
            return;
        
        // Get the entire group connected to this cell
        int color = _cells[x, y];
        var group = GetConnectedCells(x, y);
        
        // Add all cells in the group to the count
        colorCounts[color] += group.Count;
        
        // Mark all as counted to avoid double-counting
        foreach (var cell in group)
        {
            counted.Add(cell);
        }
    }

    /// <summary>
    /// Clone the current board state for save/restore as jagged array (JSON-serializable).
    /// </summary>
    public int[][] CloneCells()
    {
        var clone = new int[_width][];
        for (int x = 0; x < _width; x++)
        {
            clone[x] = new int[_height];
            for (int y = 0; y < _height; y++)
            {
                clone[x][y] = _cells[x, y];
            }
        }
        return clone;
    }

    /// <summary>
    /// Restore board state from saved cells (jagged array).
    /// </summary>
    public void RestoreCells(int[][] cells)
    {
        if (cells != null && cells.Length == _width && cells[0]?.Length == _height)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y] = cells[x][y];
                }
            }
        }
    }
}
