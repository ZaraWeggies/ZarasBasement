using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ZarasBasement.Core.Games;
using ZarasBasement.Core.Input;
using ZarasBasement.Core.Screens;using ZarasBasement.Core.UI;using ZarasBasement.Core.Services;

namespace ZarasBasement.Core.Hub;

/// <summary>
/// The main hub screen - a basement environment with clickable game stations.
/// </summary>
public class HubScreen : Screen
{
    private readonly ScreenManager _screenManager;
    private readonly SaveManager _saveManager;
    
    private Texture2D? _pixel;
    private SpriteFont? _font;
    private readonly Dictionary<string, Texture2D> _thumbnails = new();
    
    private readonly List<Station> _stations = new();
    private Station? _hoveredStation;
    private int _lastViewportWidth;
    private int _lastViewportHeight;
    
    // Colors for the basement environment
    private static readonly Color WallColor = new(45, 35, 40);
    private static readonly Color FloorColor = new(60, 50, 45);
    private static readonly Color CeilingColor = new(35, 30, 35);
    
    // Title
    private const string Title = "Zara's Basement";
    private float _titlePulse;

    public HubScreen(ScreenManager screenManager, SaveManager saveManager)
    {
        _screenManager = screenManager;
        _saveManager = saveManager;
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);
        
        // Create 1x1 pixel texture for drawing
        _pixel = new Texture2D(GraphicsDevice!, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        // Load font
        _font = content.Load<SpriteFont>("Fonts/Hud");
        
        // Load thumbnails for each game
        LoadThumbnails(content);
        
        // Stations will be created in OnEnter when viewport is ready
    }

    private void LoadThumbnails(ContentManager content)
    {
        foreach (var gameId in GameRegistry.GameIds)
        {
            var info = GameRegistry.GetInfo(gameId);
            if (info != null && !string.IsNullOrEmpty(info.ThumbnailPath))
            {
                try
                {
                    var thumbnail = content.Load<Texture2D>(info.ThumbnailPath);
                    _thumbnails[gameId] = thumbnail;
                }
                catch
                {
                    // Thumbnail not found, that's ok
                }
            }
        }
    }

    private void CreateStations()
    {
        _stations.Clear();
        
        var viewport = GraphicsDevice!.Viewport;
        LayoutHelper.Update(viewport);
        
        var gameIds = new List<string>(GameRegistry.GameIds);
        
        // If no games registered, add a placeholder
        if (gameIds.Count == 0)
        {
            int centerX = viewport.Width / 2;
            int floorY = (int)(viewport.Height * 0.7f);
            _stations.Add(new Station
            {
                GameId = "placeholder",
                Type = StationType.ArcadeCabinet,
                Bounds = new Rectangle(centerX - 100, floorY - 350, 200, 350)
            });
            return;
        }
        
        if (LayoutHelper.IsPortrait)
        {
            CreatePortraitLayout(gameIds, viewport);
        }
        else
        {
            CreateLandscapeLayout(gameIds, viewport);
        }
    }
    
    private void CreatePortraitLayout(List<string> gameIds, Viewport viewport)
    {
        // Portrait: smaller stations in a 2-column grid
        int columns = 2;
        int margin = 20;
        int spacing = 15;
        int stationWidth = (viewport.Width - margin * 2 - spacing) / columns;
        int stationHeight = (int)(stationWidth * 1.5f);
        int startY = 70; // Below title
        int startX = margin;
        
        for (int i = 0; i < gameIds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            
            int x = startX + col * (stationWidth + spacing);
            int y = startY + row * (stationHeight + spacing);
            
            int typeIndex = i % 3;
            var stationType = typeIndex switch
            {
                0 => StationType.ArcadeCabinet,
                1 => StationType.Computer,
                _ => StationType.Poster
            };
            
            _stations.Add(new Station
            {
                GameId = gameIds[i],
                Type = stationType,
                Bounds = new Rectangle(x, y, stationWidth, stationHeight)
            });
        }
    }
    
    private void CreateLandscapeLayout(List<string> gameIds, Viewport viewport)
    {
        // Landscape: original horizontal layout
        int centerX = viewport.Width / 2;
        int floorY = (int)(viewport.Height * 0.7f);
        int stationWidth = 200;
        int stationHeight = 350;
        int spacing = 30;
        
        int totalWidth = gameIds.Count * stationWidth + (gameIds.Count - 1) * spacing;
        int startX = centerX - totalWidth / 2;
        
        for (int i = 0; i < gameIds.Count; i++)
        {
            var gameId = gameIds[i];
            
            int typeIndex = i % 3;
            var stationType = typeIndex switch
            {
                0 => StationType.ArcadeCabinet,
                1 => StationType.Computer,
                _ => StationType.Poster
            };
            
            // Posters are smaller and higher up (on wall)
            int width = stationType == StationType.Poster ? 80 : stationWidth;
            int height = stationType == StationType.Poster ? 100 : stationHeight;
            int y = stationType == StationType.Poster ? floorY - 180 : floorY - stationHeight;
            
            _stations.Add(new Station
            {
                GameId = gameId,
                Type = stationType,
                Bounds = new Rectangle(
                    startX + i * (stationWidth + spacing) + (stationWidth - width) / 2,
                    y,
                    width,
                    height
                )
            });
        }
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // Force station recreation on next draw
        _lastViewportWidth = 0;
        _lastViewportHeight = 0;
    }
    
