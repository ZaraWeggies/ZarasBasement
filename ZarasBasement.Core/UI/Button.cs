using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZarasBasement.Core.Input;

namespace ZarasBasement.Core.UI;

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

    // Flat modern colors
    private static readonly Color ButtonNormal = new(80, 80, 100);
    private static readonly Color ButtonHover = new(100, 100, 130);
    private static readonly Color ButtonPressed = new(60, 60, 80);
    private static readonly Color ButtonDisabled = new(60, 60, 70);
    private static readonly Color ButtonBorder = new(120, 120, 150);
    private static readonly Color ButtonText = new(255, 255, 255);
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
        // Determine button color based on state
        Color bgColor;
        if (!Enabled)
            bgColor = ButtonDisabled;
        else if (_isPressed && _isHovered)
            bgColor = ButtonPressed;
        else if (_isHovered)
            bgColor = ButtonHover;
        else
            bgColor = ButtonNormal;

        // Draw border
        spriteBatch.Draw(pixel, _bounds, ButtonBorder);
        
        // Draw fill (inset by 2 pixels)
        var innerRect = new Rectangle(
            _bounds.X + 2,
            _bounds.Y + 2,
            _bounds.Width - 4,
            _bounds.Height - 4
        );
        spriteBatch.Draw(pixel, innerRect, bgColor);

        // Draw text
        if (font != null && !string.IsNullOrEmpty(Text))
        {
            var textSize = font.MeasureString(Text);
            var textPos = new Vector2(
                _bounds.X + (_bounds.Width - textSize.X) / 2,
                _bounds.Y + (_bounds.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, Text, textPos, Enabled ? ButtonText : ButtonTextDisabled);
        }
    }
}
