using System;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ZarasBasement.Core.Input;
using ZarasBasement.Core.Screens;
using ZarasBasement.Core.UI;

namespace ZarasBasement.Core.Games.Pinball;

/// <summary>
/// Pinball - A classic pinball game.
/// </summary>
public class PinballGame : Screen, IMinigame
{
    public string Id => "pinball";

    /// <inheritdoc />
    public GameInfo Info { get; } = new()
    {
        Id = "pinball",
        Title = "Pinball",
        Description = "Classic pinball game.",
        ThumbnailPath = "Games/Pinball/thumbnail",
        Difficulty = 2,
        EstimatedMinutes = 5,
        Tags = new[] { "Arcade", "Classic" }
    };

    public event Action? RequestQuit;
    public event Action<GameStats>? StatsChanged;
    public event Action? ClearSaveState;

    public override void Draw(SpriteBatch spriteBatch)
    {
        throw new NotImplementedException();
    }

    public GameStats GetStats()
    {
        throw new NotImplementedException();
    }

    public override void Initialize()
    {
        
    }

    public void SetStats(GameStats stats)
    {
        throw new NotImplementedException();
    }

    public override void Update(GameTime gameTime)
    {
        throw new NotImplementedException();
    }
}