    public override void OnViewportChanged(int width, int height)
    {
        // Stations will be recreated automatically in Draw
    }

    public override void Update(GameTime gameTime)
    {
        InputHelper.Update();
        
        // Update title pulse animation
        _titlePulse += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
        
        // Update stations
        _hoveredStation = null;
        foreach (var station in _stations)
        {
            station.Update(gameTime);
            
            if (station.IsHovered)
            {
                _hoveredStation = station;
            }
            
            if (station.WasClicked && station.GameId != "placeholder")
            {
                LaunchGame(station.GameId);
            }
        }
    }

    private void LaunchGame(string gameId)
    {
        var game = GameRegistry.CreateGame(gameId);
        if (game == null) return;
        
        // Load stats
        var stats = _saveManager.LoadStats(gameId);
        game.SetStats(stats);
        
        // Check for saved game state and restore if available
        if (game.SupportsSaveState && _saveManager.HasSavedGame(gameId))
        {
            var savedState = _saveManager.LoadGameStateRaw(gameId);
            if (savedState.HasValue)
            {
                game.RestoreSaveState(savedState.Value);
            }
        }
        
        // Wire up events
        game.RequestQuit += () =>
        {
            // Save game state before quitting (if game supports it and is in progress)
            if (game.SupportsSaveState)
            {
                var state = game.GetSaveState();
                if (state != null)
                {
                    _saveManager.SaveGameState(gameId, state);
                }
            }
            _screenManager.PopScreen();
        };
        game.StatsChanged += updatedStats => _saveManager.SaveStats(updatedStats);
        game.ClearSaveState += () => _saveManager.ClearGameState(gameId);
        
        // Push game screen
        _screenManager.PushScreen(game);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_pixel == null || _font == null || GraphicsDevice == null) return;
        
        var viewport = GraphicsDevice.Viewport;
        
        // Recreate stations if viewport changed (handles initial load and resize)
        if (viewport.Width != _lastViewportWidth || viewport.Height != _lastViewportHeight)
        {
            _lastViewportWidth = viewport.Width;
            _lastViewportHeight = viewport.Height;
            CreateStations();
        }
        
        // In portrait mode, push floor down; in landscape, show basement floor at 70%
        int floorY = LayoutHelper.IsPortrait 
            ? (int)(viewport.Height * 0.92f)  // Near bottom in portrait
            : (int)(viewport.Height * 0.7f);  // Traditional basement floor
        
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        // Draw basement background
        DrawBasement(spriteBatch, viewport, floorY);
        
        // Draw stations
        foreach (var station in _stations)
        {
            var info = GameRegistry.GetInfo(station.GameId);
            var stats = info != null ? _saveManager.LoadStats(station.GameId) : null;
            _thumbnails.TryGetValue(station.GameId, out var thumbnail);
            station.Draw(spriteBatch, _pixel, _font, info, stats, thumbnail);
        }
        
        // Draw title
        DrawTitle(spriteBatch, viewport);
        
        // Draw tooltip for hovered station
        if (_hoveredStation != null)
        {
            DrawTooltip(spriteBatch, viewport);
        }
        
        // Draw instructions
        DrawInstructions(spriteBatch, viewport);
        
