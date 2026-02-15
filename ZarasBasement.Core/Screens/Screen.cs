using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Zara_s_Basement.Core.UI;

namespace Zara_s_Basement.Core.Screens;

/// <summary>
/// Base class for screens with sensible defaults.
/// </summary>
public abstract class Screen : IScreen
{
    protected ContentManager? Content { get; private set; }
    protected GraphicsDevice? GraphicsDevice { get; private set; }
    
    private int _lastViewportWidth;
    private int _lastViewportHeight;

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

    /// <inheritdoc />
    public virtual void OnViewportChanged(int width, int height) { }

    /// <summary>
    /// Call at start of Draw to check for viewport changes and invoke OnViewportChanged if needed.
    /// </summary>
    protected bool CheckViewportChanged()
    {
        if (GraphicsDevice == null) return false;
        
        var viewport = GraphicsDevice.Viewport;
        if (viewport.Width != _lastViewportWidth || viewport.Height != _lastViewportHeight)
        {
            _lastViewportWidth = viewport.Width;
            _lastViewportHeight = viewport.Height;
            LayoutHelper.Update(viewport);
            OnViewportChanged(viewport.Width, viewport.Height);
            return true;
        }
        return false;
    }
}
