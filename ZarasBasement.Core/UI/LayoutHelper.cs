using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZarasBasement.Core.UI;

/// <summary>
/// Centralized layout helper for responsive UI across portrait/landscape and mobile/desktop.
/// All screens should use this to determine layout parameters.
/// </summary>
public static class LayoutHelper
{
    private static int _lastWidth;
    private static int _lastHeight;
    
    /// <summary>
    /// Whether the current viewport is in portrait orientation (height > width).
    /// </summary>
    public static bool IsPortrait { get; private set; }
    
    /// <summary>
    /// Whether the current viewport is in landscape orientation (width >= height).
    /// </summary>
    public static bool IsLandscape => !IsPortrait;
    
    /// <summary>
    /// Whether running on a mobile platform (Android/iOS).
    /// </summary>
    public static bool IsMobile => ZarasBasementGame.IsMobile;
    
    /// <summary>
    /// Whether running on desktop (Windows/macOS/Linux).
    /// </summary>
    public static bool IsDesktop => ZarasBasementGame.IsDesktop;
    
    /// <summary>
    /// Current viewport width.
    /// </summary>
    public static int ViewportWidth { get; private set; }
    
    /// <summary>
    /// Current viewport height.
    /// </summary>
    public static int ViewportHeight { get; private set; }
    
    /// <summary>
    /// Scale factor for UI elements (based on screen density).
    /// Mobile portrait gets larger UI elements for touch.
    /// </summary>
    public static float UIScale { get; private set; } = 1f;
    
    /// <summary>
    /// Safe area inset from top (for notches, etc.).
    /// </summary>
    public static int SafeAreaTop { get; private set; }
    
    /// <summary>
    /// Safe area inset from bottom.
    /// </summary>
    public static int SafeAreaBottom { get; private set; }
    
    /// <summary>
    /// Event fired when viewport size or orientation changes.
    /// </summary>
    public static event Action? ViewportChanged;
    
    /// <summary>
    /// Update layout calculations based on current viewport.
    /// Call this at the start of Draw() before laying out UI.
    /// Returns true if viewport changed since last call.
    /// </summary>
    public static bool Update(Viewport viewport)
    {
        if (viewport.Width == _lastWidth && viewport.Height == _lastHeight)
            return false;
        
        _lastWidth = viewport.Width;
        _lastHeight = viewport.Height;
        
        ViewportWidth = viewport.Width;
        ViewportHeight = viewport.Height;
        IsPortrait = viewport.Height > viewport.Width;
        
        // Calculate UI scale based on screen size
        // Base reference: 720p landscape (1280x720)
        float referenceHeight = IsPortrait ? 1280f : 720f;
        UIScale = Math.Max(1f, viewport.Height / referenceHeight);
        
        // On mobile portrait, scale up more for touch targets
        if (IsMobile && IsPortrait)
        {
            UIScale *= 1.2f;
        }
        
        // Estimate safe areas (simplified - real implementation would query OS)
        SafeAreaTop = IsMobile && IsPortrait ? 40 : 0;
        SafeAreaBottom = IsMobile && IsPortrait ? 20 : 0;
        
        ViewportChanged?.Invoke();
        return true;
    }
    
    /// <summary>
    /// Get the usable content area (viewport minus safe areas).
    /// </summary>
    public static Rectangle GetContentArea()
    {
        return new Rectangle(
            0,
            SafeAreaTop,
            ViewportWidth,
            ViewportHeight - SafeAreaTop - SafeAreaBottom
        );
    }
    
    /// <summary>
    /// Scale a base size by the current UI scale.
    /// </summary>
    public static int Scale(int baseSize)
    {
        return (int)(baseSize * UIScale);
    }
    
    /// <summary>
    /// Scale a base size by the current UI scale.
    /// </summary>
    public static float Scale(float baseSize)
    {
        return baseSize * UIScale;
    }
    
    /// <summary>
    /// Get scaled button size appropriate for current platform/orientation.
    /// </summary>
    public static Vector2 GetButtonSize(int baseWidth = 80, int baseHeight = 32)
    {
        float scale = IsMobile ? UIScale * 1.5f : UIScale;
        return new Vector2(baseWidth * scale, baseHeight * scale);
    }
    
    /// <summary>
    /// Center a rectangle horizontally in the viewport.
    /// </summary>
    public static Rectangle CenterHorizontally(int width, int y, int height)
    {
        return new Rectangle(
            (ViewportWidth - width) / 2,
            y,
            width,
            height
        );
    }
    
    /// <summary>
    /// Center a rectangle in the viewport.
    /// </summary>
    public static Rectangle Center(int width, int height)
    {
        return new Rectangle(
            (ViewportWidth - width) / 2,
            (ViewportHeight - height) / 2,
            width,
            height
        );
    }
    
    /// <summary>
    /// Calculate optimal cell size for a grid that fits in available space.
    /// </summary>
    public static int CalculateGridCellSize(int gridWidth, int gridHeight, int availableWidth, int availableHeight, int maxCellSize = 40)
    {
        int cellFromWidth = availableWidth / gridWidth;
        int cellFromHeight = availableHeight / gridHeight;
        return Math.Min(Math.Min(cellFromWidth, cellFromHeight), maxCellSize);
    }
    
    /// <summary>
    /// Layout items in a row, centered horizontally.
    /// </summary>
    public static Rectangle[] LayoutRow(int count, int itemWidth, int itemHeight, int spacing, int y)
    {
        var result = new Rectangle[count];
        int totalWidth = count * itemWidth + (count - 1) * spacing;
        int startX = (ViewportWidth - totalWidth) / 2;
        
        for (int i = 0; i < count; i++)
        {
            result[i] = new Rectangle(
                startX + i * (itemWidth + spacing),
                y,
                itemWidth,
                itemHeight
            );
        }
        
        return result;
    }
    
    /// <summary>
    /// Layout items in a column, centered vertically.
    /// </summary>
    public static Rectangle[] LayoutColumn(int count, int itemWidth, int itemHeight, int spacing, int x)
    {
        var result = new Rectangle[count];
        int totalHeight = count * itemHeight + (count - 1) * spacing;
        int startY = (ViewportHeight - totalHeight) / 2;
        
        for (int i = 0; i < count; i++)
        {
            result[i] = new Rectangle(
                x,
                startY + i * (itemHeight + spacing),
                itemWidth,
                itemHeight
            );
        }
        
        return result;
    }
    
    /// <summary>
    /// Layout items in a grid (wrapping).
    /// </summary>
    public static Rectangle[] LayoutGrid(int count, int itemWidth, int itemHeight, int spacingX, int spacingY, int columns, int startY)
    {
        var result = new Rectangle[count];
        int rows = (count + columns - 1) / columns;
        int totalWidth = columns * itemWidth + (columns - 1) * spacingX;
        int startX = (ViewportWidth - totalWidth) / 2;
        
        for (int i = 0; i < count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            result[i] = new Rectangle(
                startX + col * (itemWidth + spacingX),
                startY + row * (itemHeight + spacingY),
                itemWidth,
                itemHeight
            );
        }
        
        return result;
    }
}
