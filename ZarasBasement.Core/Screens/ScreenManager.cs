using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ZarasBasement.Core.Screens;

/// <summary>
/// Manages a stack of screens and handles transitions between them.
/// </summary>
public class ScreenManager
{
    private readonly Stack<IScreen> _screenStack = new();
    private readonly ContentManager _content;
    private readonly GraphicsDevice _graphicsDevice;
    
    // Transition state
    private IScreen? _transitionFrom;
    private IScreen? _transitionTo;
    private ScreenTransition _currentTransition = ScreenTransition.None;
    private float _transitionProgress;
    private bool _isTransitioning;
    private bool _transitionHalfway;
    
    // Transition settings
    private const float TransitionDuration = 0.3f; // seconds per half-transition
    
    // Fade overlay
    private Texture2D? _fadeTexture;

    /// <summary>
    /// The currently active screen (top of stack).
    /// </summary>
    public IScreen? ActiveScreen => _screenStack.Count > 0 ? _screenStack.Peek() : null;

    /// <summary>
    /// Whether a transition is currently in progress.
    /// </summary>
    public bool IsTransitioning => _isTransitioning;

    /// <summary>
    /// Event fired when a screen change completes.
    /// </summary>
    public event Action<IScreen?>? ScreenChanged;

    public ScreenManager(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;
        CreateFadeTexture();
    }

    private void CreateFadeTexture()
    {
        _fadeTexture = new Texture2D(_graphicsDevice, 1, 1);
        _fadeTexture.SetData(new[] { Color.Black });
    }

    /// <summary>
    /// Push a new screen onto the stack.
    /// </summary>
    public void PushScreen(IScreen screen, ScreenTransition transition = ScreenTransition.Fade)
    {
        if (_isTransitioning) return;

        screen.Initialize();
        screen.LoadContent(_content);

        if (_screenStack.Count == 0 || transition == ScreenTransition.None)
        {
            ActiveScreen?.OnExit();
            _screenStack.Push(screen);
            screen.OnEnter();
            ScreenChanged?.Invoke(screen);
        }
        else
        {
            StartTransition(ActiveScreen, screen, transition, () =>
            {
                ActiveScreen?.OnExit();
                _screenStack.Push(screen);
                screen.OnEnter();
                ScreenChanged?.Invoke(screen);
            });
        }
    }

    /// <summary>
    /// Pop the current screen off the stack.
    /// </summary>
    public void PopScreen(ScreenTransition transition = ScreenTransition.Fade)
    {
        if (_isTransitioning || _screenStack.Count == 0) return;

        if (_screenStack.Count == 1 || transition == ScreenTransition.None)
        {
            var popped = _screenStack.Pop();
            popped.OnExit();
            popped.UnloadContent();
            ActiveScreen?.OnEnter();
            ScreenChanged?.Invoke(ActiveScreen);
        }
        else
        {
            var fromScreen = ActiveScreen;
            // Peek at what will be the new top
            _screenStack.Pop();
            var toScreen = ActiveScreen;
            _screenStack.Push(fromScreen!); // Put it back temporarily

            StartTransition(fromScreen, toScreen, transition, () =>
            {
                var popped = _screenStack.Pop();
                popped.OnExit();
                popped.UnloadContent();
                ActiveScreen?.OnEnter();
                ScreenChanged?.Invoke(ActiveScreen);
            });
        }
    }

    /// <summary>
    /// Replace the current screen with a new one.
    /// </summary>
    public void SwitchScreen(IScreen screen, ScreenTransition transition = ScreenTransition.Fade)
    {
        if (_isTransitioning) return;

        screen.Initialize();
        screen.LoadContent(_content);

        if (_screenStack.Count == 0 || transition == ScreenTransition.None)
        {
            if (_screenStack.Count > 0)
            {
                var old = _screenStack.Pop();
                old.OnExit();
                old.UnloadContent();
            }
            _screenStack.Push(screen);
            screen.OnEnter();
            ScreenChanged?.Invoke(screen);
        }
        else
        {
            StartTransition(ActiveScreen, screen, transition, () =>
            {
                if (_screenStack.Count > 0)
                {
                    var old = _screenStack.Pop();
                    old.OnExit();
                    old.UnloadContent();
                }
                _screenStack.Push(screen);
                screen.OnEnter();
                ScreenChanged?.Invoke(screen);
            });
        }
    }

    private Action? _onTransitionComplete;

    private void StartTransition(IScreen? from, IScreen? to, ScreenTransition transition, Action onComplete)
    {
        _transitionFrom = from;
        _transitionTo = to;
        _currentTransition = transition;
        _transitionProgress = 0f;
        _isTransitioning = true;
        _transitionHalfway = false;
        _onTransitionComplete = onComplete;
    }

    public void Update(GameTime gameTime)
    {
        if (_isTransitioning)
        {
            UpdateTransition(gameTime);
        }
        else
        {
            // Update all screens that should receive updates
            foreach (var screen in _screenStack)
            {
                screen.Update(gameTime);
                if (screen.BlocksInput) break;
            }
        }
    }

    private void UpdateTransition(GameTime gameTime)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _transitionProgress += delta / TransitionDuration;

        if (!_transitionHalfway && _transitionProgress >= 1f)
        {
            // Halfway point - execute the screen change
            _transitionHalfway = true;
            _transitionProgress = 0f;
            _onTransitionComplete?.Invoke();
        }
        else if (_transitionHalfway && _transitionProgress >= 1f)
        {
            // Transition complete
            _isTransitioning = false;
            _transitionFrom = null;
            _transitionTo = null;
            _currentTransition = ScreenTransition.None;
            _onTransitionComplete = null;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw screens from bottom to top
        var screens = new List<IScreen>(_screenStack);
        screens.Reverse();

        // Find first screen that blocks draw
        int startIndex = 0;
        for (int i = screens.Count - 1; i >= 0; i--)
        {
            if (screens[i].BlocksDraw)
            {
                startIndex = i;
                break;
            }
        }

        // Draw from that screen upward
        for (int i = startIndex; i < screens.Count; i++)
        {
            screens[i].Draw(spriteBatch);
        }

        // Draw transition overlay
        if (_isTransitioning && _currentTransition == ScreenTransition.Fade && _fadeTexture != null)
        {
            float alpha = _transitionHalfway ? 1f - _transitionProgress : _transitionProgress;
            var viewport = _graphicsDevice.Viewport;
            
            spriteBatch.Begin();
            spriteBatch.Draw(
                _fadeTexture,
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                Color.Black * alpha
            );
            spriteBatch.End();
        }
    }

    /// <summary>
    /// Clear all screens from the stack.
    /// </summary>
    public void ClearScreens()
    {
        while (_screenStack.Count > 0)
        {
            var screen = _screenStack.Pop();
            screen.OnExit();
            screen.UnloadContent();
        }
    }
}
