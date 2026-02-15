using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zara_s_Basement.Core.Games;
using Zara_s_Basement.Core.Input;

namespace Zara_s_Basement.Core.Hub;

/// <summary>
/// A clickable game station in the hub (arcade cabinet, poster, etc.)
/// </summary>
public class Station
{
    /// <summary>
    /// ID of the game this station launches.
    /// </summary>
    public string GameId { get; init; } = "";

    /// <summary>
    /// Position and size in screen coordinates.
    /// </summary>
    public Rectangle Bounds { get; set; }

    /// <summary>
    /// Whether the station is currently being hovered.
    /// </summary>
    public bool IsHovered { get; private set; }

    /// <summary>
    /// Whether the station was just clicked.
    /// </summary>
    public bool WasClicked { get; private set; }

    /// <summary>
    /// Visual style of the station.
    /// </summary>
    public StationType Type { get; init; } = StationType.ArcadeCabinet;

    /// <summary>
    /// Hover animation progress (0-1).
    /// </summary>
    public float HoverProgress { get; private set; }

    // Animation
    private const float HoverAnimSpeed = 5f;

    public void Update(GameTime gameTime)
    {
        IsHovered = InputHelper.IsPointerOver(Bounds);
        WasClicked = InputHelper.WasClicked(Bounds);

        // Smooth hover animation
        float target = IsHovered ? 1f : 0f;
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        HoverProgress = MathHelper.Lerp(HoverProgress, target, HoverAnimSpeed * delta);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, GameInfo? info, GameStats? stats, Texture2D? thumbnail = null)
    {
        // Draw cabinet/poster base
        Color baseColor = Type switch
        {
            StationType.ArcadeCabinet => new Color(60, 60, 80),
            StationType.Poster => new Color(180, 160, 140),
            StationType.Computer => new Color(200, 200, 190),
            _ => Color.Gray
        };

        // Highlight when hovered
        if (HoverProgress > 0.01f)
        {
            baseColor = Color.Lerp(baseColor, Color.White, HoverProgress * 0.3f);
        }

        // Draw the station shape based on type
        switch (Type)
        {
            case StationType.ArcadeCabinet:
                DrawArcadeCabinet(spriteBatch, pixel, font, baseColor, info, stats, thumbnail);
                break;
            case StationType.Poster:
                DrawPoster(spriteBatch, pixel, font, baseColor, info, stats, thumbnail);
                break;
            case StationType.Computer:
                DrawComputer(spriteBatch, pixel, font, baseColor, info, stats, thumbnail);
                break;
        }
    }

