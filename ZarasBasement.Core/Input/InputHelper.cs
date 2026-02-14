using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Zara_s_Basement.Core.Input;

/// <summary>
/// Unified input handling for mouse and touch.
/// </summary>
public static class InputHelper
{
    private static MouseState _currentMouse;
    private static MouseState _previousMouse;
    private static TouchCollection _currentTouch;
    private static TouchCollection _previousTouch;

    /// <summary>
    /// Current pointer position (mouse or first touch).
    /// </summary>
    public static Vector2 PointerPosition { get; private set; }

    /// <summary>
    /// Whether a pointer is currently active (mouse over window or touch active).
    /// </summary>
    public static bool HasPointer { get; private set; }

    /// <summary>
    /// Update input state. Call once per frame.
    /// </summary>
    public static void Update()
    {
        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();

        _previousTouch = _currentTouch;
        _currentTouch = TouchPanel.GetState();

        // Determine pointer position (touch takes priority on mobile)
        if (_currentTouch.Count > 0)
        {
            PointerPosition = _currentTouch[0].Position;
            HasPointer = true;
        }
        else
        {
            PointerPosition = new Vector2(_currentMouse.X, _currentMouse.Y);
            HasPointer = _currentMouse.X >= 0 && _currentMouse.Y >= 0;
        }
    }

    /// <summary>
    /// Check if pointer was just pressed this frame.
    /// </summary>
    public static bool IsPointerPressed()
    {
        // Check for new touch
        foreach (var touch in _currentTouch)
        {
            if (touch.State == TouchLocationState.Pressed)
                return true;
        }

        // Check for mouse click
        return _currentMouse.LeftButton == ButtonState.Pressed &&
               _previousMouse.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Check if pointer was just released this frame.
    /// </summary>
    public static bool IsPointerReleased()
    {
        // Check for touch release
        foreach (var touch in _currentTouch)
        {
            if (touch.State == TouchLocationState.Released)
                return true;
        }

        // Check for mouse release
        return _currentMouse.LeftButton == ButtonState.Released &&
               _previousMouse.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Check if pointer is currently held down.
    /// </summary>
    public static bool IsPointerDown()
    {
        if (_currentTouch.Count > 0)
            return true;

        return _currentMouse.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Check if a key was just pressed.
    /// </summary>
    public static bool IsKeyPressed(Keys key)
    {
        return Keyboard.GetState().IsKeyDown(key);
    }

    /// <summary>
    /// Check if pointer is within a rectangle.
    /// </summary>
    public static bool IsPointerOver(Rectangle bounds)
    {
        return bounds.Contains(PointerPosition);
    }

    /// <summary>
    /// Check if a rectangle was clicked (pointer released while over it).
    /// </summary>
    public static bool WasClicked(Rectangle bounds)
    {
        return IsPointerReleased() && IsPointerOver(bounds);
    }
}
