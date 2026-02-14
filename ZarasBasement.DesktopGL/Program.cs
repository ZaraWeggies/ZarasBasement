using Zara_s_Basement.Core;

internal class Program
{
    /// <summary>
    /// The main entry point for the application. 
    /// This creates an instance of your game and calls it's Run() method 
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void Main(string[] args)
    {
        using var game = new Zara_s_BasementGame();
        game.Run();
    }
}