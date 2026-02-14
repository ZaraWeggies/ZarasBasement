using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zara_s_Basement.Core.UI;

/// <summary>
/// 90s-style quit button that appears in all mini-games.
/// Positioned in the top-right corner with a chunky "QUIT" label.
/// </summary>
public class QuitButton
{
    private readonly Button _button;
    private int _margin = 10;

    /// <summary>
    /// Whether to use a small X button (mobile) or larger QUIT button (desktop).
    /// </summary>
    public bool CompactMode { get; set; }

    /// <summary>
    /// Event fired when quit is requested.
    /// </summary>
    public event Action? QuitRequested;

    /// <summary>
    /// Margin from screen edge.
    /// </summary>
    public int Margin
    {
        get => _margin;
        set
        {
            _margin = value;
            UpdatePosition();
        }
    }

    private Rectangle _screenBounds;

    public QuitButton(bool compactMode = false)
    {
        CompactMode = compactMode;
        _button = new Button
        {
            Text = compactMode ? "X" : "QUIT"
        };
        _button.Clicked += OnButtonClicked;
    }

    private void OnButtonClicked()
    {
        QuitRequested?.Invoke();
    }

    /// <summary>
    /// Update position based on screen size.
    /// </summary>
    public void SetScreenBounds(Rectangle screenBounds)
    {
        _screenBounds = screenBounds;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        int width = CompactMode ? 32 : 80;
        int height = CompactMode ? 32 : 28;

        _button.Bounds = new Rectangle(
            _screenBounds.Width - width - _margin,
            _margin,
            width,
            height
        );
    }

    public void Update()
    {
        _button.Update();
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        _button.Draw(spriteBatch, pixel, font);
    }
}
