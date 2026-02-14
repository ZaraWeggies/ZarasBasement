namespace Zara_s_Basement.Core.Screens;

/// <summary>
/// Types of transitions between screens.
/// </summary>
public enum ScreenTransition
{
    /// <summary>
    /// No transition, instant switch.
    /// </summary>
    None,

    /// <summary>
    /// Fade to black, then fade in new screen.
    /// </summary>
    Fade,

    /// <summary>
    /// Slide the old screen out and new screen in.
    /// </summary>
    Slide
}
