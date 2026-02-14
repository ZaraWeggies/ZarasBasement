using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zara_s_Basement.Core.Input;

namespace Zara_s_Basement.Core.UI;

/// <summary>
/// A clickable button with 90s-style beveled appearance.
/// </summary>
public class Button
{
    private Rectangle _bounds;
    private bool _isHovered;
    private bool _isPressed;

    /// <summary>
    /// Position and size of the button.
    /// </summary>
    public Rectangle Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }

    /// <summary>
    /// Text displayed on the button.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Whether the button is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether the button is currently hovered.
    /// </summary>
    public bool IsHovered => _isHovered;

    /// <summary>
    /// Whether the button is currently pressed.
    /// </summary>
    public bool IsPressed => _isPressed;

    /// <summary>
    /// Event fired when the button is clicked.
    /// </summary>
    public event Action? Clicked;

    // 90s Windows-style colors
    private static readonly Color ButtonFace = new(192, 192, 192);
    private static readonly Color ButtonHighlight = new(255, 255, 255);
    private static readonly Color ButtonShadow = new(128, 128, 128);
    private static readonly Color ButtonDarkShadow = new(64, 64, 64);
    private static readonly Color ButtonText = new(0, 0, 0);
    private static readonly Color ButtonTextDisabled = new(128, 128, 128);

    public Button() { }

    public Button(Rectangle bounds, string text)
    {
        _bounds = bounds;
        Text = text;
    }

    public void Update()
    {
        if (!Enabled)
        {
            _isHovered = false;
            _isPressed = false;
            return;
        }

        _isHovered = InputHelper.IsPointerOver(_bounds);
        
        if (_isHovered && InputHelper.IsPointerPressed())
        {
            _isPressed = true;
        }
        
        if (_isPressed && InputHelper.IsPointerReleased())
        {
            _isPressed = false;
            if (_isHovered)
            {
                Clicked?.Invoke();
            }
        }
        
        if (!InputHelper.IsPointerDown())
        {
            _isPressed = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont? font = null)
    {
        // Draw beveled border (90s Windows style)
        int bevel = 2;
        
        if (_isPressed && _isHovered)
        {
            // Pressed state - inverted bevel
            DrawBevel(spriteBatch, pixel, ButtonDarkShadow, ButtonShadow, ButtonHighlight, ButtonHighlight, bevel);
        }
        else
        {
            // Normal state
            DrawBevel(spriteBatch, pixel, ButtonHighlight, ButtonHighlight, ButtonDarkShadow, ButtonShadow, bevel);
        }

        // Draw face
        var faceRect = new Rectangle(
            _bounds.X + bevel,
            _bounds.Y + bevel,
            _bounds.Width - bevel * 2,
            _bounds.Height - bevel * 2
        );
        spriteBatch.Draw(pixel, faceRect, Enabled ? ButtonFace : new Color(212, 212, 212));

        // Draw text
        if (font != null && !string.IsNullOrEmpty(Text))
        {
            var textSize = font.MeasureString(Text);
            var textPos = new Vector2(
                _bounds.X + (_bounds.Width - textSize.X) / 2,
                _bounds.Y + (_bounds.Height - textSize.Y) / 2
            );

            // Offset text when pressed
            if (_isPressed && _isHovered)
            {
                textPos += new Vector2(1, 1);
            }

            spriteBatch.DrawString(font, Text, textPos, Enabled ? ButtonText : ButtonTextDisabled);
        }
    }

    private void DrawBevel(SpriteBatch spriteBatch, Texture2D pixel,
        Color topOuter, Color topInner, Color bottomOuter, Color bottomInner, int size)
    {
        // Top edge (outer)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, 1), topOuter);
        // Left edge (outer)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.X, _bounds.Y, 1, _bounds.Height), topOuter);
        
        // Top edge (inner)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.X + 1, _bounds.Y + 1, _bounds.Width - 2, 1), topInner);
        // Left edge (inner)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.X + 1, _bounds.Y + 1, 1, _bounds.Height - 2), topInner);

        // Bottom edge (outer)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.X, _bounds.Bottom - 1, _bounds.Width, 1), bottomOuter);
        // Right edge (outer)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.Right - 1, _bounds.Y, 1, _bounds.Height), bottomOuter);

        // Bottom edge (inner)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.X + 1, _bounds.Bottom - 2, _bounds.Width - 2, 1), bottomInner);
        // Right edge (inner)
        spriteBatch.Draw(pixel, new Rectangle(_bounds.Right - 2, _bounds.Y + 1, 1, _bounds.Height - 2), bottomInner);
    }
}