        spriteBatch.End();
    }

    private void DrawBasement(SpriteBatch spriteBatch, Viewport viewport, int floorY)
    {
        // Ceiling
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, 60), CeilingColor);
        
        // Wall
        spriteBatch.Draw(_pixel, new Rectangle(0, 60, viewport.Width, floorY - 60), WallColor);
        
        // Wall texture (brick pattern)
        int brickW = 40, brickH = 20;
        var brickLineColor = new Color(35, 25, 30);
        for (int y = 60; y < floorY; y += brickH)
        {
            // Horizontal lines
            spriteBatch.Draw(_pixel, new Rectangle(0, y, viewport.Width, 1), brickLineColor);
            
            // Vertical lines (offset every other row)
            int offset = (y / brickH) % 2 == 0 ? 0 : brickW / 2;
            for (int x = offset; x < viewport.Width; x += brickW)
            {
                spriteBatch.Draw(_pixel, new Rectangle(x, y, 1, brickH), brickLineColor);
            }
        }
        
        // Floor
        spriteBatch.Draw(_pixel, new Rectangle(0, floorY, viewport.Width, viewport.Height - floorY), FloorColor);
        
        // Floor boards
        var boardLineColor = new Color(50, 40, 35);
        int boardWidth = 60;
        for (int x = 0; x < viewport.Width; x += boardWidth)
        {
            spriteBatch.Draw(_pixel, new Rectangle(x, floorY, 2, viewport.Height - floorY), boardLineColor);
        }
        
        // Baseboard
        spriteBatch.Draw(_pixel, new Rectangle(0, floorY - 8, viewport.Width, 8), new Color(50, 40, 35));
        
        // Simple lighting effect (darker at edges)
        var shadowColor = Color.Black * 0.3f;
        int shadowWidth = 100;
        for (int i = 0; i < shadowWidth; i++)
        {
            float alpha = (1f - i / (float)shadowWidth) * 0.3f;
            spriteBatch.Draw(_pixel, new Rectangle(i, 0, 1, viewport.Height), Color.Black * alpha);
            spriteBatch.Draw(_pixel, new Rectangle(viewport.Width - i - 1, 0, 1, viewport.Height), Color.Black * alpha);
        }
    }

    private void DrawTitle(SpriteBatch spriteBatch, Viewport viewport)
    {
        var titleSize = _font!.MeasureString(Title);
        var titlePos = new Vector2(
            (viewport.Width - titleSize.X) / 2,
            20
        );
        
        // Pulsing glow effect
        float pulse = (float)Math.Sin(_titlePulse) * 0.5f + 0.5f;
        var glowColor = Color.Lerp(new Color(100, 50, 50), new Color(200, 100, 100), pulse);
        
        // Shadow
        spriteBatch.DrawString(_font, Title, titlePos + new Vector2(2, 2), Color.Black * 0.5f);
        // Main text
        spriteBatch.DrawString(_font, Title, titlePos, glowColor);
    }

    private void DrawTooltip(SpriteBatch spriteBatch, Viewport viewport)
    {
        var info = GameRegistry.GetInfo(_hoveredStation!.GameId);
        if (info == null) return;
        
        var stats = _saveManager.LoadStats(_hoveredStation.GameId);
        
        // Build tooltip text
        string tooltipText = $"{info.Title}\n{info.Description}";
        if (stats.TimesPlayed > 0)
        {
            tooltipText += $"\n\nPlayed: {stats.TimesPlayed} times";
            if (stats.HighScore > 0)
                tooltipText += $"\nHigh Score: {stats.HighScore}";
            if (stats.Wins > 0)
                tooltipText += $"\nWins: {stats.Wins}";
        }
        else
        {
            tooltipText += "\n\n[Click to play!]";
        }
        
        var textSize = _font!.MeasureString(tooltipText);
        int padding = 10;
        
        // Position tooltip near cursor but keep on screen
        var pos = InputHelper.PointerPosition + new Vector2(20, 20);
        if (pos.X + textSize.X + padding * 2 > viewport.Width)
            pos.X = viewport.Width - textSize.X - padding * 2;
        if (pos.Y + textSize.Y + padding * 2 > viewport.Height)
            pos.Y = viewport.Height - textSize.Y - padding * 2;
        
        // Background
        var bgRect = new Rectangle(
            (int)pos.X,
            (int)pos.Y,
            (int)textSize.X + padding * 2,
            (int)textSize.Y + padding * 2
        );
        spriteBatch.Draw(_pixel, bgRect, new Color(20, 20, 30) * 0.9f);
        
        // Border
        spriteBatch.Draw(_pixel, new Rectangle(bgRect.X, bgRect.Y, bgRect.Width, 2), Color.Gray);
        spriteBatch.Draw(_pixel, new Rectangle(bgRect.X, bgRect.Bottom - 2, bgRect.Width, 2), Color.Gray);
        spriteBatch.Draw(_pixel, new Rectangle(bgRect.X, bgRect.Y, 2, bgRect.Height), Color.Gray);
        spriteBatch.Draw(_pixel, new Rectangle(bgRect.Right - 2, bgRect.Y, 2, bgRect.Height), Color.Gray);
        
        // Text
        spriteBatch.DrawString(_font, tooltipText, pos + new Vector2(padding, padding), Color.White);
    }

    private void DrawInstructions(SpriteBatch spriteBatch, Viewport viewport)
    {
        string instructions = "Click a game to play!";
        if (GameRegistry.Count == 0)
        {
            instructions = "No games installed yet...";
        }
        
        var size = _font!.MeasureString(instructions);
        var pos = new Vector2(
            (viewport.Width - size.X) / 2,
            viewport.Height - 30
        );
        
        spriteBatch.DrawString(_font, instructions, pos, Color.Gray);
    }

    public override void UnloadContent()
    {
        _pixel?.Dispose();
        base.UnloadContent();
    }
}
