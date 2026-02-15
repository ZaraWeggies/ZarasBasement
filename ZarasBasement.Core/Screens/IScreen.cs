using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ZarasBasement.Core.Screens;

/// <summary>
/// Interface for all game screens (hub, games, menus, etc.)
/// </summary>
public interface IScreen
{
    /// <summary>
    /// Whether this screen blocks input to screens below it in the stack.
    /// </summary>
    bool BlocksInput { get; }

    /// <summary>
    /// Whether this screen blocks drawing of screens below it in the stack.
    /// </summary>
    bool BlocksDraw { get; }

    /// <summary>
    /// Initialize the screen (called once when screen is created).
    /// </summary>
    void Initialize();

    /// <summary>
    /// Load content for the screen.
    /// </summary>
    /// <param name="content">Content manager to load assets.</param>
    void LoadContent(ContentManager content);

    /// <summary>
    /// Unload content when screen is disposed.
    /// </summary>
    void UnloadContent();

    /// <summary>
    /// Called when this screen becomes the active screen.
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Called when this screen is no longer the active screen.
    /// </summary>
    void OnExit();

    /// <summary>
    /// Update the screen logic.
    /// </summary>
    /// <param name="gameTime">Timing values.</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Draw the screen.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for rendering.</param>
    void Draw(SpriteBatch spriteBatch);

    /// <summary>
    /// Called when the viewport size changes (e.g., orientation change or window resize).
    /// </summary>
    void OnViewportChanged(int width, int height);
}
