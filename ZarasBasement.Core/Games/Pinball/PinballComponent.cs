
using Microsoft.Xna.Framework;
using System.Text.Json;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class PinballComponent
{
    public required string Type { get; set; }
    public Rectangle Bounds { get; set; }
    public float Rotation { get; set; }
    public bool IsActive { get; set; }
    public int ScoreValue { get; set; }
    public float Speed { get; set; }
    public Color Color { get; set; } = Color.White;
    public float Angle { get; set; }
    public Point Position { get; set; }
    public string SpritePath { get; set; } = "";
    public string SoundEffectPath { get; set; } = "";
    public enum SoundEffectType
    {
        Once,
        Loop
    }

    public Task<Action?> OnHit()
    {
        // Placeholder for hit logic - in a real implementation, this would trigger animations, sound effects, and score updates.
        return Task.FromResult<Action?>(null);
    }
    
    // Additional properties like score value, hit points, etc. can be added as needed.
}


public class Plunger : PinballComponent
{
    public float PullBackDistance { get; set; }
    public float MaxPullBack { get; set; }
    public float LaunchForce { get; set; }
}

public class Ball : PinballComponent
{
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public bool IsInPlay { get; set; }
    public int Lives { get; set; }
    public float Radius { get; set; }
    public float Mass { get; set; }
    public float Friction { get; set; }
    public float Restitution { get; set; }
    public float Spin { get; set; }
    public float AngularVelocity { get; set; }
    public float AngularFriction { get; set; }
    public float AngularRestitution { get; set; }
    public float MaxSpeed { get; set; }
    public float MinSpeed { get; set; }
}

public enum PinballComponentType
{
    Bumper,
    Flipper,
    Target,
    Ramp,
    Spinner,
    Wall,
    Hole,
    Slingshot,
    Kicker,
    Gate,
    Plunger,
    Ball,
    Bridge,
    Tunnel
}