    private void DrawArcadeCabinet(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font,
        Color baseColor, GameInfo? info, GameStats? stats, Texture2D? thumbnail)
    {
        // Cabinet body
        spriteBatch.Draw(pixel, Bounds, baseColor);

        // Screen area (inner rectangle)
        int screenMargin = 8;
        var screenRect = new Rectangle(
            Bounds.X + screenMargin,
            Bounds.Y + screenMargin,
            Bounds.Width - screenMargin * 2,
            (int)(Bounds.Height * 0.6f)
        );
        spriteBatch.Draw(pixel, screenRect, new Color(20, 30, 20));

        // Draw thumbnail if available
        if (thumbnail != null)
        {
            // Scale to fit screen area with some padding
            int padding = 4;
            var thumbRect = new Rectangle(
                screenRect.X + padding,
                screenRect.Y + padding,
                screenRect.Width - padding * 2,
                screenRect.Height - padding * 2
            );
            spriteBatch.Draw(thumbnail, thumbRect, Color.White);
        }

        // Game title below screen (caption style)
        int titleHeight = 20;
        if (info != null)
        {
            var titleSize = font.MeasureString(info.Title);
            var titleX = screenRect.X + (screenRect.Width - titleSize.X) / 2;
            var titlePos = new Vector2(titleX, screenRect.Bottom + 4);
            spriteBatch.DrawString(font, info.Title, titlePos + new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(font, info.Title, titlePos, Color.LimeGreen);
        }

        // High score below title
        if (stats != null && stats.HighScore > 0)
        {
            var scorePos = new Vector2(screenRect.X + 4, screenRect.Bottom + titleHeight + 4);
            spriteBatch.DrawString(font, $"HI: {stats.HighScore}", scorePos, Color.Yellow);
        }

        // Control panel area (below title and score)
        var panelRect = new Rectangle(
            Bounds.X + screenMargin,
            screenRect.Bottom + titleHeight + 24,
            Bounds.Width - screenMargin * 2,
            Bounds.Height - screenRect.Height - screenMargin - titleHeight - 24
        );
        spriteBatch.Draw(pixel, panelRect, new Color(40, 40, 50));

        // Hover indicator
        if (HoverProgress > 0.1f)
        {
            var glowColor = Color.Cyan * (HoverProgress * 0.5f);
            // Draw glow border
            int glowSize = 3;
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X - glowSize, Bounds.Y - glowSize, Bounds.Width + glowSize * 2, glowSize), glowColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X - glowSize, Bounds.Bottom, Bounds.Width + glowSize * 2, glowSize), glowColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X - glowSize, Bounds.Y, glowSize, Bounds.Height), glowColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.Right, Bounds.Y, glowSize, Bounds.Height), glowColor);
        }
    }

    private void DrawPoster(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font,
        Color baseColor, GameInfo? info, GameStats? stats, Texture2D? thumbnail)
    {
        // Poster background
        spriteBatch.Draw(pixel, Bounds, baseColor);

        // Border
        int border = 2;
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, border), Color.SaddleBrown);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Bottom - border, Bounds.Width, border), Color.SaddleBrown);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, border, Bounds.Height), Color.SaddleBrown);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.Right - border, Bounds.Y, border, Bounds.Height), Color.SaddleBrown);

        // Hover indicator (draw before title so title is on top)
        if (HoverProgress > 0.1f)
        {
            var glowColor = Color.Gold * (HoverProgress * 0.4f);
            spriteBatch.Draw(pixel, Bounds, glowColor);
        }

        // Title (always on top)
        if (info != null)
        {
            var titlePos = new Vector2(Bounds.X + 6, Bounds.Y + 6);
            // Shadow for better readability
            spriteBatch.DrawString(font, info.Title, titlePos + new Vector2(1, 1), Color.Black * 0.5f);
            spriteBatch.DrawString(font, info.Title, titlePos, Color.Maroon);
        }
    }

    private void DrawComputer(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font,
        Color baseColor, GameInfo? info, GameStats? stats, Texture2D? thumbnail)
    {
        // Monitor
        spriteBatch.Draw(pixel, Bounds, baseColor);

        // Screen
        int margin = 6;
        var screenRect = new Rectangle(
            Bounds.X + margin,
            Bounds.Y + margin,
            Bounds.Width - margin * 2,
            Bounds.Height - margin * 2 - 15
        );
        spriteBatch.Draw(pixel, screenRect, new Color(0, 0, 40));

        // Title
        if (info != null)
        {
            var titlePos = new Vector2(screenRect.X + 4, screenRect.Y + 4);
            spriteBatch.DrawString(font, info.Title, titlePos, Color.White);
        }

        // Stand
        var standRect = new Rectangle(
            Bounds.X + Bounds.Width / 2 - 10,
            Bounds.Bottom - 12,
            20,
            12
        );
        spriteBatch.Draw(pixel, standRect, new Color(80, 80, 80));

        // Hover indicator  
        if (HoverProgress > 0.1f)
        {
            var screenGlow = Color.Lerp(new Color(0, 0, 40), new Color(0, 60, 120), HoverProgress);
            spriteBatch.Draw(pixel, screenRect, screenGlow);
            if (info != null)
            {
                var titlePos = new Vector2(screenRect.X + 4, screenRect.Y + 4);
                spriteBatch.DrawString(font, info.Title, titlePos, Color.White);
            }
        }
    }
}

/// <summary>
/// Visual style of a station.
/// </summary>
public enum StationType
{
    ArcadeCabinet,
    Poster,
    Computer
}
