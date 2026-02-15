using System;
using Zara_s_Basement.Core.Localization;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zara_s_Basement.Core.Games;
using Zara_s_Basement.Core.Games.Assimilate;
using Zara_s_Basement.Core.Hub;
using Zara_s_Basement.Core.Screens;
using Zara_s_Basement.Core.Services;

namespace Zara_s_Basement.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    public class Zara_s_BasementGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch? spriteBatch;
        
        // Core systems
        private ScreenManager? screenManager;
        private SaveManager? saveManager;

        /// <summary>
        /// Indicates if the game is running on a mobile platform.
        /// </summary>
        public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Indicates if the game is running on a desktop platform.
        /// </summary>
        public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings, 
        /// initializes services like settings and leaderboard managers, and sets up the 
        /// screen manager for screen transitions.
        /// </summary>
        public Zara_s_BasementGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";

            // Configure screen orientations.
            if (IsMobile)
            {
                graphicsDeviceManager.SupportedOrientations = 
                    DisplayOrientation.Portrait | 
                    DisplayOrientation.LandscapeLeft | 
                    DisplayOrientation.LandscapeRight;
            }
            else
            {
                graphicsDeviceManager.SupportedOrientations = 
                    DisplayOrientation.LandscapeLeft | 
                    DisplayOrientation.LandscapeRight;
            }
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the 
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // TODO You should load this from a settings file or similar,
            // based on what the user or operating system selected.
            var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
            LocalizationManager.SetCulture(selectedLanguage);
            
            // Set up window size for desktop
            if (IsDesktop)
            {
                graphicsDeviceManager.PreferredBackBufferWidth = 1280;
                graphicsDeviceManager.PreferredBackBufferHeight = 720;
                graphicsDeviceManager.ApplyChanges();
                IsMouseVisible = true;
                
                // Set window title
                Window.Title = "Zara's Basement";
            }
        }

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            
            // Create sprite batch
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Initialize save manager
            saveManager = new SaveManager();
            
            // Initialize screen manager
            screenManager = new ScreenManager(Content, GraphicsDevice);
            
            // Register all games
            RegisterGames();
            
            // Start with the hub screen
            var hubScreen = new HubScreen(screenManager, saveManager);
            screenManager.PushScreen(hubScreen, ScreenTransition.None);
        }
        
        /// <summary>
        /// Register all available mini-games.
        /// </summary>
        private void RegisterGames()
        {
            // Assimilate! - Flood-it puzzle game
            var assimilateInfo = new GameInfo
            {
                Id = "assimilate",
                Title = "Assimilate!",
                Description = "Fill the board with one color in limited moves.",
                ThumbnailPath = "Games/Assimilate/thumbnail",
                Difficulty = 2,
                EstimatedMinutes = 3,
                Tags = new[] { "Puzzle", "Strategy" }
            };
            GameRegistry.Register(assimilateInfo, () => new AssimilateGame());
            
            // TODO: Register more games here as they're added
        }

        /// <summary>
        /// Updates the game's logic, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for game updates.
        /// </param>
        protected override void Update(GameTime gameTime)
        {
            // Exit the game if the Back button (GamePad) or Escape key (Keyboard) is pressed
            // when on the hub screen (not during gameplay)
            if (screenManager?.ActiveScreen is HubScreen)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                    || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
            }

            // Update screen manager
            screenManager?.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game's graphics, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for rendering.
        /// </param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear screen
            GraphicsDevice.Clear(Color.Black);

            // Draw current screen
            if (spriteBatch != null)
            {
                screenManager?.Draw(spriteBatch);
            }

            base.Draw(gameTime);
        }
    }
}