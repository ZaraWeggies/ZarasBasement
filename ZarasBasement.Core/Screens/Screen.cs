using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Zara_s_Basement.Core.Screens;

/// <summary>
/// Base class for screens with sensible defaults.
/// </summary>
public abstract class Screen : IScreen
{
    protected ContentManager? Content { get; private set; }
    protected GraphicsDevice? GraphicsDevice { get; private set; }

    /// <inheritdoc />
    public virtual bool BlocksInput => true;

    /// <inheritdoc />
    public virtual bool BlocksDraw => true;

    /// <inheritdoc />
    public virtual void Initialize() { }

    /// <inheritdoc />
    public virtual void LoadContent(ContentManager content)
    {
        Content = content;
        var graphicsService = content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
        GraphicsDevice = graphicsService?.GraphicsDevice;
    }

    /// <inheritdoc />
    public virtual void UnloadContent() { }

    /// <inheritdoc />
    public virtual void OnEnter() { }

    /// <inheritdoc />
    public virtual void OnExit() { }

    /// <inheritdoc />
    public abstract void Update(GameTime gameTime);

    /// <inheritdoc />
    public abstract void Draw(SpriteBatch spriteBatch);
}
